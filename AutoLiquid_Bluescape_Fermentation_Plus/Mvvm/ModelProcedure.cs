using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Documents;
using AutoLiquid_Bluescape_Fermentation_Plus.Mvvm;

namespace AutoLiquid_Bluescape_Fermentation_Plus.MVVM
{
    /// <summary>
    /// 程序model
    /// </summary>
    public class ModelProcedure
    {
        public ModelProcedure()
        {
            // 当前程序所有步骤
            ProcedureSteps = new ObservableCollection<ModelColumnStep>();
        }

        public ObservableCollection<ModelColumnStep> ProcedureSteps { get; set; }
    }
}
