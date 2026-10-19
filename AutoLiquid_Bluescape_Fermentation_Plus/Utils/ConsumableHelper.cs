using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using AutoLiquid_Bluescape_Fermentation_Plus.EntityCommon;
using AutoLiquid_Bluescape_Fermentation_Plus.EntityJson;
using AutoLiquid_Library.Enum;
using Path = System.Windows.Shapes.Path;

namespace AutoLiquid_Bluescape_Fermentation_Plus.Utils
{
    /// <summary>
    /// 耗材帮助类
    /// </summary>
    public class ConsumableHelper
    {
        /// <summary>
        /// 根据组名找出参数组
        /// </summary>
        /// <param name="headIndex">移液头Index</param>
        /// <param name="consumableTypeName"></param>
        /// <returns></returns>
        public static Consumable GetConsumableType(int headIndex, string consumableTypeName)
        {
            Consumable cGroup;
            cGroup = ParamsHelper.CommonSettingList[headIndex].Consumables.FirstOrDefault(x => x.GroupName.Equals(consumableTypeName));
            return cGroup;
        }

        /// <summary>
        /// 根据Index找出参数组
        /// </summary>
        /// <param name="headIndex"></param>
        /// <param name="consumableIndex"></param>
        /// <returns></returns>
        public static Consumable GetConsumableType(int headIndex, int consumableIndex)
        {
            Consumable cGroup;
            if (consumableIndex < ParamsHelper.CommonSettingList[headIndex].Consumables.Count)
                cGroup = ParamsHelper.CommonSettingList[headIndex].Consumables[consumableIndex];
            else
                cGroup = ParamsHelper.CommonSettingList[headIndex].Consumables[0]; ;
            return cGroup;
        }

        /// <summary>
        /// 获取孔Index（index规则：A1~H1 -> 0~8）
        /// </summary>
        /// <param name="headIndex">移液头Index</param>
        /// <param name="consumableType">耗材类型</param>
        /// <param name="holeIndexStr">孔字符，如A1、B3等</param>
        /// <returns></returns>
        public static HoleIndex GetHoleIndex(int headIndex, Consumable consumableType, string holeIndexStr)
        {
            var count = ParamsHelper.CommonSettingList[headIndex].A1Pos == EA1Pos.LeftTop ? consumableType.RowCount : consumableType.ColCount;
            var posLetter = holeIndexStr.Substring(0, 1);
            var posNum = holeIndexStr.Substring(1);
            var result = (Convert.ToInt32(posNum) - 1) * count + (posLetter.ToCharArray()[0] - 65);
            return new HoleIndex { OriIndex = result };
        }

        /// <summary>
        /// 获取孔名（如A1、B2等）
        /// </summary>
        /// <param name="headIndex"></param>
        /// <param name="consumableType"></param>
        /// <param name="holeIndex"></param>
        /// <returns></returns>
        public static string GetHoleString(int headIndex, Consumable consumableType, HoleIndex holeIndex)
        {
            var posLetter = ParamsHelper.CommonSettingList[headIndex].A1Pos == EA1Pos.LeftTop ?
                ViewUtils.PosLetterList[holeIndex.OriIndex % consumableType.RowCount]
                : ViewUtils.PosLetterList[holeIndex.OriIndex % consumableType.ColCount];
            var posNum = ParamsHelper.CommonSettingList[headIndex].A1Pos == EA1Pos.LeftTop ?
                ViewUtils.PosNumList[holeIndex.OriIndex / consumableType.RowCount]
                : ViewUtils.PosNumList[holeIndex.OriIndex / consumableType.ColCount];

            return posLetter + posNum;
        }

        /// <summary>
        /// 获取孔数目
        /// </summary>
        /// <param name="fromHoleIndex"></param>
        /// <param name="toHoleIndex"></param>
        /// <returns></returns>
        public static int GetHoleCount(HoleIndex fromHoleIndex, HoleIndex toHoleIndex)
        {
            return toHoleIndex.OriIndex - fromHoleIndex.OriIndex + 1;
        }

        /// <summary>
        /// 获取孔位List
        /// </summary>
        /// <param name="fromHoleIndex"></param>
        /// <param name="toHoleIndex"></param>
        /// <returns></returns>
        public static List<HoleIndex> GetHoleIndexList(HoleIndex fromHoleIndex, HoleIndex toHoleIndex)
        {
            var list = new List<HoleIndex>();
            for (var i = fromHoleIndex.OriIndex; i <= toHoleIndex.OriIndex; i++)
            {
                list.Add(new HoleIndex { OriIndex = i });
            }
            return list;
        }

        /// <summary>
        /// 错位布局下，按视觉列计算实际孔数（与 WindowHoleSelect.SelectHoles 高亮逻辑保持一致）。
        /// 同视觉列 → 返回视觉列孔数；跨视觉列 → 返回整段孔数；非错位 → 范围孔数。
        /// </summary>
        public static int GetHoleCount(HoleIndex fromHoleIndex, HoleIndex toHoleIndex, Consumable consumable)
        {
            int from = fromHoleIndex.OriIndex;
            int to = toHoleIndex.OriIndex;
            if (from < 0 || to < 0) return to - from + 1;
            if (consumable == null || !consumable.Staggered) return to - from + 1;

            int rowCount = consumable.RowCount;
            int fromKey = (from / rowCount) * 2 + (from % rowCount) % 2;
            int toKey = (to / rowCount) * 2 + (to % rowCount) % 2;

            if (fromKey != toKey) return to - from + 1;

            int count = 0;
            for (int i = from; i <= to; i++)
            {
                int key = (i / rowCount) * 2 + (i % rowCount) % 2;
                if (key == fromKey) count++;
            }
            return count;
        }

        /// <summary>
        /// 错位布局下，只返回同视觉列的孔位列表（移液序列生成用）
        /// </summary>
        public static List<HoleIndex> GetHoleIndexList(HoleIndex fromHoleIndex, HoleIndex toHoleIndex, Consumable consumable)
        {
            var list = new List<HoleIndex>();
            int from = fromHoleIndex.OriIndex;
            int to = toHoleIndex.OriIndex;
            if (from < 0 || to < 0) return list;
            if (consumable == null || !consumable.Staggered)
            {
                for (int i = from; i <= to; i++)
                    list.Add(new HoleIndex { OriIndex = i });
                return list;
            }

            int rowCount = consumable.RowCount;
            int fromKey = (from / rowCount) * 2 + (from % rowCount) % 2;
            int toKey = (to / rowCount) * 2 + (to % rowCount) % 2;

            if (fromKey != toKey)
            {
                for (int i = from; i <= to; i++)
                    list.Add(new HoleIndex { OriIndex = i });
                return list;
            }

            for (int i = from; i <= to; i++)
            {
                int key = (i / rowCount) * 2 + (i % rowCount) % 2;
                if (key == fromKey)
                    list.Add(new HoleIndex { OriIndex = i });
            }
            return list;
        }
    }
}
