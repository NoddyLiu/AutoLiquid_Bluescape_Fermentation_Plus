using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;

namespace AutoLiquid_Library.Utils
{
    /// <summary>
    /// 对象通用工具类
    /// </summary>
    public class ObjectUtils
    {
        /// <summary>
        /// 深拷贝（不会共用对象的引用）
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="other"></param>
        /// <returns></returns>
        public static T DeepCopy<T>(T other)
        {
            using (MemoryStream ms = new MemoryStream())
            {
                BinaryFormatter formatter = new BinaryFormatter();
                formatter.Context = new StreamingContext(StreamingContextStates.Clone);
                formatter.Serialize(ms, other);
                ms.Position = 0;
                return (T)formatter.Deserialize(ms);
            }
        }

        /// <summary>
        /// 只保留字符串的字母数字，并转为大写
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public static string GetAlphanumericToUpperOnly(string str)
        {
            return Regex.Replace(str, "[^a-zA-Z0-9]", "").Replace(".", "").ToUpper();
        }

        /// <summary>
        /// 根据标记获取特殊指令
        /// </summary>
        /// <param name="cmd"></param>
        /// <param name="cmdTag"></param>
        /// <param name="isTxtLink"></param>
        /// <returns></returns>
        public static string GetCmdAccordTag(string cmd, string cmdTag, bool isTxtLink)
        {
            // cmd是否有多行
            string[] lineCmd = cmd.Split(new[] { "\r\n" }, StringSplitOptions.RemoveEmptyEntries);

            foreach (var t in lineCmd)
            {
                string[] cmdOneLines = t.Split(new[] { ';', '；' }, StringSplitOptions.RemoveEmptyEntries);

                if (cmdOneLines.Length == 1)
                {
                    if (cmdOneLines[0].ToLower().Contains(cmdTag.ToLower()))
                    {
                        if (isTxtLink)
                            return cmdOneLines[0].Split(new[] { ':', '：' }, StringSplitOptions.RemoveEmptyEntries).ElementAt(1);
                        return cmdOneLines[0].Split(new[] { ';', '；' }, StringSplitOptions.RemoveEmptyEntries).ElementAt(0).Split(new[] { ':', '：' }, StringSplitOptions.RemoveEmptyEntries).ElementAt(1);
                    }
                }
                else if (cmdOneLines.Length > 1)
                {
                    foreach (var cmdSingle in cmdOneLines)
                    {
                        if (cmdSingle.ToLower().Contains(cmdTag.ToLower()))
                            return cmdSingle.Split(new[] { ':', '：' }, StringSplitOptions.RemoveEmptyEntries).ElementAt(1);
                    }
                }
            }
            return "";
        }

        /// <summary>
        /// 根据百分比计算速度指令
        /// </summary>
        /// <param name="initCmd"></param>
        /// <param name="percent"></param>
        /// <returns></returns>
        public static string CalcSpeedCmdValueByPercent(string initCmd, decimal percent)
        {
            if (initCmd.Equals(""))
                return "";

            // 输出指令
            var outCmdStr = "";
            if (percent >= 10.0m)
            {
                // 输出指令
                outCmdStr = initCmd.Substring(0, 2);
                // 数值指令
                var valueStr = initCmd.Substring(2).Trim();
                // 以空格分隔数值
                var valueArray = valueStr.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                // 重组指令
                foreach (var value in valueArray)
                {
                    outCmdStr += " " + Convert.ToInt32(Int32.Parse(value) * percent * 0.01m);
                }
            }

            return outCmdStr.Trim();
        }

        /// <summary>
        /// 根据子list内容个数拆分列表
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="source"></param>
        /// <param name="size"></param>
        /// <returns></returns>
        public static List<List<T>> SplitList<T>(List<T> source, int size)
        {
            var list = new List<List<T>>();

            for (int i = 0; i < source.Count; i += size)
            {
                if (i + size < source.Count)
                    list.Add(source.GetRange(i, size));
                // 最后的超出范围，要截取
                else
                {
                    var lastSize = source.Count - i;
                    list.Add(source.GetRange(i, lastSize));
                }

            }

            return list;
        }

        /// <summary>
        /// 二维数组数值填充
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="arr"></param>
        /// <param name="value"></param>
        public static void Fill2DArray<T>(T[,] arr, T value)
        {
            int numRows = arr.GetLength(0);
            int numCols = arr.GetLength(1);

            for (int i = 0; i < numRows; ++i)
            {
                for (int j = 0; j < numCols; ++j)
                {
                    arr[i, j] = value;
                }
            }
        }

        /// <summary>
        /// 从Enum中获取描述Description
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static string GetDescriptionFromEnum(System.Enum value)
        {
            DescriptionAttribute attribute = value.GetType()
                .GetField(value.ToString())
                .GetCustomAttributes(typeof(DescriptionAttribute), false)
                .SingleOrDefault() as DescriptionAttribute;
            return attribute == null ? value.ToString() : attribute.Description;
        }

        /// <summary>
        /// 从描述Description获取
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="description"></param>
        /// <returns></returns>
        public static T GetEnumFromDescription<T>(string description)
        {
            var type = typeof(T);
            if (!type.IsEnum)
                throw new ArgumentException();
            FieldInfo[] fields = type.GetFields();
            var field = fields
                .SelectMany(f => f.GetCustomAttributes(
                    typeof(DescriptionAttribute), false), (
                    f, a) => new { Field = f, Att = a }).SingleOrDefault(a => ((DescriptionAttribute)a.Att)
                                                                              .Description == description);
            return field == null ? default(T) : (T)field.Field.GetRawConstantValue();
        }
        
        /// <summary>
        /// List交换位置
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="list"></param>
        /// <param name="indexA"></param>
        /// <param name="indexB"></param>
        public static void Swap<T>(IList<T> list, int indexA, int indexB)
        {
            T tmp = list[indexA];
            list[indexA] = list[indexB];
            list[indexB] = tmp;
        }
    }
}
