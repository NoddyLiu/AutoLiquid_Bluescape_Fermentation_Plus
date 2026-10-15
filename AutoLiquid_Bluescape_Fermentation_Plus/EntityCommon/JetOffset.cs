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
    /// 喷液量偏移补偿 model
    /// </summary>
    public class JetOffset
    {
        // 位置Index
        public int PosIndex;

        // 靶孔喷液体积
        public decimal VolumeOffset = 0m;
    }
}
