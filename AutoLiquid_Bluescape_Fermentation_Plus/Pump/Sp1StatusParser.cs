using System;

namespace AutoLiquid_Bluescape_Fermentation_Plus.Pump
{
    /// <summary>
    /// SP1 错误码（statusByte 低 4 bit）。
    /// 与 Android 端 Sp1ErrorCode 一一对应。
    /// </summary>
    public enum Sp1ErrorCode
    {
        None = 0,
        InitFailed = 1,             // 初始化错误（致命）
        UnknownCmd = 2,             // 未知指令
        BadParam = 3,               // 参数错误
        BadSequence = 4,            // 非法指令序列
        Reserved5 = 5,
        Reserved6 = 6,              // 保留/EEPROM 失败
        NotInitialized = 7,         // 泵未初始化
        PistonOverload = 9,         // 活塞过载（致命）
        ValveOverload = 10,         // 阀过载（致命）
        PistonMoveNotAllowed = 11,  // 当前状态不允许活塞移动
        CmdOverflow = 15,           // 指令溢出
    }

    /// <summary>
    /// Sp1ErrorCode 的辅助方法（用扩展方法兼容 net48）。
    /// </summary>
    public static class Sp1ErrorCodeExtensions
    {
        public static string DescZh(this Sp1ErrorCode code)
        {
            switch (code)
            {
                case Sp1ErrorCode.None: return "无错误";
                case Sp1ErrorCode.InitFailed: return "初始化错误（致命）";
                case Sp1ErrorCode.UnknownCmd: return "未知指令";
                case Sp1ErrorCode.BadParam: return "参数错误";
                case Sp1ErrorCode.BadSequence: return "非法指令序列";
                case Sp1ErrorCode.Reserved5: return "保留";
                case Sp1ErrorCode.Reserved6: return "保留/EEPROM失败（以设备实际为准）";
                case Sp1ErrorCode.NotInitialized: return "泵未初始化";
                case Sp1ErrorCode.PistonOverload: return "活塞过载（致命）";
                case Sp1ErrorCode.ValveOverload: return "阀过载（致命）";
                case Sp1ErrorCode.PistonMoveNotAllowed: return "当前状态不允许活塞移动";
                case Sp1ErrorCode.CmdOverflow: return "指令溢出（长度/时序不合法）";
                default: return "未知错误(" + (int)code + ")";
            }
        }

        /// <summary>把 raw int 解释为 enum，未知值返回 null。</summary>
        public static Sp1ErrorCode? From(int code)
        {
            if (Enum.IsDefined(typeof(Sp1ErrorCode), code))
                return (Sp1ErrorCode)code;
            return null;
        }
    }

    /// <summary>
    /// 状态字包装（raw + 派生）。
    /// </summary>
    public sealed class Sp1StatusWord
    {
        public int Raw { get; }
        public bool IsIdle { get; }
        public int ErrorCode { get; }
        public Sp1ErrorCode? Error { get; }

        public bool IsBusy { get { return !IsIdle; } }
        public bool HasError { get { return ErrorCode != 0; } }

        public Sp1StatusWord(int raw, bool isIdle, int errorCode, Sp1ErrorCode? error)
        {
            Raw = raw;
            IsIdle = isIdle;
            ErrorCode = errorCode;
            Error = error;
        }
    }
}
