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
    /// <summary>
    /// 移液头状态信息
    /// </summary>
    public class HeadStatus
    {
        // 执行到哪一步
        public EHeadStatus Head = EHeadStatus.Null;

        // 执行到程序链的哪一个Seq
        public int SeqIndex = 0;

        /**
         * 源盘信息
         */
        // 耗材
        public Consumable SourceConsumableType;
        // 盘位Index
        public int SourceTemplateIndex;
        // 孔位Index
        public HoleIndex SourceHoleIndex;
    }
}
