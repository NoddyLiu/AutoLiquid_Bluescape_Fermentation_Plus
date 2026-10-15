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
using DataHelper = AutoLiquid_Bluescape_Fermentation_Plus.Utils.DataHelper;

namespace AutoLiquid_Bluescape_Fermentation_Plus.UserControls
{
    /// <summary>
    /// 体积设置控件
    /// </summary>
    public partial class ControlSettingVolume : UserControl
    {
        // 移液头Index
        private int mHeadIndex;

        public ControlSettingVolume(int headIndex)
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
            this.TextBoxAbsorbAirBeforePercent.Text = ParamsHelper.CommonSettingList[this.mHeadIndex].AbsorbAirBeforePercent.ToString();
            this.TextBoxAbsorbAirAfterPercent.Text = ParamsHelper.CommonSettingList[this.mHeadIndex].AbsorbAirAfterPercent.ToString();
            this.TextBoxAbsorbLiquidMorePercent.Text = ParamsHelper.CommonSettingList[this.mHeadIndex].AbsorbLiquidMoreOne2MorePercent.ToString();
            this.TextBoxJetLiquidMoreScale.Text = ParamsHelper.CommonSettingList[this.mHeadIndex].JetLiquidMoreOne2MoreScale.ToString();
        }

        private void ControlEvent()
        {
            this.TextBoxAbsorbAirBeforePercent.TextChanged += TextBoxOnTextChanged;
            this.TextBoxAbsorbAirAfterPercent.TextChanged += TextBoxOnTextChanged;
            this.TextBoxAbsorbLiquidMorePercent.TextChanged += TextBoxOnTextChanged;
            this.TextBoxJetLiquidMoreScale.TextChanged += TextBoxOnTextChanged;
        }

        private void TextBoxOnTextChanged(object sender, TextChangedEventArgs e)
        {
            if (sender.Equals(this.TextBoxAbsorbAirBeforePercent))
                DataHelper.SaveDecimal(this.mHeadIndex, this.TextBoxAbsorbAirBeforePercent.Text.Trim(), ref ParamsHelper.CommonSettingList[this.mHeadIndex].AbsorbAirBeforePercent);
            else if (sender.Equals(this.TextBoxAbsorbAirAfterPercent))
                DataHelper.SaveDecimal(this.mHeadIndex, this.TextBoxAbsorbAirAfterPercent.Text.Trim(), ref ParamsHelper.CommonSettingList[this.mHeadIndex].AbsorbAirAfterPercent);
            else if (sender.Equals(this.TextBoxAbsorbLiquidMorePercent))
                DataHelper.SaveDecimal(this.mHeadIndex, this.TextBoxAbsorbLiquidMorePercent.Text.Trim(), ref ParamsHelper.CommonSettingList[this.mHeadIndex].AbsorbLiquidMoreOne2MorePercent);
            else if (sender.Equals(this.TextBoxJetLiquidMoreScale))
                DataHelper.SaveDecimal(this.mHeadIndex, this.TextBoxJetLiquidMoreScale.Text.Trim(), ref ParamsHelper.CommonSettingList[this.mHeadIndex].JetLiquidMoreOne2MoreScale);
        }
    }
}
