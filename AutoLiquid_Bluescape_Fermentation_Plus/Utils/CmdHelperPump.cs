using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using AutoLiquid_Bluescape_Fermentation_Plus;
using AutoLiquid_Bluescape_Fermentation_Plus.EntityCommon;
using AutoLiquid_Bluescape_Fermentation_Plus.EntityJson;
using AutoLiquid_Library.Enum;
using AutoLiquid_Library.Exceptions;
using AutoLiquid_Library.Utils;
using ControlzEx.Standard;

namespace AutoLiquid_Bluescape_Fermentation_Plus.Utils
{
    /// <summary>
    /// 加液泵指令帮助类
    /// </summary>
    public class CmdHelperPump
    {
        // 通信窗体
        public static DAERun.FrmMain frmDAE = new DAERun.FrmMain("192.168.1.4", 4000, "workplate_pump.xml");

        // 清洗液注入、排出轴列表
        public static List<string> CLEANING_FILL_AXIS_LIST = new List<string> { "W" };
        public static List<string> CLEANING_DRAIN_AXIS_LIST = new List<string> { "Q" };

        // 指令超时时间
        private static long CMD_TIMEOUT = 120000;
        // 指令超时时间（离线调试模式）
        private static long CMD_TIMEOUT_TEST = 2000;

        // /// <summary>
        // /// 注入清洗液
        // /// <param name="isNeedManualStop">是否需要手动停止功能</param>
        // ///  <param name="waitAnswer">是否等待下位机执行完毕应答</param>
        // /// </summary>
        // public static void CleaningLiquidFill(decimal volume, bool isNeedManualStop, bool waitAnswer = true)
        // {
        //     LogHelper.Info((string)Application.Current.FindResource("CleaningLiquidFill"), "");
        //     // 先复位泵
        //     DoCmd(CLEANING_FILL_AXIS_LIST[0] + "I0", isNeedManualStop);
        //     DoCmd(CLEANING_FILL_AXIS_LIST[0] + "S" + volume.ToString("F3"), isNeedManualStop, waitAnswer);
        // }
        //
        // /// <summary>
        // /// 排出清洗液
        // /// <param name="isNeedManualStop">是否需要手动停止功能</param>
        // /// <param name="waitAnswer">是否等待下位机执行完毕应答</param>
        // /// </summary>
        // public static void CleaningLiquidDrain(decimal volume, bool isNeedManualStop, bool waitAnswer = true)
        // {
        //     LogHelper.Info((string)Application.Current.FindResource("CleaningLiquidDrain"), "");
        //     // 先复位泵
        //     DoCmd(CLEANING_DRAIN_AXIS_LIST[0] + "I0", isNeedManualStop);
        //     DoCmd(CLEANING_DRAIN_AXIS_LIST[0] + "S" + volume.ToString("F3"), isNeedManualStop, waitAnswer);
        // }
        //
        // /// <summary>
        // /// 设置轴速度（可复位该轴速度）
        // /// <param name="headUsedIndex">移液头Index</param>
        // /// <param name="axis">轴类型</param>
        // /// <param name="speedCmd">速度指令（可不用填写轴前缀，如“FX”、“XF”等，且默认执行可刷写速度，如“XF”、“YF”等。如果为空，则单纯复位该轴速度）</param>
        // /// <param name="speedPercent">速度百分比</param>
        // /// <param name="isNeedManualStop">是否需要手动停止功能</param>
        // /// </summary>
        // public static void SpeedSet(int headUsedIndex, EAxis axis, string speedCmd, decimal speedPercent, bool isNeedManualStop)
        // {
        //     var commonSetting = ParamsHelper.CommonSettingList[headUsedIndex];
        //
        //     if (axis == EAxis.All)
        //     {
        //         SpeedSetSub(headUsedIndex, EAxis.W, CLEANING_FILL_AXIS_LIST[headUsedIndex] + "F 0", commonSetting.DefaultWSpeed, commonSetting.WSpeedPercent, isNeedManualStop);
        //         SpeedSetSub(headUsedIndex, EAxis.Q, CLEANING_DRAIN_AXIS_LIST[headUsedIndex] + "F 0", commonSetting.DefaultQSpeed, commonSetting.QSpeedPercent, isNeedManualStop);
        //     }
        // }

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

                if (axis == EAxis.W)
                    speedCmd = CLEANING_FILL_AXIS_LIST[headUsedIndex] + "F " + speedCmd;
                else if (axis == EAxis.Q)
                    speedCmd = CLEANING_DRAIN_AXIS_LIST[headUsedIndex] + "F " + speedCmd;

                var cmd = ObjectUtils.CalcSpeedCmdValueByPercent(speedCmd, percent);
                DoCmd(cmd, isNeedManualStop);
            }
        }

        /// <summary>
        /// 执行指令（指令执行入口，免除了繁琐的ManualStop调用）
        /// </summary>
        /// <param name="cmd"></param>
        /// <param name="isNeedManualStop">是否需要手动停止功能（用于仓门）</param>
        /// <param name="waitAnswer">是否等待下位机执行完毕应答</param>
        /// <returns></returns>
        public static bool DoCmd(string cmd, bool isNeedManualStop, bool waitAnswer = true)
        {
            var cmdTimeOut = CMD_TIMEOUT;

            if (cmd.Equals(""))
                return true;

            LogHelper.Info((string)Application.Current.FindResource("Cmd"), cmd);

            if (!MainWindow.isCanNetSuccess || frmDAE.socketSend == null || !frmDAE.socketSend.Connected)
            {
                Thread.Sleep(50);
                if (isNeedManualStop)
                    ManualStop(cmd);
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
                ManualStop(cmd);

            return result;
        }

        /// <summary>
        /// 手动停止
        /// </summary>
        /// <param name="currentCmd">上一条可能被拦截的指令</param>
        public static void ManualStop(string currentCmd)
        {
            while (CmdHelper.isManualPause)
            {
                if (CmdHelper.isManualStop)
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

            // 恢复执行上一条被拦截的指令
            if (MainWindow.mIsEmergencyStopClosedAgain)
            {
                MainWindow.mIsEmergencyStopClosedAgain = false;
                DoCmd(currentCmd, true);
            }
        }
    }
}
