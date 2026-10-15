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
using DataHelper = AutoLiquid_Bluescape_Fermentation_Plus.Utils.DataHelper;

namespace AutoLiquid_Bluescape_Fermentation_Plus.UserControls
{
    /// <summary>
    /// 盘位摆放控件
    /// </summary>
    public partial class ControlSettingPlatePutting : UserControl
    {
        // 移液头Index
        private int mHeadIndex;

        public ControlSettingPlatePutting(int headIndex)
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
            if (ParamsHelper.CommonSettingList[this.mHeadIndex].A1Pos == EA1Pos.LeftTop)
                this.RBtnLeftTop.IsChecked = true;
            else
                this.RBtnLeftBottom.IsChecked = true;
        }

        private void ControlEvent()
        {
            this.RBtnLeftTop.Checked += RBtnOnChecked;
            this.RBtnLeftBottom.Checked += RBtnOnChecked;
        }

        private void RBtnOnChecked(object sender, RoutedEventArgs e)
        {
            var a1Pos = EA1Pos.LeftTop;
            if (sender.Equals(this.RBtnLeftBottom))
                a1Pos = EA1Pos.LeftBottom;
            // 两个移液头一起修改，保持一致
            DataHelper.SaveA1Pos(a1Pos);
        }
    }
}
