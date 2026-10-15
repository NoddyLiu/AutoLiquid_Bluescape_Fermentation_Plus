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
    /// 调试参数
    /// </summary>
    [Serializable()]
    public class Debug
    {
        /**
         * 微调参数
         */
        // 是否微调（否：粗调）
        public bool IsThin = true;
        // 微调步距
        public decimal StepThin = 5m;
        // 粗调步距
        public decimal StepThick = 10m;
    }
}
