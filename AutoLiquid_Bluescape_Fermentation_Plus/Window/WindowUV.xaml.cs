using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using AutoLiquid_Bluescape_Fermentation_Plus.Utils;
using MahApps.Metro.Controls;

namespace AutoLiquid_Bluescape_Fermentation_Plus.Window
{
    /// <summary>
    /// 紫外灯控制窗体
    /// </summary>
    public partial class WindowUV : MetroWindow
    {
        // 时间间隔
        private static int mInterval = 1000;

        // 剩余时间定时器
        private Timer mTimerTimeRemain = null;
        // 剩余时间
        private double mTimeRermain = 0;

        public WindowUV()
        {
            InitializeComponent();

            this.Closing += OnClosing;

            // 初始化控件
            InitWidget();
            // 控件事件
            ControlEvent();
        }

        private void OnClosing(object sender, CancelEventArgs e)
        {
            CmdHelper.UVClose(false);
            mTimerTimeRemain.Stop();
        }

        private void InitWidget()
        {
            ViewUtils.ShowLogo(this);

            mTimerTimeRemain = new Timer { Interval = mInterval, AutoReset = true, Enabled = false };
            mTimerTimeRemain.Elapsed += (sender, args) =>
            {
                // 主线程执行
                Dispatcher.Invoke(() =>
                {
                    mTimeRermain--;
                    this.ProgressBar.Value += 1;
                    RefreshRemainTime(mTimeRermain);

                    // 剩余时间为0，就关闭紫外灯
                    if (mTimeRermain == 0)
                    {
                        this.mTimerTimeRemain.Enabled = false;
                        CmdHelper.UVClose(false);
                    }
                });
            };
        }

        private void ControlEvent()
        {
            this.BtnConfirm.Click += BtnOnClick;
            this.BtnCancel.Click += BtnOnClick;
        }

        private void BtnOnClick(object sender, RoutedEventArgs e)
        {
            if (sender.Equals(this.BtnConfirm))
            {
                try
                {
                    SetTime(int.Parse(this.TextBoxUVTimeSet.Text));
                }
                catch (Exception exception)
                {
                    MessageBox.Show((string)this.FindResource("Prompt_Data_Input_Error"));
                }

            }
            else if (sender.Equals(this.BtnCancel))
            {
                this.Close();
            }
        }

        /// <summary>
        /// 设置时长
        /// </summary>
        /// <param name="seconds">总秒数</param>
        private void SetTime(double seconds)
        {
            if (seconds <= 0)
                return;
            mTimerTimeRemain.Enabled = false;
            mTimeRermain = seconds;
            this.ProgressBar.Maximum = seconds;
            this.ProgressBar.Value = 0;
            mTimerTimeRemain.Enabled = true;
            RefreshRemainTime(seconds);

            // 开启紫外灯
            CmdHelper.UVOpen(false);
        }

        /// <summary>
        /// 刷新界面剩余时间
        /// </summary>
        /// <param name="seconds">总秒数</param>
        private void RefreshRemainTime(double seconds)
        {
            TimeSpan time = TimeSpan.FromSeconds(seconds);
            string result = time.ToString(@"hh\:mm\:ss");
            this.TextBlockTimeRemain.Text = result;
        }
    }
}
