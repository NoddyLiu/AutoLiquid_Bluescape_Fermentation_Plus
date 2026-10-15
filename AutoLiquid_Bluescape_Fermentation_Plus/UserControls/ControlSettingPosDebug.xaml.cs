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
    /// 位置微调控件
    /// </summary>
    public partial class ControlSettingPosDebug : UserControl
    {
        // 移液头Index
        private int mHeadIndex;

        public ControlSettingPosDebug(int headIndex)
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
            if (ParamsHelper.Debug.IsThin)
                this.RBtnDebugThin.IsChecked = true;
            else
                this.RBtnDebugThick.IsChecked = true;
        }

        private void ControlEvent()
        {
            this.RBtnDebugThin.Checked += RBtnOnChecked;
            this.RBtnDebugThick.Checked += RBtnOnChecked;
        }

        private void RBtnOnChecked(object sender, RoutedEventArgs e)
        {
            if (sender.Equals(this.RBtnDebugThin))
                DataHelper.SaveBool(this.mHeadIndex, true, ref ParamsHelper.Debug.IsThin, () => { FileUtils.SaveDebug(ParamsHelper.Debug); });
            else if (sender.Equals(this.RBtnDebugThick))
                DataHelper.SaveBool(this.mHeadIndex, false, ref ParamsHelper.Debug.IsThin, () => { FileUtils.SaveDebug(ParamsHelper.Debug); });
        }
    }
}
