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
    /// 清洗区设置控件
    /// </summary>
    public partial class ControlSettingCleaning : UserControl
    {
        // 移液头Index
        private int mHeadIndex;

        public ControlSettingCleaning(int headIndex)
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
            this.ControlCleaningPos1X.SetParam(mHeadIndex, ParamsHelper.CommonSettingList[this.mHeadIndex].CleaningPosList[0].X, EAxis.X);
            this.ControlCleaningPos1Y.SetParam(mHeadIndex, ParamsHelper.CommonSettingList[this.mHeadIndex].CleaningPosList[0].Y, EAxis.Y);
            this.ControlCleaningPos1Z.SetParam(mHeadIndex, ParamsHelper.CommonSettingList[this.mHeadIndex].CleaningPosList[0].Z, EAxis.Z);

            this.ControlCleaningPos2X.SetParam(mHeadIndex, ParamsHelper.CommonSettingList[this.mHeadIndex].CleaningPosList[1].X, EAxis.X);
            this.ControlCleaningPos2Y.SetParam(mHeadIndex, ParamsHelper.CommonSettingList[this.mHeadIndex].CleaningPosList[1].Y, EAxis.Y);
            this.ControlCleaningPos2Z.SetParam(mHeadIndex, ParamsHelper.CommonSettingList[this.mHeadIndex].CleaningPosList[1].Z, EAxis.Z);

            this.ControlCleaningPos3X.SetParam(mHeadIndex, ParamsHelper.CommonSettingList[this.mHeadIndex].CleaningPosList[2].X, EAxis.X);
            this.ControlCleaningPos3Y.SetParam(mHeadIndex, ParamsHelper.CommonSettingList[this.mHeadIndex].CleaningPosList[2].Y, EAxis.Y);
            this.ControlCleaningPos3Z.SetParam(mHeadIndex, ParamsHelper.CommonSettingList[this.mHeadIndex].CleaningPosList[2].Z, EAxis.Z);

            this.ControlCleaningPos4X.SetParam(mHeadIndex, ParamsHelper.CommonSettingList[this.mHeadIndex].CleaningPosList[3].X, EAxis.X);
            this.ControlCleaningPos4Y.SetParam(mHeadIndex, ParamsHelper.CommonSettingList[this.mHeadIndex].CleaningPosList[3].Y, EAxis.Y);
            this.ControlCleaningPos4Z.SetParam(mHeadIndex, ParamsHelper.CommonSettingList[this.mHeadIndex].CleaningPosList[3].Z, EAxis.Z);

            this.ControlCleaningPos5X.SetParam(mHeadIndex, ParamsHelper.CommonSettingList[this.mHeadIndex].CleaningPosList[4].X, EAxis.X);
            this.ControlCleaningPos5Y.SetParam(mHeadIndex, ParamsHelper.CommonSettingList[this.mHeadIndex].CleaningPosList[4].Y, EAxis.Y);
            this.ControlCleaningPos5Z.SetParam(mHeadIndex, ParamsHelper.CommonSettingList[this.mHeadIndex].CleaningPosList[4].Z, EAxis.Z);

            this.ControlCleaningPos6X.SetParam(mHeadIndex, ParamsHelper.CommonSettingList[this.mHeadIndex].CleaningPosList[5].X, EAxis.X);
            this.ControlCleaningPos6Y.SetParam(mHeadIndex, ParamsHelper.CommonSettingList[this.mHeadIndex].CleaningPosList[5].Y, EAxis.Y);
            this.ControlCleaningPos6Z.SetParam(mHeadIndex, ParamsHelper.CommonSettingList[this.mHeadIndex].CleaningPosList[5].Z, EAxis.Z);

            // this.TextBoxVolumeCleaningLiquid.Text = ParamsHelper.CommonSettingList[this.mHeadIndex].CleaningLiquidVolume.ToString();

            this.TextBoxAbsorbAirBeforeCleaning.Text = ParamsHelper.CommonSettingList[this.mHeadIndex].AbsorbAirBeforeCleaning.ToString();
            this.TextBoxVolumeLiquidCleaningHead.Text = ParamsHelper.CommonSettingList[this.mHeadIndex].CleaningLiquidVolume.ToString();
            this.TextBoxVolumeAirCleaningHead.Text = ParamsHelper.CommonSettingList[this.mHeadIndex].CleaningHeadVolume.ToString();
            this.TextBoxTimeCleaningHead.Text = ParamsHelper.CommonSettingList[this.mHeadIndex].CleaningHeadTime.ToString();
            this.TextBoxCleaningHeadAbsorbAirAfterAbsorb.Text = ParamsHelper.CommonSettingList[this.mHeadIndex].CleaningHeadAbsorbAirAfterAbsorb.ToString();

            this.TextBoxCleaningHeadAbsorbAirAfterJet.Text = ParamsHelper.CommonSettingList[this.mHeadIndex].CleaningHeadAbsorbAirAfterJet.ToString();
            //this.TextBoxCleaningHeadSpeed.Text = ParamsHelper.CommonSettingList[this.mHeadIndex].CleaningHeadAbsorbAirAfterJetSpeed;
            this.ComboBoxCleaningHeadSpeed.ItemsSource = Enumerable.Range(0, 41).Select(x => x.ToString()).ToList();
            this.ComboBoxCleaningHeadSpeed.SelectedItem = ParamsHelper.CommonSettingList[this.mHeadIndex].CleaningHeadAbsorbAirAfterJetSpeed;
            this.TextBoxCleaningHeadJetAirDelay.Text = ParamsHelper.CommonSettingList[this.mHeadIndex].CleaningHeadJetAirDelay.ToString();
            this.TextBoxOpenPumpDelay.Text = ParamsHelper.CommonSettingList[this.mHeadIndex].OpenPumpDelay.ToString();
        }

        private void ControlEvent()
        {
            this.ControlCleaningPos1X.TextBoxPos.TextChanged += TextBoxOnTextChanged;
            this.ControlCleaningPos1Y.TextBoxPos.TextChanged += TextBoxOnTextChanged;
            this.ControlCleaningPos1Z.TextBoxPos.TextChanged += TextBoxOnTextChanged;

            this.ControlCleaningPos2X.TextBoxPos.TextChanged += TextBoxOnTextChanged;
            this.ControlCleaningPos2Y.TextBoxPos.TextChanged += TextBoxOnTextChanged;
            this.ControlCleaningPos2Z.TextBoxPos.TextChanged += TextBoxOnTextChanged;

            this.ControlCleaningPos3X.TextBoxPos.TextChanged += TextBoxOnTextChanged;
            this.ControlCleaningPos3Y.TextBoxPos.TextChanged += TextBoxOnTextChanged;
            this.ControlCleaningPos3Z.TextBoxPos.TextChanged += TextBoxOnTextChanged;

            this.ControlCleaningPos4X.TextBoxPos.TextChanged += TextBoxOnTextChanged;
            this.ControlCleaningPos4Y.TextBoxPos.TextChanged += TextBoxOnTextChanged;
            this.ControlCleaningPos4Z.TextBoxPos.TextChanged += TextBoxOnTextChanged;

            this.ControlCleaningPos5X.TextBoxPos.TextChanged += TextBoxOnTextChanged;
            this.ControlCleaningPos5Y.TextBoxPos.TextChanged += TextBoxOnTextChanged;
            this.ControlCleaningPos5Z.TextBoxPos.TextChanged += TextBoxOnTextChanged;

            this.ControlCleaningPos6X.TextBoxPos.TextChanged += TextBoxOnTextChanged;
            this.ControlCleaningPos6Y.TextBoxPos.TextChanged += TextBoxOnTextChanged;
            this.ControlCleaningPos6Z.TextBoxPos.TextChanged += TextBoxOnTextChanged;

            // this.TextBoxVolumeCleaningLiquid.TextChanged += TextBoxOnTextChanged;
            this.TextBoxAbsorbAirBeforeCleaning.TextChanged += TextBoxOnTextChanged;
            this.TextBoxVolumeLiquidCleaningHead.TextChanged += TextBoxOnTextChanged;
            this.TextBoxVolumeAirCleaningHead.TextChanged += TextBoxOnTextChanged;
            this.TextBoxTimeCleaningHead.TextChanged += TextBoxOnTextChanged;
            this.TextBoxCleaningHeadAbsorbAirAfterAbsorb.TextChanged += TextBoxOnTextChanged;

            this.TextBoxCleaningHeadAbsorbAirAfterJet.TextChanged += TextBoxOnTextChanged;
            //this.TextBoxCleaningHeadSpeed.TextChanged += TextBoxOnTextChanged;
            this.ComboBoxCleaningHeadSpeed.SelectionChanged += ComboBoxSelectionChanged;
            this.TextBoxCleaningHeadJetAirDelay.TextChanged += TextBoxOnTextChanged;
            this.TextBoxOpenPumpDelay.TextChanged += TextBoxOnTextChanged;

            // this.BtnCleaningFill.Click += BtnOnClick;
            // this.BtnCleaningDrain.Click += BtnOnClick;
        }

        private void TextBoxOnTextChanged(object sender, TextChangedEventArgs e)
        {
            if (sender.Equals(this.ControlCleaningPos1X.TextBoxPos))
                DataHelper.SaveDecimal(this.mHeadIndex, this.ControlCleaningPos1X.TextBoxPos.Text.Trim(), ref ParamsHelper.CommonSettingList[this.mHeadIndex].CleaningPosList[0].X);
            else if (sender.Equals(this.ControlCleaningPos1Y.TextBoxPos))
                DataHelper.SaveDecimal(this.mHeadIndex, this.ControlCleaningPos1Y.TextBoxPos.Text.Trim(), ref ParamsHelper.CommonSettingList[this.mHeadIndex].CleaningPosList[0].Y);
            else if (sender.Equals(this.ControlCleaningPos1Z.TextBoxPos))
                DataHelper.SaveDecimal(this.mHeadIndex, this.ControlCleaningPos1Z.TextBoxPos.Text.Trim(), ref ParamsHelper.CommonSettingList[this.mHeadIndex].CleaningPosList[0].Z);

            else if (sender.Equals(this.ControlCleaningPos2X.TextBoxPos))
                DataHelper.SaveDecimal(this.mHeadIndex, this.ControlCleaningPos2X.TextBoxPos.Text.Trim(), ref ParamsHelper.CommonSettingList[this.mHeadIndex].CleaningPosList[1].X);
            else if (sender.Equals(this.ControlCleaningPos2Y.TextBoxPos))
                DataHelper.SaveDecimal(this.mHeadIndex, this.ControlCleaningPos2Y.TextBoxPos.Text.Trim(), ref ParamsHelper.CommonSettingList[this.mHeadIndex].CleaningPosList[1].Y);
            else if (sender.Equals(this.ControlCleaningPos2Z.TextBoxPos))
                DataHelper.SaveDecimal(this.mHeadIndex, this.ControlCleaningPos2Z.TextBoxPos.Text.Trim(), ref ParamsHelper.CommonSettingList[this.mHeadIndex].CleaningPosList[1].Z);

            else if (sender.Equals(this.ControlCleaningPos3X.TextBoxPos))
                DataHelper.SaveDecimal(this.mHeadIndex, this.ControlCleaningPos3X.TextBoxPos.Text.Trim(), ref ParamsHelper.CommonSettingList[this.mHeadIndex].CleaningPosList[2].X);
            else if (sender.Equals(this.ControlCleaningPos3Y.TextBoxPos))
                DataHelper.SaveDecimal(this.mHeadIndex, this.ControlCleaningPos3Y.TextBoxPos.Text.Trim(), ref ParamsHelper.CommonSettingList[this.mHeadIndex].CleaningPosList[2].Y);
            else if (sender.Equals(this.ControlCleaningPos3Z.TextBoxPos))
                DataHelper.SaveDecimal(this.mHeadIndex, this.ControlCleaningPos3Z.TextBoxPos.Text.Trim(), ref ParamsHelper.CommonSettingList[this.mHeadIndex].CleaningPosList[2].Z);

            else if (sender.Equals(this.ControlCleaningPos4X.TextBoxPos))
                DataHelper.SaveDecimal(this.mHeadIndex, this.ControlCleaningPos4X.TextBoxPos.Text.Trim(), ref ParamsHelper.CommonSettingList[this.mHeadIndex].CleaningPosList[3].X);
            else if (sender.Equals(this.ControlCleaningPos4Y.TextBoxPos))
                DataHelper.SaveDecimal(this.mHeadIndex, this.ControlCleaningPos4Y.TextBoxPos.Text.Trim(), ref ParamsHelper.CommonSettingList[this.mHeadIndex].CleaningPosList[3].Y);
            else if (sender.Equals(this.ControlCleaningPos4Z.TextBoxPos))
                DataHelper.SaveDecimal(this.mHeadIndex, this.ControlCleaningPos4Z.TextBoxPos.Text.Trim(), ref ParamsHelper.CommonSettingList[this.mHeadIndex].CleaningPosList[3].Z);

            else if (sender.Equals(this.ControlCleaningPos5X.TextBoxPos))
                DataHelper.SaveDecimal(this.mHeadIndex, this.ControlCleaningPos5X.TextBoxPos.Text.Trim(), ref ParamsHelper.CommonSettingList[this.mHeadIndex].CleaningPosList[4].X);
            else if (sender.Equals(this.ControlCleaningPos5Y.TextBoxPos))
                DataHelper.SaveDecimal(this.mHeadIndex, this.ControlCleaningPos5Y.TextBoxPos.Text.Trim(), ref ParamsHelper.CommonSettingList[this.mHeadIndex].CleaningPosList[4].Y);
            else if (sender.Equals(this.ControlCleaningPos5Z.TextBoxPos))
                DataHelper.SaveDecimal(this.mHeadIndex, this.ControlCleaningPos5Z.TextBoxPos.Text.Trim(), ref ParamsHelper.CommonSettingList[this.mHeadIndex].CleaningPosList[4].Z);

            else if (sender.Equals(this.ControlCleaningPos6X.TextBoxPos))
                DataHelper.SaveDecimal(this.mHeadIndex, this.ControlCleaningPos6X.TextBoxPos.Text.Trim(), ref ParamsHelper.CommonSettingList[this.mHeadIndex].CleaningPosList[5].X);
            else if (sender.Equals(this.ControlCleaningPos6Y.TextBoxPos))
                DataHelper.SaveDecimal(this.mHeadIndex, this.ControlCleaningPos6Y.TextBoxPos.Text.Trim(), ref ParamsHelper.CommonSettingList[this.mHeadIndex].CleaningPosList[5].Y);
            else if (sender.Equals(this.ControlCleaningPos6Z.TextBoxPos))
                DataHelper.SaveDecimal(this.mHeadIndex, this.ControlCleaningPos6Z.TextBoxPos.Text.Trim(), ref ParamsHelper.CommonSettingList[this.mHeadIndex].CleaningPosList[5].Z);

            // else if (sender.Equals(this.TextBoxVolumeCleaningLiquid))
            //     DataHelper.SaveDecimal(this.mHeadIndex, this.TextBoxVolumeCleaningLiquid.Text.Trim(), ref ParamsHelper.CommonSettingList[this.mHeadIndex].CleaningLiquidVolume);
            else if (sender.Equals(this.TextBoxAbsorbAirBeforeCleaning))
                DataHelper.SaveDecimal(this.mHeadIndex, this.TextBoxAbsorbAirBeforeCleaning.Text.Trim(), ref ParamsHelper.CommonSettingList[this.mHeadIndex].AbsorbAirBeforeCleaning);
            else if (sender.Equals(this.TextBoxVolumeLiquidCleaningHead))
                DataHelper.SaveDecimal(this.mHeadIndex, this.TextBoxVolumeLiquidCleaningHead.Text.Trim(), ref ParamsHelper.CommonSettingList[this.mHeadIndex].CleaningLiquidVolume);
            else if (sender.Equals(this.TextBoxVolumeAirCleaningHead))
                DataHelper.SaveDecimal(this.mHeadIndex, this.TextBoxVolumeAirCleaningHead.Text.Trim(), ref ParamsHelper.CommonSettingList[this.mHeadIndex].CleaningHeadVolume);
            else if (sender.Equals(this.TextBoxTimeCleaningHead))
                DataHelper.SaveInt(this.mHeadIndex, this.TextBoxTimeCleaningHead.Text.Trim(), ref ParamsHelper.CommonSettingList[this.mHeadIndex].CleaningHeadTime);
            else if (sender.Equals(this.TextBoxCleaningHeadAbsorbAirAfterAbsorb))
                DataHelper.SaveDecimal(this.mHeadIndex, this.TextBoxCleaningHeadAbsorbAirAfterAbsorb.Text.Trim(), ref ParamsHelper.CommonSettingList[this.mHeadIndex].CleaningHeadAbsorbAirAfterAbsorb);

            else if (sender.Equals(this.TextBoxCleaningHeadAbsorbAirAfterJet))
                DataHelper.SaveDecimal(this.mHeadIndex, this.TextBoxCleaningHeadAbsorbAirAfterJet.Text.Trim(), ref ParamsHelper.CommonSettingList[this.mHeadIndex].CleaningHeadAbsorbAirAfterJet);
            //else if (sender.Equals(this.TextBoxCleaningHeadSpeed))
            //    DataHelper.SaveString(this.mHeadIndex, this.TextBoxCleaningHeadSpeed.Text.Trim(), ref ParamsHelper.CommonSettingList[this.mHeadIndex].CleaningHeadAbsorbAirAfterJetSpeed);
            else if (sender.Equals(this.TextBoxCleaningHeadJetAirDelay))
                DataHelper.SaveDecimal(this.mHeadIndex, this.TextBoxCleaningHeadJetAirDelay.Text.Trim(), ref ParamsHelper.CommonSettingList[this.mHeadIndex].CleaningHeadJetAirDelay);
            else if (sender.Equals(this.TextBoxOpenPumpDelay))
                DataHelper.SaveDecimal(this.mHeadIndex, this.TextBoxOpenPumpDelay.Text.Trim(), ref ParamsHelper.CommonSettingList[this.mHeadIndex].OpenPumpDelay);
        }

        private void ComboBoxSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (this.ComboBoxCleaningHeadSpeed.SelectedItem is string value)
            {
                DataHelper.SaveString(this.mHeadIndex, value, ref ParamsHelper.CommonSettingList[this.mHeadIndex].CleaningHeadAbsorbAirAfterJetSpeed);
            }
        }

        // private void BtnOnClick(object sender, RoutedEventArgs e)
        // {
        //     if (sender.Equals(this.BtnCleaningFill))
        //     {
        //         CmdHelperPump.CleaningLiquidFill(ParamsHelper.CommonSettingList[0].CleaningLiquidVolume, false, false);
        //     }
        //     else if (sender.Equals(this.BtnCleaningDrain))
        //     {
        //         CmdHelperPump.CleaningLiquidDrain(ParamsHelper.CommonSettingList[0].CleaningLiquidVolume, false, false);
        //     }
        // }
    }
}
