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
    /// 最大量程设置
    /// </summary>
    [Serializable()]
    public class Range
    {
        // 10毫升
        public decimal Ten = 10m;

        // 20毫升
        public decimal Twenty = 20m;

        // 200毫升
        public decimal TwoHundred = 200m;

        // 1000毫升
        public decimal OneThousand = 1000m;
    }
}
