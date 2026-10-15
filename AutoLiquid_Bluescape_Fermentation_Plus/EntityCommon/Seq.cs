using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutoLiquid_Bluescape_Fermentation_Plus.Utils;
using AutoLiquid_Bluescape_Fermentation_Plus.EntityJson;
using AutoLiquid_Library.Enum;

namespace AutoLiquid_Bluescape_Fermentation_Plus.EntityCommon
{
    /// <summary>
    /// 动作序列（包含移液、搅拌、等待、循环）
    /// </summary>
    public class Seq
    {
        // 该程序属于程序链Index
        public int ProcedureIndexInChain = 0;
        // 该程序属于设置Index
        public int ProcedureIndexInSetting = 0;
        // 该Seq属于程序中的哪个步骤的Index
        public int SeqBelongStepIndex = 0;
        // 该Seq位于程序链中所有seq位置Index（因为默写步骤会拆分成多个seq）
        public int SeqIndex = 0;

        // 步骤名称
        public string Name = string.Empty;

        // 步骤类型
        public EStepType StepType = EStepType.LiquidTransfer;
        // 如果步骤类型为等待，是否阻塞
        public bool WaitStepBlock = false;

        #region 移液相关
        /**
         * 源盘
         */
        // 源盘盘名
        public string SourceTemplateName = "";
        // 源盘盘位Index
        public int SourceTemplateIndex;
        // 源盘类型Index（0：发酵区；1：留样区）
        public int SourceTemplateTypeIndex;
        // 源盘孔Index
        public HoleIndex SourceHoleIndex = new HoleIndex();
        // 源盘耗材类型
        public Consumable SourceTemplateConsumableType;

        /**
         * 靶盘（可能存在一吸多喷）
         */
        // 靶盘盘名
        public string TargetTemplateName = "";
        // 靶盘盘位Index
        public List<int> TargetTemplateIndexList = new List<int>();
        // 源盘类型Index（0：发酵区；1：留样区）
        public int TargetTemplateTypeIndex;
        // 靶盘孔Index
        public List<HoleIndex> TargetHoleIndexList = new List<HoleIndex>();
        // 靶盘耗材类型
        public Consumable TargetTemplateConsumableType;
        // 是否梯度稀释
        public bool SerialDilute;
        // 喷液体积补偿
        public List<JetOffset> JetOffsetList = new List<JetOffset>();

        /**
         * 体积
         */
        // 每次体积
        public List<decimal> VolumeEachList = new List<decimal>();

        /**
         * 混合信息
         */
        // 吸前混合体积
        public decimal AbsorbMixingVolume;
        // 吸前混合次数
        public decimal AbsorbMixingCount;
        // 喷后混合体积
        public decimal JetMixingVolume;
        // 喷后混合次数
        public decimal JetMixingCount;

        /**
         * 吸液靠壁信息
         */
        // 吸液靠壁
        public List<EWall> AbsorbWallList = new List<EWall>();
        // 喷液靠壁
        public List<EWall> JetWallList = new List<EWall>();

        /**
           * 特殊指令
           */
        // 是否文件链接
        public bool IsTxtLink = false;
        // 是否只有特殊指令
        public bool IsCmdOnly;
        public string Cmd = "";
        public string CmdAbsorbBefore = "";
        public string CmdAbsorbAfter = "";
        public string CmdJetBefore = "";
        public string CmdJetAfter = "";
        // 是否注释该行序列（即不执行）
        public bool IsComment;

        // 使用的移液头Index
        public int HeadUsedIndex = 0;

        // 使用的钢针Index
        public int SteelNeedleChoiceIndex = 0;

        // 是否泵分液
        public bool IsPumpLiquid = false;

        /**
         * 清洗
         */
        // 是否需要到清洗区清洗 
        public bool IsNeedCleaning = true;
        // 清洗位置（-1为空）
        public int[] CleaningPosIndexArray = { 0, -1, -1 };
        #endregion

        #region 搅拌、等待、循环等相关
        /**
        * 时间（分钟）
        */
        public int Time = 0;

        /**
         * 循环
         */
        // 循环起止位置：LoopRange[0]---如1为开始步骤1；LoopRange[1]---如2为结束步骤2
        public int[] LoopStepRange = new[] { 1, 1 };
        // 循环次数
        public int LoopTimes = 1;

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
         * 等待
         */
        // 是否从本步骤开始计时
        public bool WaitCountdownFromThisStep = false;
        #endregion
    }
}
