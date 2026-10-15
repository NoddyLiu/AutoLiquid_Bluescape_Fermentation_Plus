using System;
using System.Collections.Generic;
using System.IO;
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
using AutoLiquid_Bluescape_Fermentation_Plus.EntityJson;
using AutoLiquid_Bluescape_Fermentation_Plus.Mvvm;
using AutoLiquid_Bluescape_Fermentation_Plus.Utils;
using AutoLiquid_Library.Enum;
using AutoLiquid_Library.Utils;
using AutoLiquid_Bluescape_Fermentation_Plus.MVVM;
using MahApps.Metro.Controls;

namespace AutoLiquid_Bluescape_Fermentation_Plus.Window
{
    /// <summary>
    /// 程序窗体
    /// </summary>
    public partial class WindowProcedure : MetroWindow
    {
        public WindowProcedure()
        {
            InitializeComponent();

            this.Loaded += OnLoaded;
            this.Unloaded += (sender, args) => { MainWindow.mMainWindow.RefreshProcedureChain(); };
        }

        private void OnLoaded(object sender, RoutedEventArgs routedEventArgs)
        {
            InitWidget();

            // 控件事件
            ControlEvent();
        }

        private void InitWidget()
        {
            ViewUtils.ShowLogo(this);

            RefreshProcedure();

            RefreshOther();
        }

        private void ControlEvent()
        {
            // 程序选择
            this.ComboBoxProcedures.SelectionChanged += ComboBoxProceduresOnSelectionChanged;
            this.BtnProcedureNew.Click += BtnOnClick;
            this.BtnProcedureDel.Click += BtnOnClick;

            // 钢针选择
            this.ComboBoxSteelNeedleChoice.SelectionChanged += ComboBoxSteelNeedleChoiceOnSelectionChanged;

            // 期望运行时间
            this.TextBoxRunTimeExpect.TextChanged += TextBoxOnTextChanged;
        }

        private void ComboBoxProceduresOnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var selectedIndex = this.ComboBoxProcedures.SelectedIndex;
            if (selectedIndex == -1)
                selectedIndex = 0;
            ParamsHelper.ProcedureAll.ProcedureSelectedIndex = selectedIndex;
            FileUtils.SaveProcedureAll(ParamsHelper.ProcedureAll);

            // 数据绑定
            this.ControlDataGridProcedure.BindingSelectedProcedure();

            RefreshOther();
        }

        private void ComboBoxSteelNeedleChoiceOnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var selectedIndex = this.ComboBoxSteelNeedleChoice.SelectedIndex;
            if (selectedIndex == -1)
                selectedIndex = 0;
            ParamsHelper.ProcedureAll.Procedures[ParamsHelper.ProcedureAll.ProcedureSelectedIndex].SteelNeedleChoiceIndex = selectedIndex;
            FileUtils.SaveProcedureAll(ParamsHelper.ProcedureAll);
        }

        private void BtnOnClick(object sender, RoutedEventArgs e)
        {
            if (sender.Equals(this.BtnProcedureNew))
            {
                if (WindowProcedureNew.EnsureExecute())
                {
                    RefreshProcedure();
                }
            }
            else if (sender.Equals(this.BtnProcedureDel))
            {
                // 判断是否只剩下一个程序
                if (ParamsHelper.ProcedureAll.Procedures.Count == 1)
                {
                    MessageBox.Show((string)this.FindResource("Prompt_CanNot_Delete_All_Procedure"));
                    return;
                }
                if (MessageBox.Show((string)this.FindResource("Prompt_If_Delete_Procedure_1") + ParamsHelper.ProcedureAll.Procedures[ParamsHelper.ProcedureAll.ProcedureSelectedIndex].Name + (string)this.FindResource("Prompt_If_Delete_Procedure_2"), (string)Application.Current.FindResource("Prompt"), MessageBoxButton.YesNo, MessageBoxImage.Warning) ==
                    MessageBoxResult.Yes)
                {
                    ParamsHelper.ProcedureAll.Procedures.RemoveAt(ParamsHelper.ProcedureAll.ProcedureSelectedIndex);
                    FileUtils.SaveProcedureAll(ParamsHelper.ProcedureAll);
                    RefreshProcedure();
                }
            }
        }

        private void TextBoxOnTextChanged(object sender, TextChangedEventArgs e)
        {
            if (sender.Equals(this.TextBoxRunTimeExpect))
                Utils.DataHelper.SaveInt(0, this.TextBoxRunTimeExpect.Text.Trim(), ref ParamsHelper.ProcedureAll.Procedures[ParamsHelper.ProcedureAll.ProcedureSelectedIndex].RunTimeExpect);
        }

        /// <summary>
        /// 刷新程序
        /// </summary>
        private void RefreshProcedure()
        {
            // 程序选择
            this.ComboBoxProcedures.ItemsSource = ParamsHelper.ProcedureAll.Procedures.Select(p => p.Name).ToList();
            this.ComboBoxProcedures.SelectedIndex = ParamsHelper.ProcedureAll.ProcedureSelectedIndex;

            // 数据绑定
            this.ControlDataGridProcedure.BindingSelectedProcedure();
        }

        /// <summary>
        /// 刷新其他参数
        /// </summary>
        private void RefreshOther()
        {
            // 钢针选择
            this.ComboBoxSteelNeedleChoice.SelectedIndex = ParamsHelper.ProcedureAll.Procedures[ParamsHelper.ProcedureAll.ProcedureSelectedIndex].SteelNeedleChoiceIndex;
            // 期望运行时间
            this.TextBoxRunTimeExpect.Text = ParamsHelper.ProcedureAll.Procedures[ParamsHelper.ProcedureAll.ProcedureSelectedIndex].RunTimeExpect.ToString();
        }
    }
}
