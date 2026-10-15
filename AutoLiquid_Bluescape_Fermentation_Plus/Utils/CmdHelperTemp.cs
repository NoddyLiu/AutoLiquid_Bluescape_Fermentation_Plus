using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using AutoLiquid_Library.Enum;
using AutoLiquid_Library.Exceptions;
using AutoLiquid_Library.Utils;

namespace AutoLiquid_Bluescape_Fermentation_Plus.Utils
{
    /// <summary>
    /// 温度、搅拌等指令帮助类
    /// </summary>
    public class CmdHelperTemp
    {
        // 通信窗体
        public static DAERun.FrmMain frmDAE = new DAERun.FrmMain("192.168.1.3", 4000, "");
        
        // 指令超时时间
        private static long CMD_TIMEOUT = 30000;
        // 指令超时时间（离线调试模式）
        private static long CMD_TIMEOUT_TEST = 2000;

        /// <summary>
        /// 搅拌开始（默认常关，所以开发1，关发0）
        /// </summary>
        /// <param name="num">转子序号：1~4；5：全部</param>
        /// <param name="isNeedManualStop"></param>
        /// <returns></returns>
        public static bool ShakeStart(int num, bool isNeedManualStop)
        {
            var result = true;
            switch (num)
            {
                case 1:
                    result = DoCmd("OO21", isNeedManualStop);
                    break;
                case 2:
                    result = DoCmd("OO31", isNeedManualStop);
                    break;
                case 3:
                    result = DoCmd("OO41", isNeedManualStop);
                    break;
                case 4:
                    result = DoCmd("OO51", isNeedManualStop);
                    break;
                case 5:
                    result = DoCmd("OO21,OO31,OO41,OO51", isNeedManualStop);
                    break;
            }

            return result;
        }

        /// <summary>
        /// 搅拌停止
        /// </summary>
        /// <param name="num">转子序号：1~4；5：全部</param>
        /// <param name="isNeedManualStop"></param>
        /// <returns></returns>
        public static bool ShakeStop(int num, bool isNeedManualStop)
        {
            var result = true;
            switch (num)
            {
                case 1:
                    result = DoCmd("OO20", isNeedManualStop);
                    break;
                case 2:
                    result = DoCmd("OO30", isNeedManualStop);
                    break;
                case 3:
                    result = DoCmd("OO40", isNeedManualStop);
                    break;
                case 4:
                    result = DoCmd("OO50", isNeedManualStop);
                    break;
                case 5:
                    result = DoCmd("OO20,OO30,OO40,OO50", isNeedManualStop);
                    break;
            }

            return result;
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
        
            if (!MainWindow.isCanNetSuccess || frmDAE.socketSend == null)
            {
                Thread.Sleep(50);
                if (isNeedManualStop)
                    ManualStop(intercept);
                return true;
            }
        
            // 已经过去的时间
            var timePassedMS = 0;
            var result = false;
            new Thread(() =>
            {
                result = frmDAE.DoCmd(cmd, waitAnswer);
            }).Start();
            
            while (timePassedMS < cmdTimeOut)
            {
                // 指令被拦截了
                if (MainWindow.mIsEmergencyStopIntercept && intercept)
                {
                    LogHelper.Info("被拦截的指令：", cmd);
                    MainWindow.mEmergencyStopLastCmd = cmd;
                    break;
                }
            
                if (result)
                    break;
            
                Thread.Sleep(10);
                timePassedMS += 10;
            }

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
        /// <summary>
        /// 手动停止
        /// </summary>
        /// <param name="intercept">如果启用了门，是否需要拦截操作</param>
        public static void ManualStop(bool intercept = true)
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
