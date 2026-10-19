using System;
using System.Collections.Generic;
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
using MessageBox = System.Windows.MessageBox;
using UserControl = System.Windows.Controls.UserControl;

namespace AutoLiquid_Bluescape_Fermentation_Plus.UserControls
{
    /// <summary>
    /// 耗材参数组界面
    /// </summary>
    public partial class ControlSettingGroup : UserControl
    {
        // 耗材Index
        private int mGroupIndex;
        // 耗材
        private Consumable mConsumable;

        // 移液头Index
        private int mHeadIndex;

        // 盘位设置List
        public List<ControlTemplateSetting> ControlTemplateSettingList = new List<ControlTemplateSetting>();

        public ControlSettingGroup(int groupIndex, int headIndex)
        {
            InitializeComponent();

            this.mGroupIndex = groupIndex;
            this.mHeadIndex = headIndex;
            this.mConsumable = ParamsHelper.CommonSettingList[this.mHeadIndex].Consumables[this.mGroupIndex];

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
            // 盘位设置
            TemplateSetting();

            RefreshWidget();
        }

        /// <summary>
        /// 盘位设置
        /// </summary>
        private void TemplateSetting()
        {
            var rowCount = ParamsHelper.Layout.RowCount;
            var colCount = ParamsHelper.Layout.ColCount;
            for (var row = 0; row < rowCount; row++)
            {
                this.GridTemplateSetting.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) });
            }
            for (var col = 0; col < colCount; col++)
            {
                this.GridTemplateSetting.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Auto) });
            }
            // 行与行分隔线
            for (var separator = 0; separator < rowCount - 1; separator++)
            {
                this.GridTemplateSetting.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) });
            }
            // 总行数（包括分隔线）
            var rowCountTotal = rowCount + rowCount - 1;
            // 盘位计数Index
            var templateIndexTick = -1;
            for (var row = 0; row < rowCountTotal; row++)
            {
                // 奇数行：孔位置
                if (row % 2 == 0)
                {
                    for (var col = 0; col < colCount; col++)
                    {
                        templateIndexTick += 1;
                        var template = new ControlTemplateSetting(this.mConsumable, this.mGroupIndex, templateIndexTick, mHeadIndex);

                        Grid.SetRow(template, row);
                        Grid.SetColumn(template, col);
                        this.GridTemplateSetting.Children.Add(template);
                        ControlTemplateSettingList.Add(template);
                    }
                }
                // 偶数行：分隔线
                else
                {
                    var rectangle = new Rectangle();
                    rectangle.Fill = Brushes.LightGray;
                    rectangle.Height = 2;
                    rectangle.Margin = new Thickness(5, 10, 5, 0);
                    Grid.SetRow(rectangle, row);
                    Grid.SetColumn(rectangle, 0);
                    Grid.SetColumnSpan(rectangle, colCount + 3);
                    this.GridTemplateSetting.Children.Add(rectangle);
                }
            }
        }

        /// <summary>
        /// 刷新控件
        /// </summary>
        public void RefreshWidget()
        {
            // 通用设置
            this.ControlNormalHeight.SetParam(mHeadIndex, this.mConsumable.NormalHeight, EAxis.Z);

            this.CheckBoxNormalHeightReset.IsChecked =
                this.mConsumable.NormalHeightReset;

            this.ControlLiquidAbsorbHeight.SetParam(mHeadIndex, this.mConsumable.LiquidAbsorbHeight, EAxis.Z);
            this.TextBoxAbsorbSpeedCmd.Text = this.mConsumable.AbsorbSpeed;
            this.TextBoxLiquidAbsorbDelay.Text =
                this.mConsumable.LiquidAbsorbDelay.ToString();
            this.TextBoxAbsorbHeight2NormalHeightSpeedCmd.Text = this.mConsumable.AbsorbHeight2NormalHeightSpeed;
            this.TextBoxLiquidAbsorbDelayAfterLift.Text = this.mConsumable
                .LiquidAbsorbDelayAfterLift.ToString();

            this.ControlLiquidJetHeight.SetParam(mHeadIndex, this.mConsumable.LiquidJetHeight, EAxis.Z);
            this.TextBoxJetSpeedCmd.Text = this.mConsumable.JetSpeed;
            this.TextBoxLiquidJetDelay.Text = this.mConsumable.LiquidJetDelay.ToString();
            this.TextBoxJetHeight2NormalHeightSpeedCmd.Text = this.mConsumable.JetHeight2NormalHeightSpeed;

            this.ControlLiquidJetWallHeight.SetParam(mHeadIndex, this.mConsumable.LiquidJetWallHeight, EAxis.Z);
            this.TextBoxLiquidJetWallOffset.Text = this.mConsumable.LiquidJetWallOffset.ToString();
            this.TextBoxLiquidJetWallTrigger.Text = this.mConsumable.LiquidJetWallTrigger.ToString();
            this.CheckBoxWallJet.IsChecked = this.mConsumable.WallJet;

            this.ControlCoverHeight.SetParam(mHeadIndex, this.mConsumable.CoverHeight, EAxis.M);

            this.ControlDemoHeight.SetParam(mHeadIndex, this.mConsumable.DemoHeight, EAxis.M);
            this.TextBoxLiquidDemoHeightDelay.Text = this.mConsumable.DemoHeightDelay.ToString();

            //this.ControlAbsorbMixingHeight.SetParam(mHeadIndex, this.mConsumable.AbsorbMixingHeight, EAxis.Z);
            //this.TextBoxAbsorbMixingSpeed.Text = this.mConsumable.AbsorbMixingSpeed;
            //this.ControlJetMixingHeight.SetParam(mHeadIndex, this.mConsumable.JetMixingHeight, EAxis.Z);
            //this.TextBoxJetMixingSpeed.Text = this.mConsumable.JetMixingSpeed;

            // 行列设置
            this.TextBoxRowCount.Text = this.mConsumable.RowCount.ToString();
            this.TextBoxColCount.Text = this.mConsumable.ColCount.ToString();
            this.TextBoxRowStep.Text = this.mConsumable.HoleStep.Y.ToString();
            this.TextBoxColStep.Text = this.mConsumable.HoleStep.X.ToString();
            this.CheckBoxStaggered.IsChecked = this.mConsumable.Staggered;

            // 盘位设置
            this.CheckBoxAutoFill.IsChecked = this.mConsumable.HoleStartPosAutoFill;
            this.TextBoxTemplateRowStep.Text = this.mConsumable.TemplateStep.Y.ToString();
            this.TextBoxTemplateColStep.Text = this.mConsumable.TemplateStep.X.ToString();
            // 如果勾选了自动填充，触发事件
            this.ControlTemplateSettingList[0].ControlTemplateHole1X.TextBoxPos.TextChanged += TextBoxOnTextChanged;
            this.ControlTemplateSettingList[0].ControlTemplateHole1Y.TextBoxPos.TextChanged += TextBoxOnTextChanged;
        }

        private void ControlEvent()
        {
            // 通用设置
            this.ControlNormalHeight.TextBoxPos.TextChanged += TextBoxOnTextChanged;
            this.CheckBoxNormalHeightReset.Checked += CheckBoxOnChecked;
            this.CheckBoxNormalHeightReset.Unchecked += CheckBoxOnUnChecked;

            this.ControlLiquidAbsorbHeight.TextBoxPos.TextChanged += TextBoxOnTextChanged;
            this.TextBoxAbsorbSpeedCmd.TextChanged += TextBoxOnTextChanged;
            this.TextBoxLiquidAbsorbDelay.TextChanged += TextBoxOnTextChanged;
            this.TextBoxAbsorbHeight2NormalHeightSpeedCmd.TextChanged += TextBoxOnTextChanged;
            this.TextBoxLiquidAbsorbDelayAfterLift.TextChanged += TextBoxOnTextChanged;

            this.ControlLiquidJetHeight.TextBoxPos.TextChanged += TextBoxOnTextChanged;
            this.TextBoxJetSpeedCmd.TextChanged += TextBoxOnTextChanged;
            this.TextBoxLiquidJetDelay.TextChanged += TextBoxOnTextChanged;
            this.TextBoxJetHeight2NormalHeightSpeedCmd.TextChanged += TextBoxOnTextChanged;

            this.ControlLiquidJetWallHeight.TextBoxPos.TextChanged += TextBoxOnTextChanged;
            this.TextBoxLiquidJetWallOffset.TextChanged += TextBoxOnTextChanged;
            this.TextBoxLiquidJetWallTrigger.TextChanged += TextBoxOnTextChanged;
            this.CheckBoxWallJet.Checked += CheckBoxOnChecked;
            this.CheckBoxWallJet.Unchecked += CheckBoxOnUnChecked;

            this.ControlCoverHeight.TextBoxPos.TextChanged += TextBoxOnTextChanged;

            this.ControlDemoHeight.TextBoxPos.TextChanged += TextBoxOnTextChanged;
            this.TextBoxLiquidDemoHeightDelay.TextChanged += TextBoxOnTextChanged;

            //this.ControlAbsorbMixingHeight.TextBoxPos.TextChanged += TextBoxOnTextChanged;
            //this.TextBoxAbsorbMixingSpeed.TextChanged += TextBoxOnTextChanged;
            //this.ControlJetMixingHeight.TextBoxPos.TextChanged += TextBoxOnTextChanged;
            //this.TextBoxJetMixingSpeed.TextChanged += TextBoxOnTextChanged;

            // 行列设置
            this.TextBoxRowCount.TextChanged += TextBoxOnTextChanged;
            this.TextBoxColCount.TextChanged += TextBoxOnTextChanged;
            this.TextBoxRowStep.TextChanged += TextBoxOnTextChanged;
            this.TextBoxColStep.TextChanged += TextBoxOnTextChanged;
            this.CheckBoxStaggered.Checked += CheckBoxOnChecked;
            this.CheckBoxStaggered.Unchecked += CheckBoxOnUnChecked;

            // 盘位设置
            this.CheckBoxAutoFill.Checked += CheckBoxOnChecked;
            this.CheckBoxAutoFill.Unchecked += CheckBoxOnUnChecked;
            this.TextBoxTemplateRowStep.TextChanged += TextBoxOnTextChanged;
            this.TextBoxTemplateColStep.TextChanged += TextBoxOnTextChanged;
        }

        private void TextBoxOnTextChanged(object sender, TextChangedEventArgs e)
        {
            // 通用设置
            if (sender.Equals(this.ControlNormalHeight.TextBoxPos))
                DataHelper.SaveDecimal(this.mHeadIndex, this.ControlNormalHeight.TextBoxPos.Text.Trim(), ref this.mConsumable.NormalHeight);
            else if (sender.Equals(this.ControlLiquidAbsorbHeight.TextBoxPos))
                DataHelper.SaveDecimal(this.mHeadIndex, this.ControlLiquidAbsorbHeight.TextBoxPos.Text.Trim(), ref this.mConsumable.LiquidAbsorbHeight);
            else if (sender.Equals(this.TextBoxAbsorbSpeedCmd))
                DataHelper.SaveString(this.mHeadIndex, this.TextBoxAbsorbSpeedCmd.Text.Trim(), ref this.mConsumable.AbsorbSpeed);
            else if (sender.Equals(this.TextBoxLiquidAbsorbDelay))
                DataHelper.SaveDecimal(this.mHeadIndex, this.TextBoxLiquidAbsorbDelay.Text.Trim(), ref this.mConsumable.LiquidAbsorbDelay);
            else if (sender.Equals(this.TextBoxAbsorbHeight2NormalHeightSpeedCmd))
                DataHelper.SaveString(this.mHeadIndex, this.TextBoxAbsorbHeight2NormalHeightSpeedCmd.Text.Trim(), ref this.mConsumable.AbsorbHeight2NormalHeightSpeed);
            else if (sender.Equals(this.TextBoxLiquidAbsorbDelayAfterLift))
                DataHelper.SaveDecimal(this.mHeadIndex, this.TextBoxLiquidAbsorbDelayAfterLift.Text.Trim(), ref this.mConsumable.LiquidAbsorbDelayAfterLift);

            else if (sender.Equals(this.ControlLiquidJetHeight.TextBoxPos))
                DataHelper.SaveDecimal(this.mHeadIndex, this.ControlLiquidJetHeight.TextBoxPos.Text.Trim(), ref this.mConsumable.LiquidJetHeight);
            else if (sender.Equals(this.TextBoxJetSpeedCmd))
                DataHelper.SaveString(this.mHeadIndex, this.TextBoxJetSpeedCmd.Text.Trim(), ref this.mConsumable.JetSpeed);
            else if (sender.Equals(this.TextBoxLiquidJetDelay))
                DataHelper.SaveDecimal(this.mHeadIndex, this.TextBoxLiquidJetDelay.Text.Trim(), ref this.mConsumable.LiquidJetDelay);
            else if (sender.Equals(this.TextBoxJetHeight2NormalHeightSpeedCmd))
                DataHelper.SaveString(this.mHeadIndex, this.TextBoxJetHeight2NormalHeightSpeedCmd.Text.Trim(), ref this.mConsumable.JetHeight2NormalHeightSpeed);

            else if (sender.Equals(this.ControlLiquidJetWallHeight.TextBoxPos))
                DataHelper.SaveDecimal(this.mHeadIndex, this.ControlLiquidJetWallHeight.TextBoxPos.Text.Trim(), ref this.mConsumable.LiquidJetWallHeight);
            else if (sender.Equals(this.TextBoxLiquidJetWallOffset))
                DataHelper.SaveDecimal(this.mHeadIndex, this.TextBoxLiquidJetWallOffset.Text.Trim(), ref this.mConsumable.LiquidJetWallOffset);
            else if (sender.Equals(this.TextBoxLiquidJetWallTrigger))
                DataHelper.SaveDecimal(this.mHeadIndex, this.TextBoxLiquidJetWallTrigger.Text.Trim(), ref this.mConsumable.LiquidJetWallTrigger);

            //else if (sender.Equals(this.ControlAbsorbMixingHeight.TextBoxPos))
            //    DataHelper.SaveDecimal(this.mHeadIndex, this.ControlAbsorbMixingHeight.TextBoxPos.Text.Trim(), ref this.mConsumable.AbsorbMixingHeight);
            //else if (sender.Equals(this.TextBoxAbsorbMixingSpeed))
            //    DataHelper.SaveString(this.mHeadIndex, this.TextBoxAbsorbMixingSpeed.Text.Trim(), ref this.mConsumable.AbsorbMixingSpeed);
            //else if (sender.Equals(this.ControlJetMixingHeight.TextBoxPos))
            //    DataHelper.SaveDecimal(this.mHeadIndex, this.ControlJetMixingHeight.TextBoxPos.Text.Trim(), ref this.mConsumable.JetMixingHeight);
            //else if (sender.Equals(this.TextBoxJetMixingSpeed))
            //    DataHelper.SaveString(this.mHeadIndex, this.TextBoxJetMixingSpeed.Text.Trim(), ref this.mConsumable.JetMixingSpeed);

            else if (sender.Equals(this.ControlCoverHeight.TextBoxPos))
                DataHelper.SaveDecimal(this.mHeadIndex, this.ControlCoverHeight.TextBoxPos.Text.Trim(), ref this.mConsumable.CoverHeight);

            else if (sender.Equals(this.ControlDemoHeight.TextBoxPos))
                DataHelper.SaveDecimal(this.mHeadIndex, this.ControlDemoHeight.TextBoxPos.Text.Trim(), ref this.mConsumable.DemoHeight);
            else if (sender.Equals(this.TextBoxLiquidDemoHeightDelay))
                DataHelper.SaveDecimal(this.mHeadIndex, this.TextBoxLiquidDemoHeightDelay.Text.Trim(), ref this.mConsumable.DemoHeightDelay);

            // 行列设置
            else if (sender.Equals(this.TextBoxRowCount))
                DataHelper.SaveInt(this.mHeadIndex, this.TextBoxRowCount.Text.Trim(), ref this.mConsumable.RowCount);
            else if (sender.Equals(this.TextBoxColCount))
                DataHelper.SaveInt(this.mHeadIndex, this.TextBoxColCount.Text.Trim(), ref this.mConsumable.ColCount);
            else if (sender.Equals(this.TextBoxRowStep))
                DataHelper.SaveDecimal(this.mHeadIndex, this.TextBoxRowStep.Text.Trim(), ref this.mConsumable.HoleStep.Y);
            else if (sender.Equals(this.TextBoxColStep))
                DataHelper.SaveDecimal(this.mHeadIndex, this.TextBoxColStep.Text.Trim(), ref this.mConsumable.HoleStep.X);

            // 盘位设置
            else if (sender.Equals(this.TextBoxTemplateRowStep))
                DataHelper.SaveDecimal(this.mHeadIndex, this.TextBoxTemplateRowStep.Text.Trim(), ref this.mConsumable.TemplateStep.Y);
            else if (sender.Equals(this.TextBoxTemplateColStep))
                DataHelper.SaveDecimal(this.mHeadIndex, this.TextBoxTemplateColStep.Text.Trim(), ref this.mConsumable.TemplateStep.X);
            else if (sender.Equals(this.ControlTemplateSettingList[0].ControlTemplateHole1X.TextBoxPos))
            {
                // 自动填充
                if (this.mConsumable.HoleStartPosAutoFill && this.mConsumable.TemplateStep.X != 0)
                {
                    var rowCount = ParamsHelper.Layout.RowCount;
                    var colCount = ParamsHelper.Layout.ColCount;
                    for (var row = 0; row < rowCount; row++)
                    {
                        for (var col = 0; col < colCount; col++)
                        {
                            if (row == 0 && col == 0)
                                continue;
                            var index = row * colCount + col;
                            var controlTemplateSetting = this.ControlTemplateSettingList[index];
                            controlTemplateSetting.ControlTemplateHole1X.TextBoxPos.Text =
                                (this.mConsumable.HoleStartPosList[0].X + col * this.mConsumable.TemplateStep.X)
                                .ToString();
                        }
                    }
                }
            }
            else if (sender.Equals(this.ControlTemplateSettingList[0].ControlTemplateHole1Y.TextBoxPos))
            {
                // 自动填充
                if (this.mConsumable.HoleStartPosAutoFill && this.mConsumable.TemplateStep.Y != 0)
                {
                    var rowCount = ParamsHelper.Layout.RowCount;
                    var colCount = ParamsHelper.Layout.ColCount;
                    for (var row = 0; row < rowCount; row++)
                    {
                        for (var col = 0; col < colCount; col++)
                        {
                            if (row == 0 && col == 0)
                                continue;
                            var index = row * colCount + col;
                            var controlTemplateSetting = this.ControlTemplateSettingList[index];
                            controlTemplateSetting.ControlTemplateHole1Y.TextBoxPos.Text =
                                (this.mConsumable.HoleStartPosList[0].Y + row * this.mConsumable.TemplateStep.Y)
                                .ToString();
                        }
                    }
                }
            }
        }

        private void CheckBoxOnChecked(object sender, RoutedEventArgs e)
        {
            if (sender.Equals(this.CheckBoxNormalHeightReset))
                DataHelper.SaveBool(this.mHeadIndex, true, ref this.mConsumable.NormalHeightReset);
            else if (sender.Equals(this.CheckBoxWallJet))
                DataHelper.SaveBool(this.mHeadIndex, true, ref this.mConsumable.WallJet);
            else if (sender.Equals(this.CheckBoxStaggered))
                DataHelper.SaveBool(this.mHeadIndex, true, ref this.mConsumable.Staggered);
            else if (sender.Equals(this.CheckBoxAutoFill))
            {
                DataHelper.SaveBool(this.mHeadIndex, true, ref this.mConsumable.HoleStartPosAutoFill);
                if (this.mConsumable.TemplateStep.X == 0 || this.mConsumable.TemplateStep.Y == 0)
                {
                    MessageBox.Show((string)this.FindResource("Prompt_Pls_Fill_Template_Step_And_Check"));
                }
            }
        }

        private void CheckBoxOnUnChecked(object sender, RoutedEventArgs e)
        {
            if (sender.Equals(this.CheckBoxNormalHeightReset))
                DataHelper.SaveBool(this.mHeadIndex, false, ref this.mConsumable.NormalHeightReset);
            else if (sender.Equals(this.CheckBoxWallJet))
                DataHelper.SaveBool(this.mHeadIndex, false, ref this.mConsumable.WallJet);
            else if (sender.Equals(this.CheckBoxStaggered))
                DataHelper.SaveBool(this.mHeadIndex, false, ref this.mConsumable.Staggered);
            else if (sender.Equals(this.CheckBoxAutoFill))
                DataHelper.SaveBool(this.mHeadIndex, false, ref this.mConsumable.HoleStartPosAutoFill);
        }
    }
}
