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
    /// 移液头设置父类
    /// </summary>
    [Serializable()]
    public class Head
    {
        // 是否启用移液头
        public bool Available = true;

        /**
         * 串口相关
         */
        public string SerialPortName = "COM1";
        public int SerialBaudRate = 9600;

        /**
         * Y轴相关
         */
        // Y轴是否可用
        public bool YAvailable = true;
        // Y轴是否使用移液头移动
        public bool YMoveWithHead = true;

        /**
         * P轴相关
         */
        // P轴是否可用
        public bool PAvailable = true;

        /**
         * 移液头属性
         */
        // 通道行数
        public int ChannelRow = 1;
        // 通道列数
        public int ChannelCol = 1;
        // 移液头量程
        public ELiquidRange HeadLiquidRange = ELiquidRange.Ten;
        // 行走逻辑
        public EWalkingLogic WalkingLogic = EWalkingLogic.SameTime;
    }
}
