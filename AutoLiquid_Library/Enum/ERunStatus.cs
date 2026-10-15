namespace AutoLiquid_Library.Enum
{
    /// <summary>
    /// 客户端运行状态
    /// </summary>
    public enum ERunStatus
    {
        Initializing, // 初始化中
        Running, // 运行中
        Pause, // 暂停
        Stop, // 停止
        Continue, // 继续
        Countdown // 倒计时中
    }
}