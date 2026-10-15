using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using AutoLiquid_Library.Enum;

namespace AutoLiquid_Library.Utils
{
    /// <summary>
    /// Excel处理类
    /// </summary>
    public class ExcelHelper
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="wallExcelStr"></param>
        /// <returns></returns>
        public static List<EWall> GetWallList(string wallExcelStr)
        {
            var wallList = new List<EWall>();
            string[] itemArray = wallExcelStr.Split(new[] { ',', '，' }, StringSplitOptions.RemoveEmptyEntries);
            foreach (var item in itemArray)
            {
                if (item.Equals("左"))
                    wallList.Add(EWall.Left);
                else if (item.Equals("右"))
                    wallList.Add(EWall.Right);
                else if (item.Equals("前"))
                    wallList.Add(EWall.Front);
                else if (item.Equals("后"))
                    wallList.Add(EWall.Back);
            }

            return wallList;
        }
    }
}
