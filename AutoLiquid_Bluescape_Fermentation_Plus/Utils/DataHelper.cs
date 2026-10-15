using AutoLiquid_Library.Enum;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Threading;

namespace AutoLiquid_Bluescape_Fermentation_Plus.Utils
{
    /// <summary>
    /// 数据处理帮助类
    /// </summary>
    public class DataHelper
    {
        /// <summary>
        /// 保存整数值
        /// </summary>
        /// <param name="headIndex">移液头Index</param>
        /// <param name="inputValue">保存的值</param>
        /// <param name="param">Json属性</param>
        /// <param name="saveAction">保存方法</param>
        public static void SaveInt(int headIndex, string inputValue, ref int param, Action saveAction = null)
        {
            // 避免输入负号出现异常
            if (inputValue.Equals("") || inputValue.Equals("-"))
                return;

            var result = int.TryParse(inputValue, out var outputValue);
            if (result)
            {
                param = outputValue;
                if (saveAction == null)
                    FileUtils.SaveCommonSettings(headIndex, ParamsHelper.CommonSettingList[headIndex]);
                else
                    saveAction.Invoke();
            }
            else
            {
                // MessageBox要在主线程运行，否则报错
                Dispatcher.CurrentDispatcher.Invoke(() =>
                {
                    MessageBox.Show((string)Application.Current.FindResource("Prompt_Data_Input_Error"));
                });
            }
        }

        /// <summary>
        /// 保存小数值
        /// </summary>
        /// <param name="headIndex">移液头Index</param>
        /// <param name="inputValue">保存的值</param>
        /// <param name="param">Json属性</param>
        /// <param name="saveAction">保存方法</param>
        public static void SaveDecimal(int headIndex, string inputValue, ref decimal param, Action saveAction = null)
        {
            // 避免输入负号出现异常
            if (inputValue.Equals("") || inputValue.Equals("-"))
                return;

            var result = decimal.TryParse(inputValue, out var outputValue);
            if (result)
            {
                param = outputValue;
                if (saveAction == null)
                    FileUtils.SaveCommonSettings(headIndex, ParamsHelper.CommonSettingList[headIndex]);
                else
                    saveAction.Invoke();
            }
            else
            {
                // MessageBox要在主线程运行，否则报错
                Dispatcher.CurrentDispatcher.Invoke(() =>
                {
                    MessageBox.Show((string)Application.Current.FindResource("Prompt_Data_Input_Error"));
                });
            }
        }


        /// <summary>
        /// 保存字符串
        /// </summary>
        /// <param name="headIndex">移液头Index</param>
        /// <param name="inputValue">保存的值</param>
        /// <param name="param">Json属性</param>
        /// <param name="saveAction">保存方法</param>
        public static void SaveString(int headIndex, string inputValue, ref string param, Action saveAction = null)
        {
            param = inputValue;
            if (saveAction == null)
                FileUtils.SaveCommonSettings(headIndex, ParamsHelper.CommonSettingList[headIndex]);
            else
                saveAction.Invoke();
        }

        /// <summary>
        /// 保存布尔值
        /// </summary>
        /// <param name="headIndex">移液头Index</param>
        /// <param name="inputValue">保存的值</param>
        /// <param name="param">Json属性</param>
        /// <param name="saveAction">保存方法</param>
        public static void SaveBool(int headIndex, bool inputValue, ref bool param, Action saveAction = null)
        {
            param = inputValue;
            if (saveAction == null)
                FileUtils.SaveCommonSettings(headIndex, ParamsHelper.CommonSettingList[headIndex]);
            else
                saveAction.Invoke();
        }

        /// <summary>
        /// 保存量程值
        /// </summary>
        /// <param name="inputValue">保存的值</param>
        /// <param name="param">Json属性</param>
        /// <param name="saveAction">保存方法</param>
        public static void SaveLiquidRange(ELiquidRange inputValue, ref ELiquidRange param, Action saveAction)
        {
            param = inputValue;
            saveAction.Invoke();
        }

        /// <summary>
        /// 保存盘位摆放方向
        /// </summary>
        /// <param name="inputValue">保存的值</param>
        public static void SaveA1Pos(EA1Pos inputValue)
        {
            ParamsHelper.CommonSettingList[0].A1Pos = inputValue;
            FileUtils.SaveCommonSettings(0, ParamsHelper.CommonSettingList[0]);
            ParamsHelper.CommonSettingList[1].A1Pos = inputValue;
            FileUtils.SaveCommonSettings(1, ParamsHelper.CommonSettingList[1]);
        }
    }
}
