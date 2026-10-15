using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using AutoLiquid_Bluescape_Fermentation_Plus.EntityJson;
using Newtonsoft.Json;

namespace AutoLiquid_Bluescape_Fermentation_Plus.Utils
{
    /// <summary>
    /// 文件操作类
    /// </summary>
    public class FileUtils
    {
        /// <summary>
        /// 获取json
        /// </summary>
        /// <param name="fileName">文件名</param>
        /// <returns></returns>
        public static String GetJsonFromLocalStorage(String fileName)
        {
            FileStream fileStream = null;
            StreamReader streamReader = null;
            String jsonStr = "";

            try
            {
                DirectoryInfo directoryInfo = Directory.CreateDirectory(AppDomain.CurrentDomain.BaseDirectory +
                                                                        Path.DirectorySeparatorChar + AutoLiquid_Library.Utils.ConstantsUtils.SAVE_FOLDER); // 保存在当前程序目录

                fileStream = new FileStream(directoryInfo.FullName + Path.DirectorySeparatorChar + fileName,
                    FileMode.OpenOrCreate);

                using (streamReader = new StreamReader(fileStream))
                {
                    jsonStr = streamReader.ReadToEnd();
                }
            }
            catch (Exception e)
            {
            }
            finally
            {
                streamReader?.Close();
                fileStream?.Close();
            }

            return jsonStr;
        }

        /// <summary>
        /// 保存json
        /// </summary>
        /// <param name="json"></param>
        /// <param name="fileName">文件名</param>
        /// <returns></returns>
        public static bool SaveJsonToLocalStorage(String json, String fileName)
        {
            bool result = true; // 运行结果

            try
            {
                DirectoryInfo directoryInfo = Directory.CreateDirectory(AppDomain.CurrentDomain.BaseDirectory +
                                                                        Path.DirectorySeparatorChar + AutoLiquid_Library.Utils.ConstantsUtils.SAVE_FOLDER); // 保存在当前程序目录

                // 先清空原文件内容，再写入
                File.WriteAllText(directoryInfo.FullName + Path.DirectorySeparatorChar + fileName, String.Empty);
                File.WriteAllText(directoryInfo.FullName + Path.DirectorySeparatorChar + fileName, json);
            }
            catch (Exception e)
            {
                result = false;
            }

            return result;
        }

        /// <summary>
        /// 保存json
        /// </summary>
        /// <param name="json"></param>
        /// <param name="path">用户选择的全路径</param>
        /// <returns></returns>
        public static bool SaveJsonToUserSelectPath(String json, String path)
        {
            bool result = true; // 运行结果

            try
            {
                File.WriteAllText(path, json);
            }
            catch (Exception e)
            {
                result = false;
            }

            return result;
        }


        /// <summary>
        /// 获取json
        /// </summary>
        /// <param name="path">文件名</param>
        /// <returns></returns>
        public static String GetJsonFromUserSelectPath(String path)
        {
            FileStream fileStream = null;
            StreamReader streamReader = null;
            String jsonStr = "";

            try
            {
                fileStream = new FileStream(path, FileMode.OpenOrCreate);

                using (streamReader = new StreamReader(fileStream))
                {
                    jsonStr = streamReader.ReadToEnd();
                }
            }
            catch (Exception e)
            {
            }
            finally
            {
                streamReader?.Close();
                fileStream?.Close();
            }

            return jsonStr;
        }

        /// <summary>
        /// 保存盘位布局
        /// </summary>
        /// <param name="layout"></param>
        public static void SaveLayout(Layout layout)
        {
            FileUtils.SaveJsonToLocalStorage(JsonConvert.SerializeObject(layout, Formatting.Indented), AutoLiquid_Library.Utils.ConstantsUtils.FILE_LAYOUT);
        }

        /// <summary>
        /// 保存通用设置
        /// </summary>
        /// <param name="headIndex"></param>
        /// <param name="common"></param>
        public static void SaveCommonSettings(int headIndex, Common common)
        {
            if (headIndex == 0)
                FileUtils.SaveJsonToLocalStorage(JsonConvert.SerializeObject(common, Formatting.Indented), AutoLiquid_Library.Utils.ConstantsUtils.FILE_COMMON_SETTING);
            else if (headIndex == 1)
                FileUtils.SaveJsonToLocalStorage(JsonConvert.SerializeObject(common, Formatting.Indented), AutoLiquid_Library.Utils.ConstantsUtils.FILE_COMMON_SETTING_2);
        }

        /// <summary>
        /// 保存移液头设置
        /// </summary>
        /// <param name="headIndex"></param>
        /// <param name="head"></param>
        public static void SaveHead(int headIndex, Head head)
        {
            if (headIndex == 0)
                FileUtils.SaveJsonToLocalStorage(JsonConvert.SerializeObject(head, Formatting.Indented), AutoLiquid_Library.Utils.ConstantsUtils.FILE_HEAD_1);
            else if (headIndex == 1)
                FileUtils.SaveJsonToLocalStorage(JsonConvert.SerializeObject(head, Formatting.Indented), AutoLiquid_Library.Utils.ConstantsUtils.FILE_HEAD_2);
        }

        /// <summary>
        /// 保存最大吸液量程
        /// </summary>
        /// <param name="range"></param>
        public static void SaveRange(Range range)
        {
            FileUtils.SaveJsonToLocalStorage(JsonConvert.SerializeObject(range, Formatting.Indented), AutoLiquid_Library.Utils.ConstantsUtils.FILE_RANGE);
        }

        /// <summary>
        /// 输入输出设置
        /// </summary>
        /// <param name="io"></param>
        public static void SaveIO(IO io)
        {
            FileUtils.SaveJsonToLocalStorage(JsonConvert.SerializeObject(io, Formatting.Indented), AutoLiquid_Library.Utils.ConstantsUtils.FILE_IO);
        }

        /// <summary>
        /// 保存权限管理
        /// </summary>
        /// <param name="permission"></param>
        public static void SavePermission(Permission permission)
        {
            FileUtils.SaveJsonToLocalStorage(JsonConvert.SerializeObject(permission, Formatting.Indented), AutoLiquid_Library.Utils.ConstantsUtils.FILE_PERMISSION);
        }

        /// <summary>
        /// 保存调试参数
        /// </summary>
        /// <param name="debug"></param>
        public static void SaveDebug(Debug debug)
        {
            FileUtils.SaveJsonToLocalStorage(JsonConvert.SerializeObject(debug, Formatting.Indented), AutoLiquid_Library.Utils.ConstantsUtils.FILE_DEBUG);
        }

        /// <summary>
        /// 保存程序
        /// </summary>
        /// <param name="procedureAll"></param>
        public static void SaveProcedureAll(ProcedureAll procedureAll)
        {
            FileUtils.SaveJsonToLocalStorage(JsonConvert.SerializeObject(procedureAll, Formatting.Indented), AutoLiquid_Library.Utils.ConstantsUtils.FILE_PROCEDURE_ALL);
        }
    }
}
