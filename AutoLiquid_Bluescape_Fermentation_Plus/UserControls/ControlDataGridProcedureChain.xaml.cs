using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using AutoLiquid_Bluescape_Fermentation_Plus.EntityJson;
using AutoLiquid_Bluescape_Fermentation_Plus.Utils;
using AutoLiquid_Bluescape_Fermentation_Plus.EntityCommon;
using AutoLiquid_Bluescape_Fermentation_Plus.Mvvm;
using AutoLiquid_Library.Enum;
using AutoLiquid_Library.Utils;

namespace AutoLiquid_Bluescape_Fermentation_Plus.UserControls
{
    /// <summary>
    /// 所有Seq指令DataGrid控件
    /// </summary>
    public partial class ControlDataGridProcedureChain : UserControl
    {
        // 程序链model（用于显示界面）
        private ModelProcedureChain _modelProcedureChain;

        public ControlDataGridProcedureChain()
        {
            InitializeComponent();

            InitWidget();

            // 控件事件
            ControlEvent();
        }

        private void InitWidget()
        {
            _modelProcedureChain = new ModelProcedureChain();
            this.DataContext = _modelProcedureChain;
        }

        private void ControlEvent()
        {

        }

        /// <summary>
        /// 刷新绑定程序链
        /// </summary>
        public void RefreshProcedures(List<int> procedureIndexList)
        {
            // 清空binding
            _modelProcedureChain.Procedures.Clear();
            foreach (var procedureIndex in procedureIndexList)
            {
                var procedure = ParamsHelper.ProcedureAll.Procedures[procedureIndex];
                AddBindingSeq(procedure.Name, new ModelSummary());
            }
        }

        /// <summary>
        /// 数据绑定：添加程序
        /// </summary>
        /// <param name="procedureName"></param>
        /// <param name="seqSummaryModel"></param>
        private void AddBindingSeq(string procedureName, ModelSummary summaryModel)
        {
            summaryModel.Summary = procedureName;
            _modelProcedureChain.Procedures.Add(summaryModel);
        }
    }
}
