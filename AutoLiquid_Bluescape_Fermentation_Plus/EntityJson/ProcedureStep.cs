using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Documents;
using AutoLiquid_Bluescape_Fermentation_Plus.EntityCommon;
using AutoLiquid_Library.Enum;

namespace AutoLiquid_Bluescape_Fermentation_Plus.EntityJson
{
    /// <summary>
    /// 程序步骤
    /// </summary>
    [Serializable()]
    public class ProcedureStep
    {
        // 步骤名称
        public string Name = string.Empty;

        // 步骤类型
        public EStepType StepType = EStepType.LiquidTransfer;

        /**
         * 移液头
         */
        // 使用的移液头Index
        public int HeadIndex = 0;

        /**
         * 源盘
         */
        // 盘位
        public int SourceTemplateIndex = 0;
        // 类型Index（0：发酵区；1：留样区）
        public int SourceTemplateTypeIndex = 0;
        // 孔位
        public HoleIndex SourceHoleIndexFrom = new HoleIndex();
        public HoleIndex SourceHoleIndexTo = new HoleIndex();
        // 耗材
        public string SourceConsumable = String.Empty;

        /**
         * 靶盘
         */
        // 盘位
        public int TargetTemplateIndex = 0;
        // 类型Index（0：发酵区；1：留样区）
        public int TargetTemplateTypeIndex = 1;
        // 孔位
        public HoleIndex TargetHoleIndexFrom = new HoleIndex();
        public HoleIndex TargetHoleIndexTo = new HoleIndex();
        // 耗材
        public string TargetConsumable = String.Empty;
        // 每孔体积
        public decimal VolumeEach = 10.0m;

        /**
         * 清洗区
         */
        // 是否需要清洗
        public bool IsNeedCleaning = true;
        // 清洗位置（-1为空）
        public int[] CleaningPosIndexArray = { 0, -1, -1 };

        /**
         * 吸前混合
         */
        //// 混合体积
        //public decimal AbsorbMixingVolume = 0.0m;
        //// 混合次数
        //public int AbsorbMixingTimes = 0;

        /**
         * 喷后混合
         */
        //// 混合体积
        //public decimal JetMixingVolume = 0.0m;
        //// 混合次数
        //public int JetMixingTimes = 0;

        /**
         * 时间（分钟）
         */
        public int Time = 0;

        /**
         * 等待
         */
        // 阻塞
        public bool Block = true;
        // 等待序号（即从哪个步骤序号开始计时）
        public int WaitForOrder = 0;

        /**
         * 温度
         */
        public decimal Temp = 0.0m;
        //// 预热
        //public bool PreTemp = false;

        /**
         * 速度
         */
        public int SpeedLevel = 1;

        /**
         * 循环
         */
        // 循环起止位置：LoopRange[0]---如1为开始步骤1；LoopRange[1]---如2为结束步骤2
        public int[] LoopStepRange = new[] { 1, 1 };
        // 循环次数
        public int LoopTimes = 1;
    }
}
