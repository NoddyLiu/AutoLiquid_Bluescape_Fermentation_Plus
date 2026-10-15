using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using AutoLiquid_Library.Enum;
using AutoLiquid_Bluescape_Fermentation_Plus.EntityCommon;
using AutoLiquid_Bluescape_Fermentation_Plus.EntityJson;
using Newtonsoft.Json;
using Serilog;
using Serilog.Core;
using Serilog.Events;

namespace AutoLiquid_Bluescape_Fermentation_Plus.Utils
{
    /// <summary>
    /// 日志工具类
    /// </summary>
    public class LogHelper
    {
        /// <summary>
        /// 信息日志title
        /// </summary>
        /// <param name="info"></param>
        public static void InfoTitle(string info)
        {
            Log.Information("---------------------------------------------------------------------------------------------------");
            Log.Information("---------------------------------------------------------------------------------------------------");
            Log.Information("---------------------------------------------------------------------------------------------------");
            Log.Information("--------------------------------------" + info + "--------------------------------------");
        }

        /// <summary>
        /// 信息日志title
        /// </summary>
        /// <param name="info"></param>
        public static void InfoTail(string info)
        {
            Log.Information("--------------------------------------" + info + "--------------------------------------");
            Log.Information("---------------------------------------------------------------------------------------------------");
            Log.Information("---------------------------------------------------------------------------------------------------");
            Log.Information("---------------------------------------------------------------------------------------------------");
        }

        /// <summary>
        /// 信息日志
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        public static void Info(string key, string value)
        {
            Log.Information(key +  value);
        }

        /// <summary>
        /// 错误日志
        /// </summary>
        /// <param name="error"></param>
        public static void Error(string error)
        {
            Log.Error(error);
        }
    }
}
