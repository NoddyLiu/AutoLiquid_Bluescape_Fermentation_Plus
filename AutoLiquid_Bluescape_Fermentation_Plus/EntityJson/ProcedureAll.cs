using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Documents;
using System.Windows.Media;

namespace AutoLiquid_Bluescape_Fermentation_Plus.EntityJson
{
    /// <summary>
    /// 所有程序
    /// </summary>
    [Serializable()]
    public class ProcedureAll
    {
        // 所有程序
        public List<Procedure> Procedures = new List<Procedure>();
        // 选择的程序
        public int ProcedureSelectedIndex = 0;

        // 选择的程序链Index
        public List<int> ProcedureIndexChain = new List<int>();
    }

    /// <summary>
    /// 单个程序
    /// </summary>
    [Serializable()]
    public class Procedure
    {
        // 程序名称
        public string Name = String.Empty;

        // 程序步骤
        public List<ProcedureStep> Steps = new List<ProcedureStep>();

        // “等待”步骤是否阻塞：非阻塞---前一个步骤运行开始马上计时；阻塞---前一个步骤运行结束后才计时
        [Obsolete("弃用，改成step属性Block", true)]
        public bool WaitStepBlock = false;

        // 钢针选择
        public int SteelNeedleChoiceIndex = 0;
        
        // 期望运行时间（分钟）
        public int RunTimeExpect = 0;
    }
}
