using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
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
using AutoLiquid_Library.Enum;
using AutoLiquid_Library.Utils;
using DataHelper = AutoLiquid_Bluescape_Fermentation_Plus.Utils.DataHelper;

namespace AutoLiquid_Bluescape_Fermentation_Plus.UserControls
{
    /// <summary>
    /// 钢针设置控件
    /// </summary>
    public partial class ControlSettingSteelNeedle : UserControl
    {
        // 移液头Index
        private int mHeadIndex;

        public ControlSettingSteelNeedle(int headIndex)
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
            this.ControlSteelNeedlePosX.SetParam(mHeadIndex, ParamsHelper.CommonSettingList[this.mHeadIndex].SteelNeedleInstallPos.X, EAxis.X);
            this.ControlSteelNeedlePosY.SetParam(mHeadIndex, ParamsHelper.CommonSettingList[this.mHeadIndex].SteelNeedleInstallPos.Y, EAxis.Y);

            this.TextBoxRunTimeSingleHole14.Text = ParamsHelper.CommonSettingList[this.mHeadIndex].SteelNeedleRunTimeSingleArray[0].ToString();
            this.TextBoxSpeedAbsorbAndJet14.Text = ParamsHelper.CommonSettingList[this.mHeadIndex].SteelNeedleSpeedArray[0];
            this.TextBoxRunTimeSingleHole15.Text = ParamsHelper.CommonSettingList[this.mHeadIndex].SteelNeedleRunTimeSingleArray[1].ToString();
            this.TextBoxSpeedAbsorbAndJet15.Text = ParamsHelper.CommonSettingList[this.mHeadIndex].SteelNeedleSpeedArray[1];
            this.TextBoxRunTimeSingleHole16.Text = ParamsHelper.CommonSettingList[this.mHeadIndex].SteelNeedleRunTimeSingleArray[2].ToString();
            this.TextBoxSpeedAbsorbAndJet16.Text = ParamsHelper.CommonSettingList[this.mHeadIndex].SteelNeedleSpeedArray[2];
            this.TextBoxRunTimeSingleHole17.Text = ParamsHelper.CommonSettingList[this.mHeadIndex].SteelNeedleRunTimeSingleArray[3].ToString();
            this.TextBoxSpeedAbsorbAndJet17.Text = ParamsHelper.CommonSettingList[this.mHeadIndex].SteelNeedleSpeedArray[3];
            this.TextBoxRunTimeSingleHole18.Text = ParamsHelper.CommonSettingList[this.mHeadIndex].SteelNeedleRunTimeSingleArray[4].ToString();
            this.TextBoxSpeedAbsorbAndJet18.Text = ParamsHelper.CommonSettingList[this.mHeadIndex].SteelNeedleSpeedArray[4];
        }

        private void ControlEvent()
        {
            this.ControlSteelNeedlePosX.TextBoxPos.TextChanged += TextBoxOnTextChanged;
            this.ControlSteelNeedlePosY.TextBoxPos.TextChanged += TextBoxOnTextChanged;

            this.TextBoxRunTimeSingleHole14.TextChanged += TextBoxOnTextChanged;
            this.TextBoxSpeedAbsorbAndJet14.TextChanged += TextBoxOnTextChanged;
            this.TextBoxRunTimeSingleHole15.TextChanged += TextBoxOnTextChanged;
            this.TextBoxSpeedAbsorbAndJet15.TextChanged += TextBoxOnTextChanged;
            this.TextBoxRunTimeSingleHole16.TextChanged += TextBoxOnTextChanged;
            this.TextBoxSpeedAbsorbAndJet16.TextChanged += TextBoxOnTextChanged;
            this.TextBoxRunTimeSingleHole17.TextChanged += TextBoxOnTextChanged;
            this.TextBoxSpeedAbsorbAndJet17.TextChanged += TextBoxOnTextChanged;
            this.TextBoxRunTimeSingleHole18.TextChanged += TextBoxOnTextChanged;
            this.TextBoxSpeedAbsorbAndJet18.TextChanged += TextBoxOnTextChanged;
        }

        private void TextBoxOnTextChanged(object sender, TextChangedEventArgs e)
        {
            if (sender.Equals(this.ControlSteelNeedlePosX.TextBoxPos))
                DataHelper.SaveDecimal(this.mHeadIndex, this.ControlSteelNeedlePosX.TextBoxPos.Text.Trim(), ref ParamsHelper.CommonSettingList[this.mHeadIndex].SteelNeedleInstallPos.X);
            else if (sender.Equals(this.ControlSteelNeedlePosY.TextBoxPos))
                DataHelper.SaveDecimal(this.mHeadIndex, this.ControlSteelNeedlePosY.TextBoxPos.Text.Trim(), ref ParamsHelper.CommonSettingList[this.mHeadIndex].SteelNeedleInstallPos.Y);

            else if (sender.Equals(this.TextBoxRunTimeSingleHole14))
                DataHelper.SaveInt(this.mHeadIndex, this.TextBoxRunTimeSingleHole14.Text.Trim(), ref ParamsHelper.CommonSettingList[this.mHeadIndex].SteelNeedleRunTimeSingleArray[0]);
            else if (sender.Equals(this.TextBoxSpeedAbsorbAndJet14))
                DataHelper.SaveString(this.mHeadIndex, this.TextBoxSpeedAbsorbAndJet14.Text.Trim(), ref ParamsHelper.CommonSettingList[this.mHeadIndex].SteelNeedleSpeedArray[0]);
            else if (sender.Equals(this.TextBoxRunTimeSingleHole15))
                DataHelper.SaveInt(this.mHeadIndex, this.TextBoxRunTimeSingleHole15.Text.Trim(), ref ParamsHelper.CommonSettingList[this.mHeadIndex].SteelNeedleRunTimeSingleArray[1]);
            else if (sender.Equals(this.TextBoxSpeedAbsorbAndJet15))
                DataHelper.SaveString(this.mHeadIndex, this.TextBoxSpeedAbsorbAndJet15.Text.Trim(), ref ParamsHelper.CommonSettingList[this.mHeadIndex].SteelNeedleSpeedArray[1]);
            else if (sender.Equals(this.TextBoxRunTimeSingleHole16))
                DataHelper.SaveInt(this.mHeadIndex, this.TextBoxRunTimeSingleHole16.Text.Trim(), ref ParamsHelper.CommonSettingList[this.mHeadIndex].SteelNeedleRunTimeSingleArray[2]);
            else if (sender.Equals(this.TextBoxSpeedAbsorbAndJet16))
                DataHelper.SaveString(this.mHeadIndex, this.TextBoxSpeedAbsorbAndJet16.Text.Trim(), ref ParamsHelper.CommonSettingList[this.mHeadIndex].SteelNeedleSpeedArray[2]);
            else if (sender.Equals(this.TextBoxRunTimeSingleHole17))
                DataHelper.SaveInt(this.mHeadIndex, this.TextBoxRunTimeSingleHole17.Text.Trim(), ref ParamsHelper.CommonSettingList[this.mHeadIndex].SteelNeedleRunTimeSingleArray[3]);
            else if (sender.Equals(this.TextBoxSpeedAbsorbAndJet17))
                DataHelper.SaveString(this.mHeadIndex, this.TextBoxSpeedAbsorbAndJet17.Text.Trim(), ref ParamsHelper.CommonSettingList[this.mHeadIndex].SteelNeedleSpeedArray[3]);
            else if (sender.Equals(this.TextBoxRunTimeSingleHole18))
                DataHelper.SaveInt(this.mHeadIndex, this.TextBoxRunTimeSingleHole18.Text.Trim(), ref ParamsHelper.CommonSettingList[this.mHeadIndex].SteelNeedleRunTimeSingleArray[4]);
            else if (sender.Equals(this.TextBoxSpeedAbsorbAndJet18))
                DataHelper.SaveString(this.mHeadIndex, this.TextBoxSpeedAbsorbAndJet18.Text.Trim(), ref ParamsHelper.CommonSettingList[this.mHeadIndex].SteelNeedleSpeedArray[4]);
        }
    }
}
