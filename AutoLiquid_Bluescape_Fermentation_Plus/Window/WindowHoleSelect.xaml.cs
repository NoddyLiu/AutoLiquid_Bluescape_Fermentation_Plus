using AutoLiquid_Bluescape_Fermentation_Plus.EntityCommon;
using AutoLiquid_Bluescape_Fermentation_Plus.EntityJson;
using AutoLiquid_Bluescape_Fermentation_Plus.Utils;
using AutoLiquid_Library.Enum;
using MahApps.Metro.Controls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;

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

        // 标记窗口是否已初始化完成（防止双击打开对话框时的"穿透"点击误修改为选中状态）
        private bool mIsInitialized = false;

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

            // === 新增：避免双击打开对话框时的"穿透"点击误修改为选中状态 ===
            this.Loaded += (s, e) =>
            {
                // 延迟到 ContextIdle 优先级，确保双击事件链全部处理完成后才允许点击事件生效
                this.Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.ContextIdle, new Action(() =>
                {
                    this.mIsInitialized = true;
                }));
            };
            // === 新增结束 ===
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

        //private void LoadHoles()
        //{
        //    StackPanelTipsBox.Children.Clear();
        //    holes = new List<Hole>();
        //    var labelFontSize = (int)Math.Round(holeSize, MidpointRounding.AwayFromZero) / 3;
        //    // A1位置
        //    Brush borderColor = Brushes.Gray;


        //    for (var row = 0; row < this.mHoleRowCount + 1; row++)
        //    {
        //        var eachRowStackPanel = ViewUtils.CreateRowStackPanel();

        //        // === 新增：错位布局，偶数行（B/D/F/H）向左偏移半个孔距 ===
        //        if (this.mConsumable.Staggered && row > 0 && row % 2 == 0)
        //        {
        //            eachRowStackPanel.RenderTransform = new TranslateTransform(-holeSize / 2, 0);
        //        }
        //        // === 新增结束 ===

        //        for (var col = 0; col < this.mHoleColCount + 1; col++)
        //        {
        //            var grid = ViewUtils.CreateGrid(holeSize);

        //            // 第一列
        //            if (col == 0 && row > 0)
        //            {
        //                // 字母
        //                var letter = ViewUtils.CreateLabel(holeSize, ViewUtils.PosLetterList[row - 1], labelFontSize);
        //                grid.Children.Add(letter);
        //            }
        //            // 第一行
        //            else if (row == 0 && col > 0)
        //            {
        //                // 数字
        //                var num = ViewUtils.CreateLabel(holeSize, ViewUtils.PosNumList[col - 1], labelFontSize - 1);
        //                grid.Children.Add(num);
        //            }
        //            else if (row > 0 && col > 0)
        //            {
        //                var holeIndex = (col - 1) * this.mHoleRowCount + row - 1;

        //                // 孔
        //                var hole = new Hole { Circle = ViewUtils.CreateEllipse(holeSize, borderColor) };
        //                var letter = ViewUtils.PosLetterList[row - 1];
        //                var num = ViewUtils.PosNumList[col - 1];

        //                hole.Circle.Fill = Brushes.White;
        //                hole.Circle.ToolTip = letter + num;

        //                hole.Index = holeIndex;
        //                holes.Add(hole);

        //                hole.Circle.MouseLeftButtonUp += (sender, args) =>
        //                {
        //                    // === 新增：防止对话框打开时的"穿透"点击误修改为选中状态 ===
        //                    if (!this.mIsInitialized) return;
        //                    // === 新增结束 ===

        //                    // 当前没有选择
        //                    if (this.mHoleFromIndex.OriIndex == -1 && this.mHoleToIndex.OriIndex == -1)
        //                    {
        //                        this.mHoleFromIndex.OriIndex = holeIndex;
        //                        this.mHoleToIndex.OriIndex = holeIndex;
        //                    }
        //                    // 当前只选了开始孔
        //                    else if (this.mHoleFromIndex.OriIndex == this.mHoleToIndex.OriIndex)
        //                    {
        //                        if (holeIndex >= this.mHoleFromIndex.OriIndex)
        //                            this.mHoleToIndex.OriIndex = holeIndex;
        //                        else
        //                            this.mHoleFromIndex.OriIndex = holeIndex;
        //                    }
        //                    // 当前已选择开始孔和结束孔
        //                    else
        //                    {
        //                        this.mHoleFromIndex.OriIndex = holeIndex;
        //                        this.mHoleToIndex.OriIndex = holeIndex;
        //                    }

        //                    SelectHoles(this.mHoleFromIndex, this.mHoleToIndex);
        //                };

        //                grid.Children.Add(hole.Circle);
        //            }
        //            eachRowStackPanel.Children.Add(grid);
        //        }
        //        StackPanelTipsBox.Children.Add(eachRowStackPanel);
        //    }
        //}
        private void LoadHoles()
        {
            StackPanelTipsBox.Children.Clear();
            holes = new List<Hole>();
            var labelFontSize = (int)Math.Round(holeSize, MidpointRounding.AwayFromZero) / 3;
            Brush borderColor = Brushes.Gray;

            for (var row = 0; row < this.mHoleRowCount + 1; row++)
            {
                if (this.mConsumable.Staggered)
                {
                    var rowGrid = new Grid();
                    rowGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(holeSize) });
                    rowGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Auto) });

                    var leftGrid = ViewUtils.CreateGrid(holeSize);
                    if (row > 0)
                    {
                        var letter = ViewUtils.CreateLabel(holeSize, ViewUtils.PosLetterList[row - 1], labelFontSize);
                        leftGrid.Children.Add(letter);
                    }
                    Grid.SetColumn(leftGrid, 0);
                    rowGrid.Children.Add(leftGrid);

                    var rightPanel = new StackPanel
                    {
                        Orientation = Orientation.Horizontal,
                        HorizontalAlignment = HorizontalAlignment.Left,
                        Margin = new Thickness(holeSize / 2, 0, 0, 0)
                    };

                    if (row > 0 && row % 2 == 1)
                    {
                        // === 错位布局：奇数行（A/C/E/G，行索引 1/3/5/7）整体右移半个孔间距（halfStep）
                        //孔间距 = holeSize(孔径) + holeSize*0.5(右外边距) = 1.5*holeSize
                        //     halfStep = 0.75*holeSize
                        //     这样 A/C/E/G 行每个孔就落在"上下两偶数行（B/D/F/H）相邻两孔"的正中间 = brick pattern
                        //     例如：A1 (X=2.75) 正好在 B1 (X=2.0) 和 B2 (X=3.5) 的正中
                        //           此时 A1 在 B1 的偏右，符合发酵瓶250ml的实际视觉
                        rightPanel.RenderTransform = new TranslateTransform(holeSize * 0.75, 0);
                    }

                    for (var col = 1; col < this.mHoleColCount + 1; col++)
                    {
                        var grid = ViewUtils.CreateGrid(holeSize);
                        // === 调大 X 方向间距 ===
                        grid.Margin = new Thickness(0, 0, holeSize * 0.5, 0);

                        if (row == 0)
                        {
                            var num = ViewUtils.CreateLabel(holeSize, ViewUtils.PosNumList[col - 1], labelFontSize - 1);
                            grid.Children.Add(num);
                        }
                        else
                        {
                            var holeIndex = (col - 1) * this.mHoleRowCount + row - 1;
                            var hole = new Hole { Circle = ViewUtils.CreateEllipse(holeSize, borderColor) };
                            var letterStr = ViewUtils.PosLetterList[row - 1];
                            var numStr = ViewUtils.PosNumList[col - 1];

                            hole.Circle.Fill = Brushes.White;
                            hole.Circle.ToolTip = letterStr + numStr;
                            hole.Index = holeIndex;
                            holes.Add(hole);

                            hole.Circle.MouseLeftButtonUp += (sender, args) =>
                            {
                                if (!this.mIsInitialized) return;
                                if (this.mHoleFromIndex.OriIndex == -1 && this.mHoleToIndex.OriIndex == -1)
                                {
                                    this.mHoleFromIndex.OriIndex = holeIndex;
                                    this.mHoleToIndex.OriIndex = holeIndex;
                                }
                                else if (this.mHoleFromIndex.OriIndex == this.mHoleToIndex.OriIndex)
                                {
                                    // === 错位布局：起点和终点在同一视觉列时，吸附到整列 ===
                                    //     例：先点 B1(1) 再点 H1(7)，两个 key 都是 1 → 吸附成 (1, 7)
                                    //     然后 SelectHoles 再按视觉列过滤，结果只高亮 B1/D1/F1/H1（4 个）
                                    if (this.mConsumable.Staggered
                                        && GetVisualColKey(this.mHoleFromIndex.OriIndex) == GetVisualColKey(holeIndex))
                                    {
                                        var (snappedLo, snappedHi) = SnapToVisualColumn(this.mHoleFromIndex.OriIndex, holeIndex);
                                        this.mHoleFromIndex.OriIndex = snappedLo;
                                        this.mHoleToIndex.OriIndex = snappedHi;
                                    }
                                    else if (holeIndex >= this.mHoleFromIndex.OriIndex)
                                    {
                                        this.mHoleToIndex.OriIndex = holeIndex;
                                    }
                                    else
                                    {
                                        this.mHoleFromIndex.OriIndex = holeIndex;
                                    }
                                }
                                else
                                {
                                    if (this.mConsumable.Staggered)
                                    {
                                        // === 修正：用刚刚点的那一格当 anchor ===
                                        // 之前用 mHoleFromIndex.OriIndex（已有区间的起点）当 anchor，
                                        // 导致点 B1 时 snap 到 A1 那一列，跨选就变成 A1~H1 全选
                                        int anchor = holeIndex;
                                        var (snappedLo, snappedHi) = SnapToVisualColumn(anchor, holeIndex);
                                        this.mHoleFromIndex.OriIndex = snappedLo;
                                        this.mHoleToIndex.OriIndex = snappedHi;
                                    }
                                    else
                                    {
                                        this.mHoleFromIndex.OriIndex = holeIndex;
                                        this.mHoleToIndex.OriIndex = holeIndex;
                                    }
                                }
                                SelectHoles(this.mHoleFromIndex, this.mHoleToIndex);
                            };

                            grid.Children.Add(hole.Circle);
                        }

                        rightPanel.Children.Add(grid);
                    }
                    Grid.SetColumn(rightPanel, 1);
                    rowGrid.Children.Add(rightPanel);

                    StackPanelTipsBox.Children.Add(rowGrid);
                }
                else
                {
                    // === 非错位布局：使用原始单行 StackPanel 逻辑（保持不变）===
                    var eachRowStackPanel = ViewUtils.CreateRowStackPanel();
                    for (var col = 0; col < this.mHoleColCount + 1; col++)
                    {
                        var grid = ViewUtils.CreateGrid(holeSize);

                        if (col == 0 && row > 0)
                        {
                            var letter = ViewUtils.CreateLabel(holeSize, ViewUtils.PosLetterList[row - 1], labelFontSize);
                            grid.Children.Add(letter);
                        }
                        else if (row == 0 && col > 0)
                        {
                            var num = ViewUtils.CreateLabel(holeSize, ViewUtils.PosNumList[col - 1], labelFontSize - 1);
                            grid.Children.Add(num);
                        }
                        else if (row > 0 && col > 0)
                        {
                            var holeIndex = (col - 1) * this.mHoleRowCount + row - 1;
                            var hole = new Hole { Circle = ViewUtils.CreateEllipse(holeSize, borderColor) };
                            var letterStr = ViewUtils.PosLetterList[row - 1];
                            var numStr = ViewUtils.PosNumList[col - 1];

                            hole.Circle.Fill = Brushes.White;
                            hole.Circle.ToolTip = letterStr + numStr;
                            hole.Index = holeIndex;
                            holes.Add(hole);

                            hole.Circle.MouseLeftButtonUp += (sender, args) =>
                            {
                                if (!this.mIsInitialized) return;
                                if (this.mHoleFromIndex.OriIndex == -1 && this.mHoleToIndex.OriIndex == -1)
                                {
                                    this.mHoleFromIndex.OriIndex = holeIndex;
                                    this.mHoleToIndex.OriIndex = holeIndex;
                                }
                                else if (this.mHoleFromIndex.OriIndex == this.mHoleToIndex.OriIndex)
                                {
                                    if (holeIndex >= this.mHoleFromIndex.OriIndex)
                                        this.mHoleToIndex.OriIndex = holeIndex;
                                    else
                                        this.mHoleFromIndex.OriIndex = holeIndex;
                                }
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
                // 重新校验前先清掉上次的提示
                this.LabelPromptLogicIncorrect.Visibility = Visibility.Hidden;

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
                        var sourceHoleCount = ConsumableHelper.GetHoleCount(this.mHoleFromIndex, this.mHoleToIndex, this.mConsumable);
                        if (sourceHoleCount > 1)
                        {
                            var targetConsumable = ConsumableHelper.GetConsumableType(this.mStep.HeadIndex, this.mStep.TargetConsumable);
                            var targetHoleCount = ConsumableHelper.GetHoleCount(this.mStep.TargetHoleIndexFrom, this.mStep.TargetHoleIndexTo, targetConsumable);
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
                        var targetHoleCount = ConsumableHelper.GetHoleCount(this.mHoleFromIndex, this.mHoleToIndex, this.mConsumable);
                        if (targetHoleCount > 1)
                        {
                            var sourceConsumable = ConsumableHelper.GetConsumableType(this.mStep.HeadIndex, this.mStep.SourceConsumable);
                            var sourceHoleCount = ConsumableHelper.GetHoleCount(this.mStep.SourceHoleIndexFrom, this.mStep.SourceHoleIndexTo, sourceConsumable);
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
                // === 错位布局：
                // - from 和 to 在同一视觉列 → 只高亮该列（用户要的 B1/D1/F1/H1）
                //   - from 和 to 跨视觉列        → 整段高亮（保留老行为，A1↔H1 还是 8 个全亮）
                bool sameVisualCol = this.mConsumable.Staggered
                    && GetVisualColKey(fromIndex.OriIndex) == GetVisualColKey(toIndex.OriIndex);
                int anchorKey = sameVisualCol ? GetVisualColKey(fromIndex.OriIndex) : -1;

                foreach (var hole in holes)
                {
                    var holeIndex = hole.Index;
                    bool inRange = holeIndex >= fromIndex.OriIndex && holeIndex <= toIndex.OriIndex;
                    bool inVisualCol = !sameVisualCol || GetVisualColKey(holeIndex) == anchorKey;
                    hole.Circle.Fill = (inRange && inVisualCol) ? Brushes.Blue : Brushes.White;
                }
            }
        }

        /// <summary>
        /// 计算 OriIndex 所在的视觉列 Key（错位布局专用）：
        ///   visualColKey = (xIndex, yIndex % 2)
        ///   同一 Key 的孔位才是"视觉上同一列"
        /// </summary>
        private int GetVisualColKey(int oriIndex)
        {
            int xIndex = oriIndex / this.mHoleRowCount;
            int yIndex = oriIndex % this.mHoleRowCount;
            // yIndex%2 表示是错位行的"左"还是"右"位置
            // xIndex 表示列号 0/1/2...
            // 错位布局里视觉列 = (xIndex, yIndex % 2)
            return xIndex * 2 + (yIndex % 2);
        }

        /// <summary>
        /// 把一个新 OriIndex 吸附到 anchorOriIndex 所在视觉列上：
        /// 在 [min(anchor, new), max(anchor, new)] 区间里挑出所有与 anchor 同视觉列的孔位
        /// </summary>
        private (int fromIdx, int toIdx) SnapToVisualColumn(int anchorOriIndex, int newOriIndex)
        {
            int anchorKey = GetVisualColKey(anchorOriIndex);

            int lo = Math.Min(anchorOriIndex, newOriIndex);
            int hi = Math.Max(anchorOriIndex, newOriIndex);

            int snappedLo = anchorOriIndex;
            int snappedHi = anchorOriIndex;

            // === 修正：去掉 break，让循环跨过非视觉列继续找下一个匹配 ===
            for (int i = anchorOriIndex + 1; i <= hi; i++)
            {
                if (GetVisualColKey(i) == anchorKey)
                    snappedHi = i;
            }
            for (int i = anchorOriIndex - 1; i >= lo; i--)
            {
                if (GetVisualColKey(i) == anchorKey)
                    snappedLo = i;
            }
            return (snappedLo, snappedHi);
        }
    }
}
