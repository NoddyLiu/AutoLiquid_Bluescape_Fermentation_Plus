namespace AutoLiquid_Library.Enum
{
    /// <summary>
    /// 移液头状态
    /// </summary>
    public enum EHeadStatus
    {
        Null, // 未知状态，默认
        Absorbed, // 已经吸液
        Jetted, // 已经喷液
        Cleaned // 已经清洗
    }
}