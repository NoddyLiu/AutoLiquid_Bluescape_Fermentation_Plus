using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Documents;

namespace AutoLiquid_Bluescape_Fermentation_Plus.EntityJson
{
    /// <summary>
    /// 权限
    /// </summary>
    [Serializable()]
    public class Permission
    {
        // 是否启用密码权限
        public bool Available = false;

        // 密码Hash值
        public string PwdHash = "";
    }
}
