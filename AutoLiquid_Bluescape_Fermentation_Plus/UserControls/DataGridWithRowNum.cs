using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Animation;

namespace AutoLiquid_Bluescape_Fermentation_Plus.UserControls
{
    /// <summary>
    /// 带行号的DataGrid
    /// </summary>
    public class DataGridWithRowNum : DataGrid
    {
        public DataGridWithRowNum()
        {

            LoadingRow += (sender, args) =>
            {
                if (GetShowRowNumbers(this))
                    RefreshRowNumber(args.Row);
            };
        }

        // 是否显示行号
        public static readonly DependencyProperty ShowRowNumbersProperty =
            DependencyProperty.RegisterAttached("ShowRowNumbers", typeof(bool), typeof(DataGridWithRowNum), new UIPropertyMetadata(false));

        public static bool GetShowRowNumbers(DependencyObject obj)
        {
            return (bool)obj.GetValue(ShowRowNumbersProperty);
        }

        public static void SetShowRowNumbers(DependencyObject obj, bool value)
        {
            obj.SetValue(ShowRowNumbersProperty, value);
        }


        protected override void OnExecutedDelete(ExecutedRoutedEventArgs e)
        {
            base.OnExecutedDelete(e);

            if (GetShowRowNumbers(this))
                RefreshRowNumbers();
        }

        protected override void OnItemsChanged(NotifyCollectionChangedEventArgs e)
        {
            base.OnItemsChanged(e);

            if (GetShowRowNumbers(this))
                RefreshRowNumbers();
        }

        private void RefreshRowNumbers()
        {
            for (int i = 0; i < Items.Count; i++)
                RefreshRowNumber((DataGridRow)ItemContainerGenerator.ContainerFromIndex(i));
        }

        private void RefreshRowNumber(DataGridRow row)
        {
            if (row != null)
                row.Header = ((row.GetIndex()) + 1).ToString();
        }
    }
}
