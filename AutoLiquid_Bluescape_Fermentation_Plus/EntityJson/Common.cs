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
    /// 移液头通用设置
    /// </summary>
    [Serializable()]
    public class Common
    {
        /**
         * 速度设定
         */
        // 默认X
        public string DefaultXSpeed = "";
        // X速度百分比
        public decimal XSpeedPercent = 100.00m;
        // 默认Y
        public string DefaultYSpeed = "";
        // Y速度百分比
        public decimal YSpeedPercent = 100.00m;
        // 默认Z
        public string DefaultZSpeed = "";
        // Z速度百分比
        public decimal ZSpeedPercent = 100.00m;
        // 默认P
        public string DefaultPSpeed = "";
        // P速度百分比
        public decimal PSpeedPercent = 100.00m;
        // 默认W
        public string DefaultWSpeed = "";
        // W速度百分比
        public decimal WSpeedPercent = 100.00m;
        // 默认Q
        public string DefaultQSpeed = "";
        // Q速度百分比
        public decimal QSpeedPercent = 100.00m;
        // 默认M
        public string DefaultMSpeed = "";
        // M速度百分比
        public decimal MSpeedPercent = 100.00m;

        /**
         * 耗材（可添加删除）
         */
        public List<Consumable> Consumables = new List<Consumable>();

        /**
         * 吸喷液前处理
         */
        // 吸液前吸空气量（毫升）
        public decimal AbsorbAirBeforePercent = 0.0m;
        // 吸液后吸空气量（量程百分比）
        public decimal AbsorbAirAfterPercent = 0.0m;
        // 一吸多喷吸液后多吸体积（量程百分比）
        public decimal AbsorbLiquidMoreOne2MorePercent = 0.0m;
        // 一吸多喷吸液后喷出体积比例
        public decimal JetLiquidMoreOne2MoreScale = 0.0m;

        /**
         * 分段喷液
         */
        // 第1段喷液速度
        public string SpeedJet1 = "";
        // 第1段喷液速度百分比
        public decimal Jet1SpeedPercent = 100.00m;
        // 第2段喷液速度
        public string SpeedJet2 = "";
        // 第2段喷液速度百分比
        public decimal Jet2SpeedPercent = 100.00m;
        // 第2段喷液体积
        public decimal VolumeJet2 = 0.0m;
        // 两段喷液间停留时间（ms）
        public int DelayBetweenJet = 0;

        /**
         * 盘位摆放（即A1方向）
         */
        public EA1Pos A1Pos = EA1Pos.LeftTop;

        /**
         * 搅拌
         */
        public string ShakeSpeedMaxCmd = "";

        /**
         * 温度设置
         */
        // 制冷温度（弃用）
        public decimal CoolTemp = 0.0m;
        // 加热补偿（弃用）
        public decimal HeatTempCompensation = 0.0m;
        // 留样区制冷温度补偿
        public decimal CoolTempSampleCompensation = 0.0m;
        // 留样区制冷温度
        public decimal CoolTempSample = 0.0m;

        /**
         * 压盖（弃用）
         */
        [Obsolete("弃用，改成耗材属性")]
        public decimal CoverHeight = 30.0m;

        /**
         * 清洗区
         */
        // 位置
        public List<Position> CleaningPosList = new List<Position>();
        // 清洗液体积
        public decimal CleaningLiquidVolume = 0.0m;
        // 清洗前吸空气量
        public decimal AbsorbAirBeforeCleaning = 0.0m;
        // 清洗吸喷体积（即空气量）
        public decimal CleaningHeadVolume = 0.0m;
        // 清洗吸喷次数
        public int CleaningHeadTime = 0;
        // 吸液后吸空气量
        public decimal CleaningHeadAbsorbAirAfterAbsorb = 0.0m;
        // 喷液后吸空气量
        public decimal CleaningHeadAbsorbAirAfterJet = 0.0m;
        // 喷液后吸空气速度（即后来的清洗速度：清洗和喷后吸空气用同一个速度）
        public string CleaningHeadAbsorbAirAfterJetSpeed = "";
        // 喷空气后等待时间
        public decimal CleaningHeadJetAirDelay = 0.0m;
        // 开泵延时
        public decimal OpenPumpDelay = 0.0m;

        /**
         * 废液槽
         */
        // 位置
        public Position WastePos = new Position();

        /**
         * 钢针
         */
        // 安装位置
        public Position SteelNeedleInstallPos = new Position();
        // 单孔运行时间（秒）
        public int[] SteelNeedleRunTimeSingleArray = { 0, 0, 0, 0, 0 };
        // 吸喷液速度
        public string[] SteelNeedleSpeedArray = { "", "", "", "", "" };
    }
}
