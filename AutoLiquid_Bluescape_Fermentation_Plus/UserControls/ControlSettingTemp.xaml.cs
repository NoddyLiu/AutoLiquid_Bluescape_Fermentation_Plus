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
using AutoLiquid_Library.Exceptions;
using DataHelper = AutoLiquid_Bluescape_Fermentation_Plus.Utils.DataHelper;

namespace AutoLiquid_Bluescape_Fermentation_Plus.UserControls
{
    /// <summary>
    /// 温度设置控件
    /// </summary>
    public partial class ControlSettingTemp : UserControl
    {
        // 移液头Index
        private int mHeadIndex;

        // 温度查询定时器
        System.Timers.Timer tempTimer;

        public ControlSettingTemp(int headIndex)
        {
            InitializeComponent();

            this.mHeadIndex = headIndex;

            this.Loaded += OnLoaded;
            this.Unloaded += OnUnloaded;
        }

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            // 初始化控件
            InitWidget();
            // 控件事件
            ControlEvent();
        }

        private void OnUnloaded(object sender, RoutedEventArgs e)
        {
            this.tempTimer.Enabled = false;
            this.tempTimer.Close();
        }

        private void InitWidget()
        {
            this.TextBoxCoolTempSampleCompensation.Text = ParamsHelper.CommonSettingList[this.mHeadIndex].CoolTempSampleCompensation.ToString();
            this.TextBoxCoolTempSample.Text = ParamsHelper.CommonSettingList[this.mHeadIndex].CoolTempSample.ToString();

            // 温度查询定时器
            QueryTempTimer();
        }

        private void QueryTempTimer()
        {
            tempTimer = new System.Timers.Timer(3000);
            tempTimer.Elapsed += new System.Timers.ElapsedEventHandler(TemperatureTimer);
            tempTimer.AutoReset = true;
            tempTimer.Enabled = true;
        }

        private void ControlEvent()
        {
            this.TextBoxCoolTempSampleCompensation.TextChanged += TextBoxOnTextChanged;
            this.TextBoxCoolTempSample.TextChanged += TextBoxOnTextChanged;
        }

        private void TextBoxOnTextChanged(object sender, TextChangedEventArgs e)
        {
            if (sender.Equals(this.TextBoxCoolTempSampleCompensation))
                DataHelper.SaveDecimal(this.mHeadIndex, this.TextBoxCoolTempSampleCompensation.Text.Trim(), ref ParamsHelper.CommonSettingList[this.mHeadIndex].CoolTempSampleCompensation);
            else if (sender.Equals(this.TextBoxCoolTempSample))
            {
                DataHelper.SaveDecimal(this.mHeadIndex, this.TextBoxCoolTempSample.Text.Trim(), ref ParamsHelper.CommonSettingList[this.mHeadIndex].CoolTempSample);
                // 设置留样区制冷温度
                // CmdHelperTemp.SetCoolTempSample(ParamsHelper.CommonSettingList[0].CoolTempSample + ParamsHelper.CommonSettingList[0].CoolTempSampleCompensation, false);
            }
        }

        public void TemperatureTimer(object source, System.Timers.ElapsedEventArgs e)
        {
            // 留样区温度
            // if (!CmdHelper.frmDAE.isResetting)
            // CmdHelperTemp.QueryCoolTempSample(false);
            Dispatcher.Invoke(() =>
            {
                // decimal t1 = CmdHelperTemp.frmDAE.TemperatureResult - ParamsHelper.CommonSettingList[0].CoolTempSampleCompensation;
                // this.LabelCurrentCoolTempSample.Content = t1;
            });
        }
    }
}
