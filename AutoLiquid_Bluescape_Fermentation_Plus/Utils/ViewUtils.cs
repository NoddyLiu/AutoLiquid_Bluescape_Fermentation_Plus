using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using AutoLiquid_Library.Enum;
using Spire.Xls;
using Path = System.Windows.Shapes.Path;

namespace AutoLiquid_Bluescape_Fermentation_Plus.Utils
{
    /// <summary>
    /// 界面工具类
    /// </summary>
    public class ViewUtils
    {
        // 位置信息
        public static List<string> PosLetterList = new List<string>
        {
            "A", "B", "C", "D", "E", "F", "G", "H","I","J","K", "L", "M", "N", "O", "P","Q","R","S","T","U","V","W","X","Y","Z"
        };
        public static List<string> PosNumList = new List<string>
        {
            "1", "2", "3", "4", "5", "6", "7", "8","9","10","11","12","13", "14", "15", "16", "17", "18", "19", "20","21","22","23","24","25","26","27","28","29","30"
        };

        /// <summary>
        ///  根据index返回孔位置（例如返回A1、B2等）
        /// </summary>
        /// <param name="holeIndex"></param>
        /// <param name="rowCount"></param>
        /// <param name="colCount"></param>
        /// <param name="a1Pos"></param>
        /// <returns></returns>
        public static string HolePosStr(int holeIndex, int rowCount, int colCount, EA1Pos a1Pos)
        {
            var result = "";
            if (a1Pos == EA1Pos.LeftTop)
            {
                var rowIndex = holeIndex % rowCount;
                var colIndex = holeIndex / rowCount;
                result = PosLetterList[rowIndex] + PosNumList[colIndex];
            }
            else
            {
                var rowIndex = holeIndex / colCount;
                var colIndex = holeIndex % colCount;
                result = PosLetterList[colIndex] + PosNumList[rowIndex];
            }

            return result;
        }

        /// <summary>
        /// 创建列StackPanel
        /// </summary>
        /// <returns></returns>
        public static StackPanel CreateRowStackPanel()
        {
            var eachRowStackPanel = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                HorizontalAlignment = HorizontalAlignment.Center
            };
            return eachRowStackPanel;
        }

        /// <summary>
        /// 创建Grid
        /// </summary>
        /// <param name="holeSize"></param>
        /// <returns></returns>
        public static Grid CreateGrid(double holeSize)
        {
            var grid = new Grid
            {
                Width = holeSize,
                Height = holeSize,
            };

            return grid;
        }

        /// <summary>
        /// 创建圆
        /// </summary>
        /// <param name="holeSize"></param>
        /// <param name="borderColor"></param>
        /// <returns></returns>
        public static Ellipse CreateEllipse(double holeSize, Brush borderColor)
        {
            var hole = new Ellipse
            {
                Fill = Brushes.White,
                StrokeThickness = 2,
                Stroke = borderColor,
                HorizontalAlignment = HorizontalAlignment.Center,
                Width = holeSize,
                Height = holeSize,
            };

            return hole;
        }

        /// <summary>
        /// 创建Label
        /// </summary>
        /// <param name="holeSize"></param>
        /// <param name="text"></param>
        /// <param name="fontSize"></param>
        /// <returns></returns>
        public static Label CreateLabel(double holeSize, string text, double fontSize)
        {
            var tb = new Label()
            {
                HorizontalContentAlignment = HorizontalAlignment.Center,
                VerticalContentAlignment = VerticalAlignment.Center,
                Background = Brushes.Transparent,
                Content = text,
                FontSize = fontSize<=1? 1 :fontSize,
                Width = holeSize,
                Height = holeSize
            };

            return tb;
        }

        /// <summary>
        /// 设置父控件下所有子控件的isEnable属性（CheckBox除外）
        /// </summary>
        /// <param name="parent"></param>
        /// <param name="isEnable"></param>
        /// <returns></returns>
        public static void SetEnableExceptCheckbox(Panel parent, bool isEnable)
        {
            foreach (UIElement element in parent.Children)
            {
                if (element.GetType() == typeof(StackPanel))
                {
                    var stackPanel = element as StackPanel;
                    SetEnableExceptCheckbox(stackPanel, isEnable);

                }
                else if (element.GetType() == typeof(Grid))
                {
                    var grid = element as Grid;
                    SetEnableExceptCheckbox(grid, isEnable);
                }
                else
                {
                    if (element.GetType() != typeof(CheckBox))
                        element.IsEnabled = isEnable;
                }
            }
        }

        /// <summary>
        /// 设置父控件下所有子控件的isEnable属性（CheckBox、Button除外）
        /// </summary>
        /// <param name="parent"></param>
        /// <param name="isEnable"></param>
        /// <returns></returns>
        public static void SetEnableExceptCheckboxAndButton(Panel parent, bool isEnable)
        {
            foreach (UIElement element in parent.Children)
            {
                if (element.GetType() == typeof(StackPanel))
                {
                    var stackPanel = element as StackPanel;
                    SetEnableExceptCheckbox(stackPanel, isEnable);

                }
                else if (element.GetType() == typeof(Grid))
                {
                    var grid = element as Grid;
                    SetEnableExceptCheckbox(grid, isEnable);
                }
                else
                {
                    if (element.GetType() != typeof(CheckBox))
                        element.IsEnabled = isEnable;
                    if (element.GetType() != typeof(Button))
                        element.IsEnabled = isEnable;
                }
            }
        }

        /// <summary>
        /// 获取指定父类的所有子控件
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="depObj"></param>
        /// <returns></returns>
        public static IEnumerable<T> FindLogicalChildren<T>(DependencyObject depObj) where T : DependencyObject
        {
            if (depObj != null)
            {
                foreach (object rawChild in LogicalTreeHelper.GetChildren(depObj))
                {
                    if (rawChild is DependencyObject)
                    {
                        DependencyObject child = (DependencyObject)rawChild;
                        if (child is T)
                        {
                            yield return (T)child;
                        }

                        foreach (T childOfChild in FindLogicalChildren<T>(child))
                        {
                            yield return childOfChild;
                        }
                    }
                }
            }
        }

        /// <summary>
        /// 显示Logo
        /// </summary>
        /// <param name="window"></param>
        public static void ShowLogo(System.Windows.Window window)
        {
            var logoFIle = AppDomain.CurrentDomain.BaseDirectory + System.IO.Path.DirectorySeparatorChar + "logo.png";
            if (File.Exists(logoFIle))
                window.Icon = new BitmapImage(new Uri(logoFIle));
        }

        /// <summary>
        /// 检查Excel内容是否为空
        /// </summary>
        /// <param name="cell"></param>
        /// <returns></returns>
        public static bool CheckExcelCellEmpty(CellRange cell)
        {
            var value = cell.Value;
            return value == null || value.Trim().Equals("");
        }
    }
}
