using AutoLiquid_Bluescape_Fermentation_Plus.Utils;
using Serilog;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.IO.Ports;
using System.Threading;

namespace AutoLiquid_Bluescape_Fermentation_Plus.Pump
{
    /// <summary>
    /// System.IO.Ports 串口配置（与 Android 端 TtySConfig 字段一一对应）。
    ///
    /// 注意：原生 SerialPort 没有 Block/NonBlock 切换字段（那属于 jni 层 android_serialport_api），
    /// 这里保留 Block 字段仅供未来拓展，默认 false。
    /// </summary>
    public sealed class TtySConfig
    {
        public string PortName { get; set; }       // e.g. "COM3" (Windows) / "/dev/ttyS3" (Linux/macOS)
        public int BaudRate { get; set; } = 9600;
        public int DataBits { get; set; } = 8;
        public StopBits StopBits { get; set; } = StopBits.One;
        public Parity Parity { get; set; } = Parity.None;
        public bool Block { get; set; }              // 仅占位（未来可扩展为 native 调用的非阻塞标志）
        public int ReadQueueCapacity { get; set; } = 256;
    }

    /// <summary>
    /// 基于 System.IO.Ports 的串口传输实现（对应 Android 端 TtySTransport.kt）。
    ///
    /// 行为对齐：
    /// - 后台读线程：DataReceived 事件触发读串口，把字节块投到 BlockingCollection 队列
    /// - 队列满时丢最旧 + 入新（避免阻塞 IO 线程）
    /// - 读：阻塞 poll(timeout) 拿第一段，再短轮询合并直到 maxLen
    /// - 写：lock + Write + Flush
    /// - ClearRxQueue 在 Sp1OemClient 发送前会显式调用
    /// </summary>
    public sealed class TtySTransport : ITransport, ISupportsPreSendClear, IDisposable
    {
        private readonly TtySConfig _config;
        private SerialPort _serialPort;

        private volatile bool _running;
        private readonly TtyRxQueue _rxQueue;
        private readonly object _writeLock = new object();
        // 后台读线程异常日志限流计数（防止串口掉线后刷屏）
        private int _rxErrCount;

        public PumpManager mgr;

        public TtySTransport(TtySConfig config)
        {
            if (config == null) throw new ArgumentNullException(nameof(config));
            _config = config;
            _rxQueue = new TtyRxQueue(Math.Max(8, _config.ReadQueueCapacity));
        }

        public bool IsOpen { get { return _running; } }

        public void Open()
        {
            if (_running) return;
            try
            {
                _serialPort = new SerialPort(_config.PortName, _config.BaudRate, _config.Parity, _config.DataBits, _config.StopBits);
                _serialPort.Open();
                _serialPort.DataReceived += OnDataReceived;
                _running = true;
                _rxQueue.Clear();

                // 注册泵
                mgr = new PumpManager(this, null);
                mgr.RegisterPump(PumpId.Left);
                _ = mgr.OpenAllAsync();
                LogHelper.Info("注射泵已连接", "");
            }
            catch (Exception ex)
            {
                _running = false;
                SafeCloseInternal();
                LogHelper.Error("[注射泵] 串口打开失败, port=" + _config.PortName
                    + ", baud=" + _config.BaudRate
                    + ", dataBits=" + _config.DataBits
                    + ", stopBits=" + _config.StopBits
                    + ", parity=" + _config.Parity
                    + ", err=" + ex.GetType().Name + ": " + ex.Message);
                throw;
            }
        }

        public void Close()
        {
            _running = false;
            try
            {
                if (_serialPort != null)
                {
                    try { _serialPort.DataReceived -= OnDataReceived; } catch { }
                }
            }
            catch { /* ignore */ }

            SafeCloseInternal();
            _rxQueue.Clear();
        }

        /// <summary>
        /// 由 SerialPort 在后台 IO 线程触发。
        /// </summary>
        //private void OnDataReceived(object sender, SerialDataReceivedEventArgs e)
        //{
        //    SerialPort sp;
        //    try { sp = _serialPort; }
        //    catch { return; }
        //    if (sp == null || !_running) return;

        //    int avail;
        //    try { avail = sp.BytesToRead; }
        //    catch { return; }
        //    if (avail <= 0) return;

        //    // 单次最多读 1KB，防止长帧把 IO 线程吊死
        //    int toRead = Math.Min(avail, 1024);
        //    var buf = new byte[toRead];
        //    int read = 0;
        //    try { read = sp.Read(buf, 0, toRead); }
        //    catch { return; }

        //    if (read > 0)
        //    {
        //        var chunk = new byte[read];
        //        Array.Copy(buf, 0, chunk, 0, read);
        //        _rxQueue.Enqueue(chunk);
        //    }
        //}
        private void OnDataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            SerialPort sp;
            try { sp = _serialPort; }
            catch (Exception ex)
            {
                LogRxError("取串口对象失败", ex);
                return;
            }
            if (sp == null || !_running) return;

            int avail;
            try { avail = sp.BytesToRead; }
            catch (Exception ex)
            {
                LogRxError("BytesToRead 失败", ex);
                return;
            }
            if (avail <= 0) return;

            // 单次最多读 1KB，防止长帧把 IO 线程吊死
            int toRead = Math.Min(avail, 1024);
            var buf = new byte[toRead];
            int read = 0;
            try { read = sp.Read(buf, 0, toRead); }
            catch (Exception ex)
            {
                LogRxError("读取失败", ex);
                return;
            }

            if (read > 0)
            {
                var chunk = new byte[read];
                Array.Copy(buf, 0, chunk, 0, read);
                _rxQueue.Enqueue(chunk);
                _rxErrCount = 0;   // 读成功后复位限流
            }
        }

        /// <summary>后台读线程异常日志（前 3 次必打，之后只计数，防止串口掉线刷屏）。</summary>
        private void LogRxError(string scene, Exception ex)
        {
            _rxErrCount++;
            if (_rxErrCount > 3) return;
            LogHelper.Error("[注射泵] 后台读线程" + scene + ", port=" + _config.PortName
                + ", 累计=" + _rxErrCount
                + ", err=" + ex.GetType().Name + ": " + ex.Message);
        }

        //public void Write(byte[] data)
        //{
        //    if (data == null) throw new ArgumentNullException(nameof(data));
        //    var sp = _serialPort;
        //    if (sp == null || !_running)
        //        throw new InvalidOperationException("Transport not open");

        //    lock (_writeLock)
        //    {
        //        sp.Write(data, 0, data.Length);
        //    }
        //    // SerialPort 没有独立 Flush（BaseStream.Flush 同样有效，Write 之后 SerialPort 内部 buffer 会自己排空；
        //    // 这里为了对齐 Android 端的"write+flush"语义，主动 flush BaseStream）
        //    try { sp.BaseStream.Flush(); } catch { /* best effort */ }
        //}
        public void Write(byte[] data)
        {
            if (data == null) throw new ArgumentNullException(nameof(data));
            var sp = _serialPort;
            if (sp == null || !_running)
            {
                LogHelper.Error("[注射泵] 串口未打开就尝试发送, port=" + _config.PortName + ", len=" + data.Length);
                throw new InvalidOperationException("Transport not open");
            }

            lock (_writeLock)
            {
                try
                {
                    sp.Write(data, 0, data.Length);
                }
                catch (Exception ex)
                {
                    LogHelper.Error("[注射泵] 串口写失败, port=" + _config.PortName
                        + ", len=" + data.Length
                        + ", err=" + ex.GetType().Name + ": " + ex.Message);
                    throw;
                }
            }
            // SerialPort 没有独立 Flush（BaseStream.Flush 同样有效，Write 之后 SerialPort 内部 buffer 会自己排空；
            // 这里为了对齐 Android 端的"write+flush"语义，主动 flush BaseStream）
            try { sp.BaseStream.Flush(); } catch { /* best effort */ }
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

        private static byte[] SubArray(byte[] src, int offset, int length)
        {
            var dst = new byte[length];
            Array.Copy(src, offset, dst, 0, length);
            return dst;
        }

        private void SafeCloseInternal()
        {
            try { if (_serialPort != null) { _serialPort.Dispose(); } } catch { }
            finally { _serialPort = null; }
        }

        public void Dispose()
        {
            Close();
            SafeCloseInternal();
            _rxQueue.Dispose();
        }
    }

    /// <summary>
    /// TtySTransport 内部串口接收队列（对应 Android 端 rxQueue: LinkedBlockingQueue）。
    /// 用 BlockingCollection&lt;byte[]&gt; 实现；满了丢最旧再入新。
    /// </summary>
    internal sealed class TtyRxQueue : IDisposable
    {
        private readonly BlockingCollection<byte[]> _q;
        private bool _disposed;

        public TtyRxQueue(int capacity)
        {
            _q = new BlockingCollection<byte[]>(capacity);
        }

        public void Enqueue(byte[] item)
        {
            if (_disposed) return;
            // 队列满 → 丢最旧再入新（与 Android 端 rxQueue.offer/peek/poll 一致）
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
