using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutoLiquid_Library.Enum;

namespace AutoLiquid_Bluescape_Fermentation_Plus.Utils
{
    public class ConstantsUtils
    {
        // 量程最大值
        public static Dictionary<ELiquidRange, decimal> LiquidRangeMaxDic = new Dictionary<ELiquidRange, decimal>
        {
            {ELiquidRange.Ten, ParamsHelper.Range.Ten},
            {ELiquidRange.Twenty, ParamsHelper.Range.Twenty },
            {ELiquidRange.TwoHundred, ParamsHelper.Range.TwoHundred},
            {ELiquidRange.OneThousand, ParamsHelper.Range.OneThousand},
        };
    }
}
