using System;
using System.Collections.Generic;
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
using System.Windows.Navigation;
using System.Windows.Shapes;
using AutoLiquid_Library.Enum;

namespace AutoLiquid_Bluescape_Fermentation_Plus.UserControls
{
    /// <summary>
    /// 进度条控件
    /// </summary>
    public partial class ControlProgressBar : UserControl
    {
        // 时间间隔
        private static int mInterval = 1000;

        // 总用时定时器
        private Timer mTimerTimeUsed = null;
        // 剩余时间定时器
        private Timer mTimerTimeRemain = null;

        // 时间
        private double mTimeUsed = 0;
        private double mTimeRermain = 0;

        public ControlProgressBar()
        {
            InitializeComponent();

            if (null == mTimerTimeUsed)
            {
                mTimerTimeUsed = new Timer { Interval = mInterval, AutoReset = true, Enabled = false };
                mTimerTimeUsed.Elapsed += (sender, args) =>
                {
                    // 主线程执行
                    Dispatcher.Invoke(() =>
                    {
                        // 获取当前数值，然后+1s
                        var val2Set = mTimeUsed + 1;
                        SetTimeUsed(val2Set);
                    });
                };
            }

            if (null == mTimerTimeRemain)
            {
                mTimerTimeRemain = new Timer { Interval = mInterval, AutoReset = true, Enabled = false };
                mTimerTimeRemain.Elapsed += (sender, args) =>
                {
                    // 主线程执行
                    Dispatcher.Invoke(() =>
                    {
                        // 获取当前数值，然后-1s
                        var val2Set = mTimeRermain - 1;
                        // 如果倒计时为少于0，但还没完成，就默认增加20s
                        if (val2Set < 0)
                        {
                            mTimeRermain = 20;
                            return;
                        }
                        SetTimeRemain(val2Set);
                    });
                };
            }
        }

        /// <summary>
        /// 设置进度条状态
        /// </summary>
        /// <param name="runStatus"></param>
        public void SetProgressBar(ERunStatus runStatus)
        {
            if (runStatus == ERunStatus.Initializing)
            {
                this.Visibility = Visibility.Hidden;
            }
            else if (runStatus == ERunStatus.Running)
            {
                this.Visibility = Visibility.Visible;
                // 用时归0
                SetTimeUsed(0);
                mTimerTimeUsed.Enabled = true;
                mTimerTimeRemain.Enabled = true;
            }
            else if (runStatus == ERunStatus.Continue)
            {
                this.Visibility = Visibility.Visible;
                mTimerTimeUsed.Enabled = true;
                mTimerTimeRemain.Enabled = true;
            }
            else if (runStatus == ERunStatus.Pause)
            {
                this.Visibility = Visibility.Visible;
                mTimerTimeUsed.Enabled = false;
                mTimerTimeRemain.Enabled = false;
            }
            else if (runStatus == ERunStatus.Stop)
            {
                this.Visibility = Visibility.Visible;
                // 剩余时间归0
                mTimerTimeUsed.Enabled = false;
                mTimerTimeRemain.Enabled = false;
                SetTimeRemain(0);
            }
        }

        /// <summary>
        /// 设置时间（根据总秒数）
        /// </summary>
        /// <param name="textBlock"></param>
        /// <param name="seconds">总秒数</param>
        private void SetTime(TextBlock textBlock, double seconds)
        {
            TimeSpan time = TimeSpan.FromSeconds(seconds);
            string result = time.ToString(@"hh\:mm\:ss");
            textBlock.Text = result;
        }

        /// <summary>
        /// 设置总用时长
        /// </summary>
        /// <param name="seconds">总秒数</param>
        private void SetTimeUsed(double seconds)
        {
            mTimeUsed = seconds;
            SetTime(this.TextBlockTimeUsed, mTimeUsed);
        }

        /// <summary>
        /// 设置剩余时长
        /// </summary>
        /// <param name="seconds">总秒数</param>
        private void SetTimeRemain(double seconds)
        {
            mTimeRermain = seconds;
            SetTime(this.TextBlockTimeRemain, mTimeRermain);
        }

        /// <summary>
        /// 更新剩余时长
        /// </summary>
        /// <param name="seconds"></param>
        public void UpdateTimeRemain(double seconds)
        {
            // 停止定时器
            mTimerTimeRemain.Enabled = false;

            SetTimeRemain(seconds);

            // 启动定时器
            mTimerTimeRemain.Enabled = true;
        }

        /// <summary>
        /// 增加进度
        /// </summary>
        /// <param name="percentIncrease">增加百分比</param>
        public void IncreaseProgressBar(double percentIncrease)
        {
            Dispatcher.Invoke(() =>
            {
                this.ProgressBar.Value += percentIncrease;
            });
        }

        /// <summary>
        /// 设置进度
        /// </summary>
        /// <param name="percent"></param>
        public void SetProgressBar(double percent)
        {
            Dispatcher.Invoke(() =>
            {
                this.ProgressBar.Value = percent;
            });
        }
    }
}
