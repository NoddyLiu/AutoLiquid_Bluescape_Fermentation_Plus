using AutoLiquid_Bluescape_Fermentation_Plus.Pump;
using AutoLiquid_Bluescape_Fermentation_Plus.Utils;
using AutoLiquid_Library.Enum;
using System;
using System.Collections.Generic;
using System.IO.Ports;
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
using System.Windows.Threading;
using DataHelper = AutoLiquid_Bluescape_Fermentation_Plus.Utils.DataHelper;

namespace AutoLiquid_Bluescape_Fermentation_Plus.UserControls
{
    /// <summary>
    /// 注射泵属性控件
    /// </summary>
    public partial class ControlSettingPump : UserControl
    {
        // 移液头Index
        private int mHeadIndex;

        public ControlSettingPump(int headIndex)
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
            //RefreshPortList();

            this.ComboBoxSpeed.ItemsSource = Enumerable.Range(0, 41).Select(x => x.ToString()).ToList();
            this.ComboBoxSpeed.SelectedItem = 10+"";
        }

        /// <summary>
        /// 扫描当前可用串口，刷新到 cmbPortName 下拉框。
        /// 保留当前选中（如果还在列表里）；否则选第一个。
        /// </summary>
        //public void RefreshPortList()
        //{
        //    string[] ports;
        //    try { ports = SerialPort.GetPortNames(); }
        //    catch (Exception ex)
        //    {
        //        ports = new string[0];
        //        LogHelper.Error("扫描串口失败");
        //    }

        //    this.ComboBoxPortName.Items.Clear();
        //    if (ports == null || ports.Length == 0)
        //    {
        //        this.ComboBoxPortName.Items.Add("(未发现串口)");
        //        this.ComboBoxPortName.SelectedIndex = 0;
        //        LogHelper.Error("未发现任何可用串口（请确认设备已插好）");
        //    }
        //    else
        //    {
        //        foreach (var p in ports) this.ComboBoxPortName.Items.Add(p);
        //        this.ComboBoxPortName.SelectedItem = ParamsHelper.HeadList[this.mHeadIndex].SerialPortName;
        //    }
        //}

        private void ControlEvent()
        {
            //this.ComboBoxPortName.SelectionChanged += ComboBoxOnSelectionChanged;

            this.BtnInit.Click += BtnOnClick;
            this.BtnAbsorbLiquid.Click += BtnOnClick;
            this.BtnJetLiquid.Click += BtnOnClick;
            this.BtnPos0.Click += BtnOnClick;
        }

        private void ComboBoxOnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (sender.Equals(this.ComboBoxPortName))
            {
                var row = (string)this.ComboBoxPortName.SelectedItem;
                DataHelper.SaveString(this.mHeadIndex, (this.ComboBoxPortName.SelectedItem).ToString(), ref ParamsHelper.HeadList[this.mHeadIndex].SerialPortName, () => FileUtils.SaveHead(this.mHeadIndex, ParamsHelper.HeadList[this.mHeadIndex]));
            }
        }

        private void BtnOnClick(object sender, RoutedEventArgs e)
        {
            if (sender.Equals(this.BtnInit))
            {
                _ = CmdHelper.pumpTransport.mgr.RunWorkflowAsync(PumpSelection.Left, ctx =>
                 {
                     ctx.Parallel("初始化", wb =>
                     {
                         wb.Init();
                     });
                 });
            }
            else if (sender.Equals(this.BtnAbsorbLiquid))
            {
                CmdHelper.pumpTransport.mgr.RunWorkflowAsync(PumpSelection.Left, ctx =>
                {
                    ctx.Parallel("设置速度", wb =>
                    {
                        var speed = int.Parse(this.ComboBoxSpeed.SelectedItem.ToString());
                        wb.SetSpeed(speed);
                    });
                }).GetAwaiter().GetResult();
                _ = CmdHelper.pumpTransport.mgr.RunWorkflowAsync(PumpSelection.Left, ctx =>
                {
                    ctx.Parallel("吸液", wb =>
                    {
                        CmdHelper.Ps(0, int.Parse(this.TextBoxAbsorbLiquid.Text), false);
                    });
                });
            }
            else if (sender.Equals(this.BtnJetLiquid))
            {
                CmdHelper.pumpTransport.mgr.RunWorkflowAsync(PumpSelection.Left, ctx =>
                {
                    ctx.Parallel("设置速度", wb =>
                    {
                        var speed = int.Parse(this.ComboBoxSpeed.SelectedItem.ToString());
                        wb.SetSpeed(speed);
                    });
                }).GetAwaiter().GetResult();
                _ = CmdHelper.pumpTransport.mgr.RunWorkflowAsync(PumpSelection.Left, ctx =>
                {
                    ctx.Parallel("喷液", wb =>
                    {
                        CmdHelper.Ps(0, int.Parse(this.TextBoxJetLiquid.Text) * -1, false);
                    });
                });
            }
            else if (sender.Equals(this.BtnPos0))
            {
                 CmdHelper.pumpTransport.mgr.RunWorkflowAsync(PumpSelection.Left, ctx =>
                {
                    ctx.Parallel("设置速度", wb =>
                    {
                        var speed = int.Parse(this.ComboBoxSpeed.SelectedItem.ToString());
                        wb.SetSpeed(speed);
                    });
                }).GetAwaiter().GetResult();
                _ = CmdHelper.pumpTransport.mgr.RunWorkflowAsync(PumpSelection.Left, ctx =>
                {
                    ctx.Parallel("回零", wb =>
                    {
                        CmdHelper.Pa(0, 0, false);
                    });
                });
            }
        }
    }
}
