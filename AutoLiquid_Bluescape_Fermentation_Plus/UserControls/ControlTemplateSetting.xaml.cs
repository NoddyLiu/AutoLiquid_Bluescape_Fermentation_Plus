using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using AutoLiquid_Bluescape_Fermentation_Plus.EntityJson;
using AutoLiquid_Bluescape_Fermentation_Plus.Utils;
using AutoLiquid_Library.Enum;
using CheckBox = System.Windows.Controls.CheckBox;
using DataHelper = AutoLiquid_Bluescape_Fermentation_Plus.Utils.DataHelper;
using MessageBox = System.Windows.MessageBox;
using TextBox = System.Windows.Controls.TextBox;
using UserControl = System.Windows.Controls.UserControl;

namespace AutoLiquid_Bluescape_Fermentation_Plus.UserControls
{
    /// <summary>
    /// 盘位设置控件
    /// </summary>
    public partial class ControlTemplateSetting : UserControl
    {
        // 耗材Index
        private int mGroupIndex;
        // 耗材
        private Consumable mConsumable;
        // 盘位Index
        public int mTemplateIndex;
        // 移液头Index
        private int mHeadIndex;

        public ControlTemplateSetting(Consumable mConsumable, int groupIndex, int templateIndex, int headIndex)
        {
            InitializeComponent();

            this.mGroupIndex = groupIndex;
            this.mTemplateIndex = templateIndex;
            this.mHeadIndex = headIndex;
            this.mConsumable = mConsumable;

            InitWidget();
            ControlEvent();
        }

        private void InitWidget()
        {
            // 标题
            this.LabelTemplateName.Content = (string)this.FindResource("Template") + (this.mTemplateIndex + 1);

            // 孔位置
            this.ControlTemplateHole1X.SetParam(mHeadIndex, this.mConsumable.HoleStartPosList[this.mTemplateIndex].X, EAxis.X);
            this.ControlTemplateHole1Y.SetParam(mHeadIndex, this.mConsumable.HoleStartPosList[this.mTemplateIndex].Y, EAxis.Y);
        }

        private void ControlEvent()
        {
            this.ControlTemplateHole1X.TextBoxPos.TextChanged += TextBoxPosOnTextChanged;
            this.ControlTemplateHole1Y.TextBoxPos.TextChanged += TextBoxPosOnTextChanged;
        }

        private void TextBoxPosOnTextChanged(object sender, TextChangedEventArgs e)
        {
            if (sender.Equals(this.ControlTemplateHole1X.TextBoxPos))
                DataHelper.SaveDecimal(this.mHeadIndex, this.ControlTemplateHole1X.TextBoxPos.Text.Trim(), ref this.mConsumable.HoleStartPosList[this.mTemplateIndex].X);
            else if (sender.Equals(this.ControlTemplateHole1Y.TextBoxPos))
                DataHelper.SaveDecimal(this.mHeadIndex, this.ControlTemplateHole1Y.TextBoxPos.Text.Trim(), ref this.mConsumable.HoleStartPosList[this.mTemplateIndex].Y);
        }
    }
}
