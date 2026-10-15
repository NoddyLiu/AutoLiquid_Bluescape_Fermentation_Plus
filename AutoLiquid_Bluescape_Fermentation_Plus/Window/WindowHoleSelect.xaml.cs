using AutoLiquid_Library.Enum;
using MahApps.Metro.Controls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Media;
using System.Windows.Shapes;
using AutoLiquid_Bluescape_Fermentation_Plus.EntityCommon;
using AutoLiquid_Bluescape_Fermentation_Plus.EntityJson;
using AutoLiquid_Bluescape_Fermentation_Plus.Utils;

namespace AutoLiquid_Bluescape_Fermentation_Plus.Window
{
    /// <summary>
    /// 孔位选择窗体
    /// </summary>
    public partial class WindowHoleSelect : MetroWindow
    {
        // 盘类型（源盘 或 靶盘）
        private ETemplateType mTemplateType;

        // 盘名
        private string mTemplateName;

        // 孔选择index
        private HoleIndex mHoleFromIndex;
        private HoleIndex mHoleToIndex;
        // 孔原来选择的index（用户的选择不符合逻辑时，用于复位mHoleFromIndex和mHoleToIndex的OriIndex属性）
        private int mHoleFromIndexOri;
        private int mHoleToIndexOri;

        // 耗材类型
        private Consumable mConsumable;
        private int mHoleRowCount, mHoleColCount;

        // 选择的步骤
        private ProcedureStep mStep;

        // 孔大小
        private double holeSize = 50;

        private List<Hole> holes = new List<Hole>();

        public WindowHoleSelect(ETemplateType templateType, string templateName, HoleIndex holeFromIndex, HoleIndex holeToIndex, Consumable consumable, ProcedureStep step)
        {
            InitializeComponent();

            this.mTemplateType = templateType;
            this.mTemplateName = templateName;
            this.mHoleFromIndex = holeFromIndex;
            this.mHoleToIndex = holeToIndex;
            this.mConsumable = consumable;
            this.mHoleFromIndexOri = holeFromIndex.OriIndex;
            this.mHoleToIndexOri = holeToIndex.OriIndex;
            this.mHoleRowCount = this.mConsumable.RowCount;
            this.mHoleColCount = this.mConsumable.ColCount;
            this.mStep = step;

            InitWidget();

            ControlEvent();
        }

        private void InitWidget()
        {
            ViewUtils.ShowLogo(this);

            this.TextBlockTemplateName.Text = this.mStep.Name + "---" + this.mTemplateName;

            LoadHoles();

            SelectHoles(this.mHoleFromIndex, this.mHoleToIndex);

            this.Width = holeSize * (this.mHoleColCount + 1) + 280;
            this.Height = holeSize * (this.mHoleRowCount + 1) + 280;
        }

        private void LoadHoles()
        {
            StackPanelTipsBox.Children.Clear();
            holes = new List<Hole>();
            var labelFontSize = (int)Math.Round(holeSize, MidpointRounding.AwayFromZero) / 3;
            // A1位置
            Brush borderColor = Brushes.Gray;


            for (var row = 0; row < this.mHoleRowCount + 1; row++)
            {
                var eachRowStackPanel = ViewUtils.CreateRowStackPanel();
                for (var col = 0; col < this.mHoleColCount + 1; col++)
                {
                    var grid = ViewUtils.CreateGrid(holeSize);

                    // 第一列
                    if (col == 0 && row > 0)
                    {
                        // 字母
                        var letter = ViewUtils.CreateLabel(holeSize, ViewUtils.PosLetterList[row - 1], labelFontSize);
                        grid.Children.Add(letter);
                    }
                    // 第一行
                    else if (row == 0 && col > 0)
                    {
                        // 数字
                        var num = ViewUtils.CreateLabel(holeSize, ViewUtils.PosNumList[col - 1], labelFontSize - 1);
                        grid.Children.Add(num);
                    }
                    else if (row > 0 && col > 0)
                    {
                        var holeIndex = (col - 1) * this.mHoleRowCount + row - 1;

                        // 孔
                        var hole = new Hole { Circle = ViewUtils.CreateEllipse(holeSize, borderColor) };
                        var letter = ViewUtils.PosLetterList[row - 1];
                        var num = ViewUtils.PosNumList[col - 1];

                        hole.Circle.Fill = Brushes.White;
                        hole.Circle.ToolTip = letter + num;

                        hole.Index = holeIndex;
                        holes.Add(hole);

                        hole.Circle.MouseLeftButtonUp += (sender, args) =>
                        {
                            // 当前没有选择
                            if (this.mHoleFromIndex.OriIndex == -1 && this.mHoleToIndex.OriIndex == -1)
                            {
                                this.mHoleFromIndex.OriIndex = holeIndex;
                                this.mHoleToIndex.OriIndex = holeIndex;
                            }
                            // 当前只选了开始孔
                            else if (this.mHoleFromIndex.OriIndex == this.mHoleToIndex.OriIndex)
                            {
                                if (holeIndex >= this.mHoleFromIndex.OriIndex)
                                    this.mHoleToIndex.OriIndex = holeIndex;
                                else
                                    this.mHoleFromIndex.OriIndex = holeIndex;
                            }
                            // 当前已选择开始孔和结束孔
                            else
                            {
                                this.mHoleFromIndex.OriIndex = holeIndex;
                                this.mHoleToIndex.OriIndex = holeIndex;
                            }

                            SelectHoles(this.mHoleFromIndex, this.mHoleToIndex);
                        };

                        grid.Children.Add(hole.Circle);
                    }
                    eachRowStackPanel.Children.Add(grid);
                }
                StackPanelTipsBox.Children.Add(eachRowStackPanel);
            }
        }

        private void ControlEvent()
        {
            this.BtnSave.Click += BtnOnClick;
            this.BtnCancel.Click += BtnOnClick;
            this.cbSelectAll.Click += CbSelectAll_Click;
        }

        private void CbSelectAll_Click(object sender, RoutedEventArgs e)
        {
            if (cbSelectAll.IsChecked.Value)
            {
                this.mHoleFromIndex.OriIndex = 0;
                this.mHoleToIndex.OriIndex = this.mHoleRowCount * this.mHoleColCount - 1;
            }
            else
            {
                this.mHoleFromIndex.OriIndex = -1;
                this.mHoleToIndex.OriIndex = -1;
            }

            SelectHoles(this.mHoleFromIndex, this.mHoleToIndex);
        }


        private void BtnOnClick(object sender, RoutedEventArgs e)
        {
            if (sender.Equals(this.BtnSave))
            {
                // 全不选
                if (this.mHoleFromIndex.OriIndex == -1 || this.mHoleToIndex.OriIndex == -1)
                {
                    this.mHoleFromIndex.OriIndex = 0;
                    this.mHoleToIndex.OriIndex = 0;
                    FileUtils.SaveProcedureAll(ParamsHelper.ProcedureAll);
                }
                else
                {
                    /**
                    * 逻辑判断：
                    * 1、源盘 --> 靶盘 ：1 --> 1~N （一吸多喷）
                    * 2、源盘 --> 靶盘 ：N --> 1 （一吸一喷）
                    * 3、源盘 --> 靶盘 ：N --> N （两者数量必须相等，即孔孔对应，一吸一喷）
                    */
                    if (mTemplateType == ETemplateType.Source)
                    {
                        var sourceHoleCount = ConsumableHelper.GetHoleCount(this.mHoleFromIndex, this.mHoleToIndex);
                        if (sourceHoleCount > 1)
                        {
                            var targetHoleCount = ConsumableHelper.GetHoleCount(this.mStep.TargetHoleIndexFrom, this.mStep.TargetHoleIndexTo);
                            if (targetHoleCount != 1)
                            {
                                if (sourceHoleCount != targetHoleCount)
                                {
                                    this.LabelPromptLogicIncorrect.Visibility = Visibility.Visible;
                                    return;
                                }
                            }
                        }
                    }
                    else if (mTemplateType == ETemplateType.Target)
                    {
                        var targetHoleCount = ConsumableHelper.GetHoleCount(this.mHoleFromIndex, this.mHoleToIndex);
                        if (targetHoleCount > 1)
                        {
                            var sourceHoleCount = ConsumableHelper.GetHoleCount(this.mStep.SourceHoleIndexFrom, this.mStep.SourceHoleIndexTo);
                            if (sourceHoleCount != 1)
                            {
                                if (targetHoleCount != sourceHoleCount)
                                {
                                    this.LabelPromptLogicIncorrect.Visibility = Visibility.Visible;
                                    return;
                                }
                            }
                        }
                    }
                }

                FileUtils.SaveProcedureAll(ParamsHelper.ProcedureAll);

                this.DialogResult = true;
                this.Close();
            }
            else if (sender.Equals(this.BtnCancel))
            {
                // 复位起始位置
                this.mHoleFromIndex.OriIndex = this.mHoleFromIndexOri;
                this.mHoleToIndex.OriIndex = this.mHoleToIndexOri;
                this.DialogResult = false;
                this.Close();
            }
        }

        /// <summary>
        /// 选择孔位并刷新界面
        /// </summary>
        /// <param name="fromIndex"></param>
        /// <param name="toIndex"></param>
        private void SelectHoles(HoleIndex fromIndex, HoleIndex toIndex)
        {
            var holeCount = this.mHoleRowCount * this.mHoleColCount;

            if (fromIndex.OriIndex == -1 && toIndex.OriIndex == -1)
            {
                for (var index = 0; index < holeCount; index++)
                {
                    holes.ElementAt(index).Circle.Fill = Brushes.White;
                }
            }
            else
            {
                foreach (var hole in holes)
                {
                    var holeIndex = hole.Index;
                    hole.Circle.Fill = holeIndex >= fromIndex.OriIndex && holeIndex <= toIndex.OriIndex ? Brushes.Blue : Brushes.White;
                }
            }
        }
    }
}
