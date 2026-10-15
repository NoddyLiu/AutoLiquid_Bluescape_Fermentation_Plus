using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using AutoLiquid_Bluescape_Fermentation_Plus.EntityJson;
using AutoLiquid_Bluescape_Fermentation_Plus.Utils;
using AutoLiquid_Library.Utils;
using MahApps.Metro.Controls;

namespace AutoLiquid_Bluescape_Fermentation_Plus.Window
{
    /// <summary>
    /// 程序链新增程序输入窗体
    /// </summary>
    public partial class WindowProcedureChainNew : MetroWindow
    {
        // 点击结果
        public bool ClickResult = false;

        public WindowProcedureChainNew()
        {
            InitializeComponent();

            InitWidget();

            ControlEvent();
        }

        public static bool EnsureExecute()
        {
            WindowProcedureChainNew wpcn = new WindowProcedureChainNew();
            wpcn.ShowDialog();
            // 点击关闭后，返回点击结果
            return wpcn.ClickResult;
        }

        private void InitWidget()
        {
            ViewUtils.ShowLogo(this);

            this.ComboBoxProcedures.ItemsSource = ParamsHelper.ProcedureAll.Procedures.Select(p => p.Name);
            this.ComboBoxProcedures.SelectedIndex = 0;
        }

        private void ControlEvent()
        {
            this.BtnConfirm.Click += BtnOnClick;
            this.BtnCancel.Click += BtnOnClick;
        }

        private void BtnOnClick(object sender, RoutedEventArgs e)
        {
            if (sender.Equals(this.BtnConfirm))
            {
                ParamsHelper.ProcedureAll.ProcedureIndexChain.Add(this.ComboBoxProcedures.SelectedIndex);
                FileUtils.SaveProcedureAll(ParamsHelper.ProcedureAll);

                ClickResult = true;
                this.Close();
            }
            else if (sender.Equals(this.BtnCancel))
            {
                ClickResult = false;
                this.Close();
            }
        }
    }
}
