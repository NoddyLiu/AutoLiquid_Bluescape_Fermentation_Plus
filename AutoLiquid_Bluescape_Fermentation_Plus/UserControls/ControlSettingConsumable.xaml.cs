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
using AutoLiquid_Bluescape_Fermentation_Plus.EntityJson;
using AutoLiquid_Bluescape_Fermentation_Plus.Utils;
using AutoLiquid_Bluescape_Fermentation_Plus.Window;
using AutoLiquid_Library.Enum;
using AutoLiquid_Library.Utils;
using MahApps.Metro.Controls;

namespace AutoLiquid_Bluescape_Fermentation_Plus.UserControls
{
    /// <summary>
    /// 耗材定义控件
    /// </summary>
    public partial class ControlSettingConsumable : UserControl
    {
        // 移液头Index
        private int mHeadIndex;

        // 耗材详细界面
        private List<ControlSettingGroup> ControlSettingGroups = new List<ControlSettingGroup>();

        public ControlSettingConsumable(int headIndex)
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
            for (var i = 0; i < ParamsHelper.CommonSettingList[this.mHeadIndex].Consumables.Count; i++)
            {
                AddTabItem(i);
            }
            // 标签添加按钮（最后一个标签）
            this.TabControlGroup.Items.Add(new MetroTabItem { Header = "+", Foreground = Brushes.LightSkyBlue, Background = Brushes.Transparent });
            // 刷新标签名
            RefreshGroupTitle();
        }

        private void ControlEvent()
        {
            // 标签相关事件
            ControlTabEvent();
        }

        /// <summary>
        /// 添加TabItem
        /// </summary>
        /// <param name="tabIndex"></param>
        private void AddTabItem(int tabIndex)
        {
            ControlSettingGroups.Add(new ControlSettingGroup(tabIndex, this.mHeadIndex));
            var metroTabStyle = Application.Current.FindResource("MetroTabStyle") as Style;
            var metroTabStyleWithClose = Application.Current.FindResource("MetroTabStyleWithClose") as Style;
            var tabItem = tabIndex == 0 ? new MetroTabItem { Style = metroTabStyle } : new MetroTabItem { Style = metroTabStyleWithClose };
            this.TabControlGroup.Items.Insert(tabIndex, tabItem);
            // 显示页面
            ((MetroTabItem)this.TabControlGroup.Items[tabIndex]).Content = ControlSettingGroups.ElementAt(tabIndex);
        }

        /// <summary>
        /// 插入TabItem
        /// </summary>
        /// <param name="tabIndex"></param>
        private void InsertTabItem(int tabIndex)
        {
            ControlSettingGroups.Insert(tabIndex, new ControlSettingGroup(tabIndex, this.mHeadIndex));
            var metroTabStyle = Application.Current.FindResource("MetroTabStyle") as Style;
            var metroTabStyleWithClose = Application.Current.FindResource("MetroTabStyleWithClose") as Style;
            var tabItem = new MetroTabItem { Style = metroTabStyleWithClose };
            this.TabControlGroup.Items.Insert(tabIndex, tabItem);
            // 显示页面
            ((MetroTabItem)this.TabControlGroup.Items[tabIndex]).Content = ControlSettingGroups.ElementAt(tabIndex);
        }

        /// <summary>
        /// 刷新分组标题
        /// </summary>
        private void RefreshGroupTitle()
        {
            for (var i = 0; i < ParamsHelper.CommonSettingList[this.mHeadIndex].Consumables.Count; i++)
            {
                var commonGroup = ParamsHelper.CommonSettingList[this.mHeadIndex].Consumables[i];
                ((MetroTabItem)this.TabControlGroup.Items[i]).Header = commonGroup.GroupName;
            }
        }

        private void ControlTabEvent()
        {
            // 耗材标签双击事件
            for (var i = 0; i < ControlSettingGroups.Count; i++)
            {
                ControlMouseDoubleClickEvent(i);
            }

            // 标签关闭事件
            this.TabControlGroup.TabItemClosingEvent += TabControlOnTabItemClosingEvent;

            // 标签添加点击事件
            this.TabControlGroup.SelectionChanged += TabControlGroupOnSelectionChanged;
        }

        private void ControlMouseDoubleClickEvent(int tabIndex)
        {
            ((MetroTabItem)this.TabControlGroup.Items[tabIndex]).MouseDoubleClick += TabItemGroupNameModifyOnMouseDoubleClick;
        }

        private void TabControlOnTabItemClosingEvent(object sender, BaseMetroTabControl.TabItemClosingEventArgs e)
        {
            var closingTabIndex = ParamsHelper.CommonSettingList[this.mHeadIndex].Consumables.FindIndex(p => p.GroupName.Equals(e.ClosingTabItem.Header.ToString()));
            if (closingTabIndex < 0)
                return;
            var consumableName = ((TabItem)this.TabControlGroup.Items[closingTabIndex]).Header;

            // 提示删除耗材会影响程序相关属性
            foreach (var procedure in ParamsHelper.ProcedureAll.Procedures)
            {
                // 是否禁止删除
                var delBan = procedure.Steps.Count(p => p.SourceConsumable.Equals(consumableName)) > 0 || procedure.Steps.Count(p => p.TargetConsumable.Equals(consumableName)) > 0;
                if (delBan)
                {
                    if (MessageBox.Show((string)this.FindResource("Prompt_CanNot_Delete_All_Consumable_1") + consumableName + (string)this.FindResource("Prompt_CanNot_Delete_All_Consumable_2"),
                            (string)this.FindResource("Prompt"), MessageBoxButton.OK, MessageBoxImage.Warning) ==
                        MessageBoxResult.OK)
                    {
                        e.Cancel = true;
                        return;
                    }
                }
            }

            if (MessageBox.Show((string)this.FindResource("Prompt_If_Delete_Tab_1") + consumableName + (string)this.FindResource("Prompt_If_Delete_Tab_2"),
                    (string)this.FindResource("Prompt"), MessageBoxButton.YesNo, MessageBoxImage.Warning) ==
                MessageBoxResult.No)
            {
                e.Cancel = true;
            }
            else
            {
                ParamsHelper.CommonSettingList[this.mHeadIndex].Consumables.RemoveAt(closingTabIndex);
                FileUtils.SaveCommonSettings(this.mHeadIndex, ParamsHelper.CommonSettingList[this.mHeadIndex]);
                ControlSettingGroups.RemoveAt(closingTabIndex);

                // 避免已选的Tab和关闭的Tab相同时，标签删除不了
                if (this.TabControlGroup.SelectedIndex == closingTabIndex)
                    this.TabControlGroup.SelectedIndex = 0;
            }
        }

        private void TabControlGroupOnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            // 如果点击“+”标签，就选中倒数第二个Tab
            if (this.TabControlGroup.SelectedIndex == this.TabControlGroup.Items.Count - 1)
            {
                var checkGroupNameTick = 0;
                var groupNameNew = "";
                while (true)
                {
                    checkGroupNameTick++;
                    groupNameNew = (string)this.FindResource("Consumable") +
                                      (ParamsHelper.CommonSettingList[this.mHeadIndex].Consumables.Count + checkGroupNameTick);
                    if (!ParamsHelper.CommonSettingList[this.mHeadIndex].Consumables.Exists(p => p.GroupName.Equals(groupNameNew)))
                        break;
                }
                // 新耗材默认参数跟前一个耗材一样
                var newCommonGroup = new Consumable();
                var commonGroupsCount = ParamsHelper.CommonSettingList[this.mHeadIndex].Consumables.Count;
                if (commonGroupsCount > 1)
                    newCommonGroup =
                        ObjectUtils.DeepCopy(ParamsHelper.CommonSettingList[this.mHeadIndex].Consumables[commonGroupsCount - 1]);
                newCommonGroup.GroupName = groupNameNew;
                ParamsHelper.CommonSettingList[this.mHeadIndex].Consumables.Add(newCommonGroup);
                var templateCount = ParamsHelper.Layout.RowCount * ParamsHelper.Layout.ColCount;
                var groupIndex = ParamsHelper.CommonSettingList[this.mHeadIndex].Consumables.Count - 1;
                ParamsHelper.AddPositionHole(ParamsHelper.CommonSettingList[this.mHeadIndex].Consumables[groupIndex].HoleStartPosList, templateCount);
                ParamsHelper.AddTemplateAvailableSub(
                    ParamsHelper.CommonSettingList[this.mHeadIndex].Consumables[groupIndex].TemplateAvailableList, templateCount);
                FileUtils.SaveCommonSettings(this.mHeadIndex, ParamsHelper.CommonSettingList[this.mHeadIndex]);

                // 默认插入到倒数第二个Tab
                AddTabItem(this.TabControlGroup.Items.Count - 1);
                var tabSelectedIndex = this.TabControlGroup.Items.Count - 2;
                this.TabControlGroup.SelectedIndex = tabSelectedIndex;

                RefreshGroupTitle();
                ControlMouseDoubleClickEvent(tabSelectedIndex);
            }
        }

        /// <summary>
        /// 修改耗材名称
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void TabItemGroupNameModifyOnMouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            Point position = e.GetPosition(this);
            double mouseY = position.Y;

            // y方向小于等于40像素点，才执行动作，避免误双击
            if (mouseY <= 40)
            {
                var doubleClickTabIndex = ParamsHelper.CommonSettingList[this.mHeadIndex].Consumables.FindIndex(p => p.GroupName.Equals(((MetroTabItem)sender).Header.ToString()));
                if (doubleClickTabIndex < 0)
                    return;
                if (sender.Equals(((MetroTabItem)this.TabControlGroup.Items[doubleClickTabIndex])))
                {
                    if (WindowChangeGroupTitle.EnsureExecute(this.mHeadIndex, doubleClickTabIndex))
                        RefreshGroupTitle();
                }
            }
        }
    }
}
