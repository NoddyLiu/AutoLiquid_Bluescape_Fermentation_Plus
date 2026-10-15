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
    /// 一吸多喷（每次）model
    /// </summary>
    public class One2More
    {
        // 单次吸液体积
        public decimal SourceVolumeAbsorbEach;

        // 靶孔喷液体积
        public List<decimal> TargetVolumeJetListEach = new List<decimal>();

        // 靶盘index
        public List<int> TargetTemplateIndexListEach = new List<int>();

        // 靶孔index
        public List<HoleIndex> TargetHoleIndexListEach = new List<HoleIndex>();
    }
}
