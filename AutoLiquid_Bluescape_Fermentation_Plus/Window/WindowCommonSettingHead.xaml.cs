using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using AutoLiquid_Library.Enum;
using AutoLiquid_Library.Exceptions;
using AutoLiquid_Library.Utils;
using AutoLiquid_Bluescape_Fermentation_Plus.EntityJson;
using AutoLiquid_Bluescape_Fermentation_Plus.UserControls;
using AutoLiquid_Bluescape_Fermentation_Plus.Utils;
using MahApps.Metro.Controls;
using ConstantsUtils = AutoLiquid_Bluescape_Fermentation_Plus.Utils.ConstantsUtils;
using Position = AutoLiquid_Bluescape_Fermentation_Plus.EntityJson.Position;
using AutoLiquid_Bluescape_Fermentation_Plus.EntityCommon;

namespace AutoLiquid_Bluescape_Fermentation_Plus.Window
{
    /// <summary>
    /// 移液头设置界面
    /// </summary>
    public partial class WindowCommonSettingHead : MetroWindow
    {
        // 移液头Index
        private int mHeadIndex;

        public WindowCommonSettingHead(int headIndex)
        {
            InitializeComponent();

            this.mHeadIndex = headIndex;

            this.Loaded += OnLoaded;
            this.Closed += OnClosed;
        }

        private void OnLoaded(object sender, RoutedEventArgs e)
        {

            // 初始化控件
            InitWidget();
            // 控件事件
            ControlEvent();
        }

        private void OnClosed(object sender, EventArgs e)
        {

        }

        private void InitWidget()
        {
            ViewUtils.ShowLogo(this);

            if (this.mHeadIndex == 1)
                this.Title = (string)this.FindResource("Head2Setting");

            /**
             * 耗材类型
             */
            this.GroupBoxConsumable.Content = new ControlSettingConsumable(this.mHeadIndex);

            /**
             * 注射泵
             */
            this.GroupBoxPump.Content = new ControlSettingPump(this.mHeadIndex);

            /**
             * 压盖
             */
            this.GroupBoxCover.Content = new ControlSettingCover(this.mHeadIndex);

            /**
             * 搅拌
             */
            this.GroupBoxShake.Content = new ControlSettingShake(this.mHeadIndex);

            /**
             * 清洗区设置
             */
            this.GroupBoxCleaning.Content = new ControlSettingCleaning(this.mHeadIndex);

            /**
             * 废液槽设置
             */
            this.GroupBoxWaste.Content = new ControlSettingWaste(this.mHeadIndex);

            /**
             * 钢针设置
             */
            this.GroupBoxSteelNeedle.Content = new ControlSettingSteelNeedle(this.mHeadIndex);

            /**
             * 移液头属性
             */
            this.GroupBoxHeadAttribute.Content = new ControlSettingHeadAttribute(this.mHeadIndex);

            /**
             * 温度设置
             */
            // this.GroupBoxTemp.Content = new ControlSettingTemp(this.mHeadIndex);

            /**
             * 体积设置
             */
            this.GroupBoxVolume.Content = new ControlSettingVolume(this.mHeadIndex);

            /**
             * 速度设置
             */
            this.GroupBoxSpeed.Content = new ControlSettingSpeed(this.mHeadIndex);

            /**
             * 盘位摆放
             */
            this.GroupBoxPlatePutting.Content = new ControlSettingPlatePutting(this.mHeadIndex);

            /**
             * 位置调试
             */
            this.GroupBoxDebugPos.Content = new ControlSettingPosDebug(this.mHeadIndex);
        }

        private void ControlEvent()
        {
            this.BtnResetX.Click += BtnClick;
            this.BtnResetY.Click += BtnClick;
            this.BtnResetZ.Click += BtnClick;
            this.BtnResetP.Click += BtnClick;
            this.BtnResetM.Click += BtnClick;
            this.BtnResetAll.Click += BtnClick;
            this.BtnCommand.Click += BtnClick;

        }

        private void BtnClick(object sender, RoutedEventArgs routedEventArgs)
        {
            if (sender.Equals(this.BtnResetX))
            {
                BackgroundProcess.RunAsync(
                    delegate () { CmdHelper.Xi(this.mHeadIndex); },
                    delegate ()
                    {
                    });
            }
            else if (sender.Equals(this.BtnResetY))
            {
                BackgroundProcess.RunAsync(
                    delegate () { CmdHelper.Yi(this.mHeadIndex); },
                    delegate ()
                    {
                    });
            }
            else if (sender.Equals(this.BtnResetZ))
            {
                BackgroundProcess.RunAsync(
                    delegate () { CmdHelper.Zi(this.mHeadIndex); },
                    delegate ()
                    {
                    });
            }

            else if (sender.Equals(this.BtnResetP))
            {
                BackgroundProcess.RunAsync(
                    delegate () { CmdHelper.Pi(this.mHeadIndex); },
                    delegate ()
                    {
                    });
            }
            else if (sender.Equals(this.BtnResetM))
            {
                BackgroundProcess.RunAsync(
                    delegate () { CmdHelper.Mi(this.mHeadIndex); },
                    delegate ()
                    {
                    });
            }

            else if (sender.Equals(this.BtnResetAll))
            {
                BackgroundProcess.RunAsync(
                    delegate ()
                    {
                        CmdHelper.InitMachine(false, false);
                    },
                    delegate ()
                    {
                    });
            }
            else
            {
                BackgroundProcess.RunAsync(
              delegate ()
              {
                  var cmdStr = "";
                  Dispatcher.Invoke(() => { cmdStr = this.TextBoxCommand.Text; });
                  CmdHelper.frmDAE.DoCmd(cmdStr);
              },
              delegate ()
              {
              });
            }

        }
    }
}
