using AutoLiquid_Bluescape_Fermentation_Plus.Utils;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace AutoLiquid_Bluescape_Fermentation_Plus.Pump
{
    /// <summary>
    /// 单台泵的协议客户端（对应 Android 端 Sp1OemClient.kt）。
    ///
    /// 关键约束：
    /// 1. 多泵共用同一 RS485 总线：所有 client 必须共享一把 <c>busMutex</c>
    ///    （由 <see cref="PumpManager"/> 创建并传入）。
    /// 2. 发送前清空接收队列：如果 transport 实现 <see cref="ISupportsPreSendClear"/>，会显式调用，
    ///    避免上一帧的残留字节混入下一帧解析。
    /// 3. 超时与重试：<see cref="AwaitOneResponseAsync"/> 在 <c>timeoutMs</c> 内流式解帧；
    ///    超时抛 <see cref="TimeoutException"/>。
    /// 4. 状态字查询自带重试容错：<see cref="QueryStatusWordAsync"/> 对异常返回 null。
    /// </summary>
    public sealed class Sp1OemClient
    {
        private readonly ITransport _transport;
        private readonly char _addr;
        private readonly long _defaultTimeoutMs;
        private readonly SemaphoreSlim _busMutex;

        public Sp1OemClient(ITransport transport,
                             char pumpAddrAscii,
                             SemaphoreSlim busMutex = null,
                             long defaultTimeoutMs = 5000)
        {
            if (transport == null) throw new ArgumentNullException(nameof(transport));
            _transport = transport;
            _addr = Sp1Oem.NormalizePumpAddrAscii(pumpAddrAscii);
            _defaultTimeoutMs = defaultTimeoutMs;
            _busMutex = busMutex ?? new SemaphoreSlim(1, 1);
        }

        /// <summary>泵标识，用于日志，形如 左泵(addr=1)。</summary>
        public string PumpTag
        {
            get
            {
                PumpId? pid = PumpIdExtensions.FromAddress(_addr);
                return pid.HasValue
                    ? (pid.Value.DisplayName() + "(addr=" + _addr + ")")
                    : ("泵(addr=" + _addr + ")");
            }
        }

        public Task OpenAsync(CancellationToken ct = default)
        {
            return Task.Run(() => _transport.Open(), ct);
        }

        public Task CloseAsync(CancellationToken ct = default)
        {
            return Task.Run(() => _transport.Close(), ct);
        }

        /// <summary>
        /// 发送 OEM dataBlock（ASCII 命令串），等待一帧合法回复。
        /// </summary>
        //public async Task<OemResponse> RequestAsync(string dataBlockAscii, long timeoutMs, CancellationToken ct = default)
        //{
        //    await _busMutex.WaitAsync(ct).ConfigureAwait(false);
        //    try
        //    {
        //        var frame = new OemFrame(_addr, '1', dataBlockAscii ?? "");
        //        var bytes = OemCodec.Encode(frame);

        //        // 发送前清空队列，防止粘连
        //        var clearable = _transport as ISupportsPreSendClear;
        //        if (clearable != null) clearable.ClearRxQueue();

        //        await Task.Run(() => _transport.Write(bytes), ct).ConfigureAwait(false);
        //        return await AwaitOneResponseAsync(timeoutMs, ct).ConfigureAwait(false);
        //    }
        //    finally
        //    {
        //        _busMutex.Release();
        //    }
        //}
        public async Task<OemResponse> RequestAsync(string dataBlockAscii, long timeoutMs, CancellationToken ct = default)
        {
            await _busMutex.WaitAsync(ct).ConfigureAwait(false);
            string cmd = dataBlockAscii ?? "";
            try
            {
                byte[] bytes;
                try
                {
                    bytes = OemCodec.Encode(new OemFrame(_addr, '1', cmd));
                }
                catch (Exception ex)
                {
                    LogHelper.Error(PumpTag + " 指令编码失败, cmd=" + cmd + ", err=" + ex.GetType().Name + ": " + ex.Message);
                    throw;
                }

                // 发送前清空队列，防止粘连
                var clearable = _transport as ISupportsPreSendClear;
                if (clearable != null) clearable.ClearRxQueue();

                try
                {
                    await Task.Run(() => _transport.Write(bytes), ct).ConfigureAwait(false);
                }
                catch (OperationCanceledException) { throw; }   // 取消不算故障
                catch (Exception ex)
                {
                    LogHelper.Error(PumpTag + " 指令发送失败, cmd=" + cmd
                        + ", tx=" + OemCodec.ToHexString(bytes)
                        + ", err=" + ex.GetType().Name + ": " + ex.Message);
                    throw;
                }

                try
                {
                    return await AwaitOneResponseAsync(timeoutMs, ct).ConfigureAwait(false);
                }
                catch (OperationCanceledException) { throw; }
                catch (TimeoutException ex)
                {
                    LogHelper.Error(PumpTag + " 指令响应超时, cmd=" + cmd
                        + ", timeout=" + timeoutMs + "ms, err=" + ex.Message);
                    throw;
                }
                catch (Exception ex)
                {
                    LogHelper.Error(PumpTag + " 指令响应异常, cmd=" + cmd
                        + ", err=" + ex.GetType().Name + ": " + ex.Message);
                    throw;
                }
            }
            finally
            {
                _busMutex.Release();
            }
        }

        public Task<OemResponse> RequestAsync(string dataBlockAscii, CancellationToken ct = default)
        {
            return RequestAsync(dataBlockAscii, _defaultTimeoutMs, ct);
        }

        /// <summary>
        /// 发送"指令串"并默认追加 R 执行（也可自己传已带 R 的串）。
        /// 文档：除"报告类指令"外，多数动作需 R 执行。
        /// </summary>
        public async Task<OemResponse> ExecuteChainAsync(string chainAscii,
                                                        bool autoAppendR = true,
                                                        long? timeoutMs = null,
                                                        CancellationToken ct = default)
        {
            string normalized = chainAscii ?? "";
            if (autoAppendR && !normalized.EndsWith("R"))
                normalized = normalized + "R";
            return await RequestAsync(normalized, timeoutMs ?? _defaultTimeoutMs, ct).ConfigureAwait(false);
        }

        /// <summary>查询（? 系列 / Q 等），通常不需要 R。</summary>
        public Task<OemResponse> QueryAsync(string queryAscii, long? timeoutMs = null, CancellationToken ct = default)
        {
            return RequestAsync(queryAscii ?? "", timeoutMs ?? _defaultTimeoutMs, ct);
        }

        /// <summary>
        /// 查询错误码和状态字；调用 Q 命令，从响应帧的 statusByte 解出。
        /// </summary>
        //public async Task<Sp1StatusWord> QueryStatusWordAsync(long? timeoutMs = null, CancellationToken ct = default)
        //{
        //    try
        //    {
        //        var resp = await QueryAsync(Sp1Cmd.QErrorAndStatusWord(), timeoutMs, ct).ConfigureAwait(false);
        //        int errCode = resp.ErrorCode;
        //        return new Sp1StatusWord(resp.StatusByte, resp.IsIdle, errCode, Sp1ErrorCodeExtensions.From(errCode));
        //    }
        //    catch (Exception)
        //    {
        //        return null;
        //    }
        //}
        public async Task<Sp1StatusWord> QueryStatusWordAsync(long? timeoutMs = null, CancellationToken ct = default)
        {
            try
            {
                var resp = await QueryAsync(Sp1Cmd.QErrorAndStatusWord(), timeoutMs, ct).ConfigureAwait(false);
                int errCode = resp.ErrorCode;
                return new Sp1StatusWord(resp.StatusByte, resp.IsIdle, errCode, Sp1ErrorCodeExtensions.From(errCode));
            }
            catch (OperationCanceledException)
            {
                throw;   // 取消不记为故障
            }
            catch (Exception ex)
            {
                // 原来这里完全静默返回 null，导致上层分不清"查询失败"和"泵不空闲"
                LogHelper.Error(PumpTag + " 状态字查询失败(Q指令), timeout=" + (timeoutMs ?? _defaultTimeoutMs)
                    + "ms, err=" + ex.GetType().Name + ": " + ex.Message);
                return null;
            }
        }

        /// <summary>
        /// 流式读取 chunk 流式解帧，直到命中一个完整响应或超时。
        /// 命中后立即返回；buffer 过大时裁掉老数据（保留 1KB 尾部 + 重置 cursor）。
        /// </summary>
        //private async Task<OemResponse> AwaitOneResponseAsync(long timeoutMs, CancellationToken ct)
        //{
        //    var startMs = CurrentTimeMillis();
        //    var buffer = new List<byte>();
        //    int cursor = 0;

        //    while (CurrentTimeMillis() - startMs < timeoutMs)
        //    {
        //        ct.ThrowIfCancellationRequested();

        //        byte[] chunk;
        //        try
        //        {
        //            // Transport.Read 是阻塞调用，包到 Task.Run 异步化
        //            chunk = await Task.Run(() => _transport.Read(256, 120), ct).ConfigureAwait(false);
        //        }
        //        catch (Exception)
        //        {
        //            chunk = new byte[0];
        //        }

        //        if (chunk != null && chunk.Length > 0)
        //        {
        //            buffer.AddRange(chunk);

        //            while (true)
        //            {
        //                byte[] ba = buffer.ToArray();
        //                var pair = OemCodec.TryDecodeOne(ba, cursor);
        //                var resp = pair.Item1;
        //                int next = pair.Item2;
        //                if (resp != null) return resp;
        //                if (next <= cursor) break;
        //                cursor = next;
        //                if (cursor >= buffer.Count) break;
        //            }

        //            // 防止 buffer 无限增长（保留尾部 1KB）
        //            if (buffer.Count > 4096)
        //            {
        //                buffer = buffer.GetRange(buffer.Count - 1024, 1024);
        //                cursor = 0;
        //            }
        //        }
        //        else
        //        {
        //            await Task.Delay(10, ct).ConfigureAwait(false);
        //        }
        //    }

        //    throw new TimeoutException("OEM response timeout: " + timeoutMs + "ms");
        //}
        private async Task<OemResponse> AwaitOneResponseAsync(long timeoutMs, CancellationToken ct)
        {
            var startMs = CurrentTimeMillis();
            var buffer = new List<byte>();
            int cursor = 0;
            int readFailCount = 0;
            bool readFailLogged = false;

            while (CurrentTimeMillis() - startMs < timeoutMs)
            {
                ct.ThrowIfCancellationRequested();

                byte[] chunk;
                try
                {
                    // Transport.Read 是阻塞调用，包到 Task.Run 异步化
                    chunk = await Task.Run(() => _transport.Read(256, 120), ct).ConfigureAwait(false);
                }
                catch (Exception ex)
                {
                    // 原来直接吞掉，串口掉了也会一路走到超时，日志里看不出根因
                    readFailCount++;
                    chunk = new byte[0];
                    if (!readFailLogged)
                    {
                        readFailLogged = true;
                        LogHelper.Error(PumpTag + " 串口读取异常(本轮共" + readFailCount + "次), err="
                            + ex.GetType().Name + ": " + ex.Message);
                    }
                }

                if (chunk != null && chunk.Length > 0)
                {
                    buffer.AddRange(chunk);

                    while (true)
                    {
                        byte[] ba = buffer.ToArray();
                        var pair = OemCodec.TryDecodeOne(ba, cursor);
                        var resp = pair.Item1;
                        int next = pair.Item2;
                        if (resp != null) return resp;
                        if (next <= cursor) break;
                        cursor = next;
                        if (cursor >= buffer.Count) break;
                    }

                    // 防止 buffer 无限增长（保留尾部 1KB）
                    if (buffer.Count > 4096)
                    {
                        buffer = buffer.GetRange(buffer.Count - 1024, 1024);
                        cursor = 0;
                    }
                }
                else
                {
                    await Task.Delay(10, ct).ConfigureAwait(false);
                }
            }

            long elapsed = CurrentTimeMillis() - startMs;
            throw new TimeoutException("OEM response timeout: " + timeoutMs + "ms"
                + " pump=" + _addr
                + " actualElapsed=" + elapsed + "ms"
                + " rxBytes=" + buffer.Count
                + " readFail=" + readFailCount);
        }

        private static long CurrentTimeMillis()
        {
            return (long)(DateTime.UtcNow - new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc)).TotalMilliseconds;
        }
    }
}
