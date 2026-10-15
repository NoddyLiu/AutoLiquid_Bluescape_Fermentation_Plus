using System;
using System.Text;

namespace AutoLiquid_Bluescape_Fermentation_Plus.Pump
{
    /// <summary>
    /// OEM 协议关键常量与地址校验。
    /// STX=0x02, ETX=0x03；
    /// BCC：从 STX 到 ETX（含）逐字节 XOR 得到 1 byte 校验。
    /// </summary>
    public static class Sp1Oem
    {
        public const byte Stx = 0x02;
        public const byte Etx = 0x03;

        /// <summary>
        /// SP1 OEM 协议字符集（ASCII subset）。
        /// 所有协议层字符串字段必须用这个 charset 做编解码。
        /// </summary>
        public static readonly Encoding Ascii = Encoding.ASCII;

        /// <summary>
        /// 校验地址是 OEM 地址字符 '0'..'9' / 'A'..'E'（共 15 个有效地址）。
        /// </summary>
        public static char NormalizePumpAddrAscii(char addr)
        {
            if (!((addr >= '0' && addr <= '9') || (addr >= 'A' && addr <= 'E')))
                throw new ArgumentException("Pump address must be ASCII '0'..'9' or 'A'..'E'");
            return addr;
        }
    }
}
