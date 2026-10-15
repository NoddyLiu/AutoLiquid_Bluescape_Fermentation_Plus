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
using System.Windows.Media;
using System.Windows.Threading;

namespace AutoLiquid_Bluescape_Fermentation_Plus.Utils
{
    /// <summary>
    /// 注射泵帮助类
    /// </summary>
    public class PumpHelper
    {
        // 注射泵步数上限
        public const int PUMP_STEP_LIMIT = 6000;
        // 泵最大量程(单位：μl)
        public const decimal RANGE_LIMIT_UL = 10000.0m;   // 默认 10ml


        /// <summary>
        /// 注射泵根据体积计算步数
        /// </summary>
        /// <param name="volumeUl"></param>
        /// <returns></returns>
        public static int GetStepsFromVolume(decimal volumeUl)
        {
            if (volumeUl <= 0 || RANGE_LIMIT_UL <= 0) return 0;
            var maxSteps = PUMP_STEP_LIMIT;
            return (int)((volumeUl / RANGE_LIMIT_UL) * maxSteps);
        }
    }
}
