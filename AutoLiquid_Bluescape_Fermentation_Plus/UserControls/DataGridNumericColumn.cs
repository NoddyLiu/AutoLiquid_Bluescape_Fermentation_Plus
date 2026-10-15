using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Animation;

namespace AutoLiquid_Bluescape_Fermentation_Plus.UserControls
{
    /// <summary>
    /// 自定义数字列
    /// </summary>
    public class DataGridNumericColumn : DataGridTextColumn
    {
        private decimal oldValue = 0.0m;

        protected override object PrepareCellForEdit(FrameworkElement editingElement, RoutedEventArgs editingEventArgs)
        {
            TextBox edit = editingElement as TextBox;
            edit.GotFocus += EditOnGotFocus;
            edit.PreviewTextInput += OnPreviewTextInput;

            return base.PrepareCellForEdit(editingElement, editingEventArgs);
        }

        private void EditOnGotFocus(object sender, RoutedEventArgs e)
        {
            oldValue = decimal.Parse(((TextBox)sender).Text);
        }

        void OnPreviewTextInput(object sender, System.Windows.Input.TextCompositionEventArgs e)
        {
            // 判断是否为非数字
            Regex re = new Regex("[^0-9.-]+");
            var result = re.IsMatch(e.Text);
            if (result)
            {
                Dispatcher.Invoke(() =>
                {
                    MessageBox.Show((string)App.Current.FindResource("Prompt_Pls_Input_Correct_Value"));
                    // 输入数值不正确，恢复为原来的数值
                    ((TextBox)sender).Text = oldValue.ToString();
                    e.Handled = true;
                });
            }
        }
    }
}
