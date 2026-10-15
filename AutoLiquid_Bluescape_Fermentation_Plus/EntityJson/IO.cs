using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Documents;
using AutoLiquid_Library.Enum;

namespace AutoLiquid_Bluescape_Fermentation_Plus.EntityJson
{
    /// <summary>
    /// 输入输出
    /// </summary>
    [Serializable()]
    public class IO
    {
        // 是否启用仓门
        public bool DoorAvailable = false;

        /**
         * 外部设备通信
         */
        // 是否打开串口通信
        public bool SerialPortAvailable = false;
        // 串口号
        public string SeialPort = "COM1";

        // 是否启用急停
        public bool EmergencyStopAvailable = false;

    }
}
