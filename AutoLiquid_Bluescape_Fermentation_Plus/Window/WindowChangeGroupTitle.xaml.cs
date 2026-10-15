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
using AutoLiquid_Bluescape_Fermentation_Plus.Utils;
using MahApps.Metro.Controls;

namespace AutoLiquid_Bluescape_Fermentation_Plus.Window
{
    /// <summary>
    /// Interaction logic for WindowChangeGroupTitle.xaml
    /// </summary>
    public partial class WindowChangeGroupTitle : MetroWindow
    {
        // 移液头Index
        private int mHeadIndex;

        // 组Index
        private int mGroupIndex;

        // 点击结果
        public bool ClickResult = false;

        public WindowChangeGroupTitle(int headIndex, int groupIndex)
        {
            InitializeComponent();

            this.mHeadIndex = headIndex;
            this.mGroupIndex = groupIndex;

            InitWidget();

            ControlEvent();
        }

        public static bool EnsureExecute(int headIndex, int groupIndex)
        {
            WindowChangeGroupTitle wcgt = new WindowChangeGroupTitle(headIndex, groupIndex);
            wcgt.ShowDialog();
            // 点击关闭后，返回点击结果
            return wcgt.ClickResult;
        }

        private void InitWidget()
        {
            ViewUtils.ShowLogo(this);

            this.LabelOldName.Content = ParamsHelper.CommonSettingList[this.mHeadIndex].Consumables[mGroupIndex].GroupName;
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
                var oldName = ParamsHelper.CommonSettingList[this.mHeadIndex].Consumables[mGroupIndex].GroupName;
                // 判断新名字是否为空
                var newName = this.TextboxNewName.Text.Trim();
                if (newName.Equals(""))
                {
                    MessageBox.Show((string)this.FindResource("Prompt_Name_Cannot_Empty"));
                    return;
                }

                // 判断是否已经存在该耗材
                if (ParamsHelper.CommonSettingList[this.mHeadIndex].Consumables
                        .Count(p => p.GroupName.Equals(newName)) > 0)
                {
                    MessageBox.Show((string)this.FindResource("Prompt_Consumable_Name_Exist"));
                    return;
                }

                // 程序相关的耗材名一并更新
                foreach (var procedure in ParamsHelper.ProcedureAll.Procedures)
                {
                    procedure.Steps.Where(p => p.SourceConsumable == oldName).ToList().ForEach(p => p.SourceConsumable = newName);
                    procedure.Steps.Where(p => p.TargetConsumable == oldName).ToList().ForEach(p => p.TargetConsumable = newName);
                }
                FileUtils.SaveProcedureAll(ParamsHelper.ProcedureAll);

                ParamsHelper.CommonSettingList[this.mHeadIndex].Consumables[mGroupIndex].GroupName = newName;
                FileUtils.SaveCommonSettings(this.mHeadIndex, ParamsHelper.CommonSettingList[this.mHeadIndex]);

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
