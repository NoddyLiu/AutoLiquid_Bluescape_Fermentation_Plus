using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutoLiquid_Library.Utils
{
    public class Code
    {
        /**
        * 命令码
        */
        // 控制设备
        public static byte CMD_CONTROL = 0xC0;
        // 查询状态
        public static byte CMD_STATUS = 0xC1;

        /**
         * 设备类型/动作类型
         */
        // 移液机器人/启停程序
        public static byte DEVICE_PROGRAM = 0xA0;
        // 照明灯
        public static byte DEVICE_LIGHT = 0xA1;
        // 紫外灯
        public static byte DEVICE_UV = 0xA2;
        // 传递窗
        public static byte DEVICE_DELIVERY = 0xA3;

        /**
         * 错误码
         */
        // 与主机通信失败
        public static byte ERROR_COMM = 0xE0;
        // 没有可执行的程序
        public static byte ERROR_NO_PROGRAM = 0xE1;
    }
}
