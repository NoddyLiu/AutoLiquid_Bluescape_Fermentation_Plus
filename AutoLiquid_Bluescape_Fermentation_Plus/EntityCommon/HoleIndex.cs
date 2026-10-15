using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Shapes;

namespace AutoLiquid_Bluescape_Fermentation_Plus.EntityCommon
{
    /// <summary>
    /// 孔位Index
    /// </summary>
    [Serializable()]
    public class HoleIndex
    {
        // 原始位置Index
        public int OriIndex = 0;

        /**
         * 以下用于灵活取枪头、吸喷液
         */
        // X轴孔偏移个数（正数为正向偏移，负数为负向偏移）
        public int XHoleOffset = 0;
        // Y轴孔偏移个数（正数为正向偏移，负数为负向偏移）
        public int YHoleOffset = 0;
    }
}
