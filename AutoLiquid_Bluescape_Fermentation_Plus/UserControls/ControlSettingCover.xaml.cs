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
using AutoLiquid_Library.Enum;
using AutoLiquid_Library.Utils;
using DataHelper = AutoLiquid_Bluescape_Fermentation_Plus.Utils.DataHelper;

namespace AutoLiquid_Bluescape_Fermentation_Plus.UserControls
{
    /// <summary>
    /// 压盖设置控件（弃用）
    /// </summary>
    public partial class ControlSettingCover : UserControl
    {
        // 移液头Index
        private int mHeadIndex;

        public ControlSettingCover(int headIndex)
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
            this.ControlCoverHeight.SetParam(mHeadIndex, ParamsHelper.CommonSettingList[this.mHeadIndex].CoverHeight, EAxis.M);
        }

        private void ControlEvent()
        {
            this.ControlCoverHeight.TextBoxPos.TextChanged += TextBoxOnTextChanged;
        }

        private void TextBoxOnTextChanged(object sender, TextChangedEventArgs e)
        {
            if (sender.Equals(this.ControlCoverHeight.TextBoxPos))
                DataHelper.SaveDecimal(this.mHeadIndex, this.ControlCoverHeight.TextBoxPos.Text.Trim(), ref ParamsHelper.CommonSettingList[this.mHeadIndex].CoverHeight);
        }
    }
}
