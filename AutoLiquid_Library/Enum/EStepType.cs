using System;
using System.ComponentModel;

namespace AutoLiquid_Library.Enum
{
    /// <summary>
    /// 程序步骤类型
    /// </summary>
    public enum EStepType
    {
        // 移液
        [Description("移液")]
        LiquidTransfer,

        // 搅拌启动
        [Description("搅拌启动")]
        ShakeStart,

        // 搅拌停止
        [Description("搅拌停止")]
        ShakeStop,

        // 清洗
        [Description("清洗")]
        Cleaning,

        // 等待
        [Description("等待")]
        Wait,

        // 循环
        [Description("循环")]
        Loop,

        // 发酵温控
        [Description("发酵温控")]
        TempFermentation,

        // 留样温控
        [Description("留样温控")]
        TempSample
    }
}