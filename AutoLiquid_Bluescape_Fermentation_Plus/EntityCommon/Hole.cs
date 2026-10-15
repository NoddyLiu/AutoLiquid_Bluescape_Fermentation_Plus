using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Shapes;

namespace AutoLiquid_Bluescape_Fermentation_Plus.EntityCommon
{
    public class Hole
    {
        // 位置Index
        public int Index;

        // 板位中的圆
        public Ellipse Circle;

        // 板位中的容量文字
        public Label Word;

        // 容量
        public decimal Capacity;
    }
}
