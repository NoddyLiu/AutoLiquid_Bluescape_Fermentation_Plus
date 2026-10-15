using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutoLiquid_Library.Utils
{
    public class ConstantsUtils
    {
        /**
        * 程序保存文件夹名
        */
        public static string SAVE_FOLDER = "settings";

        /**
         * 更新日志文件名
         */
        public static string FILE_UPDATE_LOG = "AutoLiquid_Update_Log.txt";

        /**
         * json文件名
         */
        public static string FILE_COMMON_SETTING = "common_setting";
        public static string FILE_COMMON_SETTING_2 = "common_setting2";
        public static string FILE_HEAD_1 = "head1";
        public static string FILE_HEAD_2 = "head2";
        public static string FILE_RANGE = "range";
        public static string FILE_LAYOUT = "layout";
        public static string FILE_IO = "io";
        public static string FILE_PERMISSION = "permission";
        public static string FILE_DEBUG = "debug";
        public static string FILE_PROCEDURE_ALL = "procedure_all";

        /**
         * 特殊指令
         */
        // 移液头2特殊指令
        public static string Head2Cmd = "head2";
        // 吸液前特殊指令
        public static string AbsorbBeforeCmd = "AbsorbBefore";
        // 吸液后特殊指令
        public static string AbsorbAfterCmd = "AbsorbAfter";
        // 喷液前特殊指令
        public static string JetBeforeCmd = "JetBefore";
        // 喷液后特殊指令
        public static string JetAfterCmd = "JetAfter";
        // 检查试管是否存在
        public static string CheckTubeCmd = "CheckTube";
        // 指定孔位喷液体积补偿指令
        public static string JetOffsetCmd = "JetOffset";
        // 重复执行Excel表格
        public static string RepeatCmd = "Repeat";
    }
}
