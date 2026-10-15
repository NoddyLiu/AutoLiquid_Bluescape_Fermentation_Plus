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
    /// 错误 model
    /// </summary>
    public class Error
    {
        // 是否有误
        public bool Available = false;

        // 错误位置
        public int Index = -1;

        // 错误信息
        public string Info = "";
    }
}
