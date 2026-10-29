using AutoLiquid_Bluescape_Fermentation_Plus.Utils;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Net.Sockets;
using System.Threading;

namespace AutoLiquid_Bluescape_Fermentation_Plus.Pump
{
    /// <summary>
    /// 注射泵 TCP 连接配置
    /// </summary>
    public sealed class TcpConfig
    {
        // 注射泵 IP（串口转网口的设备地址）
        public string Host { get; set; } = "192.168.1.7";
        // 注射泵 TCP 端口
        public int Port { get; set; } = 8234;
        // 连接超时（毫秒）
        public int ConnectTimeoutMs { get; set; } = 3000;
        // 接收队列容量
        public int ReadQueueCapacity { get; set; } = 256;
    }

    /// <summary>
    /// 注射泵 TCP 传输实现（替代原串口 TtySTransport）。
    ///
    /// 行为与串口版对齐：
    /// - Open：TcpClient.Connect + 启动后台接收线程
    /// - Write：加锁写 + Flush；socket 断开时自动重连一次并重发
    /// - Read：阻塞取第一段，再短轮询合并直到 maxLen
    /// - ClearRxQueue：发送前清残留（实现 ISupportsPreSendClear）
    /// </summary>
    public sealed class TcpTransport : ITransport, ISupportsPreSendClear, IDisposable
    {
        private TcpConfig _config;
        private TcpClient _client;
        private NetworkStream _stream;
        private volatile bool _running;
        private Thread _rxThread;
        private readonly RxByteQueue _rxQueue;
        private readonly object _writeLock = new object();
        private int _rxErrCount;

        public PumpManager mgr;

        public TcpTransport(TcpConfig config)
        {
            if (config == null) throw new ArgumentNullException(nameof(config));
            _config = config;
            _rxQueue = new RxByteQueue(Math.Max(8, _config.ReadQueueCapacity));

            // 泵管理器在这里创建（不放 Open 里）：
            // 保证连接失败时 mgr 也不是 null，上层不会出现 NullReferenceException
            mgr = new PumpManager(this, null);
            mgr.RegisterPump(PumpId.Left);
        }

        /// <summary>当前配置，可在设置页改完后调用 <see cref="Reconnect"/> 生效</summary>
        public TcpConfig Config { get { return _config; } }

        public bool IsOpen { get { return _running && _client != null && _client.Connected; } }

        // ================= 连接管理 =================

        public void Open()
        {
            if (_running) return;
            Connect();
        }

        /// <summary>按新配置关闭并重连（设置页改完 IP/端口后调用）</summary>
        public void Reconnect(TcpConfig config = null)
        {
            if (config != null) _config = config;
            lock (_writeLock)
            {
                ReconnectInternal();
            }
        }

        private void ReconnectInternal()
        {
            _running = false;
            try { if (_rxThread != null) _rxThread.Join(200); } catch { }
            _rxThread = null;
            SafeCloseInternal();
            _rxQueue.Clear();
            Connect();
        }

        private void Connect()
        {
            try
            {
                var client = new TcpClient();
                client.NoDelay = true;   // 关掉 Nagle，指令响应更快
                // 打开 TCP KeepAlive，防止中间设备静默断链后长期不可用
                client.Client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.KeepAlive, true);

                var ar = client.BeginConnect(_config.Host, _config.Port, null, null);
                bool connected;
                using (var wait = ar.AsyncWaitHandle)
                {
                    connected = wait.WaitOne(_config.ConnectTimeoutMs, false);
                }
                if (!connected)
                {
                    client.Close();
                    throw new TimeoutException("TCP连接超时 " + _config.ConnectTimeoutMs + "ms");
                }
                client.EndConnect(ar);   // 连接失败抛 SocketException

                _client = client;
                _stream = client.GetStream();
                _running = true;
                _rxQueue.Clear();

                _rxThread = new Thread(RxLoop) { IsBackground = true, Name = "PumpTcpRx" };
                _rxThread.Start();

                LogHelper.Info("注射泵TCP已连接 ", _config.Host + ":" + _config.Port);
            }
            catch (Exception ex)
            {
                _running = false;
                SafeCloseInternal();
                LogHelper.Error("[注射泵] TCP连接失败, " + _config.Host + ":" + _config.Port
                    + ", err=" + ex.GetType().Name + ": " + ex.Message);
                throw ex;
            }
        }

        public void Close()
        {
            lock (_writeLock)
            {
                _running = false;
                try { if (_rxThread != null) _rxThread.Join(200); } catch { }
                _rxThread = null;
                SafeCloseInternal();
                _rxQueue.Clear();
            }
        }

        private void SafeCloseInternal()
        {
            try { if (_stream != null) { _stream.Close(); _stream = null; } } catch { }
            try { if (_client != null) { _client.Close(); _client = null; } } catch { }
        }

        // ================= 数据收发 =================

        public void Write(byte[] data)
        {
            if (data == null) throw new ArgumentNullException(nameof(data));

            lock (_writeLock)
            {
                if (!IsOpen)
                {
                    // 连接已断（或还没连上）：先重连一次
                    LogHelper.Error("[注射泵] TCP未连接，尝试重连 " + _config.Host + ":" + _config.Port);
                    ReconnectInternal();
                }

                try
                {
                    _stream.Write(data, 0, data.Length);
                    _stream.Flush();
                }
                catch (Exception ex)
                {
                    // 关键：只断开、不重发！
                    // 串口转网口模块在断链瞬间可能已把数据送进泵的串口，
                    // 盲目重发会导致同一条指令被泵执行两次（吸两次液等），
                    // 且 EnsureNoError 查状态字时泵已回空闲，查不出来。
                    // 正确做法：抛给上层，由上层结合泵状态字决定是否重做。
                    LogHelper.Error("[注射泵] TCP发送失败，已断开不重发, len=" + data.Length
                        + ", err=" + ex.GetType().Name + ": " + ex.Message);

                    _running = false;
                    SafeCloseInternal();
                    throw;   // 下一条指令的 Write 会自动重连
                }
            }
        }

        public byte[] Read(int maxLen = 256, int timeoutMs = 200)
        {
            if (!_running) return new byte[0];

            byte[] first = _rxQueue.TryTake(timeoutMs);
            if (first == null) return new byte[0];

            if (first.Length >= maxLen)
                return SubArray(first, 0, maxLen);

            var merged = new List<byte>(Math.Min(maxLen, first.Length + 64));
            merged.AddRange(first);

            while (merged.Count < maxLen)
            {
                var next = _rxQueue.TryTake(5);
                if (next == null) break;
                int remain = maxLen - merged.Count;
                if (next.Length <= remain)
                    merged.AddRange(next);
                else
                    merged.AddRange(SubArray(next, 0, remain));
            }

            return merged.ToArray();
        }

        public void ClearRxQueue()
        {
            _rxQueue.Clear();
        }

        // ================= 后台接收线程 =================

        private void RxLoop()
        {
            var buf = new byte[1024];
            while (_running)
            {
                try
                {
                    int read = _stream.Read(buf, 0, buf.Length);
                    if (read <= 0)
                    {
                        // 对端正常关闭连接
                        LogHelper.Error("[注射泵] TCP连接被对端关闭 " + _config.Host + ":" + _config.Port);
                        break;
                    }
                    var chunk = new byte[read];
                    Array.Copy(buf, 0, chunk, 0, read);
                    _rxQueue.Enqueue(chunk);
                    _rxErrCount = 0;
                }
                catch (Exception ex)
                {
                    if (!_running) break;
                    LogRxError("接收线程异常", ex);
                    break;   // 连接已断，退出线程；下次 Write 会触发重连
                }
            }
            _running = false;
        }

        /// <summary>接收线程异常日志限流（前 3 次必打，之后只计数，防止掉线刷屏）</summary>
        private void LogRxError(string scene, Exception ex)
        {
            _rxErrCount++;
            if (_rxErrCount > 3) return;
            LogHelper.Error("[注射泵] TCP" + scene + ", " + _config.Host + ":" + _config.Port
                + ", 累计=" + _rxErrCount
                + ", err=" + ex.GetType().Name + ": " + ex.Message);
        }

        private static byte[] SubArray(byte[] src, int offset, int length)
        {
            var dst = new byte[length];
            Array.Copy(src, offset, dst, 0, length);
            return dst;
        }

        public void Dispose()
        {
            Close();
            _rxQueue.Dispose();
        }
    }

    /// <summary>
    /// 传输层接收队列（串口 / TCP 共用）。
    /// 用 BlockingCollection&lt;byte[]&gt; 实现；满了丢最旧再入新。
    /// </summary>
    internal sealed class RxByteQueue : IDisposable
    {
        private readonly BlockingCollection<byte[]> _q;
        private bool _disposed;

        public RxByteQueue(int capacity)
        {
            _q = new BlockingCollection<byte[]>(capacity);
        }

        public void Enqueue(byte[] item)
        {
            if (_disposed) return;
            // 队列满 → 丢最旧再入新
            if (_q.Count >= _q.BoundedCapacity)
            {
                byte[] dropped;
                _q.TryTake(out dropped);
            }
            try { _q.Add(item); }
            catch (InvalidOperationException) { /* adding 已完成 */ }
        }

        /// <summary>在 timeoutMs 内阻塞等一段；超时返回 null。</summary>
        public byte[] TryTake(int timeoutMs)
        {
            if (_disposed) return null;
            try
            {
                // net48 BlockingCollection.TryTake 重载：out 在前、TimeSpan 在后
                byte[] result;
                bool ok = _q.TryTake(out result, new TimeSpan(0, 0, 0, 0, timeoutMs));
                return ok ? result : null;
            }
            catch (TimeoutException) { return null; }
            catch (OperationCanceledException) { return null; }
            catch (InvalidOperationException) { return null; }
        }

        public int Count { get { return _q.Count; } }

        public void Clear()
        {
            if (_disposed) return;
            byte[] tmp;
            while (_q.TryTake(out tmp)) { }
        }

        public void Dispose()
        {
            if (_disposed) return;
            _disposed = true;
            try { _q.CompleteAdding(); } catch { }
            _q.Dispose();
        }
    }
}