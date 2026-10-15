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
    /// 程序新增名字输入窗体
    /// </summary>
    public partial class WindowProcedureNew : MetroWindow
    {
        // 点击结果
        public bool ClickResult = false;

        public WindowProcedureNew()
        {
            InitializeComponent();

            InitWidget();

            ControlEvent();
        }

        public static bool EnsureExecute()
        {
            WindowProcedureNew wpn = new WindowProcedureNew();
            wpn.ShowDialog();
            // 点击关闭后，返回点击结果
            return wpn.ClickResult;
        }

        private void InitWidget()
        {
            ViewUtils.ShowLogo(this);
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
                // 判断新名字是否为空
                var name = this.TextboxName.Text.Trim();
                if (name.Equals(""))
                {
                    MessageBox.Show((string)this.FindResource("Prompt_Name_Cannot_Empty"));
                    return;
                }

                // 判断程序中是否已经存在该名字
                if (ParamsHelper.ProcedureAll.Procedures.Count(p => p.Name.Equals(name)) > 0)
                {
                    MessageBox.Show((string)this.FindResource("Prompt_Procedure_Name_Exist"));
                    return;
                }

                var procedureNew = ObjectUtils.DeepCopy(ParamsHelper.ProcedureAll.Procedures[ParamsHelper.ProcedureAll.ProcedureSelectedIndex]);
                procedureNew.Name = name;
                ParamsHelper.ProcedureAll.Procedures.Add(procedureNew);
                ParamsHelper.ProcedureAll.ProcedureSelectedIndex = ParamsHelper.ProcedureAll.Procedures.Count - 1;
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
