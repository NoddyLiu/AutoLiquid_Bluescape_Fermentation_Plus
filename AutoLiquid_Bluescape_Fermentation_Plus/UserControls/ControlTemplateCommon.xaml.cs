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
using AutoLiquid_Bluescape_Fermentation_Plus.EntityCommon;
using AutoLiquid_Bluescape_Fermentation_Plus.Utils;
using AutoLiquid_Library.Enum;
using MahApps.Metro.Controls;

namespace AutoLiquid_Bluescape_Fermentation_Plus.UserControls
{
    /// <summary>
    /// 通用盘位模板
    /// </summary>
    public partial class ControlTemplateCommon : UserControl
    {
        // 板信息
        public Template mTemplate;

        public ControlTemplateCommon(Template template)
        {
            InitializeComponent();

            mTemplate = template;

            this.Loaded += OnLoaded;
        }

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            InitWidget();

            ControlEvent();
        }

        private void InitWidget()
        {
            InitHoles(this.StackPanelTemplate, Brushes.Black);
        }

        private void ControlEvent()
        {

        }

        /// <summary>
        /// 盘孔
        /// </summary>
        /// <param name="parent"></param>
        /// <param name="borderColor"></param>
        //private void InitHoles(Panel parent, Brush borderColor)
        //{
        //    LabelTItle.Content = mTemplate.Title;
        //    LabelSubTItle.Content = mTemplate.SubTitle;

        //    // 判断板类型
        //    var colCount = mTemplate.ColCount;
        //    var rowCount = mTemplate.RowCount;
        //    var labelFontSize = 0;

        //    // A1位置
        //    var a1Pos = mTemplate.A1Pos;

        //    // 每个孔大小（取小值作为孔直径）
        //    var holeSizeHeight = parent.ActualHeight / (rowCount + 1);
        //    var holeSizeWidth = parent.ActualWidth / (colCount + 1);
        //    var holeSize = holeSizeHeight > holeSizeWidth ? holeSizeWidth : holeSizeHeight;
        //    labelFontSize = (int)Math.Round(holeSize, MidpointRounding.AwayFromZero) / 3;

        //    for (var row = 0; row < rowCount + 1; row++)
        //    {
        //        var eachRowStackPanel = ViewUtils.CreateRowStackPanel();

        //        // === 新增：错位布局，偶数行（B/D/F/H）向左偏移半个孔距 ===
        //        if (mTemplate.Staggered && row > 0 && row % 2 == 0)
        //        {
        //            eachRowStackPanel.RenderTransform = new TranslateTransform(-holeSize / 2, 0);
        //        }
        //        // === 新增结束 ===

        //        for (var col = 0; col < colCount + 1; col++)
        //        {
        //            var grid = ViewUtils.CreateGrid(holeSize);

        //            // A1位置：左上（孔index顺序是同一列由上到下，再由左到右）
        //            if (a1Pos == EA1Pos.LeftTop)
        //            {
        //                // 第一列
        //                if (col == 0 && row > 0)
        //                {
        //                    // 字母
        //                    var letter = ViewUtils.CreateLabel(holeSize, ViewUtils.PosLetterList[row - 1], labelFontSize);
        //                    grid.Children.Add(letter);
        //                }
        //                // 第一行
        //                else if (row == 0 && col > 0)
        //                {
        //                    // 数字
        //                    var num = ViewUtils.CreateLabel(holeSize, ViewUtils.PosNumList[col - 1], labelFontSize - 1);
        //                    grid.Children.Add(num);
        //                }
        //                else if (row > 0 && col > 0)
        //                {
        //                    var holeIndex = (col - 1) * rowCount + row - 1;
        //                    var hole = mTemplate.Holes[holeIndex];
        //                    // 孔
        //                    hole.Circle = ViewUtils.CreateEllipse(holeSize, borderColor);

        //                    if (hole.Capacity > 0.0m)
        //                    {
        //                        // 背景
        //                        hole.Circle.Fill = Brushes.Blue;
        //                        // 文字
        //                        hole.Word = ViewUtils.CreateLabel(holeSize, hole.Capacity.ToString(),
        //                            labelFontSize);
        //                        hole.Word.Foreground = Brushes.White;
        //                    }

        //                    grid.Children.Add(hole.Circle);
        //                    if (null != hole.Word)
        //                        grid.Children.Add(hole.Word);
        //                }
        //            }
        //            // A1位置：左下（孔index顺序是同一行由左到右，再由下到上）
        //            else
        //            {
        //                // 第一列
        //                if (col == 0 && row < rowCount)
        //                {
        //                    // 数字
        //                    var num = ViewUtils.CreateLabel(holeSize, ViewUtils.PosNumList[rowCount - row - 1], labelFontSize);
        //                    grid.Children.Add(num);
        //                }
        //                // 最尾行
        //                else if (row == rowCount && col > 0)
        //                {
        //                    // 字母
        //                    var letter = ViewUtils.CreateLabel(holeSize, ViewUtils.PosLetterList[col - 1], labelFontSize - 1);
        //                    grid.Children.Add(letter);
        //                }
        //                else if (row < rowCount && col > 0)
        //                {
        //                    var holeIndex = (rowCount - row - 1) * colCount + (col - 1);
        //                    var hole = mTemplate.Holes[holeIndex];
        //                    // 孔
        //                    hole.Circle = ViewUtils.CreateEllipse(holeSize, borderColor);

        //                    if (hole.Capacity > 0.0m)
        //                    {
        //                        // 背景
        //                        hole.Circle.Fill = Brushes.Blue;
        //                        // 文字
        //                        hole.Word = ViewUtils.CreateLabel(holeSize, hole.Capacity.ToString(),
        //                            labelFontSize);
        //                        hole.Word.Foreground = Brushes.White;
        //                    }

        //                    grid.Children.Add(hole.Circle);
        //                    if (null != hole.Word)
        //                        grid.Children.Add(hole.Word);
        //                }
        //            }

        //            eachRowStackPanel.Children.Add(grid);
        //        }
        //        parent.Children.Add(eachRowStackPanel);
        //    }

        //    parent.HorizontalAlignment = HorizontalAlignment.Center;
        //    parent.VerticalAlignment = VerticalAlignment.Center;
        //}
        private void InitHoles(Panel parent, Brush borderColor)
        {
            LabelTItle.Content = mTemplate.Title;
            LabelSubTItle.Content = mTemplate.SubTitle;

            var colCount = mTemplate.ColCount;
            var rowCount = mTemplate.RowCount;
            var labelFontSize = 0;
            var a1Pos = mTemplate.A1Pos;

            // 错位布局：左列多出一个字母列，所以横向空间需要乘 1.3 倍（避免孔位被压扁）
            var colFactor = (mTemplate.Staggered && a1Pos == EA1Pos.LeftTop) ? 1.3 : 1.0;
            var holeSizeHeight = parent.ActualHeight / (rowCount + 1);
            var holeSizeWidth = parent.ActualWidth / (colCount * colFactor + 1);
            var holeSize = holeSizeHeight > holeSizeWidth ? holeSizeWidth : holeSizeHeight;
            labelFontSize = (int)Math.Round(holeSize, MidpointRounding.AwayFromZero) / 3;

            for (var row = 0; row < rowCount + 1; row++)
            {
                if (mTemplate.Staggered && a1Pos == EA1Pos.LeftTop)
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
                        // === 错位布局：奇数行（A/C/E/G，行索引 1/3/5/7）整体右移半个孔间距 = 0.75*holeSize ===
                        rightPanel.RenderTransform = new TranslateTransform(holeSize * 0.75, 0);
                    }

                    for (var col = 1; col < colCount + 1; col++)
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
                            var holeIndex = (col - 1) * rowCount + row - 1;
                            var hole = mTemplate.Holes[holeIndex];
                            hole.Circle = ViewUtils.CreateEllipse(holeSize, borderColor);

                            if (hole.Capacity > 0.0m)
                            {
                                hole.Circle.Fill = Brushes.Blue;
                                hole.Word = ViewUtils.CreateLabel(holeSize, hole.Capacity.ToString(), labelFontSize);
                                hole.Word.Foreground = Brushes.White;
                            }

                            grid.Children.Add(hole.Circle);
                            if (null != hole.Word)
                                grid.Children.Add(hole.Word);
                        }

                        rightPanel.Children.Add(grid);
                    }
                    Grid.SetColumn(rightPanel, 1);
                    rowGrid.Children.Add(rightPanel);

                    parent.Children.Add(rowGrid);
                }
                else
                {
                    // === 非错位 / LeftBottom：使用原始单行 StackPanel 逻辑（保持不变）===
                    var eachRowStackPanel = ViewUtils.CreateRowStackPanel();
                    for (var col = 0; col < colCount + 1; col++)
                    {
                        var grid = ViewUtils.CreateGrid(holeSize);

                        if (a1Pos == EA1Pos.LeftTop)
                        {
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
                                var holeIndex = (col - 1) * rowCount + row - 1;
                                var hole = mTemplate.Holes[holeIndex];
                                hole.Circle = ViewUtils.CreateEllipse(holeSize, borderColor);

                                if (hole.Capacity > 0.0m)
                                {
                                    hole.Circle.Fill = Brushes.Blue;
                                    hole.Word = ViewUtils.CreateLabel(holeSize, hole.Capacity.ToString(), labelFontSize);
                                    hole.Word.Foreground = Brushes.White;
                                }

                                grid.Children.Add(hole.Circle);
                                if (null != hole.Word)
                                    grid.Children.Add(hole.Word);
                            }
                        }
                        else  // EA1Pos.LeftBottom
                        {
                            if (col == 0 && row < rowCount)
                            {
                                var num = ViewUtils.CreateLabel(holeSize, ViewUtils.PosNumList[rowCount - row - 1], labelFontSize);
                                grid.Children.Add(num);
                            }
                            else if (row == rowCount && col > 0)
                            {
                                var letter = ViewUtils.CreateLabel(holeSize, ViewUtils.PosLetterList[col - 1], labelFontSize - 1);
                                grid.Children.Add(letter);
                            }
                            else if (row < rowCount && col > 0)
                            {
                                var holeIndex = (rowCount - row - 1) * colCount + (col - 1);
                                var hole = mTemplate.Holes[holeIndex];
                                hole.Circle = ViewUtils.CreateEllipse(holeSize, borderColor);

                                if (hole.Capacity > 0.0m)
                                {
                                    hole.Circle.Fill = Brushes.Blue;
                                    hole.Word = ViewUtils.CreateLabel(holeSize, hole.Capacity.ToString(), labelFontSize);
                                    hole.Word.Foreground = Brushes.White;
                                }

                                grid.Children.Add(hole.Circle);
                                if (null != hole.Word)
                                    grid.Children.Add(hole.Word);
                            }
                        }

                        eachRowStackPanel.Children.Add(grid);
                    }
                    parent.Children.Add(eachRowStackPanel);
                }
            }

            parent.HorizontalAlignment = HorizontalAlignment.Center;
            parent.VerticalAlignment = VerticalAlignment.Center;
        }

        public void RefreshTemplateHolesStatus(int headUsedIndex, HoleIndex holeStartIndex, decimal volume)
        {
            // 板行、列数目
            var templateRowCount = this.mTemplate.RowCount;
            var templateColCount = this.mTemplate.ColCount;
            // 总孔数
            var holeTotalCount = templateRowCount * templateColCount;
            // 是否错位显示孔（一般用在384板位）
            var holeMismatch = templateRowCount * templateColCount == 384 && mTemplate.Step.X == 4.5m && mTemplate.Step.Y == 4.5m;

            // 移液头行、列数
            var channelRowCount = 1;
            var channelColCount = 1;

            //  如果移液头是多通道，则相应的孔也设置体积，用于显示
            if (channelRowCount > 1 || channelColCount > 1)
            {
                for (var row = 0; row < channelRowCount; row++)
                {
                    for (var col = 0; col < channelColCount; col++)
                    {
                        // 错位显示孔
                        var rowTickIndex = holeMismatch ? row * 2 : row;
                        var colTickIndex = holeMismatch ? col * 2 : col;

                        if(rowTickIndex >= templateRowCount || colTickIndex >= templateColCount)
                            return;

                        // A1位置：左上
                        if (this.mTemplate.A1Pos == EA1Pos.LeftTop)
                        {
                            var index = holeStartIndex.OriIndex + rowTickIndex + colTickIndex * templateRowCount;
                            if (index < holeTotalCount)
                                mTemplate.Holes[index].Capacity = volume;
                        }
                        // A1位置：左下
                        else
                        {
                            var index = holeStartIndex.OriIndex + colTickIndex + rowTickIndex * templateColCount;
                            if (index < holeTotalCount)
                                mTemplate.Holes[index].Capacity = volume;
                        }
                    }
                }
            }
            else
            {
                mTemplate.Holes[holeStartIndex.OriIndex].Capacity = volume;
            }
        }
    }
}
