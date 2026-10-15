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
    /// 废液槽设置控件
    /// </summary>
    public partial class ControlSettingWaste : UserControl
    {
        // 移液头Index
        private int mHeadIndex;

        public ControlSettingWaste(int headIndex)
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
            this.ControlWastePosX.SetParam(mHeadIndex, ParamsHelper.CommonSettingList[this.mHeadIndex].WastePos.X, EAxis.X);
            this.ControlWastePosY.SetParam(mHeadIndex, ParamsHelper.CommonSettingList[this.mHeadIndex].WastePos.Y, EAxis.Y);
            this.ControlWastePosZ.SetParam(mHeadIndex, ParamsHelper.CommonSettingList[this.mHeadIndex].WastePos.Z, EAxis.Z);
        }

        private void ControlEvent()
        {
            this.ControlWastePosX.TextBoxPos.TextChanged += TextBoxOnTextChanged;
            this.ControlWastePosY.TextBoxPos.TextChanged += TextBoxOnTextChanged;
            this.ControlWastePosZ.TextBoxPos.TextChanged += TextBoxOnTextChanged;
        }

        private void TextBoxOnTextChanged(object sender, TextChangedEventArgs e)
        {
            if (sender.Equals(this.ControlWastePosX.TextBoxPos))
                DataHelper.SaveDecimal(this.mHeadIndex, this.ControlWastePosX.TextBoxPos.Text.Trim(), ref ParamsHelper.CommonSettingList[this.mHeadIndex].WastePos.X);
            else if (sender.Equals(this.ControlWastePosY.TextBoxPos))
                DataHelper.SaveDecimal(this.mHeadIndex, this.ControlWastePosY.TextBoxPos.Text.Trim(), ref ParamsHelper.CommonSettingList[this.mHeadIndex].WastePos.Y);
            else if (sender.Equals(this.ControlWastePosZ.TextBoxPos))
                DataHelper.SaveDecimal(this.mHeadIndex, this.ControlWastePosZ.TextBoxPos.Text.Trim(), ref ParamsHelper.CommonSettingList[this.mHeadIndex].WastePos.Z);
        }
    }
}
