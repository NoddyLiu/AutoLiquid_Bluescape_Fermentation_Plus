using AutoLiquid_Bluescape_Fermentation_Plus;
using AutoLiquid_Bluescape_Fermentation_Plus.EntityCommon;
using AutoLiquid_Bluescape_Fermentation_Plus.EntityJson;
using AutoLiquid_Bluescape_Fermentation_Plus.Pump;
using AutoLiquid_Library.Enum;
using AutoLiquid_Library.Exceptions;
using AutoLiquid_Library.Utils;
using ControlzEx.Standard;
using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media.Media3D;

namespace AutoLiquid_Bluescape_Fermentation_Plus.Utils
{
    /// <summary>
    /// 通用指令帮助类
    /// </summary>
    public class CmdHelper
    {
        // 通信窗体
        public static DAERun.FrmMain frmDAE = new DAERun.FrmMain("192.168.1.5", 4000, "workplate.xml");

        // 注射泵通信
        public static TtySTransport pumpTransport = new TtySTransport(new TtySConfig
        {
            PortName = ParamsHelper.HeadList[0].SerialPortName,
            BaudRate = ParamsHelper.HeadList[0].SerialBaudRate,
        });

        // 用户手动干预
        public static bool isManualPause;
        public static bool isManualStop;


        // 减速指令
        public static string SPEED_SLOW = "F 20 400 1";

        // 移液头左右轴列表
        private static List<string> HEAD_X_AXIS_LIST = new List<string> { "X", "X" };
        // 移液头前后轴列表
        private static List<string> HEAD_Y_AXIS_LIST = new List<string> { "Y", "Y" };
        // 移液头上下轴列表
        private static List<string> HEAD_Z_AXIS_LIST = new List<string> { "Z", "Z" };
        // 移液头喷液轴列表
        //public static List<string> HEAD_P_AXIS_LIST = new List<string> { "P", "P" };
        // 发酵区压盖轴列表
        public static List<string> HEAD_COVER_AXIS_LIST = new List<string> { "M", "M" };

        // 移液头状态信息
        public static List<HeadStatus> headStatusList = new List<HeadStatus> { new HeadStatus(), new HeadStatus() };

        // 指令超时时间
        private static long CMD_TIMEOUT = 60000;
        // 指令超时时间（离线调试模式）
        private static long CMD_TIMEOUT_TEST = 2000;

        /// <summary>
        /// 初始化机器
        /// <param name="isNeedManualStop">是否需要手动停止功能</param>
        /// <param name="isFirstTimeInit">是否第一次复位</param>
        /// </summary>
        public static bool InitMachine(bool isNeedManualStop, bool isFirstTimeInit)
        {
            if (isNeedManualStop) LogHelper.Info((string)Application.Current.FindResource("Init"), "");

            /**
             * Z
             */
            var resetZ = HEAD_Z_AXIS_LIST[0] + "I";

            /**
             * 压盖
             */
            var result1 = DoCmd(HEAD_COVER_AXIS_LIST[0] + "I," + resetZ, isNeedManualStop);

            /**
             * P
             */
            //var resetP = ParamsHelper.HeadList[0].PAvailable ? HEAD_P_AXIS_LIST[0] + "I" : "";
            //var result2 = DoCmd(resetP, isNeedManualStop);
            var result2 = true;
            pumpTransport.mgr.RunWorkflowAsync(PumpSelection.Left, ctx =>
            {
                ctx.Parallel("初始化", wb =>
                {
                    wb.Init();
                });
            }).GetAwaiter().GetResult();

            /**
             * X、Y轴
             */
            var resetXAndY = HEAD_X_AXIS_LIST[0] + "I" + "," + HEAD_Y_AXIS_LIST[0] + "I";
            var result3 = DoCmd(resetXAndY, isNeedManualStop);


            // 更新移液头状态信息
            headStatusList[0].Head = EHeadStatus.Null;
            headStatusList[1].Head = EHeadStatus.Null;

            return result1 && result2 && result3;
        }

        /// <summary>
        /// 吸液
        /// </summary>
        /// <param name="headUsedIndex">移液头Index</param>
        /// <param name="steelNeedleChoiceIndex">钢针选择Index</param>
        /// <param name="consumableType">耗材类型</param>
        /// <param name="consumableTypeNext">下一个耗材类型（用于判断行走高度是走本耗材高度还是下一个耗材高度）</param>
        /// <param name="templateIndex">盘位 index</param>
        /// <param name="holeIndex">孔位</param>
        /// <param name="seq">移液信息</param>
        /// <param name="liquidVolume">总吸液容积</param>
        /// <param name="isOne2More">是否一吸多喷</param>
        /// <param name="isDemo">是否演示</param>
        public static void AbsorbLiquid(int headUsedIndex, int steelNeedleChoiceIndex, Consumable consumableType, Consumable consumableTypeNext, int templateIndex, HoleIndex holeIndex, Seq seq, decimal liquidVolume, bool isOne2More, bool isDemo)
        {
            LogHelper.Info((string)Application.Current.FindResource("AbsorbLiquid"), "");

            /*
           * 获取所有可变值
           */
            var commonSetting = ParamsHelper.CommonSettingList[headUsedIndex];
            // 行走高度
            var normalHeight = consumableType.NormalHeight > consumableTypeNext.NormalHeight ? consumableTypeNext.NormalHeight : consumableType.NormalHeight;
            // 跨耗材行走高度复位
            var normalHeightReset = consumableType.NormalHeightReset;
            // 混合高度
            var absorbMixingHeight = consumableType.AbsorbMixingHeight;
            // 吸液高度
            var liquidAbsorbHeight = consumableType.LiquidAbsorbHeight;
            // 吸液速度
            var absorbSpeed = ParamsHelper.CommonSettingList[headUsedIndex].SteelNeedleSpeedArray[steelNeedleChoiceIndex];
            // 吸液后等待
            var liquidAbsorbDelay = consumableType.LiquidAbsorbDelay;
            // 吸液后提起速度
            var absorbHeight2NormalHeightSpeed = consumableType.AbsorbHeight2NormalHeightSpeed;
            // 吸液提起后等待
            var liquidAbsorbDelayAfterLift = consumableType.LiquidAbsorbDelayAfterLift;
            // 喷液高度
            var liquidJetHeight = consumableType.LiquidJetHeight;
            // 喷液速度
            var jetSpeed = consumableType.JetSpeed;
            // 喷液后等待
            var liquidJetDelay = consumableType.LiquidJetDelay;
            // 靠壁高度
            var liquidAbsorbWallHeight = consumableType.LiquidJetWallHeight;
            // 靠壁偏移
            var liquidJetWallOffset = consumableType.LiquidJetWallOffset;
            // 演示高度
            var demoHeight = consumableType.DemoHeight;
            // 演示高度等待
            var demoHeightDelay = consumableType.DemoHeightDelay;
            // 压盖高度
            var coverHeight = consumableType.CoverHeight;
            // 量程
            var headLiquidRangeReal = Convert.ToDecimal((int)ParamsHelper.HeadList[headUsedIndex].HeadLiquidRange);
            // 吸液前吸空气量（毫升）
            var absorbAirBefore = commonSetting.AbsorbAirBeforePercent;
            // 吸液后吸空气量
            var absorbAirAfter = headLiquidRangeReal * commonSetting.AbsorbAirAfterPercent * 0.01m;
            // 一吸多喷吸液后多吸体积
            var absorbLiquidMoreOne2More = headLiquidRangeReal * commonSetting.AbsorbLiquidMoreOne2MorePercent * 0.01m;
            // 一吸多喷吸液后喷出体积比例
            var jetLiquidMoreOne2MoreScale = commonSetting.JetLiquidMoreOne2MoreScale;
            // 是否梯度稀释
            var serialDilute = seq.SerialDilute;
            // 喷液体积补偿
            var jetOffsetVolumeTotal = seq.JetOffsetList.Sum(jetOffset => jetOffset.VolumeOffset);
            liquidVolume += jetOffsetVolumeTotal;
            // 吸前混合速度
            var absorbMixingSpeed = consumableType.AbsorbMixingSpeed;
            // P默认速度
            var defaultPSpeed = commonSetting.DefaultPSpeed;
            // P速度百分比
            var pSpeedPercent = commonSetting.PSpeedPercent;
            // Z默认速度
            var defaultZSpeed = commonSetting.DefaultZSpeed;
            // Z速度百分比
            var zSpeedPercent = commonSetting.ZSpeedPercent;


            // 吸液前特殊指令
            if (!seq.CmdAbsorbBefore.Equals(""))
                MainWindow.mMainWindow.ParseCmdLine(seq.CmdAbsorbBefore, false);

            // 到孔位置
            GotoHole(headUsedIndex, consumableType, templateIndex, holeIndex, true);

            // 如果演示，只走位置，再走演示高度，等待，回零，然后return
            if (isDemo)
            {
                GotoCoverHeight(headUsedIndex, demoHeight, true, true);
                Thread.Sleep((int)(demoHeightDelay * 1000));
                GotoCoverHeight(headUsedIndex, 0, false, true);
                return;
            }

            /**
             * 混合
             */
            var mixingVolume = seq.AbsorbMixingVolume;
            var mixingCount = seq.AbsorbMixingCount;
            var absorbMixingNeed = absorbMixingHeight > 0 && mixingVolume > 0 && mixingCount > 0;
            if (absorbMixingNeed)
            {
                // 吸液前吸空气量
                if (absorbAirBefore > 0)
                    Ps(headUsedIndex, absorbAirBefore, true);

                if (!absorbMixingSpeed.Equals(""))
                    //SpeedSet(headUsedIndex, EAxis.P, absorbMixingSpeed, 100, true);
                    PumpSpeedSet(absorbMixingSpeed);

                // 混合高度
                GotoHeight(headUsedIndex, absorbMixingHeight, true);

                // 混合
                for (var i = 0; i < mixingCount; i++)
                {
                    // 吸液
                    Ps(headUsedIndex, mixingVolume, true);

                    // 喷液：最后一次喷液PA0
                    if (i == mixingCount - 1)
                    {
                        // 走到喷液高度，再PA0
                        GotoHeight(headUsedIndex, liquidJetHeight, true);
                        Pa(headUsedIndex, 0, true);
                    }
                    else
                        Ps(headUsedIndex, mixingVolume * -1, true);
                }

                // 恢复用户设置的速度
                if (!absorbMixingSpeed.Equals(""))
                    //SpeedSet(headUsedIndex, EAxis.P, defaultPSpeed, pSpeedPercent, true);
                    PumpSpeedSet(defaultPSpeed);
            }

            // 吸液前吸空气量
            if (absorbAirBefore > 0)
            {
                // 如果吸液前需要混合，就先提高到行走高度，再吸空气
                if (absorbMixingNeed)
                    GotoHeight(headUsedIndex, normalHeight, true);

                Ps(headUsedIndex, absorbAirBefore, true);
            }

            // 压盖高度
            GotoCoverHeight(headUsedIndex, coverHeight, true, true);
            // 吸液高度
            GotoHeight(headUsedIndex, liquidAbsorbHeight, true);

            // 吸液速度
            if (!absorbSpeed.Equals(""))
                //SpeedSet(headUsedIndex, EAxis.P, absorbSpeed, 100, true);
                PumpSpeedSet(absorbSpeed);

            // 吸液体积
            if (serialDilute)
                Ps(headUsedIndex, seq.VolumeEachList[0], true);
            else
            {
                if (liquidVolume > 0)
                    Ps(headUsedIndex, liquidVolume, true);
            }

            // 恢复用户设置的速度
            if (!absorbSpeed.Equals(""))
                //SpeedSet(headUsedIndex, EAxis.P, defaultPSpeed, pSpeedPercent, true);
                PumpSpeedSet(defaultPSpeed);

            // 更新移液头状态信息
            headStatusList[headUsedIndex].SourceConsumableType = consumableType;
            headStatusList[headUsedIndex].SourceTemplateIndex = templateIndex;
            headStatusList[headUsedIndex].SourceHoleIndex = holeIndex;
            headStatusList[headUsedIndex].Head = EHeadStatus.Absorbed;

            /**
             * 如果是一吸多喷，多吸体积
             */
            var jetLiquidMore = absorbLiquidMoreOne2More * jetLiquidMoreOne2MoreScale * 0.01m;
            if (isOne2More && absorbLiquidMoreOne2More > 0 && liquidVolume > 0 && !serialDilute)
            {
                // 多吸体积
                Ps(headUsedIndex, absorbLiquidMoreOne2More, true);
                // 多喷体积
                Ps(headUsedIndex, jetLiquidMore * -1, true);
            }

            // 吸液等待
            Thread.Sleep((int)(liquidAbsorbDelay * 1000));

            // 吸液后提起速度
            if (!absorbHeight2NormalHeightSpeed.Equals(""))
                SpeedSet(headUsedIndex, EAxis.Z, absorbHeight2NormalHeightSpeed, 100, true);

            // 吸液靠壁
            if (seq.AbsorbWallList.Count > 0 && liquidAbsorbWallHeight > 0)
            {
                // 靠壁高度
                GotoHeight(headUsedIndex, liquidAbsorbWallHeight, true);

                // 提起后等待
                Thread.Sleep((int)(liquidAbsorbDelayAfterLift * 1000));

                /**
                 * 多向靠壁
                 */
                foreach (var absorbWall in seq.AbsorbWallList)
                {
                    // 左靠壁
                    if (absorbWall == EWall.Left)
                        WallAndReset(headUsedIndex, consumableType, templateIndex, holeIndex, HEAD_X_AXIS_LIST[headUsedIndex] + SPEED_SLOW, HEAD_X_AXIS_LIST[headUsedIndex] + "S-" + liquidJetWallOffset.ToString("F2"));
                    // 右靠壁
                    else if (absorbWall == EWall.Right)
                        WallAndReset(headUsedIndex, consumableType, templateIndex, holeIndex, HEAD_X_AXIS_LIST[headUsedIndex] + SPEED_SLOW, HEAD_X_AXIS_LIST[headUsedIndex] + "S" + liquidJetWallOffset.ToString("F2"));
                    // 前靠壁
                    else if (absorbWall == EWall.Front)
                        WallAndReset(headUsedIndex, consumableType, templateIndex, holeIndex, HEAD_Y_AXIS_LIST[headUsedIndex] + SPEED_SLOW, HEAD_Y_AXIS_LIST[headUsedIndex] + "S" + liquidJetWallOffset.ToString("F2"));
                    // 后靠壁
                    else if (absorbWall == EWall.Back)
                        WallAndReset(headUsedIndex, consumableType, templateIndex, holeIndex, HEAD_Y_AXIS_LIST[headUsedIndex] + SPEED_SLOW, HEAD_Y_AXIS_LIST[headUsedIndex] + "S-" + liquidJetWallOffset.ToString("F2"));
                }

                // 行走高度
                GotoHeight(headUsedIndex, normalHeightReset ? 0 : normalHeight, true);
            }
            else
            {
                // 行走高度
                // GotoHeight(headUsedIndex, normalHeightReset ? 0 : normalHeight, true);
                // Z轴回到盖盖高度
                GotoHeight(headUsedIndex, coverHeight, true);
                // 压盖回零
                GotoCoverHeight(headUsedIndex, 0, false, true);

                // 提起后等待
                Thread.Sleep((int)(liquidAbsorbDelayAfterLift * 1000));
            }

            // 恢复用户设置的速度
            if (!absorbHeight2NormalHeightSpeed.Equals(""))
                SpeedSet(headUsedIndex, EAxis.Z, defaultZSpeed, zSpeedPercent, true);

            // 吸液后吸空气量
            if (absorbAirAfter > 0)
                Ps(headUsedIndex, absorbAirAfter, true);

            // 吸液后特殊指令
            if (!seq.CmdAbsorbAfter.Equals(""))
                MainWindow.mMainWindow.ParseCmdLine(seq.CmdAbsorbAfter, false);
        }

        /// <summary>
        /// 喷液（一吸一喷，有混合和分段喷液功能，1vs1）
        /// </summary>
        /// <param name="headUsedIndex">移液头Index</param>
        /// <param name="steelNeedleChoiceIndex">钢针选择Index</param>
        /// <param name="consumableType">耗材类型</param>
        /// <param name="consumableTypeNext">下一个耗材类型</param>
        /// <param name="templateIndex">靶盘盘位 index</param>
        /// <param name="holeIndex">靶盘孔位</param>
        /// <param name="seq">移液信息</param>
        /// <param name="liquidVolume">单个喷液容积</param>
        /// <param name="isDemo">是否演示</param>
        public static void JetSingleLiquid(int headUsedIndex, int steelNeedleChoiceIndex, Consumable consumableType, Consumable consumableTypeNext, int templateIndex, HoleIndex holeIndex, Seq seq, decimal liquidVolume, bool isDemo)
        {
            LogHelper.Info((string)Application.Current.FindResource("JetLiquid"), "");

            /*
             * 获取所有可变值
             */
            var commonSetting = ParamsHelper.CommonSettingList[headUsedIndex];
            // 行走高度，走下一个耗材的行走高度
            decimal normalHeight;
            if (consumableTypeNext != null)
                normalHeight = consumableType.NormalHeight > consumableTypeNext.NormalHeight
                    ? consumableTypeNext.NormalHeight
                    : consumableType.NormalHeight;
            else
                normalHeight = consumableType.NormalHeight;
            // 跨耗材行走高度复位
            var normalHeightReset = consumableType.NormalHeightReset;
            // 混合高度
            var jetMixingHeight = consumableType.JetMixingHeight;
            // 混合体积
            var mixingVolume = seq.JetMixingVolume;
            // 混合次数
            var mixingCount = seq.JetMixingCount;
            // 是否需要混合
            var jetMixingNeed = jetMixingHeight > 0 && mixingVolume > 0 && mixingCount > 0;
            // 喷液高度
            var liquidJetHeight = consumableType.LiquidJetHeight;
            // 喷液速度
            var jetSpeed = ParamsHelper.CommonSettingList[headUsedIndex].SteelNeedleSpeedArray[steelNeedleChoiceIndex];
            // 喷液后等待
            var liquidJetDelay = consumableType.LiquidJetDelay;
            // 第2段喷液体积
            var volumeJet2 = commonSetting.VolumeJet2;
            // 喷后混合速度
            var jetMixingSpeed = consumableType.JetMixingSpeed;
            // 靠壁高度
            var liquidJetWallHeight = consumableType.LiquidJetWallHeight;
            // 靠壁偏移
            var liquidJetWallOffset = consumableType.LiquidJetWallOffset;
            // 靠壁触发条件
            var liquidJetWallTrigger = consumableType.LiquidJetWallTrigger;
            // 靠壁喷液
            var wallJet = consumableType.WallJet;
            // 演示高度
            var demoHeight = consumableType.DemoHeight;
            // 演示高度等待
            var demoHeightDelay = consumableType.DemoHeightDelay;
            // 压盖高度
            var coverHeight = consumableType.CoverHeight;
            // 喷液后提起速度
            var jetHeight2NormalHeightSpeed = consumableType.JetHeight2NormalHeightSpeed;
            // P默认速度
            var defaultPSpeed = commonSetting.DefaultPSpeed;
            // P速度百分比
            var pSpeedPercent = commonSetting.PSpeedPercent;
            // Z默认速度
            var defaultZSpeed = commonSetting.DefaultZSpeed;
            // Z速度百分比
            var zSpeedPercent = commonSetting.ZSpeedPercent;

            // 到孔位置
            GotoHole(headUsedIndex, consumableType, templateIndex, holeIndex, true);

            // 如果演示，只走位置，再走演示高度，等待，回零，然后return
            if (isDemo)
            {
                GotoCoverHeight(headUsedIndex, demoHeight, true, true);
                Thread.Sleep((int)(demoHeightDelay * 1000));
                GotoCoverHeight(headUsedIndex, 0, false, true);
                return;
            }

            // 喷液前特殊指令
            if (!seq.CmdJetBefore.Equals(""))
                MainWindow.mMainWindow.ParseCmdLine(seq.CmdJetBefore, false);

            /**
           * 喷液
           */
            // 是否分段喷液
            var isJetSeparate = volumeJet2 > 0 && liquidVolume > volumeJet2;
            // 喷液速度
            if (!jetSpeed.Equals(""))
                //SpeedSet(headUsedIndex, EAxis.P, jetSpeed, 100, true);
                PumpSpeedSet(jetSpeed);
            // 先靠壁，再喷液
            if (wallJet)
            {
                WallFirstJetSecond(headUsedIndex, seq, consumableType, true, isJetSeparate, volumeJet2, templateIndex, holeIndex, liquidVolume, true);
            }
            // 先喷液，再靠壁
            else
            {
                // 压盖高度
                GotoCoverHeight(headUsedIndex, coverHeight, true, true);
                // 喷液高度
                GotoHeight(headUsedIndex, liquidJetHeight, true);

                // 喷液
                JetLiquid(headUsedIndex, liquidVolume, true, isJetSeparate, volumeJet2, true);

                // 喷液等待
                Thread.Sleep((int)(liquidJetDelay * 1000));

                // 靠壁偏移(小于触发条件才偏移)
                if (liquidVolume <= liquidJetWallTrigger && seq.JetWallList.Count > 0 && liquidJetWallHeight > 0)
                {
                    // 靠壁高度
                    GotoHeight(headUsedIndex, liquidJetWallHeight, true);

                    /**
                     * 多向靠壁
                     */
                    foreach (var jetWall in seq.JetWallList)
                    {
                        // 左靠壁
                        if (jetWall == EWall.Left)
                            WallAndReset(headUsedIndex, consumableType, templateIndex, holeIndex, HEAD_X_AXIS_LIST[headUsedIndex] + SPEED_SLOW, HEAD_X_AXIS_LIST[headUsedIndex] + "S-" + liquidJetWallOffset.ToString("F2"));
                        // 右靠壁
                        else if (jetWall == EWall.Right)
                            WallAndReset(headUsedIndex, consumableType, templateIndex, holeIndex, HEAD_X_AXIS_LIST[headUsedIndex] + SPEED_SLOW, HEAD_X_AXIS_LIST[headUsedIndex] + "S" + liquidJetWallOffset.ToString("F2"));
                        // 前靠壁
                        else if (jetWall == EWall.Front)
                            WallAndReset(headUsedIndex, consumableType, templateIndex, holeIndex, HEAD_Y_AXIS_LIST[headUsedIndex] + SPEED_SLOW, HEAD_Y_AXIS_LIST[headUsedIndex] + "S" + liquidJetWallOffset.ToString("F2"));
                        // 后靠壁
                        else if (jetWall == EWall.Back)
                            WallAndReset(headUsedIndex, consumableType, templateIndex, holeIndex, HEAD_Y_AXIS_LIST[headUsedIndex] + SPEED_SLOW, HEAD_Y_AXIS_LIST[headUsedIndex] + "S-" + liquidJetWallOffset.ToString("F2"));
                    }
                }
            }

            // 恢复用户设置的速度
            if (!jetSpeed.Equals(""))
                //SpeedSet(headUsedIndex, EAxis.P, defaultPSpeed, pSpeedPercent, true);
                PumpSpeedSet(defaultPSpeed);

            /**
             * 混合
             */
            if (jetMixingNeed)
            {
                if (!jetMixingSpeed.Equals(""))
                    //SpeedSet(headUsedIndex, EAxis.P, jetMixingSpeed, 100, true);
                    PumpSpeedSet(jetMixingSpeed);

                // 混合高度
                GotoHeight(headUsedIndex, jetMixingHeight, true);

                // 混合
                for (var i = 0; i < mixingCount; i++)
                {
                    // 吸液
                    Ps(headUsedIndex, mixingVolume, true);

                    // 喷液：最后一次喷液PA0
                    if (i == mixingCount - 1)
                    {
                        // 走到喷液高度，再PA0
                        GotoHeight(headUsedIndex, liquidJetHeight, true);
                        Pa(headUsedIndex, 0, true);
                    }
                    else
                        Ps(headUsedIndex, mixingVolume * -1, true);
                }

                // 恢复用户设置的速度
                if (!jetMixingSpeed.Equals(""))
                    //SpeedSet(headUsedIndex, EAxis.P, defaultPSpeed, pSpeedPercent, true);
                    PumpSpeedSet(defaultPSpeed);
            }

            // 喷液后提起速度
            if (!jetHeight2NormalHeightSpeed.Equals(""))
                SpeedSet(headUsedIndex, EAxis.Z, jetHeight2NormalHeightSpeed, 100, true);

            // Z轴回到盖盖高度
            GotoHeight(headUsedIndex, coverHeight, true);
            // 压盖回零
            GotoCoverHeight(headUsedIndex, 0, false, true);

            // 清洗枪头
            Cleaning(headUsedIndex, consumableType, seq, isDemo);

            // 恢复用户设置的速度
            if (!jetHeight2NormalHeightSpeed.Equals(""))
                SpeedSet(headUsedIndex, EAxis.Z, defaultZSpeed, zSpeedPercent, true);

            // 喷液后特殊指令
            if (!seq.CmdJetAfter.Equals(""))
                MainWindow.mMainWindow.ParseCmdLine(seq.CmdJetAfter, false);
        }

        /// <summary>
        /// 喷液（一吸多喷）
        /// </summary>
        /// <param name="headUsedIndex">移液头Index</param>
        /// <param name="steelNeedleChoiceIndex">钢针选择Index</param>
        /// <param name="sourceConsumableType">源孔耗材类型</param>
        /// <param name="sourceTemplateIndex">源孔盘位Index</param>
        /// <param name="sourceHoleIndex">源盘孔位Index</param>
        /// <param name="consumableType">耗材类型</param>
        /// <param name="consumableTypeNext">下一个耗材类型</param>
        /// <param name="templateIndexList">靶盘盘位List index</param>
        /// <param name="holeIndexList">靶盘孔位List index</param>
        /// <param name="seq">移液信息</param>
        /// <param name="liquidVolumeList">喷液容积List</param>
        /// <param name="isDemo">是否演示</param>
        public static void JetMultiLiquid(int headUsedIndex, int steelNeedleChoiceIndex, Consumable sourceConsumableType, int sourceTemplateIndex, HoleIndex sourceHoleIndex, Consumable consumableType, Consumable consumableTypeNext, List<int> templateIndexList, List<HoleIndex> holeIndexList, Seq seq, List<decimal> liquidVolumeList, bool isDemo)
        {
            LogHelper.Info((string)Application.Current.FindResource("JetLiquid"), "");

            /*
             * 获取所有可变值
             */
            var commonSetting = ParamsHelper.CommonSettingList[headUsedIndex];
            // 盘内行走高度
            var normalHeightInner = consumableType.NormalHeight;
            // 盘外行走高度，走下一个耗材的行走高度
            decimal normalHeightOuter;
            if (consumableTypeNext != null)
                normalHeightOuter = consumableType.NormalHeight > consumableTypeNext.NormalHeight
                    ? consumableTypeNext.NormalHeight
                    : consumableType.NormalHeight;
            else
                normalHeightOuter = consumableType.NormalHeight;
            // 跨耗材行走高度复位
            var normalHeightReset = consumableType.NormalHeightReset;
            // 混合高度
            var jetMixingHeight = consumableType.JetMixingHeight;
            // 混合体积
            var mixingVolume = seq.JetMixingVolume;
            // 混合次数
            var mixingCount = seq.JetMixingCount;
            // 是否梯度稀释
            var serialDilute = seq.SerialDilute;
            // 喷液高度
            var liquidJetHeight = consumableType.LiquidJetHeight;
            // 喷液速度
            var jetSpeed = ParamsHelper.CommonSettingList[headUsedIndex].SteelNeedleSpeedArray[steelNeedleChoiceIndex];
            // 喷液后等待
            var liquidJetDelay = consumableType.LiquidJetDelay;
            // 喷后混合速度
            var jetMixingSpeed = consumableType.JetMixingSpeed;
            // 靠壁高度
            var liquidJetWallHeight = consumableType.LiquidJetWallHeight;
            // 靠壁偏移
            var liquidJetWallOffset = consumableType.LiquidJetWallOffset;
            // 靠壁触发条件
            var liquidJetWallTrigger = consumableType.LiquidJetWallTrigger;
            // 靠壁喷液
            var wallJet = consumableType.WallJet;
            // 演示高度
            var demoHeight = consumableType.DemoHeight;
            // 演示高度等待
            var demoHeightDelay = consumableType.DemoHeightDelay;
            // 压盖高度
            var coverHeight = consumableType.CoverHeight;
            // 源盘行走高度
            var sourceNormalHeight = sourceConsumableType.NormalHeight;
            // 源盘吸液高度
            var sourceLiquidAbsorbHeight = sourceConsumableType.LiquidAbsorbHeight;
            // 量程
            var headLiquidRangeReal = Convert.ToDecimal((int)ParamsHelper.HeadList[headUsedIndex].HeadLiquidRange);
            // 吸液前吸空气量（毫升）
            var absorbAirBefore = commonSetting.AbsorbAirBeforePercent;
            // 吸液后吸空气量
            var absorbAirAfter = headLiquidRangeReal * commonSetting.AbsorbAirAfterPercent * 0.01m;
            // 一吸多喷吸液后多吸体积
            var absorbLiquidMoreOne2More = headLiquidRangeReal * commonSetting.AbsorbLiquidMoreOne2MorePercent * 0.01m;
            // 一吸多喷吸液后喷出体积比例
            var jetLiquidMoreScale = commonSetting.JetLiquidMoreOne2MoreScale;
            // 喷液后提起速度
            var jetHeight2NormalHeightSpeed = consumableType.JetHeight2NormalHeightSpeed;
            // P默认速度
            var defaultPSpeed = commonSetting.DefaultPSpeed;
            // P速度百分比
            var pSpeedPercent = commonSetting.PSpeedPercent;
            // Z默认速度
            var defaultZSpeed = commonSetting.DefaultZSpeed;
            // Z速度百分比
            var zSpeedPercent = commonSetting.ZSpeedPercent;

            // 靶孔数
            var holeCount = holeIndexList.Count;
            // 靶孔计数器
            var holeTick = 0;

            for (var i = 0; i < holeCount; i++)
            {
                // 靶盘位，判断是1靶多孔 or 多靶多孔
                var templateIndex = templateIndexList.Count > 1 ? templateIndexList[i] : templateIndexList[0];
                // 孔位
                var holeIndex = holeIndexList[i];
                // 体积
                var liquidVolume = liquidVolumeList.Count > 1 ? liquidVolumeList[i] : liquidVolumeList[0];
                // 喷液体积补偿
                if (seq.JetOffsetList.Count > 0)
                {
                    var jetOffset = seq.JetOffsetList.FirstOrDefault(p => p.PosIndex == i);
                    liquidVolume += jetOffset != null ? jetOffset.VolumeOffset : 0;
                }
                // 下一个孔体积
                var liquidVolumeNext = 0.0m;
                if (i != holeCount - 1)
                    liquidVolumeNext = liquidVolumeList.Count > 1 ? liquidVolumeList[i + 1] : liquidVolumeList[0];

                // 喷液前特殊指令
                if (!seq.CmdJetBefore.Equals(""))
                    MainWindow.mMainWindow.ParseCmdLine(seq.CmdJetBefore, false);

                // 到孔位置
                GotoHole(headUsedIndex, consumableType, templateIndex, holeIndex, true);

                // 如果演示，只走位置，再走演示高度，等待，回零，然后continue
                if (isDemo)
                {
                    GotoCoverHeight(headUsedIndex, demoHeight, true, true);
                    Thread.Sleep((int)(demoHeightDelay * 1000));
                    GotoCoverHeight(headUsedIndex, 0, false, true);
                    continue;
                }

                // 喷液速度
                if (!jetSpeed.Equals(""))
                    //SpeedSet(headUsedIndex, EAxis.P, jetSpeed, 100, true);
                    PumpSpeedSet(jetSpeed);

                // 先靠壁，再喷液
                if (wallJet)
                {
                    // 如果为第1个孔液体，喷出量 = 吸液后吸空气量 + 喷液体积
                    if (holeTick == 0 && holeCount > 1)
                        WallFirstJetSecond(headUsedIndex, seq, consumableType, false, false, 0, templateIndex, holeIndex, liquidVolume + absorbAirAfter, true);
                    // 最后1个孔喷液，且没有多吸液体，直接PA0
                    else if (holeTick == holeCount - 1 && (absorbLiquidMoreOne2More == 0 || (absorbLiquidMoreOne2More > 0 && jetLiquidMoreScale == 100)))
                        WallFirstJetSecond(headUsedIndex, seq, consumableType, true, false, 0, templateIndex, holeIndex, liquidVolume, true);
                    else
                        WallFirstJetSecond(headUsedIndex, seq, consumableType, false, false, 0, templateIndex, holeIndex, liquidVolume, true);
                }
                // 先喷液，再靠壁
                else
                {
                    // 压盖高度
                    GotoCoverHeight(headUsedIndex, coverHeight, true, true);
                    // 喷液高度
                    GotoHeight(headUsedIndex, liquidJetHeight, true);

                    /**
                     * 移液体积
                     */
                    // 如果需要混合，则喷出全部液体
                    if (serialDilute)
                    {
                        Pa(headUsedIndex, 0, true);
                        // 更新移液头状态信息
                        headStatusList[headUsedIndex].Head = EHeadStatus.Jetted;
                    }
                    else
                    {
                        // 如果为第1个孔液体，喷出量 = 吸液后吸空气量 + 喷液体积
                        if (holeTick == 0 && holeCount > 1)
                            Ps(headUsedIndex, (liquidVolume + absorbAirAfter) * -1, true);
                        // 最后1个孔喷液，且没有多吸液体，直接PA0
                        else if (holeTick == holeCount - 1 && (absorbLiquidMoreOne2More == 0 || (absorbLiquidMoreOne2More > 0 && jetLiquidMoreScale == 100)))
                        {
                            Pa(headUsedIndex, 0, true);
                            // 更新移液头状态信息
                            headStatusList[headUsedIndex].Head = EHeadStatus.Jetted;
                        }
                        else
                            Ps(headUsedIndex, liquidVolume * -1, true);
                    }

                    // 喷液等待
                    Thread.Sleep((int)(liquidJetDelay * 1000));

                    // 靠壁偏移(小于触发条件才偏移)
                    if (liquidVolume <= liquidJetWallTrigger && seq.JetWallList.Count > 0 && liquidJetWallHeight > 0)
                    {
                        GotoHeight(headUsedIndex, liquidJetWallHeight, true);

                        /**
                         * 多向靠壁
                         */
                        foreach (var jetWall in seq.JetWallList)
                        {
                            // 左靠壁
                            if (jetWall == EWall.Left)
                                WallAndReset(headUsedIndex, consumableType, templateIndex, holeIndex, HEAD_X_AXIS_LIST[headUsedIndex] + SPEED_SLOW, HEAD_X_AXIS_LIST[headUsedIndex] + "S-" + liquidJetWallOffset.ToString("F2"));
                            // 右靠壁
                            if (jetWall == EWall.Right)
                                WallAndReset(headUsedIndex, consumableType, templateIndex, holeIndex, HEAD_X_AXIS_LIST[headUsedIndex] + SPEED_SLOW, HEAD_X_AXIS_LIST[headUsedIndex] + "S" + liquidJetWallOffset.ToString("F2"));
                            // 前靠壁
                            if (jetWall == EWall.Front)
                                WallAndReset(headUsedIndex, consumableType, templateIndex, holeIndex, HEAD_Y_AXIS_LIST[headUsedIndex] + SPEED_SLOW, HEAD_Y_AXIS_LIST[headUsedIndex] + "S" + liquidJetWallOffset.ToString("F2"));
                            // 后靠壁
                            if (jetWall == EWall.Back)
                                WallAndReset(headUsedIndex, consumableType, templateIndex, holeIndex, HEAD_Y_AXIS_LIST[headUsedIndex] + SPEED_SLOW, HEAD_Y_AXIS_LIST[headUsedIndex] + "S-" + liquidJetWallOffset.ToString("F2"));
                        }
                    }
                }

                // 恢复用户设置的速度
                if (!jetSpeed.Equals(""))
                    //SpeedSet(headUsedIndex, EAxis.P, defaultPSpeed, pSpeedPercent, true);
                    PumpSpeedSet(defaultPSpeed);

                /**
                 * 混合
                 */
                if (serialDilute)
                {
                    if (!jetMixingSpeed.Equals(""))
                        //SpeedSet(headUsedIndex, EAxis.P, jetMixingSpeed, 100, true);
                        PumpSpeedSet(jetMixingSpeed);

                    // 混合高度
                    GotoHeight(headUsedIndex, jetMixingHeight, true);

                    // 混合
                    for (var mixCountIndex = 0; mixCountIndex < mixingCount; mixCountIndex++)
                    {
                        // 吸液
                        Ps(headUsedIndex, mixingVolume, true);
                        // 喷液
                        Pa(headUsedIndex, 0, true);
                    }

                    // 下一次的喷液体积
                    if (liquidVolumeNext != 0)
                    {
                        // 吸液
                        Ps(headUsedIndex, liquidVolumeNext, true);
                    }

                    // 恢复用户设置的速度
                    if (!jetMixingSpeed.Equals(""))
                        //SpeedSet(headUsedIndex, EAxis.P, defaultPSpeed, pSpeedPercent, true);
                        PumpSpeedSet(defaultPSpeed);
                }

                holeTick++;

                // 喷液后提起速度
                if (!jetHeight2NormalHeightSpeed.Equals(""))
                    SpeedSet(headUsedIndex, EAxis.Z, jetHeight2NormalHeightSpeed, 100, true);

                // Z轴回到盖盖高度
                GotoHeight(headUsedIndex, coverHeight, true);
                // // 行走高度（如果非最后一次移液，就走盘内行走高度，否则走盘外行走高度）
                // if (i != holeCount - 1)
                //     GotoHeight(headUsedIndex, normalHeightInner, true);
                // else
                //     GotoHeight(headUsedIndex, normalHeightReset ? 0 : normalHeightOuter, true);
                // 压盖回零
                GotoCoverHeight(headUsedIndex, 0, false, true);

                // 恢复用户设置的速度
                if (!jetHeight2NormalHeightSpeed.Equals(""))
                    SpeedSet(headUsedIndex, EAxis.Z, defaultZSpeed, zSpeedPercent, true);

                // 喷液后特殊指令
                if (!seq.CmdJetAfter.Equals(""))
                    MainWindow.mMainWindow.ParseCmdLine(seq.CmdJetAfter, false);
            }

            // 是否有残余液体，有的话回到源孔打出
            if (absorbLiquidMoreOne2More > 0 && jetLiquidMoreScale < 100m && !serialDilute) //  多喷量 != 多吸量，回源孔打液
            {
                //                // 行走高度
                //                GotoHeight(headUsedIndex, sourceNormalHeight > normalHeightInner ? normalHeightInner : sourceNormalHeight, EOffsetType.Template, true);
                // 到孔位置
                GotoHole(headUsedIndex, sourceConsumableType, sourceTemplateIndex, sourceHoleIndex, true);
                // 压盖高度
                GotoCoverHeight(headUsedIndex, coverHeight, true, true);
                // 吸液高度
                GotoHeight(headUsedIndex, sourceLiquidAbsorbHeight, true);
                // 喷液
                Pa(headUsedIndex, 0, true);
                // 更新移液头状态信息
                headStatusList[headUsedIndex].Head = EHeadStatus.Jetted;
                // 行走高度
                GotoHeight(headUsedIndex, normalHeightReset ? 0 : normalHeightOuter, true);
            }

            // 清洗枪头
            Cleaning(headUsedIndex, consumableType, seq, isDemo);
        }

        /// <summary>
        /// 先靠壁，再喷液
        /// </summary>
        /// <param name="headUsedIndex">移液头Index</param>
        /// <param name="seq"></param>
        /// <param name="consumableType"></param>
        /// <param name="isPA0"></param>
        /// <param name="isJetSeparate"></param>
        /// <param name="volumeJet2"></param>
        /// <param name="templateIndex"></param>
        /// <param name="holeIndex"></param>
        /// <param name="liquidVolume"></param>
        /// <param name="isNeedManualStop">是否需要手动停止功能</param>
        private static void WallFirstJetSecond(int headUsedIndex, Seq seq, Consumable consumableType, bool isPA0, bool isJetSeparate, decimal volumeJet2, int templateIndex, HoleIndex holeIndex, decimal liquidVolume, bool isNeedManualStop)
        {
            /*
             * 获取所有可变值
             */
            var commonSetting = ParamsHelper.CommonSettingList[headUsedIndex];
            // 喷液后等待
            var liquidJetDelay = consumableType.LiquidJetDelay;
            // 靠壁高度
            var liquidJetWallHeight = consumableType.LiquidJetWallHeight;
            // 靠壁偏移
            var liquidJetWallOffset = consumableType.LiquidJetWallOffset;
            // X默认速度
            var defaultXSpeed = commonSetting.DefaultXSpeed;
            // X速度百分比
            var xSpeedPercent = commonSetting.XSpeedPercent;
            // Y默认速度
            var defaultYSpeed = commonSetting.DefaultYSpeed;
            // Y速度百分比
            var ySpeedPercent = commonSetting.YSpeedPercent;


            // 减速靠壁指令
            var slowAndWallAxisCmd = "";

            // 靠壁高度
            GotoHeight(headUsedIndex, liquidJetWallHeight, isNeedManualStop);

            // 靠壁逻辑：默认是在第1个靠壁方向喷液，其他方向只靠壁
            if (seq.JetWallList.Count > 0)
            {
                // 左靠壁
                if (seq.JetWallList[0] == EWall.Left)
                    slowAndWallAxisCmd = HEAD_X_AXIS_LIST[headUsedIndex] + SPEED_SLOW + "," + HEAD_X_AXIS_LIST[headUsedIndex] + "S-" + liquidJetWallOffset.ToString("F2");
                // 右靠壁
                else if (seq.JetWallList[0] == EWall.Right)
                    slowAndWallAxisCmd = HEAD_X_AXIS_LIST[headUsedIndex] + SPEED_SLOW + "," + HEAD_X_AXIS_LIST[headUsedIndex] + "S" + liquidJetWallOffset.ToString("F2");
                // 前靠壁
                else if (seq.JetWallList[0] == EWall.Front)
                    slowAndWallAxisCmd = HEAD_Y_AXIS_LIST[headUsedIndex] + SPEED_SLOW + "," + HEAD_Y_AXIS_LIST[headUsedIndex] + "S" + liquidJetWallOffset.ToString("F2");
                // 后靠壁
                else if (seq.JetWallList[0] == EWall.Back)
                    slowAndWallAxisCmd = HEAD_Y_AXIS_LIST[headUsedIndex] + SPEED_SLOW + "," + HEAD_Y_AXIS_LIST[headUsedIndex] + "S-" + liquidJetWallOffset.ToString("F2");

                // 减速靠壁
                DoCmd(slowAndWallAxisCmd, isNeedManualStop);
            }

            // 喷液
            JetLiquid(headUsedIndex, liquidVolume, isPA0, isJetSeparate, volumeJet2, isNeedManualStop);

            // 是否有喷液后再次靠壁（从第2个靠壁方向开始）
            for (var i = 1; i < seq.JetWallList.Count; i++)
            {
                var jetWall = seq.JetWallList[i];
                // 靠壁喷液后恢复孔位置
                GotoHole(headUsedIndex, consumableType, templateIndex, holeIndex, isNeedManualStop);
                // 左靠壁
                if (jetWall == EWall.Left)
                    slowAndWallAxisCmd = HEAD_X_AXIS_LIST[headUsedIndex] + SPEED_SLOW + "," + HEAD_X_AXIS_LIST[headUsedIndex] + "S-" + liquidJetWallOffset.ToString("F2");
                // 右靠壁
                else if (jetWall == EWall.Right)
                    slowAndWallAxisCmd = HEAD_X_AXIS_LIST[headUsedIndex] + SPEED_SLOW + "," + HEAD_X_AXIS_LIST[headUsedIndex] + "S" + liquidJetWallOffset.ToString("F2");
                // 前靠壁
                else if (jetWall == EWall.Front)
                    slowAndWallAxisCmd = HEAD_Y_AXIS_LIST[headUsedIndex] + SPEED_SLOW + "," + HEAD_Y_AXIS_LIST[headUsedIndex] + "S" + liquidJetWallOffset.ToString("F2");
                // 后靠壁
                else if (jetWall == EWall.Back)
                    slowAndWallAxisCmd = HEAD_Y_AXIS_LIST[headUsedIndex] + SPEED_SLOW + "," + HEAD_Y_AXIS_LIST[headUsedIndex] + "S-" + liquidJetWallOffset.ToString("F2");
                // 减速靠壁
                DoCmd(slowAndWallAxisCmd, isNeedManualStop);
            }

            // 恢复用户设置的速度
            SpeedSet(headUsedIndex, EAxis.X, defaultXSpeed, xSpeedPercent, isNeedManualStop);
            SpeedSet(headUsedIndex, EAxis.Y, defaultYSpeed, ySpeedPercent, isNeedManualStop);

            // 喷液等待
            Thread.Sleep((int)(liquidJetDelay * 1000));

            // 靠壁喷液后恢复孔位置
            GotoHole(headUsedIndex, consumableType, templateIndex, holeIndex, isNeedManualStop);
        }

        /// <summary>
        /// 喷液
        /// </summary>
        /// <param name="headUsedIndex">移液头Index</param>
        /// <param name="liquidVolume">液体量</param>
        /// <param name="isPA0">是否执行PA0，把液体全喷完</param>
        /// <param name="isJetSeparate">是否分段喷液</param>
        /// <param name="volumeJet2">第2段喷液</param>
        /// <param name="isNeedManualStop">是否需要手动停止功能</param>
        private static void JetLiquid(int headUsedIndex, decimal liquidVolume, bool isPA0, bool isJetSeparate, decimal volumeJet2, bool isNeedManualStop)
        {
            /*
            * 获取所有可变值
            */
            var commonSetting = ParamsHelper.CommonSettingList[headUsedIndex];
            // 第1段喷液速度
            var speedJet1 = commonSetting.SpeedJet1;
            // 第1段喷液速度百分比
            var jet1SpeedPercent = commonSetting.Jet1SpeedPercent;
            // 第2段喷液速度
            var speedJet2 = commonSetting.SpeedJet2;
            // 第2段喷液速度百分比
            var jet2SpeedPercent = commonSetting.Jet2SpeedPercent;
            // 两段喷液间停留时间（ms）
            var delayBetweenJet = commonSetting.DelayBetweenJet;
            // P默认速度
            var defaultPSpeed = commonSetting.DefaultPSpeed;
            // P速度百分比
            var pSpeedPercent = commonSetting.PSpeedPercent;

            // 分段喷液
            if (isJetSeparate)
            {
                // 第1段
                //SpeedSet(headUsedIndex, EAxis.P, speedJet1, jet1SpeedPercent, isNeedManualStop);
                PumpSpeedSet(speedJet1);
                Ps(headUsedIndex, (liquidVolume - volumeJet2) * -1, isNeedManualStop);

                // 中间停留时间
                Thread.Sleep(delayBetweenJet);

                // 第2段
                //SpeedSet(headUsedIndex, EAxis.P, speedJet2, jet2SpeedPercent, isNeedManualStop);
                PumpSpeedSet(speedJet2);
                Pa(headUsedIndex, 0, isNeedManualStop);

                // 更新移液头状态信息
                headStatusList[headUsedIndex].Head = EHeadStatus.Jetted;

                // 恢复P轴速度
                //SpeedSet(headUsedIndex, EAxis.P, defaultPSpeed, pSpeedPercent, isNeedManualStop);
                PumpSpeedSet(defaultPSpeed);
            }
            // 不分段喷液
            else
            {
                // 如果分段1设置了速度，就用该速度
                if (!speedJet1.Equals(""))
                {
                    //SpeedSet(headUsedIndex, EAxis.P, speedJet1, jet1SpeedPercent, isNeedManualStop);
                    PumpSpeedSet(speedJet1);
                }

                if (isPA0)
                {
                    Pa(headUsedIndex, 0, isNeedManualStop);
                    // 更新移液头状态信息
                    headStatusList[headUsedIndex].Head = EHeadStatus.Jetted;
                }
                else
                    Ps(headUsedIndex, liquidVolume * -1, isNeedManualStop);

                // 恢复P轴速度
                //SpeedSet(headUsedIndex, EAxis.P, defaultPSpeed, pSpeedPercent, isNeedManualStop);
                PumpSpeedSet(defaultPSpeed);
            }
        }

        /// <summary>
        /// 靠壁，再复位
        /// </summary>
        /// <param name="headUsedIndex">移液头Index</param>
        /// <param name="consumableType">耗材类型</param>
        /// <param name="templateIndex">盘位 index</param>
        /// <param name="holeIndex">孔位</param>
        /// <param name="slowSpeedCmd">减速指令</param>
        /// <param name="wallAxisCmd">靠壁轴指令</param>
        private static void WallAndReset(int headUsedIndex, Consumable consumableType, int templateIndex, HoleIndex holeIndex, string slowSpeedCmd, string wallAxisCmd)
        {
            /*
           * 获取所有可变值
           */
            var commonSetting = ParamsHelper.CommonSettingList[headUsedIndex];
            // X默认速度
            var defaultXSpeed = commonSetting.DefaultXSpeed;
            // X速度百分比
            var xSpeedPercent = commonSetting.XSpeedPercent;
            // Y默认速度
            var defaultYSpeed = commonSetting.DefaultYSpeed;
            // Y速度百分比
            var ySpeedPercent = commonSetting.YSpeedPercent;

            // 减速靠壁
            DoCmd(slowSpeedCmd + "," + wallAxisCmd, true);

            // 恢复用户设置的速度
            SpeedSet(headUsedIndex, EAxis.X, defaultXSpeed, xSpeedPercent, true);
            SpeedSet(headUsedIndex, EAxis.Y, defaultYSpeed, ySpeedPercent, true);

            // 靠壁后恢复孔位置
            Thread.Sleep(100);
            GotoHole(headUsedIndex, consumableType, templateIndex, holeIndex, true);
        }

        /// <summary>
        /// 液体喷回到源孔
        /// </summary>
        /// <param name="headUsedIndex">移液头Index</param>
        /// <param name="seq">移液信息</param>
        /// <param name="sourceConsumableType">源盘耗材类型</param>
        /// <param name="sourceTemplateIndex">源盘盘位 index</param>
        /// <param name="sourceHoleIndex">源盘孔位</param>
        /// <param name="isNeedManualStop">是否需要手动停止功能</param>
        public static bool ReJet2Source(int headUsedIndex, Seq seq, Consumable sourceConsumableType, int sourceTemplateIndex, HoleIndex sourceHoleIndex, bool isNeedManualStop)
        {
            LogHelper.Info((string)Application.Current.FindResource("ReJet2Source"), "");

            /*
             * 获取所有可变值
             */
            var commonSetting = ParamsHelper.CommonSettingList[headUsedIndex];
            // 喷液高度
            var liquidAbsorbHeight = sourceConsumableType.LiquidAbsorbHeight;
            // 靠壁喷液
            var wallJet = sourceConsumableType.WallJet;
            // 压盖高度
            var coverHeight = sourceConsumableType.CoverHeight;

            // 行走高度
            GotoHeight(headUsedIndex, 0, isNeedManualStop);

            // 到孔位置
            GotoHole(headUsedIndex, sourceConsumableType, sourceTemplateIndex, sourceHoleIndex, isNeedManualStop);

            // 先靠壁，再喷液
            if (wallJet && seq.JetWallList.Count > 0)
                WallFirstJetSecond(headUsedIndex, seq, sourceConsumableType, true, false, 0, sourceTemplateIndex, sourceHoleIndex, 0, isNeedManualStop);
            else
            {
                // 压盖高度
                GotoCoverHeight(headUsedIndex, coverHeight, true, isNeedManualStop);
                // 吸液高度
                GotoHeight(headUsedIndex, liquidAbsorbHeight, isNeedManualStop);

                // 喷液
                JetLiquid(headUsedIndex, 0, true, false, 0, isNeedManualStop);
            }

            // z轴回零
            // GotoHeight(headUsedIndex, 0, isNeedManualStop);
            // Z轴回到盖盖高度
            GotoHeight(headUsedIndex, coverHeight, true);
            // 压盖回零
            GotoCoverHeight(headUsedIndex, 0, false, true);

            return true;
        }

        /// <summary>
        /// 清洗枪头
        /// </summary>
        /// <param name="headUsedIndex"></param>
        /// <param name="consumableType"></param>
        /// <param name="seq"></param>
        /// <param name="isDemo">是否演示</param>
        public static void Cleaning(int headUsedIndex, Consumable consumableType, Seq seq, bool isDemo)
        {
            /**
             * 获取所有可变值
             */
            var commonSetting = ParamsHelper.CommonSettingList[headUsedIndex];
            // 行走高度，走下一个耗材的行走高度
            decimal normalHeight = consumableType.NormalHeight;
            // 跨耗材行走高度复位
            var normalHeightReset = consumableType.NormalHeightReset;
            // 清洗位置
            var cleaningPosIndexArray = seq.CleaningPosIndexArray;
            // 清洗前吸空气量
            var absorbAirBeforeCleaning = commonSetting.AbsorbAirBeforeCleaning;
            // 清洗液体积
            var volumeLiquidCleaningHead = commonSetting.CleaningLiquidVolume;
            // 清洗吸喷体积（空气量）
            var volumeAirCleaningHead = commonSetting.CleaningHeadVolume;
            // 清洗吸喷次数
            var timeCleaningHead = commonSetting.CleaningHeadTime;
            // 吸液后吸空气量
            var cleaningHeadAbsorbAirAfterAbsorb = commonSetting.CleaningHeadAbsorbAirAfterAbsorb;
            // 喷液后吸空气量
            var cleaningHeadAbsorbAirAfterJet = commonSetting.CleaningHeadAbsorbAirAfterJet;
            // 清洗速度
            var cleaningHeadSpeed = commonSetting.CleaningHeadAbsorbAirAfterJetSpeed;
            // 喷空气后等待时间
            var cleaningHeadJetAirDelay = commonSetting.CleaningHeadJetAirDelay;
            // 开泵延时
            var openPumpDelayMillSec = commonSetting.OpenPumpDelay * 1000;
            // 废液槽位置
            var wastePos = commonSetting.WastePos;

            if (seq.IsNeedCleaning && volumeLiquidCleaningHead > 0 && volumeAirCleaningHead > 0 && timeCleaningHead > 0)
            {
                foreach (var posIndex in cleaningPosIndexArray)
                {
                    if (posIndex == -1)
                        continue;

                    // 清洗区位置
                    var cleaningPos = commonSetting.CleaningPosList[posIndex];

                    // 清洗位置
                    XaYa(headUsedIndex, cleaningPos.X, cleaningPos.Y, true);

                    // 清洗速度
                    //SpeedSet(headUsedIndex, EAxis.P, cleaningHeadSpeed, 100, true);
                    PumpSpeedSet(cleaningHeadSpeed);

                    // 清洗前吸空气量
                    if (absorbAirBeforeCleaning > 0 && !isDemo)
                        Ps(headUsedIndex, absorbAirBeforeCleaning, true);

                    // 吸液高度
                    GotoHeight(headUsedIndex, cleaningPos.Z, true);
                    // 吸液
                    if (!isDemo)
                        Ps(headUsedIndex, volumeLiquidCleaningHead, true);
                    GotoHeight(headUsedIndex, normalHeightReset ? 0 : normalHeight, true);

                    // 吸液后吸空气量
                    if (cleaningHeadAbsorbAirAfterAbsorb > 0 && !isDemo)
                        Ps(headUsedIndex, cleaningHeadAbsorbAirAfterAbsorb, true);

                    // 废液槽位置
                    XaYa(headUsedIndex, wastePos.X, wastePos.Y, true);
                    GotoHeight(headUsedIndex, wastePos.Z, true);

                    // 空气抽打
                    if (!isDemo)
                    {
                        for (var i = 0; i < timeCleaningHead; i++)
                        {
                            Ps(headUsedIndex, volumeAirCleaningHead, true);
                            Thread.Sleep(100);

                            // 最后一次抽打
                            if (i == timeCleaningHead - 1)
                            {
                                // 直接PA0
                                Pa(headUsedIndex, 0, true);
                                /**
                                 * 吸入设定的空气，再次PA0
                                 */
                                // 打开空气阀和泵
                                ValveAndPumpOpen(true);
                                // 开泵延时计时
                                var openPumpDelayStart = DateTime.Now;
                                Ps(headUsedIndex, cleaningHeadAbsorbAirAfterJet, true);
                                Pa(headUsedIndex, 0, true);
                                // 喷空气后等待时间
                                Thread.Sleep((int)(cleaningHeadJetAirDelay * 1000));
                                // 开泵延时
                                while (Convert.ToInt32((DateTime.Now - openPumpDelayStart).TotalMilliseconds) < openPumpDelayMillSec)
                                {
                                    Thread.Sleep(5);
                                }
                                // 关闭空气阀和泵
                                ValveAndPumpClose(true);
                            }
                            else
                                Ps(headUsedIndex, volumeAirCleaningHead * -1, true);
                        }
                    }

                    // 恢复速度
                    //SpeedSet(headUsedIndex, EAxis.P, ParamsHelper.CommonSettingList[0].DefaultPSpeed, ParamsHelper.CommonSettingList[0].PSpeedPercent, true);
                    PumpSpeedSet(ParamsHelper.CommonSettingList[0].DefaultPSpeed);

                    GotoHeight(headUsedIndex, normalHeightReset ? 0 : normalHeight, true);
                }
            }
        }

        /// <summary>
        /// 到孔指定位置（X、Y轴）
        /// </summary>
        /// <param name="headUsedIndex">移液头Index</param>
        /// <param name="consumableType">耗材类型</param>
        /// <param name="templateIndex">盘位index</param>
        /// <param name="holeIndex">孔index</param>
        /// <param name="isNeedManualStop">是否需要手动停止功能</param>
        /// <returns>返回位置信息</returns>
        public static Position GotoHole(int headUsedIndex, Consumable consumableType, int templateIndex, HoleIndex holeIndex, bool isNeedManualStop)
        {
            // 获取孔位置
            var pos = GetHolePos(headUsedIndex, consumableType, templateIndex, holeIndex);

            GotoHole(headUsedIndex, pos, consumableType, isNeedManualStop);

            return pos;
        }

        /// <summary>
        /// 到孔指定位置（X、Y轴）
        /// </summary>
        /// <param name="headUsedIndex">移液头Index</param>
        /// <param name="pos">位置</param>
        /// <param name="consumableType">耗材类型</param>
        /// <param name="isNeedManualStop">是否需要手动停止功能</param>
        /// <returns>返回位置信息</returns>
        private static void GotoHole(int headUsedIndex, Position pos, Consumable consumableType, bool isNeedManualStop)
        {
            if (ParamsHelper.HeadList[headUsedIndex].YAvailable)
                XaYa(headUsedIndex, pos.X, pos.Y, isNeedManualStop);
            else
                Xa(headUsedIndex, pos.X, isNeedManualStop);
        }

        /// <summary>
        /// 获取孔位置
        /// </summary>
        /// <param name="headUsedIndex">移液头Index</param>
        /// <param name="consumableType">耗材类型</param>
        /// <param name="templateIndex">盘位index</param>
        /// <param name="holeIndex">孔index</param>
        /// <returns></returns>
        private static Position GetHolePos(int headUsedIndex, Consumable consumableType, int templateIndex, HoleIndex holeIndex)
        {
            // 位置信息
            var pos = new Position();

            /*
              * 获取所有可变值
              */
            var commonSetting = ParamsHelper.CommonSettingList[headUsedIndex];
            // 首孔坐标
            var holeStartPos = consumableType.HoleStartPosList[templateIndex];
            // 盘位摆放（即A1方向）
            var a1Pos = commonSetting.A1Pos;
            // 行数
            var rowCount = consumableType.RowCount;
            // 列数
            var colCount = consumableType.ColCount;
            // 孔距
            var holeStep = consumableType.HoleStep;
            // 孔位Index
            var oriIndex = holeIndex.OriIndex;
            // 孔位偏移
            var xHoleOffset = holeIndex.XHoleOffset;
            var yHoleOffset = holeIndex.YHoleOffset;


            var xIndex = 0;
            var yIndex = 0;

            if (oriIndex != 0)
            {
                // A1位置：左上
                if (a1Pos == EA1Pos.LeftTop)
                {
                    // 每列孔数
                    xIndex = oriIndex / rowCount;
                    yIndex = oriIndex % rowCount;
                }
                // A1位置：左下
                else
                {
                    // 每行孔数
                    xIndex = oriIndex % colCount;
                    yIndex = oriIndex / colCount;
                }
            }

            xIndex += xHoleOffset;
            yIndex += yHoleOffset;

            // x、y轴位置
            var xa = holeStartPos.X + holeStep.X * xIndex;
            // 由于标准Y轴是盘推出，和多个移液头Y轴（Y轴由头移动）有差异
            var ya = holeStartPos.Y;
            if (ParamsHelper.HeadList[headUsedIndex].Available && ParamsHelper.HeadList[headUsedIndex].YMoveWithHead)
                ya = a1Pos == EA1Pos.LeftTop
                    ? ya + holeStep.Y * yIndex
                    : ya - holeStep.Y * yIndex;
            else
                ya = a1Pos == EA1Pos.LeftTop
                    ? ya - holeStep.Y * yIndex
                    : ya + holeStep.Y * yIndex;
            // 赋值位置信息
            pos.X = xa;
            pos.Y = ya;

            return pos;
        }

        /// <summary>
        /// 到指定高度
        /// </summary>
        /// <param name="headUsedIndex">移液头Index</param>
        /// <param name="height"></param>
        /// <param name="isNeedManualStop"></param>
        public static bool GotoHeight(int headUsedIndex, decimal height, bool isNeedManualStop = false)
        {
            var z = height;
            return DoCmd(HEAD_Z_AXIS_LIST[headUsedIndex] + "A" + z.ToString("F2"), isNeedManualStop);
        }
        /// <summary>
        /// 到压盖指定高度
        /// </summary>
        /// <param name="headUsedIndex">移液头Index</param>
        /// <param name="height"></param>
        /// <param name="zFirst">指令是否Z先行</param>
        /// <param name="isNeedManualStop"></param>
        public static bool GotoCoverHeight(int headUsedIndex, decimal height, bool zFirst, bool isNeedManualStop = false)
        {
            // M轴和Z轴同时运动
            if (!zFirst)
                return DoCmd(HEAD_COVER_AXIS_LIST[headUsedIndex] + "A" + height.ToString("F2") + "," + HEAD_Z_AXIS_LIST[headUsedIndex] + "A" + height.ToString("F2"), isNeedManualStop);
            return DoCmd(HEAD_Z_AXIS_LIST[headUsedIndex] + "A" + height.ToString("F2") + "," + HEAD_COVER_AXIS_LIST[headUsedIndex] + "A" + height.ToString("F2"), isNeedManualStop);
        }

        /// <summary>
        /// ZI
        /// </summary>
        /// <param name="headUsedIndex">移液头Index</param>
        /// <param name="isNeedManualStop"></param>
        public static bool Zi(int headUsedIndex, bool isNeedManualStop = false)
        {
            return DoCmd(HEAD_Z_AXIS_LIST[headUsedIndex] + "I", isNeedManualStop);
        }

        /// <summary>
        /// XA
        /// </summary>
        /// <param name="headUsedIndex">移液头Index</param>
        /// <param name="x"></param>
        /// <param name="isNeedManualStop"></param>
        public static bool Xa(int headUsedIndex, decimal x, bool isNeedManualStop = false)
        {
            return DoCmd(HEAD_X_AXIS_LIST[headUsedIndex] + "A" + x.ToString("F2"), isNeedManualStop);
        }

        /// <summary>
        /// XS
        /// </summary>
        /// <param name="headUsedIndex">移液头Index</param>
        /// <param name="x"></param>
        /// <param name="isNeedManualStop"></param>
        public static bool Xs(int headUsedIndex, decimal x, bool isNeedManualStop = false)
        {
            return DoCmd(HEAD_X_AXIS_LIST[headUsedIndex] + "S" + x.ToString("F2"), isNeedManualStop);
        }

        /// <summary>
        /// XI
        /// </summary>
        /// <param name="headUsedIndex">移液头Index</param>
        /// <param name="isNeedManualStop"></param>
        public static bool Xi(int headUsedIndex, bool isNeedManualStop = false)
        {
            return DoCmd(HEAD_X_AXIS_LIST[headUsedIndex] + "I", isNeedManualStop);
        }

        /// <summary>
        /// YA
        /// </summary>
        /// <param name="headUsedIndex">移液头Index</param>
        /// <param name="y"></param>
        /// <param name="isNeedManualStop"></param>
        public static bool Ya(int headUsedIndex, decimal y, bool isNeedManualStop = false)
        {
            return DoCmd(HEAD_Y_AXIS_LIST[headUsedIndex] + "A" + y.ToString("F2"), isNeedManualStop);
        }

        /// <summary>
        /// YI
        /// </summary>
        /// <param name="headUsedIndex">移液头Index</param>
        /// <param name="isNeedManualStop"></param>
        public static bool Yi(int headUsedIndex, bool isNeedManualStop = false)
        {
            return DoCmd(HEAD_Y_AXIS_LIST[headUsedIndex] + "I", isNeedManualStop);
        }

        /// <summary>
        /// XA、YA
        /// </summary>
        /// <param name="headUsedIndex">移液头Index</param>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="isNeedManualStop"></param>
        public static bool XaYa(int headUsedIndex, decimal x, decimal y, bool isNeedManualStop = false)
        {
            var resultX = true;
            var resultY = true;

            if (ParamsHelper.HeadList[headUsedIndex].WalkingLogic == EWalkingLogic.XFirst)
            {
                resultX = DoCmd(HEAD_X_AXIS_LIST[headUsedIndex] + "A" + x.ToString("F2"), isNeedManualStop);
                resultY = DoCmd(HEAD_Y_AXIS_LIST[headUsedIndex] + "A" + y.ToString("F2"), isNeedManualStop);
                return resultX && resultY;
            }
            if (ParamsHelper.HeadList[headUsedIndex].WalkingLogic == EWalkingLogic.YFirst)
            {
                resultY = DoCmd(HEAD_Y_AXIS_LIST[headUsedIndex] + "A" + y.ToString("F2"), isNeedManualStop);
                resultX = DoCmd(HEAD_X_AXIS_LIST[headUsedIndex] + "A" + x.ToString("F2"), isNeedManualStop);
                return resultX && resultY;
            }
            return DoCmd(HEAD_X_AXIS_LIST[headUsedIndex] + "A" + x.ToString("F2") + "," + HEAD_Y_AXIS_LIST[headUsedIndex] + "A" + y.ToString("F2"), isNeedManualStop);
        }

        /// <summary>
        /// PA
        /// </summary>
        /// <param name="headUsedIndex">移液头Index</param>
        /// <param name="p">毫升</param>
        /// <param name="isNeedManualStop"></param>
        public static bool Pa(int headUsedIndex, decimal p, bool isNeedManualStop = false)
        {
            if (ParamsHelper.HeadList[headUsedIndex].PAvailable)
            {
                //return DoCmd(HEAD_P_AXIS_LIST[headUsedIndex] + "A" + p.ToString("F3"), isNeedManualStop);

                CmdHelper.pumpTransport.mgr.RunWorkflowAsync(PumpSelection.Left, ctx =>
                {
                    ctx.Parallel("吸喷液", wb =>
                    {
                        // 吸液
                        if (p > 0)
                        {
                            var stepsPump = PumpHelper.GetStepsFromVolume(p * 1000);
                            wb.Pull(stepsPump, "", "");
                        }
                        // 喷液
                        else if (p < 0)
                        {
                            var stepsPump = PumpHelper.GetStepsFromVolume(p * -1000);
                            wb.Dispense(stepsPump, "", "");
                        }
                        // 0为喷出所有液体
                        else
                        {
                            wb.DispenseAll("", "");
                        }
                    });
                }).GetAwaiter().GetResult();
            }

            return true;
        }

        /// <summary>
        /// PS
        /// </summary>
        /// <param name="headUsedIndex">移液头Index</param>
        /// <param name="p"></param>
        /// <param name="isNeedManualStop"></param>
        public static bool Ps(int headUsedIndex, decimal p, bool isNeedManualStop)
        {
            if (ParamsHelper.HeadList[headUsedIndex].PAvailable)
            {
                //return DoCmd(HEAD_P_AXIS_LIST[headUsedIndex] + "S" + p.ToString("F3"), isNeedManualStop);

                CmdHelper.pumpTransport.mgr.RunWorkflowAsync(PumpSelection.Left, ctx =>
                {
                    ctx.Parallel("吸喷液", wb =>
                    {
                        // 吸液
                        if (p > 0)
                        {
                            var stepsPump = PumpHelper.GetStepsFromVolume(p * 1000);
                            wb.Pull(stepsPump, "", "");
                        }
                        // 喷液
                        else if (p < 0)
                        {
                            var stepsPump = PumpHelper.GetStepsFromVolume(p * -1000);
                            wb.Dispense(stepsPump, "", "");
                        }
                    });
                }).GetAwaiter().GetResult();
            }

            return true;
        }

        /// <summary>
        /// PI
        /// </summary>
        /// <param name="headUsedIndex">移液头Index</param>
        /// <param name="isNeedManualStop"></param>
        public static bool Pi(int headUsedIndex, bool isNeedManualStop = false)
        {
            if (ParamsHelper.HeadList[headUsedIndex].PAvailable)
            //return DoCmd(HEAD_P_AXIS_LIST[headUsedIndex] + "I", isNeedManualStop);
            {
                pumpTransport.mgr.RunWorkflowAsync(PumpSelection.Left, ctx =>
                {
                    ctx.Parallel("初始化", wb =>
                    {
                        wb.Init();
                    });
                }).GetAwaiter().GetResult();
            }
            return true;
        }

        /// <summary>
        /// MI
        /// </summary>
        /// <param name="headUsedIndex">移液头Index</param>
        /// <param name="isNeedManualStop"></param>
        public static bool Mi(int headUsedIndex, bool isNeedManualStop = false)
        {
            return DoCmd(HEAD_COVER_AXIS_LIST[headUsedIndex] + "I", isNeedManualStop);
        }

        /// <summary>
        /// 设置轴速度（可复位该轴速度）
        /// <param name="headUsedIndex">移液头Index</param>
        /// <param name="axis">轴类型</param>
        /// <param name="speedCmd">速度指令（可不用填写轴前缀，如“FX”、“XF”等，且默认执行可刷写速度，如“XF”、“YF”等。如果为空，则单纯复位该轴速度）</param>
        /// <param name="speedPercent">速度百分比</param>
        /// <param name="isNeedManualStop">是否需要手动停止功能</param>
        /// </summary>
        public static void SpeedSet(int headUsedIndex, EAxis axis, string speedCmd, decimal speedPercent, bool isNeedManualStop)
        {
            if (isNeedManualStop) LogHelper.Info((string)Application.Current.FindResource("SetSpeed"), "");

            var commonSetting = ParamsHelper.CommonSettingList[headUsedIndex];

            if (axis == EAxis.All)
            {
                SpeedSetSub(headUsedIndex, EAxis.X, HEAD_X_AXIS_LIST[headUsedIndex] + "F 0", commonSetting.DefaultXSpeed, commonSetting.XSpeedPercent, isNeedManualStop);
                if (ParamsHelper.HeadList[headUsedIndex].YAvailable)
                    SpeedSetSub(headUsedIndex, EAxis.Y, HEAD_Y_AXIS_LIST[headUsedIndex] + "F 0", commonSetting.DefaultYSpeed, commonSetting.YSpeedPercent, isNeedManualStop);
                SpeedSetSub(headUsedIndex, EAxis.Z, HEAD_Z_AXIS_LIST[headUsedIndex] + "F 0", commonSetting.DefaultZSpeed, commonSetting.ZSpeedPercent, isNeedManualStop);
                //if (ParamsHelper.HeadList[headUsedIndex].PAvailable)
                //    SpeedSetSub(headUsedIndex, EAxis.P, HEAD_P_AXIS_LIST[headUsedIndex] + "F 0", commonSetting.DefaultPSpeed, commonSetting.PSpeedPercent, isNeedManualStop);
                SpeedSetSub(headUsedIndex, EAxis.M, HEAD_COVER_AXIS_LIST[headUsedIndex] + "F 0", commonSetting.DefaultMSpeed, commonSetting.MSpeedPercent, isNeedManualStop);
            }
            else if (axis == EAxis.X)
            {
                SpeedSetSub(headUsedIndex, EAxis.X, HEAD_X_AXIS_LIST[headUsedIndex] + "F 0", speedCmd, speedPercent, isNeedManualStop);
            }
            else if (axis == EAxis.Y && ParamsHelper.HeadList[headUsedIndex].YAvailable)
            {
                SpeedSetSub(headUsedIndex, EAxis.Y, HEAD_Y_AXIS_LIST[headUsedIndex] + "F 0", speedCmd, speedPercent, isNeedManualStop);
            }
            else if (axis == EAxis.Z)
            {
                SpeedSetSub(headUsedIndex, EAxis.Z, HEAD_Z_AXIS_LIST[headUsedIndex] + "F 0", speedCmd, speedPercent, isNeedManualStop);
            }
            //else if (axis == EAxis.P)
            //{
            //    if (ParamsHelper.HeadList[headUsedIndex].PAvailable)
            //        SpeedSetSub(headUsedIndex, EAxis.P, HEAD_P_AXIS_LIST[headUsedIndex] + "F 0", speedCmd, speedPercent, isNeedManualStop);
            //}
            else if (axis == EAxis.M)
            {
                SpeedSetSub(headUsedIndex, EAxis.M, HEAD_COVER_AXIS_LIST[headUsedIndex] + "F 0", speedCmd, speedPercent, isNeedManualStop);
            }
        }

        /// <summary>
        /// 设置轴速度子方法
        /// </summary>
        /// <param name="headUsedIndex">移液头Index</param>
        /// <param name="axis">轴类型</param>
        /// <param name="initZeroCmd">复位指令，如：xf 0</param>
        /// <param name="speedCmd">速度指令</param>
        /// <param name="percent">速度百分比</param>
        /// <param name="isNeedManualStop">是否需要手动停止功能</param>
        public static void SpeedSetSub(int headUsedIndex, EAxis axis, string initZeroCmd, string speedCmd, decimal percent, bool isNeedManualStop)
        {
            speedCmd = speedCmd.Trim().ToUpper();
            if (speedCmd.Equals(""))
                DoCmd(initZeroCmd, isNeedManualStop);
            if (!speedCmd.Equals(""))
            {
                // 判断用户填写的速度指令是否包含前缀，如“FX”、“XF”等，没有就自动补全，且默认执行可刷写速度，如“XF”、“YF”等
                if (speedCmd.Contains("F"))
                    speedCmd = speedCmd.Substring(2).Trim();

                if (axis == EAxis.X)
                    speedCmd = HEAD_X_AXIS_LIST[headUsedIndex] + "F " + speedCmd;
                else if (axis == EAxis.Y && ParamsHelper.HeadList[headUsedIndex].YAvailable)
                    speedCmd = HEAD_Y_AXIS_LIST[headUsedIndex] + "F " + speedCmd;
                else if (axis == EAxis.Z)
                    speedCmd = HEAD_Z_AXIS_LIST[headUsedIndex] + "F " + speedCmd;
                //else if (axis == EAxis.P)
                //    speedCmd = HEAD_P_AXIS_LIST[headUsedIndex] + "F " + speedCmd;
                else if (axis == EAxis.M)
                    speedCmd = HEAD_COVER_AXIS_LIST[headUsedIndex] + "F " + speedCmd;

                var cmd = ObjectUtils.CalcSpeedCmdValueByPercent(speedCmd, percent);
                DoCmd(cmd, isNeedManualStop);
            }
        }

        /// <summary>
        /// 设置注射泵速度
        /// </summary>
        /// <param name="speedLevel"></param>
        public static void PumpSpeedSet(string speedLevel)
        {
            if (int.TryParse(speedLevel, out int result))
            {
                CmdHelper.pumpTransport.mgr.RunWorkflowAsync(PumpSelection.Left, ctx =>
                {
                    ctx.Parallel("设置速度", wb =>
                    {
                        wb.SpeedLevel(result);
                    });
                }).GetAwaiter().GetResult();
            }
            else
            {
                LogHelper.Error("速度设置失败，输入的值不是有效的数字");
            }
        }

        /// <summary>
        /// 紧急停止功能状态查询
        /// <param name="isNeedManualStop">是否需要手动停止功能</param>
        /// </summary>
        public static bool EmergencyStopQuery(bool isNeedManualStop)
        {
            LogHelper.Info((string)Application.Current.FindResource("Cmd"), "AD1");
            return DoCmd("AD1", isNeedManualStop, false);
        }

        /// <summary>
        /// 紧急停止功能打开
        /// <param name="isNeedManualStop">是否需要手动停止功能</param>
        /// </summary>
        public static void EmergencyStopOpen(bool isNeedManualStop)
        {
            LogHelper.Info((string)Application.Current.FindResource("EmergencyStopAvailable"), "");
            DoCmd("A9 1", isNeedManualStop);
        }

        /// <summary>
        /// 紧急停止功能关闭
        /// <param name="isNeedManualStop">是否需要手动停止功能</param>
        /// </summary>
        public static void EmergencyStopClose(bool isNeedManualStop)
        {
            LogHelper.Info((string)Application.Current.FindResource("EmergencyStopUnavailable"), "");
            DoCmd("A9 0", isNeedManualStop);
        }

        /// <summary>
        /// 风扇打开
        /// <param name="isNeedManualStop">是否需要手动停止功能</param>
        /// </summary>
        public static void FanOpen(bool isNeedManualStop)
        {
            LogHelper.Info((string)Application.Current.FindResource("FanAvailable"), "");
            DoCmd("AO41", isNeedManualStop);
        }

        /// <summary>
        /// 风扇关闭
        /// <param name="isNeedManualStop">是否需要手动停止功能</param>
        /// </summary>
        public static void FanClose(bool isNeedManualStop)
        {
            LogHelper.Info((string)Application.Current.FindResource("FanUnavailable"), "");
            DoCmd("AO40", isNeedManualStop);
        }

        /// <summary>
        /// 复位所有指示灯
        /// ao11亮绿色，运行中
        /// ao21亮黄色, 暂停
        //  ao31亮红灯, 出错
        /// </summary>
        /// <param name="temp"></param>
        /// <param name="isNeedManualStop"></param>
        /// <returns></returns>
        public static void ResetAllLight(bool isNeedManualStop)
        {
            DoCmd("AO10,AO20,AO30", isNeedManualStop);
        }

        /// <summary>
        /// 打开指示灯
        /// </summary>
        /// <param name="light"></param>
        /// <param name="isNeedManualStop"></param>
        public static void LightOn(ELight light, bool isNeedManualStop)
        {
            if (light == ELight.Green)
                DoCmd("AO11", isNeedManualStop);
            else if (light == ELight.Yellow)
                DoCmd("AO21", isNeedManualStop);
            else if (light == ELight.Red)
                DoCmd("AO31", isNeedManualStop);
        }

        /// <summary>
        /// 关闭指示灯
        /// </summary>
        /// <param name="light"></param>
        /// <param name="isNeedManualStop"></param>
        public static void LightOff(ELight light, bool isNeedManualStop)
        {
            if (light == ELight.Green)
                DoCmd("AO10", isNeedManualStop);
            else if (light == ELight.Yellow)
                DoCmd("AO20", isNeedManualStop);
            else if (light == ELight.Red)
                DoCmd("AO30", isNeedManualStop);
        }

        /// <summary>
        /// 紫外灯打开
        /// <param name="isNeedManualStop">是否需要手动停止功能</param>
        /// </summary>
        public static void UVOpen(bool isNeedManualStop)
        {
            LogHelper.Info((string)Application.Current.FindResource("UVAvailable"), "");
            DoCmd("AU1", isNeedManualStop);
        }

        /// <summary>
        /// 紫外灯关闭
        /// <param name="isNeedManualStop">是否需要手动停止功能</param>
        /// </summary>
        public static void UVClose(bool isNeedManualStop)
        {
            LogHelper.Info((string)Application.Current.FindResource("UVUnavailable"), "");
            DoCmd("AU0", isNeedManualStop);
        }

        /// <summary>
        /// 制冷打开
        /// <param name="isNeedManualStop">是否需要手动停止功能</param>
        /// </summary>
        public static void CoolOpen(bool isNeedManualStop)
        {
            LogHelper.Info((string)Application.Current.FindResource("CoolAvailable"), "");
            DoCmd("AO51", isNeedManualStop);
        }

        /// <summary>
        /// 制冷关闭
        /// <param name="isNeedManualStop">是否需要手动停止功能</param>
        /// </summary>
        public static void CoolClose(bool isNeedManualStop)
        {
            LogHelper.Info((string)Application.Current.FindResource("CoolUnavailable"), "");
            DoCmd("AO50", isNeedManualStop);
        }

        /// <summary>
        /// 空气阀和空气泵打开
        /// <param name="isNeedManualStop">是否需要手动停止功能</param>
        /// </summary>
        public static void ValveAndPumpOpen(bool isNeedManualStop)
        {
            LogHelper.Info((string)Application.Current.FindResource("ValveAndPumpAvailable"), "");
            DoCmd("OO11,OO21", isNeedManualStop);
        }

        /// <summary>
        /// 空气阀和空气泵关闭
        /// <param name="isNeedManualStop">是否需要手动停止功能</param>
        /// </summary>
        public static void ValveAndPumpClose(bool isNeedManualStop)
        {
            LogHelper.Info((string)Application.Current.FindResource("ValveAndPumpUnavailable"), "");
            DoCmd("OO10,OO20", isNeedManualStop);
        }

        /// <summary>
        /// 检测试管是否存在
        /// </summary>
        /// <param name="isNeedManualStop"></param>
        public static void CheckTubeExist(bool isNeedManualStop)
        {
            LogHelper.Info((string)Application.Current.FindResource("CheckTubeExist"), "");
            DoCmd("AA1", isNeedManualStop);
        }

        /// <summary>
        /// 执行指令（指令执行入口，免除了繁琐的ManualStop调用）
        /// </summary>
        /// <param name="cmd"></param>
        /// <param name="isNeedManualStop">是否需要手动停止功能（用于仓门）</param>
        /// <param name="intercept">如果启用了门，是否需要拦截操作</param>
        /// <param name="waitAnswer">是否等待下位机执行完毕应答</param>
        /// <returns></returns>
        public static bool DoCmd(string cmd, bool isNeedManualStop, bool intercept = true, bool waitAnswer = true)
        {
            var cmdTimeOut = CMD_TIMEOUT;

            if (cmd.Equals(""))
                return true;

            LogHelper.Info((string)Application.Current.FindResource("Cmd"), cmd);

            if (!MainWindow.isCanNetSuccess || frmDAE.socketSend == null || !frmDAE.socketSend.Connected)
            {
                Thread.Sleep(5);
                if (isNeedManualStop)
                    ManualStop(intercept);
                return true;
            }

            // 已经过去的时间
            var timePassedMS = 0;
            // var result = false;
            // new Thread(() =>
            // {
            //     result = frmDAE.DoCmd(cmd, waitAnswer);
            // }).Start();
            //
            // while (timePassedMS < cmdTimeOut)
            // {
            //     // 指令被拦截了
            //     if (MainWindow.mIsEmergencyStopIntercept && intercept)
            //     {
            //         LogHelper.Info("被拦截的指令：", cmd);
            //         MainWindow.mEmergencyStopLastCmd = cmd;
            //         break;
            //     }
            //
            //     if (result)
            //         break;
            //
            //     Thread.Sleep(10);
            //     timePassedMS += 10;
            // }
            // 2026-03-04：修改 DoCmd 中的线程部分，避免频繁new Thread 导致的性能问题和资源占用过多问题（尤其是在执行大量指令时），改为使用 Task.Run 来执行指令，并通过共享变量来获取结果，同时保留超时和拦截功能。
            var resultHolder = new bool[1]; // 用数组包装值类型以供闭包捕获
            var task = Task.Run(() =>
            {
                resultHolder[0] = frmDAE.DoCmd(cmd, waitAnswer);
            });

            while (timePassedMS < cmdTimeOut)
            {
                if (MainWindow.mIsEmergencyStopIntercept && intercept)
                {
                    LogHelper.Info("被拦截的指令：", cmd);
                    MainWindow.mEmergencyStopLastCmd = cmd;
                    break;
                }
                if (resultHolder[0])
                    break;
                Thread.Sleep(10);
                timePassedMS += 10;
            }
            var result = resultHolder[0];

            // 超时
            if (!MainWindow.mIsEmergencyStopIntercept)
            {
                if (!result)
                {
                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        if (MessageBox.Show((string)Application.Current.FindResource("Prompt_Cmd_TimeOut_1") + cmd + (string)Application.Current.FindResource("Prompt_Cmd_TimeOut_2"),
                                (string)Application.Current.FindResource("Prompt"), MessageBoxButton.YesNo, MessageBoxImage.Warning) ==
                            MessageBoxResult.No)
                        {
                            throw new ManualStopException();
                        }
                    });
                    // 打开红指示灯
                    CmdHelper.LightOn(ELight.Red, false);
                }
            }

            if (isNeedManualStop)
                ManualStop(intercept);

            return result;
        }

        /// <summary>
        /// 手动停止
        /// </summary>
        /// <param name="intercept">如果启用了门，是否需要拦截操作</param>
        public static void ManualStop(bool intercept = true)
        {
            while (isManualPause)
            {
                if (isManualStop)
                {
                    throw new ManualStopException();
                }

                try
                {
                    Thread.Sleep(10);
                }
                catch (Exception e)
                {
                }
            }

            if (intercept)
            {
                // 恢复执行上一条被拦截的指令
                if (MainWindow.mIsEmergencyStopClosedAgain)
                {
                    MainWindow.mIsEmergencyStopClosedAgain = false;
                    LogHelper.Info("执行被拦截的指令：", MainWindow.mEmergencyStopLastCmd);
                    DoCmd(MainWindow.mEmergencyStopLastCmd, true);
                }
            }
        }
    }
}
