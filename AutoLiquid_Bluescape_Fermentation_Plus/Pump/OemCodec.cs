using System;
using System.Text;

namespace AutoLiquid_Bluescape_Fermentation_Plus.Pump
{
    /// <summary>
    /// OEM 发送帧（请求）。
    /// 格式：STX + 地址 + 序号 + 数据 + ETX + BCC
    /// </summary>
    public sealed class OemFrame
    {
        public char PumpAddrAscii { get; }
        public char SeqAscii { get; }
        public string DataBlockAscii { get; }

        public OemFrame(char pumpAddrAscii, char seqAscii, string dataBlockAscii)
        {
            PumpAddrAscii = pumpAddrAscii;
            SeqAscii = seqAscii;
            DataBlockAscii = dataBlockAscii ?? string.Empty;
        }
    }

    /// <summary>
    /// OEM 响应帧（设备回复）。
    /// 格式：STX + 地址(1) + 状态字(1) + 数据(0..N) + ETX + BCC
    ///
    /// statusByte 语义（与 Android 端 OemResponse 对齐）：
    ///   bit5 = 1 表示设备空闲（isIdle）
    ///   bit0..bit3 = 错误码
    /// </summary>
    public sealed class OemResponse
    {
        public byte[] RawFrame { get; }
        public char PumpAddrAscii { get; }
        public int StatusByte { get; }
        public string DataBlockAscii { get; }

        public bool IsIdle { get { return ((StatusByte >> 5) & 0x01) == 1; } }
        public bool IsBusy { get { return !IsIdle; } }
        public int ErrorCode { get { return StatusByte & 0x0F; } }
        public bool HasError { get { return ErrorCode != 0; } }

        /// <summary>低 4 bit 对应的错误码枚举（未知值返回 null）。</summary>
        public Sp1ErrorCode? Error { get { return Sp1ErrorCodeExtensions.From(ErrorCode); } }

        public OemResponse(byte[] rawFrame, char pumpAddrAscii, int statusByte, string dataBlockAscii)
        {
            RawFrame = rawFrame;
            PumpAddrAscii = pumpAddrAscii;
            StatusByte = statusByte;
            DataBlockAscii = dataBlockAscii ?? string.Empty;
        }

        public override bool Equals(object obj)
        {
            var other = obj as OemResponse;
            if (other == null) return false;
            if (ReferenceEquals(this, other)) return true;
            return RawFrameEquals(other.RawFrame);
        }

        private bool RawFrameEquals(byte[] other)
        {
            if (other == null || RawFrame == null) return false;
            if (RawFrame.Length != other.Length) return false;
            for (int i = 0; i < RawFrame.Length; i++)
                if (RawFrame[i] != other[i]) return false;
            return true;
        }

        public override int GetHashCode()
        {
            unchecked
            {
                int h = 17;
                if (RawFrame != null)
                {
                    for (int i = 0; i < RawFrame.Length; i++)
                        h = h * 31 + RawFrame[i];
                }
                return h;
            }
        }

        public override string ToString()
        {
            return string.Format(
                "OemResponse(addr={0}, status=0x{1:X2}, data='{2}', isIdle={3}, errCode={4})",
                PumpAddrAscii, StatusByte, DataBlockAscii, IsIdle, ErrorCode);
        }
    }

    /// <summary>
    /// 帧编/解码（对应 Android 端 OemCodec.kt）。
    ///
    /// 发送帧格式：STX(02) + addr(1 ASCII) + seq('1') + data + ETX(03) + BCC(1)
    /// 响应帧格式：STX(02) + addr(1) + status(1) + data(0..N) + ETX(03) + BCC(1)
    ///
    /// BCC 计算：从 STX 到 ETX（含）所有字节 XOR。
    /// 例：响应 02 30 60 31 39 37 33 03 5D
    ///   STX addr status data      ETX BCC
    ///   02  '0'  0x60  '1973'    03  BCC
    ///   上式中 BCC = 02 ^ 30 ^ 60 ^ 31 ^ 39 ^ 37 ^ 33 ^ 03 = 0x5D
    /// </summary>
    public static class OemCodec
    {
        private const string Tag = "OemCodec";

        /// <summary>
        /// 把 <see cref="OemFrame"/> 编码为字节序列（含 STX..ETX..BCC）。
        /// </summary>
        public static byte[] Encode(OemFrame frame)
        {
            if (frame == null) throw new ArgumentNullException(nameof(frame));
            var addr = Sp1Oem.NormalizePumpAddrAscii(frame.PumpAddrAscii);
            if (frame.SeqAscii != '1')
                throw new ArgumentException("OEM seq must be ASCII '1'");

            // payloadAscii = addr + '1' + data
            var payloadStr = new StringBuilder();
            payloadStr.Append(addr);
            payloadStr.Append(frame.SeqAscii);
            payloadStr.Append(frame.DataBlockAscii);
            var payloadBytes = Sp1Oem.Ascii.GetBytes(payloadStr.ToString());

            // bytesWithoutBcc = STX + payload + ETX
            var bytesWithoutBcc = new byte[1 + payloadBytes.Length + 1];
            bytesWithoutBcc[0] = Sp1Oem.Stx;
            Array.Copy(payloadBytes, 0, bytesWithoutBcc, 1, payloadBytes.Length);
            bytesWithoutBcc[bytesWithoutBcc.Length - 1] = Sp1Oem.Etx;

            // BCC = XOR(STX..ETX)
            byte bcc = 0;
            for (int i = 0; i < bytesWithoutBcc.Length; i++)
                bcc = (byte)(bcc ^ bytesWithoutBcc[i]);

            var result = new byte[bytesWithoutBcc.Length + 1];
            Array.Copy(bytesWithoutBcc, 0, result, 0, bytesWithoutBcc.Length);
            result[result.Length - 1] = bcc;

            Console.WriteLine("[{0}] encode: {1}", Tag, ToHexString(result));
            return result;
        }

        /// <summary>
        /// 在流式 buffer 中试着解出一帧。
        /// 返回值是 (response, nextCursorIndex)；
        /// 当 <c>response == null</c> 时表示尚未解出，下一次再喂 <c>nextCursorIndex</c> 继续。
        /// 当 BCC 不匹配、payload 长度不足等错误时返回 (null, stxIndex+1) 让调用方按"跳一字节继续"。
        /// </summary>
        public static Tuple<OemResponse, int> TryDecodeOne(byte[] buffer, int startIndex)
        {
            if (buffer == null) throw new ArgumentNullException(nameof(buffer));
            int i = startIndex;

            // 1. 找 STX
            while (i < buffer.Length && buffer[i] != Sp1Oem.Stx) i++;
            if (i >= buffer.Length) return new Tuple<OemResponse, int>(null, buffer.Length);

            int stxIndex = i;

            // 2. 找 ETX
            int etxIndex = -1;
            i = stxIndex + 1;
            while (i < buffer.Length)
            {
                if (buffer[i] == Sp1Oem.Etx)
                {
                    etxIndex = i;
                    break;
                }
                i++;
            }
            if (etxIndex < 0) return new Tuple<OemResponse, int>(null, stxIndex);    // 等更多数据

            // 3. 检查 BCC 是否就位
            if (etxIndex + 1 >= buffer.Length) return new Tuple<OemResponse, int>(null, stxIndex); // 缺 BCC

            byte bcc = buffer[etxIndex + 1];

            // 4. 验证 BCC (XOR STX..ETX inclusive)
            byte calc = 0;
            for (int k = stxIndex; k <= etxIndex; k++)
                calc = (byte)(calc ^ buffer[k]);
            if (calc != bcc)
            {
                Console.WriteLine("[{0}] BCC mismatch: calculated=0x{1:X2}, received=0x{2:X2}",
                    Tag, calc, bcc);
                Console.WriteLine("[{0}] Frame: {1}", Tag, ToHexString(SubArray(buffer, stxIndex, etxIndex + 2 - stxIndex)));
                return new Tuple<OemResponse, int>(null, stxIndex + 1);   // 跳过这一字节继续
            }

            // 5. 提取 payload (STX 和 ETX 之间的内容)
            int payloadLen = etxIndex - stxIndex - 1;
            var payload = SubArray(buffer, stxIndex + 1, payloadLen);
            int rawLen = etxIndex + 2 - stxIndex;
            var raw = SubArray(buffer, stxIndex, rawLen);

            Console.WriteLine("[{0}] Frame: {1}", Tag, ToHexString(raw));
            Console.WriteLine("[{0}] Payload: {1}", Tag, ToHexString(payload));

            // 6. 解析 payload: addr(1) + status(1) + data(0..N)
            if (payload.Length < 2)
            {
                Console.WriteLine("[{0}] Payload too short: {1} bytes", Tag, payload.Length);
                return new Tuple<OemResponse, int>(null, etxIndex + 2);
            }

            char addr = (char)(payload[0] & 0x7F);
            int statusByte = payload[1] & 0xFF;

            string dataBlock = payload.Length > 2
                ? Sp1Oem.Ascii.GetString(payload, 2, payload.Length - 2)
                : "";

            Console.WriteLine("[{0}] Decoded: addr='{1}', statusByte=0x{2:X2}, data='{3}'",
                Tag, addr, statusByte, dataBlock);

            return new Tuple<OemResponse, int>(
                new OemResponse(raw, addr, statusByte, dataBlock),
                etxIndex + 2);
        }

        public static Tuple<OemResponse, int> TryDecodeOne(byte[] buffer)
        {
            return TryDecodeOne(buffer, 0);
        }

        private static byte[] SubArray(byte[] src, int offset, int length)
        {
            var dst = new byte[length];
            Array.Copy(src, offset, dst, 0, length);
            return dst;
        }

        public static string ToHexString(byte[] bytes)
        {
            if (bytes == null) return "";
            var sb = new StringBuilder(bytes.Length * 3);
            for (int i = 0; i < bytes.Length; i++)
            {
                if (i > 0) sb.Append(' ');
                sb.AppendFormat("{0:X2}", bytes[i]);
            }
            return sb.ToString();
        }
    }
}
