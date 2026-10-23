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
using AutoLiquid_Bluescape_Fermentation_Plus.Utils;
using AutoLiquid_Library.Utils;
using DataHelper = AutoLiquid_Bluescape_Fermentation_Plus.Utils.DataHelper;

namespace AutoLiquid_Bluescape_Fermentation_Plus.UserControls
{
    /// <summary>
    /// 搅拌设置控件
    /// </summary>
    public partial class ControlSettingShake : UserControl
    {
        // 移液头Index
        private int mHeadIndex;

        public ControlSettingShake(int headIndex)
        {
            InitializeComponent();

            this.mHeadIndex = headIndex;

            this.Loaded += OnLoaded;
        }

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            // 初始化控件
            InitWidget();
            // 控件事件
            ControlEvent();
        }

        private void InitWidget()
        {
            this.TextBoxShakeSpeedMaxCmd.Text = ParamsHelper.CommonSettingList[this.mHeadIndex].ShakeSpeedMaxCmd;
        }

        private void ControlEvent()
        {
            this.TextBoxShakeSpeedMaxCmd.TextChanged += TextBoxOnTextChanged;

            this.BtnShakeSpeedMaxStart.Click += BtnOnClick;
            this.BtnShakeSpeedMaxStop.Click += BtnOnClick;
        }

        private void BtnOnClick(object sender, RoutedEventArgs e)
        {
            if (sender.Equals(this.BtnShakeSpeedMaxStart))
            {
                BackgroundProcess.RunAsync(
                    delegate () { CmdHelperTemp.ShakeStart(3, false); },
                    delegate ()
                    {
                    });
            }
            else if (sender.Equals(this.BtnShakeSpeedMaxStop))
            {
                BackgroundProcess.RunAsync(
                    delegate () { CmdHelperTemp.ShakeStop(3, false); },
                    delegate ()
                    {
                    });
            }
        }

        private void TextBoxOnTextChanged(object sender, TextChangedEventArgs e)
        {
            if (sender.Equals(this.TextBoxShakeSpeedMaxCmd))
                DataHelper.SaveString(this.mHeadIndex, this.TextBoxShakeSpeedMaxCmd.Text.Trim(), ref ParamsHelper.CommonSettingList[this.mHeadIndex].ShakeSpeedMaxCmd);
        }
    }
}
