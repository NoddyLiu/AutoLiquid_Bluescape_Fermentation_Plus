using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Shapes;
using AutoLiquid_Bluescape_Fermentation_Plus.EntityJson;
using AutoLiquid_Library.Enum;

namespace AutoLiquid_Bluescape_Fermentation_Plus.EntityCommon
{
    public class Template
    {
        // 板标题信息
        public string Title = "";

        // 板标题副信息
        public string SubTitle = "";

        // 板类型
        public ETemplateType Type = ETemplateType.Source;

        // 行列数
        public int RowCount = 0;
        public int ColCount = 0;
        // 孔距（暂时用在识别是否384孔：4.5m）
        public Position Step = new Position { X = 9.0m, Y = 9.0m };

        // 是否错位布局（如250ml发酵瓶：8行3列，偶数行X方向向左偏移半个孔距（B、D、F...））
        public bool Staggered = false;

        // A1孔位置
        public EA1Pos A1Pos = EA1Pos.LeftTop;

        // 孔
        public List<Hole> Holes = new List<Hole>();
    }
}
