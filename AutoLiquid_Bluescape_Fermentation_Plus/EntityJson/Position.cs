using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Documents;

namespace AutoLiquid_Bluescape_Fermentation_Plus.EntityJson
{
    /// <summary>
    /// 坐标
    /// </summary>
    [Serializable()]
    public class Position
    {
        // x坐标
        public decimal X = 0.00m;

        // y坐标
        public decimal Y = 0.00m;

        // z坐标
        public decimal Z = 0.00m;

        // 预备z坐标
        public decimal PreZ = 0.00m;
    }
}
