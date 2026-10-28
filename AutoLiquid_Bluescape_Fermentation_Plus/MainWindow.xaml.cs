using AutoLiquid_Bluescape_Fermentation_Plus.EntityCommon;
using AutoLiquid_Bluescape_Fermentation_Plus.EntityJson;
using AutoLiquid_Bluescape_Fermentation_Plus.Pump;
using AutoLiquid_Bluescape_Fermentation_Plus.UserControls;
using AutoLiquid_Bluescape_Fermentation_Plus.Utils;
using AutoLiquid_Bluescape_Fermentation_Plus.Window;
using AutoLiquid_Library.Comm;
using AutoLiquid_Library.Enum;
using AutoLiquid_Library.Exceptions;
using AutoLiquid_Library.Utils;
using ControlzEx.Standard;
using DAERun;
using DAERun.CallBack;
using MahApps.Metro.Controls;
using Microsoft.Win32;
using Newtonsoft.Json;
using Serilog;
using Spire.Xls;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.IO.Ports;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Xml.Serialization;
using Application = System.Windows.Application;
using ButtonBase = System.Windows.Controls.Primitives.ButtonBase;
using ConstantsUtils = AutoLiquid_Bluescape_Fermentation_Plus.Utils.ConstantsUtils;
using DataHelper = AutoLiquid_Library.Utils.DataHelper;
using KeyEventArgs = System.Windows.Input.KeyEventArgs;
using MessageBox = System.Windows.MessageBox;
using Path = System.IO.Path;
using Position = AutoLiquid_Bluescape_Fermentation_Plus.EntityJson.Position;
using Timer = System.Timers.Timer;

namespace AutoLiquid_Bluescape_Fermentation_Plus
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : MetroWindow
    {
        public static MainWindow mMainWindow;

        // 机器当前状态
        private ERunStatus mRunStatus = ERunStatus.Initializing;

        // 所有任务名称
        private List<string> excelFilesNameList;
        // 导入的任务名称
        private string excelFilesNameImported;

        // 网络是否连接成功
        public static bool isCanNetSuccess = false;

        // 运行弹出框可拖拽
        private Point loadingStartPoint;
        private Vector loadingStartOffset;

        // 移液指令信息
        private List<Seq> seqList = new List<Seq>();

        // 盘位（key：所在盘位index）（盘位1~2：发酵区；盘位3~4：留样区）
        public static Dictionary<int, ControlTemplateCommon> templateDict = new Dictionary<int, ControlTemplateCommon>();

        // 可用盘位
        private List<Grid> templateCanUse = new List<Grid>();

        /**
         * 紧急状态
         */
        // 当前指令是否被急停拦截了
        public static bool mIsEmergencyStopIntercept;
        // 紧急停止是否再次关闭
        public static bool mIsEmergencyStopClosedAgain;
        // 急停最后一条拦截的指令
        public static string mEmergencyStopLastCmd = "";

        // 串口通信
        private SerialPort mSerialPort;

        // 温度查询定时器
        private System.Timers.Timer tempQueryTimer;

        // 网络连接定时器（每n小时发送一个xs0指令，避免断开连接。运行期间不发送）
        private System.Timers.Timer connectTimer;


        public MainWindow()
        {
            InitializeComponent();

            this.Loaded += OnLoaded;
            this.Closing += OnClosing;
            this.Closed += (s, e) =>
            {
                Application.Current.Shutdown();
            };
        }

        private void OnLoaded(object sender, RoutedEventArgs routedEventArgs)
        {
            mMainWindow = this;

            // 加载所有参数
            if (!ParamsHelper.LoadAllParams())
                MessageBox.Show((string)this.FindResource("Prompt_Params_Parse_Error_Check_Log_For_Help"));

            // 网络通信
            CanNet();
            CanNetPump();

            // 初始化控件
            InitWidget();

            // 控件事件
            ControlEvent();

            // 外部设备串口控制
            SerialPort();
        }

        private void OnClosing(object sender, CancelEventArgs e)
        {
            WindowLogin wl = new WindowLogin();
            if (ParamsHelper.Permission.Available)
                wl.ShowDialog();
            else
                wl.isSuccessful = true;
            if (wl.isSuccessful)
            {
                if (ParamsHelper.IO.DoorAvailable || ParamsHelper.IO.EmergencyStopAvailable)
                {
                    // 关闭紧急停止功能
                    CmdHelper.EmergencyStopClose(false);
                }

                if (ParamsHelper.IO.SerialPortAvailable)
                {
                    if (mSerialPort != null && mSerialPort.IsOpen)
                        mSerialPort.Close();
                }

                // // 停止制冷
                // CmdHelperTemp.StopCoolTempSample(false);
                tempQueryTimer?.Close();

                CmdHelper.frmDAE.Disconnect();
                CmdHelper.frmDAE.Close();

                CmdHelper.pumpTransport.Close();

                connectTimer?.Close();
            }
            else
                e.Cancel = true;
        }

        /// <summary>
        /// 设置主控网络通信
        /// </summary>
        private void CanNet()
        {
            if (!CmdHelper.frmDAE.TestNet())
            {
                MessageBox.Show(CmdHelper.frmDAE.remoteIP + (string)this.FindResource("Prompt_Host_Disconnect_Pls_Check_Again"));
                SetLoadingStatus(ERunStatus.Stop);
                // 打开红指示灯
                CmdHelper.LightOn(ELight.Red, false);
                return;
            }

            isCanNetSuccess = true;
            CmdHelper.frmDAE.IsShowFormTip = false;
            try
            {
                SetLoadingStatus(ERunStatus.Initializing);
                BackgroundProcess.RunAsync(delegate ()
                    {
                        // 发送速度指令
                        for (var headIndex = 0; headIndex < ParamsHelper.HeadList.Count; headIndex++)
                        {
                            if (ParamsHelper.HeadList[headIndex].Available)
                            {
                                CmdHelper.SpeedSet(headIndex, EAxis.All, "", 100, true);
                            }
                        }

                        CmdHelper.InitMachine(false, true);
                        // 风扇、紫外灯、制冷
                        if (isCanNetSuccess)
                        {
                            //CmdHelper.FanClose(false);
                        }

                        // 设置留样区制冷温度
                        // CmdHelperTemp.SetCoolTempSample(ParamsHelper.CommonSettingList[0].CoolTempSample + ParamsHelper.CommonSettingList[0].CoolTempSampleCompensation, false);

                        // 定时器
                        Timer();
                    },
                    delegate ()
                    {
                        SetLoadingStatus(ERunStatus.Stop);
                    });
            }
            catch (Exception ex)
            {
                MessageBox.Show((string)this.FindResource("Prompt_Init_Error_Pls_Check_Again"));
                LogHelper.Error(ex.StackTrace);
                SetLoadingStatus(ERunStatus.Stop);
                // 打开红指示灯
                CmdHelper.LightOn(ELight.Red, false);
            }
        }

        /// <summary>
        /// 设置注射泵网络通信
        /// </summary>
        private async Task CanNetPump()
        {
            try
            {
                CmdHelper.pumpTransport.Open();
            }
            catch (Exception ex)
            {
                MessageBox.Show((string)this.FindResource("Prompt_Pump_Disconnect_Pls_Check_Again"));
                SetLoadingStatus(ERunStatus.Stop);
                // 打开红指示灯
                CmdHelper.LightOn(ELight.Red, false);
                return;
            }
        }

        #region 串口通信
        /// <summary>
        /// 外部设备串口控制
        /// </summary>
        private void SerialPort()
        {
            if (ParamsHelper.IO.SerialPortAvailable)
            {
                try
                {
                    mSerialPort = new SerialPort(ParamsHelper.IO.SeialPort, 9600) { DtrEnable = true };
                    mSerialPort.DataReceived += new SerialDataReceivedEventHandler(SerialPortOnDataReceived);
                    mSerialPort.Open();
                }
                catch (Exception e)
                {
                    LogHelper.Error((string)this.FindResource("Prompt_Serial_Port_Error") + "：" + e.StackTrace);
                }
            }
        }

        private void SerialPortOnDataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            var serialDevice = sender as SerialPort;
            // 收取完整帧
            while (serialDevice.BytesToRead != 8)
            {
                Thread.Sleep(10);
            }
            var byteCount = serialDevice.BytesToRead;
            var bytes = new byte[byteCount];
            serialDevice.Read(bytes, 0, byteCount);
            // 构建帧
            BaseFrame baseFrame = new BaseFrame();
            try
            {
                baseFrame.CopyFromBuffer(bytes);
                // 判断校验码
                if (DataHelper.CheckFrameCodeValidate(baseFrame))
                {
                    // 控制设备（启停程序）
                    if (baseFrame.FrameData.CMDCode == Code.CMD_CONTROL && baseFrame.FrameData.ConcreteData[0] == Code.DEVICE_PROGRAM)
                    {
                        // 帧有效数据
                        ValidData validData = null;
                        // 设备或程序编号（默认为1）
                        var deviceProgramNo = baseFrame.FrameData.ConcreteData[2];

                        // 通信失败
                        if (!isCanNetSuccess)
                            validData = new ValidData(Code.CMD_CONTROL, new byte[] { Code.DEVICE_PROGRAM, Code.ERROR_COMM, deviceProgramNo });
                        // 没有可执行的程序
                        else if (excelFilesNameList.Count < deviceProgramNo || deviceProgramNo == 0)
                            validData = new ValidData(Code.CMD_CONTROL, new byte[] { Code.DEVICE_PROGRAM, Code.ERROR_NO_PROGRAM, deviceProgramNo });
                        else
                        {
                            // 启动程序
                            if (baseFrame.FrameData.ConcreteData[1] == 0x01)
                            {
                                if (!CmdHelper.frmDAE.isRunning)
                                {
                                    // 根据设备或程序编号判断需要导入运行哪个excel文件
                                    string rootPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
                                    excelFilesNameImported = excelFilesNameList.ElementAt(deviceProgramNo - 1);
                                    var filePath = rootPath + @"\" + excelFilesNameImported;
                                    ParseSeqFromProcedureChain();

                                    RunProcedure(false, false);
                                }
                                validData = new ValidData(Code.CMD_CONTROL, new byte[] { Code.DEVICE_PROGRAM, 0x00, deviceProgramNo });
                            }
                            // 停止程序
                            else if (baseFrame.FrameData.ConcreteData[1] == 0x00)
                            {
                                if (CmdHelper.frmDAE.isRunning)
                                {
                                    // 模拟点击停止
                                    mMainWindow.Dispatcher.Invoke(() =>
                                    {
                                        // 模拟手动点击“暂停”
                                        SetLoadingStatus(ERunStatus.Pause, false);
                                        Thread.Sleep(100);
                                        // 模拟手动点击“停止”
                                        SetLoadingStatus(ERunStatus.Stop, false);
                                    });
                                }
                                validData = new ValidData(Code.CMD_CONTROL, new byte[] { Code.DEVICE_PROGRAM, 0x00, deviceProgramNo });
                            }
                        }

                        BaseFrame responseFrame = new BaseFrame(validData);
                        serialDevice.Write(responseFrame.TotalData, 0, responseFrame.TotalData.Length);
                    }
                    // 查询状态（移液工作站）
                    else if (baseFrame.FrameData.CMDCode == Code.CMD_STATUS && baseFrame.FrameData.ConcreteData[0] == Code.DEVICE_PROGRAM)
                    {
                        // 帧有效数据
                        ValidData validData;

                        // 通信失败
                        if (!isCanNetSuccess)
                            validData = new ValidData(Code.CMD_STATUS, new byte[] { Code.DEVICE_PROGRAM, Code.ERROR_COMM, 0 });
                        else
                        {
                            // 运行中
                            if (CmdHelper.frmDAE.isRunning)
                                validData = new ValidData(Code.CMD_STATUS, new byte[] { Code.DEVICE_PROGRAM, 0x01, 0 });
                            // 待机中
                            else
                                validData = new ValidData(Code.CMD_STATUS, new byte[] { Code.DEVICE_PROGRAM, 0x00, 0 });
                        }
                        BaseFrame responseFrame = new BaseFrame(validData);
                        serialDevice.Write(responseFrame.TotalData, 0, responseFrame.TotalData.Length);
                    }
                }
                else
                    LogHelper.Error((string)this.FindResource("Prompt_Check_Code_Error"));
            }
            catch (Exception ex)
            {
                LogHelper.Error((string)this.FindResource("Prompt_Frame_Error") + "：" + ex.StackTrace);
            }
        }
        #endregion

        /// <summary>
        /// 设置运行状态
        /// </summary>
        /// <param name="status"></param>
        /// <param name="needControlIO">控制IO</param>
        /// <param name="runningAfterCountdown">倒数之后继续</param>
        private void SetLoadingStatus(ERunStatus status, bool needControlIO = true, bool runningAfterCountdown = false)
        {
            switch (status)
            {
                case ERunStatus.Initializing:
                    this.LayoutRunningLoading.Visibility = Visibility.Visible;
                    this.ProgressRingLoading.Visibility = Visibility.Visible;
                    this.LabelPrompt.Content = (string)this.FindResource("Prompt_Initializing");
                    this.DockPanelAllBtn.Visibility = Visibility.Collapsed;

                    CmdHelper.isManualPause = false;
                    CmdHelper.isManualStop = false;

                    // 按钮不可用
                    MainUiButtonAvailable(false);

                    if (needControlIO)
                    {
                        // 关闭紧急停止停止功能
                        if (ParamsHelper.IO.DoorAvailable || ParamsHelper.IO.EmergencyStopAvailable)
                            CmdHelper.EmergencyStopClose(false);
                    }
                    break;
                case ERunStatus.Pause:
                    this.LayoutRunningLoading.Visibility = Visibility.Visible;
                    this.ProgressRingLoading.Visibility = Visibility.Collapsed;
                    this.LabelPrompt.Content = (string)this.FindResource("Prompt_Manual_Pause");
                    this.BtnPause.Visibility = Visibility.Collapsed;
                    this.StackPanelStopOrContinue.Visibility = Visibility.Visible;

                    CmdHelper.isManualPause = true;
                    CmdHelper.isManualStop = false;
                    CmdHelper.frmDAE.isRunning = true;

                    // 按钮不可用
                    MainUiButtonAvailable(false);

                    // 更新运行进度条
                    this.ControlProgressBar.SetProgressBar(ERunStatus.Pause);

                    if (needControlIO)
                    {
                        // 打开指示黄灯
                        CmdHelper.ResetAllLight(false);
                        CmdHelper.LightOn(ELight.Yellow, false);
                    }
                    break;
                case ERunStatus.Stop:
                    this.LayoutRunningLoading.Visibility = Visibility.Collapsed;

                    CmdHelper.isManualPause = true;
                    CmdHelper.isManualStop = true;
                    CmdHelper.frmDAE.isRunning = false;

                    // 按钮可用
                    MainUiButtonAvailable(true);

                    // 更新运行进度条
                    this.ControlProgressBar.SetProgressBar(ERunStatus.Stop);

                    if (needControlIO)
                    {
                        // 关闭风扇
                        //CmdHelper.FanClose(false);
                        // 关闭指示灯
                        CmdHelper.ResetAllLight(false);
                        // 关闭阀和泵
                        CmdHelper.ValveAndPumpClose(false);
                        // 终止注射泵
                        _ = CmdHelper.pumpTransport.mgr.RunWorkflowAsync(PumpSelection.Left, ctx =>
                        {
                            ctx.Parallel("终止指令", wb =>
                            {
                                wb.Terminate();
                            });
                        });

                        // 关闭紧急停止停止功能
                        if (ParamsHelper.IO.DoorAvailable || ParamsHelper.IO.EmergencyStopAvailable)
                            CmdHelper.EmergencyStopClose(false);
                    }
                    break;
                case ERunStatus.Continue:
                case ERunStatus.Running:
                    this.LayoutRunningLoading.Visibility = Visibility.Visible;
                    this.ProgressRingLoading.Visibility = Visibility.Visible;
                    this.LabelPrompt.Content = (string)this.FindResource("Prompt_Running_Pls_Wait");
                    this.DockPanelAllBtn.Visibility = Visibility.Visible;
                    this.BtnPause.Visibility = Visibility.Visible;
                    this.StackPanelStopOrContinue.Visibility = Visibility.Collapsed;

                    CmdHelper.isManualPause = false;
                    CmdHelper.isManualStop = false;
                    CmdHelper.frmDAE.isRunning = true;

                    // 按钮不可用
                    MainUiButtonAvailable(false);

                    if (!runningAfterCountdown)
                    {
                        // 继续
                        if (status == ERunStatus.Continue)
                            // 更新运行进度条
                            this.ControlProgressBar.SetProgressBar(ERunStatus.Continue);
                        // 运行
                        else
                        {
                            // 更新运行进度条
                            this.ControlProgressBar.SetProgressBar(0.0);
                            this.ControlProgressBar.SetProgressBar(ERunStatus.Running);
                        }
                    }

                    if (needControlIO)
                    {
                        // 打开指示绿灯
                        CmdHelper.ResetAllLight(false);
                        CmdHelper.LightOn(ELight.Green, false);

                        if (status == ERunStatus.Running)
                        {
                            // 打开风扇
                            //CmdHelper.FanOpen(false);

                            // 打开紧急停止功能
                            if (ParamsHelper.IO.DoorAvailable || ParamsHelper.IO.EmergencyStopAvailable)
                                CmdHelper.EmergencyStopOpen(false);
                        }
                    }
                    break;
            }

            mRunStatus = status;
        }

        /// <summary>
        /// 主界面按钮是否可点击
        /// </summary>
        /// <param name="available"></param>
        private void MainUiButtonAvailable(bool available)
        {
            this.MenuBar.IsEnabled = available;
            this.ControlDataGridProcedureChainMask.IsEnabled = available;
            this.GridTemplateContainer.IsEnabled = available;
            this.BottomBar.IsEnabled = available;
        }

        private void InitWidget()
        {
            ViewUtils.ShowLogo(this);

            // 权限是否启用
            if (!ParamsHelper.Permission.Available)
                this.MenuItemPermission.Visibility = Visibility.Collapsed;

            /**
             * 添加可用盘位
             * 发酵盘位：1、2
             * 留样盘位：3、4
             */
            templateCanUse.AddRange(new List<Grid>
            {
                this.TemplateFermentation1, this.TemplateFermentation2,
                this.TemplateSample1, this.TemplateSample2
            });

            // 如果有紧急停止，就拦截指令
            if (ParamsHelper.IO.DoorAvailable || ParamsHelper.IO.EmergencyStopAvailable)
            {
                // 设置指令被拦截回调
                CmdHelper.frmDAE.SetInterceptedCallBack(new CmdInterceptedCallBack());
                // CmdHelper.frmDAE.SetReceivedCallBack(new CmdReceivedCallBack());
            }

            // 更新运行进度条
            this.ControlProgressBar.SetProgressBar(ERunStatus.Initializing);

            // 程序链
            RefreshProcedureChain();
        }

        /// <summary>
        /// 刷新程序链
        /// </summary>
        public void RefreshProcedureChain()
        {
            BackgroundProcess.RunAsync(() =>
            {
                var result = ParseSeqFromProcedureChain();
                Dispatcher.Invoke(() =>
                {
                    this.ControlDataGridProcedureChain.RefreshProcedures(ParamsHelper.ProcedureAll.ProcedureIndexChain);
                });
                return result;
            }, delegate (object returnResult)
            {

            });
        }

        /// <summary>
        /// 温度定时器
        /// </summary>
        private void Timer()
        {
            // 温度查询定时器
            tempQueryTimer = new System.Timers.Timer(2000);
            tempQueryTimer.Elapsed += new System.Timers.ElapsedEventHandler(TempQueryTimer);
            tempQueryTimer.AutoReset = true;
            tempQueryTimer.Enabled = true;

            // 网络连接定时器
            connectTimer = new System.Timers.Timer();
            // connectTimer.Interval = 60 * 60 * 1000 * 3; // 3小时
            connectTimer.Interval = 60 * 60 * 1000 * 0.5; // 半小时
            connectTimer.Elapsed += new System.Timers.ElapsedEventHandler(ConnectTimer);
            connectTimer.AutoReset = true;
            connectTimer.Enabled = true;
        }

        public void TempQueryTimer(object source, System.Timers.ElapsedEventArgs e)
        {
            // if (!CmdHelper.frmDAE.isRunning && !CmdHelper.frmDAE.isResetting)
            // CmdHelperTemp.QueryCoolTempSample(false);

            // 留样区制冷温度
            Dispatcher.Invoke(() =>
            {
                // decimal t1 = CmdHelperTemp.frmDAE.TemperatureResult - ParamsHelper.CommonSettingList[0].CoolTempSampleCompensation;
                // this.LabelCoolTempSample.Content = t1;
            });
        }

        public void ConnectTimer(object source, System.Timers.ElapsedEventArgs e)
        {
            if (mRunStatus == ERunStatus.Pause || mRunStatus == ERunStatus.Stop || mRunStatus == ERunStatus.Countdown)
            {
                LogHelper.Info((string)Application.Current.FindResource("Prompt_Connect_Check"), "");
                // CmdHelper.Xs(0, 0);
                CmdHelper.InitMachine(false, false);
            }
        }

        /// <summary>
        /// 指令被拦截回调
        /// </summary>
        public class CmdInterceptedCallBack : MyCallBack
        {
            public override void Callback()
            {
                if (CmdHelper.frmDAE.isRunning)
                {
                    mIsEmergencyStopIntercept = true;

                    // 马上停止下一个指令执行
                    CmdHelper.isManualPause = true;
                    CmdHelper.isManualStop = false;

                    mMainWindow.Dispatcher.Invoke(() =>
                    {
                        // 模拟手动点击“暂停”
                        mMainWindow.SetLoadingStatus(ERunStatus.Pause, false);
                        mMainWindow.CheckEmergencyStopAndShowMessageBox();
                    });
                }
            }
        }

        /// <summary>
        /// 检查急停状态并弹出框提示
        /// </summary>
        private void CheckEmergencyStopAndShowMessageBox()
        {
            if (!CmdHelper.frmDAE.IsDoorClosed)
            {
                var promptStr = ParamsHelper.IO.DoorAvailable ? (string)Application.Current.FindResource("Prompt_Door_Is_Opened_Please_Close_First") : (string)Application.Current.FindResource("Prompt_EmergencyStop_Is_Opened_Please_Init_First");
                if (MessageBox.Show(promptStr, (string)Application.Current.FindResource("Prompt"), MessageBoxButton.OK, MessageBoxImage.Warning) ==
                    MessageBoxResult.OK)
                {
                    // 查询仓门状态
                    BackgroundProcess.RunAsync(() => CmdHelper.EmergencyStopQuery(false), delegate (object returnResult)
                    {
                        Thread.Sleep(100);
                        if (CmdHelper.frmDAE.IsDoorClosed)
                        {
                            mMainWindow.Dispatcher.Invoke(() =>
                            {
                                mIsEmergencyStopClosedAgain = true;
                                mIsEmergencyStopIntercept = false;
                            });
                        }
                        else
                        {
                            mMainWindow.Dispatcher.Invoke(() =>
                            {
                                mIsEmergencyStopClosedAgain = false;
                                CheckEmergencyStopAndShowMessageBox();
                            });
                        }
                    });
                }
            }
        }

        /// <summary>
        /// 指令接收回调
        /// </summary>
        public class CmdReceivedCallBack : ReceivedCallBack
        {
            public override void Callback(byte[] bytes)
            {
                // 记录接收到的数据
                var received = String.Join(" ", bytes);
                LogHelper.Info("接收到的数据：", received);
            }
        }

        private void ControlEvent()
        {
            // 键盘事件
            this.KeyDown += OnKeyDown;

            // 菜单栏
            this.MenuItemUserSetting.Click += MenuItemOnClick;
            this.MenuItemProcedure.Click += MenuItemOnClick;
            this.MenuItemModifyPwd.Click += MenuItemOnClick;
            this.MenuItemAbout.Click += MenuItemOnClick;

            // 左侧边
            this.BtnProcedureChainNew.Click += BtnOnClick;
            this.BtnProcedureChainDel.Click += BtnOnClick;

            // 风扇
            //this.BtnFan.Click += BtnOnClick;
            // 转子
            this.BtnShake1.Click += BtnOnClick;
            this.BtnShake2.Click += BtnOnClick;
            // 安装钢针
            this.BtnInstallSteelNeedle.Click += BtnOnClick;
            // 紫外灯
            this.BtnUV.Click += BtnOnClick;
            // 制冷
            this.BtnCool.Click += BtnOnClick;

            // 演示
            this.BtnDemo.Click += BtnOnClick;
            // 运行
            this.BtnRun.Click += BtnOnClick;

            // 复位
            this.BtnReset.Click += BtnOnClick;

            // 运行弹出框
            this.BtnPause.Click += BtnOnClick;
            this.BtnStop.Click += BtnOnClick;
            this.BtnContinue.Click += BtnOnClick;

            // 导入
            this.BtnImportData.Click += BtnOnClick;

            // 运行进度框可拖拽
            this.LayoutRunningLoading.MouseDown += (sender, args) =>
            {
                loadingStartPoint = args.GetPosition(this.mainWindow);
                loadingStartOffset = new Vector(LayoutLoadingRenderTransform.X, LayoutLoadingRenderTransform.Y);
                this.LayoutRunningLoading.CaptureMouse();
            };
            this.LayoutRunningLoading.MouseMove += (sender, args) =>
            {
                if (this.LayoutRunningLoading.IsMouseCaptured)
                {
                    Vector offset = Point.Subtract(args.GetPosition(this.mainWindow), loadingStartPoint);

                    LayoutLoadingRenderTransform.X = loadingStartOffset.X + offset.X;
                    LayoutLoadingRenderTransform.Y = loadingStartOffset.Y + offset.Y;
                }
            };
            this.LayoutRunningLoading.MouseUp += (sender, args) => { this.LayoutRunningLoading.ReleaseMouseCapture(); };

            // 左侧程序链选择
            this.ControlDataGridProcedureChain.DataGridProcedures.SelectionChanged += (sender, args) =>
            {
                var rowIndex = this.ControlDataGridProcedureChain.DataGridProcedures.SelectedIndex;
                if (rowIndex < 0)
                    rowIndex = 0;
                ShowProcedureDetail(rowIndex);
            };
        }

        private void OnKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Space) // 空格，暂停 or 继续
            {
                if (CmdHelper.frmDAE.isRunning)
                {
                    if (CmdHelper.isManualPause && !CmdHelper.isManualStop) // 暂停中按空格键
                    {
                        // 继续
                        this.BtnContinue.RaiseEvent(new RoutedEventArgs(ButtonBase.ClickEvent));
                    }
                    else if (!CmdHelper.isManualPause && !CmdHelper.isManualStop) // 运行中按空格键
                    {
                        // 暂停
                        this.BtnPause.RaiseEvent(new RoutedEventArgs(ButtonBase.ClickEvent));
                    }
                }
            }
        }

        private void MenuItemOnClick(object sender, RoutedEventArgs e)
        {
            if (ParamsHelper.Permission.Available && (sender.Equals(this.MenuItemUserSetting) || sender.Equals(this.MenuItemProcedure)))
            {
                WindowLogin wl = new WindowLogin();
                wl.ShowDialog();
                if (!wl.isSuccessful)
                    return;
            }

            if (sender.Equals(this.MenuItemUserSetting))
            {
                WindowCommonSettingHead wcsh1 = new WindowCommonSettingHead(0);
                wcsh1.ShowDialog();
            }
            else if (sender.Equals(this.MenuItemProcedure))
            {
                WindowProcedure wp = new WindowProcedure();
                wp.ShowDialog();
            }
            else if (sender.Equals(this.MenuItemModifyPwd))
            {
                WindowPwdModify wpm = new WindowPwdModify();
                wpm.ShowDialog();
            }
            else if (sender.Equals(this.MenuItemAbout))
            {
                WindowAbout wa = new WindowAbout();
                wa.ShowDialog();
            }
        }

        private void BtnOnClick(object sender, RoutedEventArgs e)
        {
            bool Isgo = false;
            // 运行
            if (sender.Equals(this.BtnRun) || sender.Equals(this.BtnDemo))
            {
                // 网络未连接
                //                if (!isCanNetSuccess)
                //                {
                //                    MessageBox.Show((string)this.FindResource("Prompt_Host_Disconnect_Pls_Check_Again"));
                //                    return;
                //                }

                // 查询仓门状态
                if (ParamsHelper.IO.DoorAvailable || ParamsHelper.IO.EmergencyStopAvailable)
                {
                    CmdHelper.EmergencyStopQuery(false);
                    Thread.Sleep(100);
                    // 仓门打开中
                    if (!CmdHelper.frmDAE.IsDoorClosed)
                    {
                        MessageBox.Show((string)this.FindResource("Prompt_Door_Is_Opened_Please_Close_And_Run"));
                        return;
                    }
                }

                /**
                 * 计算额外多吸体积量是否超过了量程
                 */
                var head1RangeOver = false;
                var head2RangeOver = false;
                for (var headIndex = 0; headIndex < ParamsHelper.HeadList.Count; headIndex++)
                {
                    if (ParamsHelper.HeadList[headIndex].Available)
                    {
                        var absorbAllPercent = ParamsHelper.CommonSettingList[headIndex].AbsorbAirAfterPercent +
                                               ParamsHelper.CommonSettingList[headIndex].AbsorbLiquidMoreOne2MorePercent;
                        var headLiquidRangeMax = ConstantsUtils.LiquidRangeMaxDic[ParamsHelper.HeadList[headIndex].HeadLiquidRange];
                        var headLiquidRangeReal = Convert.ToDecimal((int)ParamsHelper.HeadList[headIndex].HeadLiquidRange);
                        // 额外多吸体积
                        var additionalVolume = ParamsHelper.CommonSettingList[headIndex].AbsorbAirBeforePercent + absorbAllPercent * 0.01m * headLiquidRangeReal;
                        // 移液头
                        if (additionalVolume >= headLiquidRangeMax)
                        {
                            if (headIndex == 0)
                                head1RangeOver = true;
                            else if (headIndex == 1)
                                head2RangeOver = true;
                        }
                    }
                }
                if (head1RangeOver || head2RangeOver)
                {
                    var headStr = head1RangeOver ? (string)this.FindResource("Head1") : (string)this.FindResource("Head2");
                    MessageBox.Show(headStr + " " + (string)this.FindResource("Prompt_Air_Volume_Is_Greater_Than_Liquid_Range"));
                    return;
                }

                // 没有选择程序链
                if (ParamsHelper.ProcedureAll.ProcedureIndexChain.Count == 0)
                {
                    MessageBox.Show((string)this.FindResource("Prompt_Pls_Add_Procedure_To_Chain_First"));
                    return;
                }

                // 运行的程序期望时间不符
                var runTimeExpectTotalSec = 0; // 期望运行总时间（秒）
                var runTimeRealTotalSec = 0; // 真实运行总时间（秒）
                foreach (var procedureIndex in ParamsHelper.ProcedureAll.ProcedureIndexChain)
                {
                    var procedure = ParamsHelper.ProcedureAll.Procedures[procedureIndex];
                    runTimeExpectTotalSec += (procedure.RunTimeExpect * 60);

                    var stepsLiquidTransfer = procedure.Steps.Where(p => p.StepType == EStepType.LiquidTransfer).ToList();
                    foreach (var step in stepsLiquidTransfer)
                    {
                        var holeCount = (step.TargetHoleIndexTo.OriIndex - step.TargetHoleIndexFrom.OriIndex) + 1;
                        runTimeRealTotalSec += (holeCount * ParamsHelper.CommonSettingList[0].SteelNeedleRunTimeSingleArray[procedure.SteelNeedleChoiceIndex]);
                    }
                }
                // 如果移液实际运行时间 > 期望运行时间，则提示修改相关参数
                if (runTimeRealTotalSec > runTimeExpectTotalSec)
                {
                    MessageBox.Show((string)this.FindResource("Prompt_Real_Run_Time_Diff_From_Expect_Run_Time"));
                    return;
                }

                // 动作过程是否符合逻辑
                var actionResult = CheckActionLogic();
                if (actionResult.Available)
                {
                    MessageBox.Show((string)this.FindResource("StepOrder") + (actionResult.Index + 1) + "：" + actionResult.Info);
                    return;
                }

                // 演示
                if (sender.Equals(this.BtnDemo))
                {
                    if (MessageBox.Show((string)this.FindResource("Prompt_Demo_Mode"), (string)this.FindResource("Prompt"), MessageBoxButton.OK, MessageBoxImage.Warning) == MessageBoxResult.OK)
                        RunProcedure(true);
                }
                // 运行
                else
                    RunProcedure(false);

            }
            // 复位
            else if (sender.Equals(this.BtnReset))
            {
                // 网络未连接
                if (!isCanNetSuccess)
                {
                    MessageBox.Show((string)this.FindResource("Prompt_Host_Disconnect_Pls_Check_Again"));
                    return;
                }

                SetLoadingStatus(ERunStatus.Initializing);
                BackgroundProcess.RunAsync(() => CmdHelper.InitMachine(false, false), delegate (object returnResult)
                {
                    SetLoadingStatus(ERunStatus.Stop);
                });
            }
            // 导入
            else if (sender.Equals(this.BtnImportData))
            {
                //ShowImportDialog();
            }
            // 暂停
            else if (sender.Equals(this.BtnPause))
            {
                SetLoadingStatus(ERunStatus.Pause);
            }
            // 停止
            else if (sender.Equals(this.BtnStop))
            {
                SetLoadingStatus(ERunStatus.Stop);

                // 如果吸液了，提示是否把液体喷回源孔
                if (CmdHelper.headStatusList[0].Head == EHeadStatus.Absorbed)
                {
                    // 哪个移液头
                    var headUsedIndex = CmdHelper.headStatusList[0].Head == EHeadStatus.Absorbed ? 0 : 1;
                    var seq = seqList[CmdHelper.headStatusList[headUsedIndex].SeqIndex];
                    if (ParamsHelper.IO.SerialPortAvailable || MessageBox.Show((string)this.FindResource("Prompt_If_Jet_Liquid_2_Source_Hole"),
                            (string)this.FindResource("Prompt"), MessageBoxButton.YesNo, MessageBoxImage.Warning) == MessageBoxResult.Yes)
                    {
                        var headStatus = CmdHelper.headStatusList[headUsedIndex];
                        if (headStatus.Head == EHeadStatus.Absorbed)
                        {
                            BackgroundProcess.RunAsync(() => CmdHelper.ReJet2Source(headUsedIndex, seq, headStatus.SourceConsumableType, headStatus.SourceTemplateIndex, headStatus.SourceHoleIndex, false), delegate (object returnResult)
                             {
                             });
                        }
                    }
                }
            }
            // 继续
            else if (sender.Equals(this.BtnContinue))
            {
                SetLoadingStatus(ERunStatus.Continue);
            }
            // 风扇
            //else if (sender.Equals(this.BtnFan))
            //{
            //    // 开风扇
            //    if (this.BtnFan.Content.Equals((string)this.FindResource("FanOpen")))
            //    {
            //        CmdHelper.FanOpen(false);
            //        this.BtnFan.Content = (string)this.FindResource("FanClose");
            //    }
            //    // 关风扇
            //    else
            //    {
            //        CmdHelper.FanClose(false);
            //        this.BtnFan.Content = (string)this.FindResource("FanOpen");
            //    }
            //}
            // 转子1
            else if (sender.Equals(this.BtnShake1))
            {
                // 开
                if (this.BtnShake1.Content.Equals((string)this.FindResource("Shake1Open")))
                {
                    CmdHelper.ShakeStart(1, false);
                    this.BtnShake1.Content = (string)this.FindResource("Shake1Close");
                }
                // 关
                else
                {
                    CmdHelper.ShakeStop(1, false);
                    this.BtnShake1.Content = (string)this.FindResource("Shake1Open");
                }
            }
            // 转子2
            else if (sender.Equals(this.BtnShake2))
            {
                // 开
                if (this.BtnShake2.Content.Equals((string)this.FindResource("Shake2Open")))
                {
                    CmdHelper.ShakeStart(2, false);
                    this.BtnShake2.Content = (string)this.FindResource("Shake2Close");
                }
                // 关
                else
                {
                    CmdHelper.ShakeStop(2, false);
                    this.BtnShake2.Content = (string)this.FindResource("Shake2Open");
                }
            }
            // 安装钢针
            else if (sender.Equals(this.BtnInstallSteelNeedle))
            {
                BackgroundProcess.RunAsync(() => CmdHelper.XaYa(0, ParamsHelper.CommonSettingList[0].SteelNeedleInstallPos.X, ParamsHelper.CommonSettingList[0].SteelNeedleInstallPos.Y), delegate (object returnResult)
                {
                });
            }
            // 紫外灯
            else if (sender.Equals(this.BtnUV))
            {
                CmdHelper.UVOpen(false);
                WindowUV wuv = new WindowUV();
                wuv.ShowDialog();
            }
            // 制冷
            else if (sender.Equals(this.BtnCool))
            {
                // 开制冷
                if (this.BtnCool.Content.Equals((string)this.FindResource("CoolOpen")))
                {
                    CmdHelper.CoolOpen(false);
                    this.BtnCool.Content = (string)this.FindResource("CoolClose");
                }
                // 关制冷
                else
                {
                    //CmdHelper.CoolClose(false);
                    this.BtnCool.Content = (string)this.FindResource("CoolOpen");
                }
            }
            // 程序链新增
            else if (sender.Equals(this.BtnProcedureChainNew))
            {
                if (WindowProcedureChainNew.EnsureExecute())
                {
                    RefreshProcedureChain();
                }
            }
            // 程序链删除
            else if (sender.Equals(this.BtnProcedureChainDel))
            {
                var selectedIndex = this.ControlDataGridProcedureChain.DataGridProcedures.SelectedIndex;
                if (selectedIndex < 0)
                {
                    MessageBox.Show((string)this.FindResource("Prompt_Pls_Select_Procedure_First"));
                    return;
                }
                if (MessageBox.Show((string)this.FindResource("Prompt_If_Delete_Procedure_1") + ParamsHelper.ProcedureAll.Procedures[ParamsHelper.ProcedureAll.ProcedureIndexChain[selectedIndex]].Name + (string)this.FindResource("Prompt_If_Delete_Procedure_2"), (string)Application.Current.FindResource("Prompt"), MessageBoxButton.YesNo, MessageBoxImage.Warning) ==
                    MessageBoxResult.Yes)
                {
                    ParamsHelper.ProcedureAll.ProcedureIndexChain.RemoveAt(selectedIndex);
                    FileUtils.SaveProcedureAll(ParamsHelper.ProcedureAll);
                    RefreshProcedureChain();
                }
            }
        }

        /// <summary>
        /// 执行程序（带耗材摆放提示）
        /// </summary>
        /// <param name="isDemo">是否演示</param>
        private void RunProcedure(bool isDemo)
        {
            // 提示是否已经摆放好板
            if (MessageBox.Show((string)this.FindResource("Prompt_Need_To_Put_Template_OK"),
                    (string)this.FindResource("Prompt"), MessageBoxButton.OKCancel, MessageBoxImage.Warning) ==
                MessageBoxResult.OK)
            {
                RunProcedure(true, isDemo);
            }
        }

        /// <summary>
        /// 执行程序
        /// </summary>
        /// <param name="isSuccessHint">完成后是否弹出框提示</param>
        /// <param name="isDemo">是否演示</param>
        private void RunProcedure(bool isSuccessHint, bool isDemo)
        {
            Dispatcher.Invoke(() => { SetLoadingStatus(ERunStatus.Running); });
            var thread = new Thread(() =>
            {
                try
                {
                    // 初始化
                    CmdHelper.InitMachine(true, false);

                    TakeAction(isDemo);

                    // 初始化
                    CmdHelper.InitMachine(true, false);

                    LogHelper.InfoTail((string)this.FindResource("ExecCompleted"));

                    Dispatcher.Invoke(() =>
                    {
                        SetLoadingStatus(ERunStatus.Stop);
                        if (isSuccessHint)
                            MessageBox.Show((string)this.FindResource("Prompt_Liquid_Relief_Success"));
                    });
                }
                catch (ManualStopException)
                {
                }
                catch (Exception ex)
                {
                    Dispatcher.Invoke(() =>
                    {
                        SetLoadingStatus(ERunStatus.Stop);
                        MessageBox.Show((string)this.FindResource("Prompt_Exec_Error") + "：" + ex.Message);
                    });
                }
            });
            thread.IsBackground = true;
            thread.Start();
        }

        /// <summary>
        /// 解析程序链
        /// </summary>
        /// <returns></returns>
        private bool ParseSeqFromProcedureChain()
        {
            var result = true;
            seqList.Clear();
            try
            {
                var round = 0;
                var seqTick = 0;
                foreach (var procedureIndex in ParamsHelper.ProcedureAll.ProcedureIndexChain)
                {
                    var procedure = ParamsHelper.ProcedureAll.Procedures[procedureIndex];

                    // 把步骤中需要开始等待计时置true
                    List<int> waitNonBlockStepOrderList = new List<int>();
                    var waitNonBlockList = procedure.Steps.Where(p => p.StepType == EStepType.Wait && !p.Block);
                    foreach (var wait in waitNonBlockList)
                    {
                        waitNonBlockStepOrderList.Add(wait.WaitForOrder);
                    }

                    for (int stepIndex = 0; stepIndex < procedure.Steps.Count; stepIndex++)
                    {
                        var step = procedure.Steps.ElementAt(stepIndex);
                        /**
                         * 移液逻辑：
                         * 1、源盘 --> 靶盘 ：1 --> N （一吸多喷）
                         * 2、源盘 --> 靶盘 ：N --> 1 （一吸一喷）
                         * 3、源盘 --> 靶盘 ：N --> N （一吸一喷）
                         */
                        // 移液
                        if (step.StepType == EStepType.LiquidTransfer)
                        {
                            // 错位布局下源/靶孔数与索引都要按视觉列过滤
                            var sourceConsumable = ConsumableHelper.GetConsumableType(0, step.SourceConsumable);
                            var targetConsumable = ConsumableHelper.GetConsumableType(0, step.TargetConsumable);
                            var sourceHoleCount = ConsumableHelper.GetHoleCount(step.SourceHoleIndexFrom, step.SourceHoleIndexTo, sourceConsumable);
                            var targetHoleCount = ConsumableHelper.GetHoleCount(step.TargetHoleIndexFrom, step.TargetHoleIndexTo, targetConsumable);

                            // 源盘 --> 靶盘 ：1 --> N （一吸多喷）
                            if (sourceHoleCount == 1 && targetHoleCount >= 1)
                            {
                                var seq = new Seq
                                {
                                    SteelNeedleChoiceIndex = procedure.SteelNeedleChoiceIndex,
                                    ProcedureIndexInChain = round,
                                    ProcedureIndexInSetting = procedureIndex,
                                    SeqBelongStepIndex = stepIndex,
                                    SeqIndex = seqTick,
                                    Name = step.Name,
                                    StepType = step.StepType,
                                    SourceTemplateTypeIndex = step.SourceTemplateTypeIndex,
                                    SourceTemplateIndex = step.SourceTemplateIndex,
                                    SourceTemplateConsumableType = ConsumableHelper.GetConsumableType(0, step.SourceConsumable),
                                    SourceHoleIndex = ConsumableHelper.GetHoleIndexList(step.SourceHoleIndexFrom, step.SourceHoleIndexTo, sourceConsumable)[0],
                                    TargetTemplateTypeIndex = step.TargetTemplateTypeIndex,
                                    TargetTemplateIndexList = new List<int> { step.TargetTemplateIndex },
                                    TargetTemplateConsumableType = ConsumableHelper.GetConsumableType(0, step.TargetConsumable),
                                    TargetHoleIndexList = ConsumableHelper.GetHoleIndexList(step.TargetHoleIndexFrom, step.TargetHoleIndexTo, targetConsumable),
                                    VolumeEachList = new List<decimal> { step.VolumeEach },
                                    IsNeedCleaning = step.IsNeedCleaning,
                                    CleaningPosIndexArray = step.CleaningPosIndexArray,
                                    WaitCountdownFromThisStep = waitNonBlockStepOrderList.Contains(stepIndex + 1)
                                };
                                seqList.Add(seq);
                            }
                            // 源盘 --> 靶盘 ：N --> 1 （一吸一喷）
                            else if (sourceHoleCount > 1 && targetHoleCount == 1)
                            {
                                var sourceHoleIndexList = ConsumableHelper.GetHoleIndexList(step.SourceHoleIndexFrom, step.SourceHoleIndexTo, sourceConsumable);
                                for (var i = 0; i < sourceHoleIndexList.Count; i++)
                                {
                                    var sourceHoleIndex = sourceHoleIndexList[i];
                                    var seq = new Seq
                                    {
                                        SteelNeedleChoiceIndex = procedure.SteelNeedleChoiceIndex,
                                        ProcedureIndexInChain = round,
                                        ProcedureIndexInSetting = procedureIndex,
                                        SeqBelongStepIndex = stepIndex,
                                        SeqIndex = seqTick,
                                        Name = step.Name,
                                        StepType = step.StepType,
                                        SourceTemplateTypeIndex = step.SourceTemplateTypeIndex,
                                        SourceTemplateIndex = step.SourceTemplateIndex,
                                        SourceTemplateConsumableType = ConsumableHelper.GetConsumableType(0, step.SourceConsumable),
                                        SourceHoleIndex = sourceHoleIndex,
                                        TargetTemplateTypeIndex = step.TargetTemplateTypeIndex,
                                        TargetTemplateIndexList = new List<int> { step.TargetTemplateIndex },
                                        TargetTemplateConsumableType = ConsumableHelper.GetConsumableType(0, step.TargetConsumable),
                                        TargetHoleIndexList = ConsumableHelper.GetHoleIndexList(step.TargetHoleIndexFrom, step.TargetHoleIndexTo, targetConsumable),
                                        VolumeEachList = new List<decimal> { step.VolumeEach },
                                        IsNeedCleaning = step.IsNeedCleaning,
                                        CleaningPosIndexArray = step.CleaningPosIndexArray,
                                        WaitCountdownFromThisStep = i == 0 && waitNonBlockStepOrderList.Contains(stepIndex + 1)
                                    };
                                    seqList.Add(seq);
                                }
                            }
                            // 源盘 --> 靶盘 ：N --> N （一吸一喷）
                            else if (sourceHoleCount > 1 && targetHoleCount > 1)
                            {
                                var sourceHoleIndexList = ConsumableHelper.GetHoleIndexList(step.SourceHoleIndexFrom, step.SourceHoleIndexTo, sourceConsumable);
                                var targetHoleIndexList = ConsumableHelper.GetHoleIndexList(step.TargetHoleIndexFrom, step.TargetHoleIndexTo, targetConsumable);
                                for (var i = 0; i < sourceHoleIndexList.Count; i++)
                                {
                                    var seq = new Seq
                                    {
                                        SteelNeedleChoiceIndex = procedure.SteelNeedleChoiceIndex,
                                        ProcedureIndexInChain = round,
                                        ProcedureIndexInSetting = procedureIndex,
                                        SeqBelongStepIndex = stepIndex,
                                        SeqIndex = seqTick,
                                        Name = step.Name,
                                        StepType = step.StepType,
                                        SourceTemplateTypeIndex = step.SourceTemplateTypeIndex,
                                        SourceTemplateIndex = step.SourceTemplateIndex,
                                        SourceTemplateConsumableType = ConsumableHelper.GetConsumableType(0, step.SourceConsumable),
                                        SourceHoleIndex = sourceHoleIndexList[i],
                                        TargetTemplateTypeIndex = step.TargetTemplateTypeIndex,
                                        TargetTemplateIndexList = new List<int> { step.TargetTemplateIndex },
                                        TargetTemplateConsumableType = ConsumableHelper.GetConsumableType(0, step.TargetConsumable),
                                        TargetHoleIndexList = new List<HoleIndex> { targetHoleIndexList[i] },
                                        VolumeEachList = new List<decimal> { step.VolumeEach },
                                        IsNeedCleaning = step.IsNeedCleaning,
                                        CleaningPosIndexArray = step.CleaningPosIndexArray,
                                        WaitCountdownFromThisStep = i == 0 && waitNonBlockStepOrderList.Contains(stepIndex + 1)
                                    };
                                    seqList.Add(seq);
                                }
                            }
                        }
                        // 其他：搅拌、等待、循环、清洗等
                        else
                        {
                            var seq = new Seq
                            {
                                ProcedureIndexInChain = round,
                                ProcedureIndexInSetting = procedureIndex,
                                SeqBelongStepIndex = stepIndex,
                                SeqIndex = seqTick,
                                Name = step.Name,
                                StepType = step.StepType,
                                WaitStepBlock = step.StepType == EStepType.Wait && step.Block,
                                SourceTemplateTypeIndex = step.SourceTemplateTypeIndex,
                                SourceTemplateIndex = step.SourceTemplateIndex,
                                SourceTemplateConsumableType = ConsumableHelper.GetConsumableType(0, step.SourceConsumable),
                                TargetTemplateTypeIndex = step.TargetTemplateTypeIndex,
                                TargetTemplateIndexList = new List<int> { step.TargetTemplateIndex },
                                TargetTemplateConsumableType = ConsumableHelper.GetConsumableType(0, step.TargetConsumable),
                                IsNeedCleaning = step.IsNeedCleaning,
                                LoopStepRange = step.LoopStepRange,
                                LoopTimes = step.LoopTimes,
                                Time = step.Time,
                                Temp = step.Temp,
                                SpeedLevel = step.SpeedLevel,
                                CleaningPosIndexArray = step.CleaningPosIndexArray,
                                WaitCountdownFromThisStep = waitNonBlockStepOrderList.Contains(stepIndex + 1)
                            };
                            seqList.Add(seq);
                        }

                        seqTick++;
                    }

                    round++;
                }

                try
                {
                    // 初始化盘位信息
                    Dispatcher.Invoke(InitTemplates);
                }
                catch (Exception ex)
                {
                    return false;
                }
            }
            catch (Exception e)
            {
                result = false;
            }

            return result;
        }

        /// <summary>
        /// 获取喷液补偿信息
        /// </summary>
        /// <param name="cmdJetOffset"></param>
        /// <returns></returns>
        private List<JetOffset> GetJetOffset(string cmdJetOffset)
        {
            var jetOffsetList = new List<JetOffset>();
            if (!cmdJetOffset.Equals(""))
            {
                var items = cmdJetOffset.Split(new[] { "(", ")" }, StringSplitOptions.RemoveEmptyEntries).Where(p => !p.Equals(",")).ToList();
                foreach (var item in items)
                {
                    var content = item.Split(new[] { ',', '，' }, StringSplitOptions.RemoveEmptyEntries);
                    var posStr = content[0];
                    var volume = decimal.Parse(content[1]);
                    // 判断是否孔位范围
                    if (posStr.Contains("-") || posStr.Contains("~"))
                    {
                        var posRange = posStr.Split(new[] { '-', '~' }, StringSplitOptions.RemoveEmptyEntries);
                        var fromPosIndex = int.Parse(posRange[0]) - 1;
                        var toPosIndex = int.Parse(posRange[1]) - 1;
                        for (var index = fromPosIndex; index <= toPosIndex; index++)
                        {
                            jetOffsetList.Add(new JetOffset { PosIndex = index, VolumeOffset = volume });
                        }
                    }
                    else
                    {
                        var posIndex = int.Parse(posStr) - 1;
                        var jetOffset = jetOffsetList.FirstOrDefault(p => p.PosIndex == posIndex);
                        if (null != jetOffset)
                            jetOffset.VolumeOffset = volume;
                        else
                            jetOffsetList.Add(new JetOffset { PosIndex = posIndex, VolumeOffset = volume });
                    }
                }
            }

            return jetOffsetList;
        }

        /// <summary>
        /// 初始化盘位信息
        /// </summary>
        private void InitTemplates()
        {
            // 先清空原盘位数据
            foreach (var t in templateCanUse)
            {
                t.Children.Clear();
            }

            templateDict.Clear();

            foreach (var seq in seqList)
            {
                // 步骤类型 = 移液，才刷新相关界面
                if (seq.StepType != EStepType.LiquidTransfer)
                    continue;

                // 只含有特殊指令
                if (seq.IsCmdOnly)
                    continue;

                var sourceTemplateConsumableType = seq.SourceTemplateConsumableType;
                var sourceTemplateTypeIndex = seq.SourceTemplateTypeIndex;
                // 盘位1~2：发酵区；盘位3~4：留样区
                var sourceTemplateIndex = seq.SourceTemplateIndex + sourceTemplateTypeIndex * 2;
                var sourceHoleIndex = seq.SourceHoleIndex;
                var volumeEachList = seq.VolumeEachList;
                var targetTemplateConsumableType = seq.TargetTemplateConsumableType;
                // 盘位1~2：发酵区；盘位3~4：留样区
                var targetTemplateTypeIndex = seq.TargetTemplateTypeIndex;
                var targetTemplateIndexList = seq.TargetTemplateIndexList.Select(p => p + targetTemplateTypeIndex * 2).ToList();
                var targetHoleIndexList = seq.TargetHoleIndexList;
                var serialDilute = seq.SerialDilute;
                var headUsedIndex = seq.HeadUsedIndex;
                var isPumpLiquid = seq.IsPumpLiquid;

                // 源盘盘位
                if (!isPumpLiquid)
                {
                    var sourceTotalVolume = 0m; // 源孔所需液体量
                    for (var i = 0; i < targetHoleIndexList.Count; i++)
                    {
                        if (serialDilute)
                            sourceTotalVolume = volumeEachList[0];
                        // 如果一吸多喷投入体积只有一个，就默认为相同体积，否则体积数必须等于靶孔数
                        else if (volumeEachList.Count > 1)
                            sourceTotalVolume += volumeEachList[i];
                        else
                            sourceTotalVolume += volumeEachList[0];
                    }
                    if (!templateDict.ContainsKey(sourceTemplateIndex))
                    {
                        var template = new Template
                        {
                            Title = sourceTemplateIndex < 2 ? (string)this.FindResource("TemplateFermentation") + (sourceTemplateIndex + 1) : (string)this.FindResource("TemplateSample") + (sourceTemplateIndex - 2 + 1),
                            SubTitle = sourceTemplateConsumableType.GroupName,
                            RowCount = sourceTemplateConsumableType.RowCount,
                            ColCount = sourceTemplateConsumableType.ColCount,
                            Step = sourceTemplateConsumableType.HoleStep,
                            A1Pos = ParamsHelper.CommonSettingList[0].A1Pos,
                            Type = ETemplateType.Source,
                            Staggered = sourceTemplateConsumableType.Staggered
                        };
                        var holeTotalCount = template.RowCount * template.ColCount; // 孔总数
                        for (var i = 0; i < holeTotalCount; i++)
                        {
                            template.Holes.Add(new Hole { Index = i, Capacity = 0 });
                        }

                        var controlTemplate = new ControlTemplateCommon(template);
                        controlTemplate.RefreshTemplateHolesStatus(headUsedIndex, sourceHoleIndex, sourceTotalVolume);
                        templateDict.Add(sourceTemplateIndex, controlTemplate);
                        templateCanUse[sourceTemplateIndex].Children.Add(controlTemplate);
                    }
                    else
                    {
                        if (!templateDict.TryGetValue(sourceTemplateIndex, out var controlTemplate))
                            return; // 该源盘位Index不存在
                        controlTemplate.RefreshTemplateHolesStatus(headUsedIndex, sourceHoleIndex, sourceTotalVolume);
                    }
                }

                // 靶盘盘位
                var targetTemplateIndexCount = targetTemplateIndexList.Count;
                for (var i = 0; i < targetTemplateIndexCount; i++)
                {
                    var targetTemplateIndex = targetTemplateIndexList[i];
                    if (!templateDict.ContainsKey(targetTemplateIndex))
                    {
                        var template = new Template
                        {
                            Title = targetTemplateIndex < 2 ? (string)this.FindResource("TemplateFermentation") + (targetTemplateIndex + 1) : (string)this.FindResource("TemplateSample") + (targetTemplateIndex - 2 + 1),
                            SubTitle = targetTemplateConsumableType.GroupName,
                            RowCount = targetTemplateConsumableType.RowCount,
                            ColCount = targetTemplateConsumableType.ColCount,
                            Step = targetTemplateConsumableType.HoleStep,
                            A1Pos = ParamsHelper.CommonSettingList[0].A1Pos,
                            Type = ETemplateType.Target,
                            Staggered = targetTemplateConsumableType.Staggered
                        };
                        var holeTotalCount = template.RowCount * template.ColCount; // 孔总数
                        for (var j = 0; j < holeTotalCount; j++)
                        {
                            template.Holes.Add(new Hole { Index = j, Capacity = 0 });
                        }

                        var controlTemplate = new ControlTemplateCommon(template);

                        // 多靶多孔
                        if (targetTemplateIndexCount > 1)
                        {
                            var targetVolumeEach = volumeEachList.Count > 1 ? volumeEachList[i] : volumeEachList[0];
                            controlTemplate.RefreshTemplateHolesStatus(headUsedIndex, targetHoleIndexList[i], targetVolumeEach);
                        }
                        // 1靶多孔
                        else
                        {
                            for (var j = 0; j < targetHoleIndexList.Count; j++)
                            {
                                var targetHoleIndex = targetHoleIndexList[j];
                                var targetVolumeEach = volumeEachList.Count > 1 ? volumeEachList[j] : volumeEachList[0];
                                controlTemplate.RefreshTemplateHolesStatus(headUsedIndex, targetHoleIndex, targetVolumeEach);
                            }
                        }

                        templateDict.Add(targetTemplateIndex, controlTemplate);
                        templateCanUse[targetTemplateIndex].Children.Add(controlTemplate);
                    }
                    else
                    {
                        if (!templateDict.TryGetValue(targetTemplateIndex, out var controlTemplate))
                            return; // 该靶盘位Index不存在

                        // 多靶多孔
                        if (targetTemplateIndexCount > 1)
                        {
                            var targetVolumeEach = volumeEachList.Count > 1 ? volumeEachList[i] : volumeEachList[0];
                            controlTemplate.RefreshTemplateHolesStatus(headUsedIndex, targetHoleIndexList[i], targetVolumeEach);
                        }
                        // 1靶多孔
                        else
                        {
                            for (var j = 0; j < targetHoleIndexList.Count; j++)
                            {
                                var targetHoleIndex = targetHoleIndexList[j];
                                var targetVolumeEach = volumeEachList.Count > 1 ? volumeEachList[j] : volumeEachList[0];
                                controlTemplate.RefreshTemplateHolesStatus(headUsedIndex, targetHoleIndex, targetVolumeEach);
                            }
                        }
                    }
                }
            }
        }

        /// <summary>
        /// 执行程序
        /// </summary>
        /// <param name="isDemo">是否演示</param>
        /// <exception cref="ManualStopException"></exception>
        private void TakeAction(bool isDemo)
        {
            // 程序链包含多个程序，用ProcedureIndexInChain区分
            var procedureChain = seqList.GroupBy(p => p.ProcedureIndexInChain);

            for (var chainIndex = 0; chainIndex < procedureChain.Count(); chainIndex++)
            {
                var procedureSeqs = procedureChain.ElementAt(chainIndex);

                LogHelper.InfoTitle((string)this.FindResource("Procedure") + "《" + ParamsHelper.ProcedureAll.Procedures[procedureSeqs.ElementAt(0).ProcedureIndexInSetting].Name + "》");

                // 显示详细步骤
                Dispatcher.Invoke(() =>
                {
                    this.ControlDataGridProcedureChain.DataGridProcedures.SelectedIndex = chainIndex;
                    this.ControlDataGridProcedureChain.DataGridProcedures.Focus();
                });

                // 循环程序
                if (procedureSeqs.Count() == 1 && procedureSeqs.ElementAt(0).StepType == EStepType.Loop)
                {
                    var seqLoop = procedureSeqs.ElementAt(0);
                    var procedureStartIndex = seqLoop.LoopStepRange[0] - 1;
                    var procedureStopIndex = seqLoop.LoopStepRange[1] - 1;
                    for (var loop = 0; loop < seqLoop.LoopTimes; loop++)
                    {
                        LogHelper.Info((string)this.FindResource("LoopProcedure"), (procedureStartIndex + 1) + "~" + (procedureStopIndex + 1));

                        for (var procedureIndex = procedureStartIndex; procedureIndex <= procedureStopIndex; procedureIndex++)
                        {
                            var procedureLoop = procedureChain.ElementAt(procedureIndex);
                            ActionProcedureExceptLoop(isDemo, procedureLoop.ToList());
                        }
                    }
                }
                // 其他程序
                else
                {
                    ActionProcedureExceptLoop(isDemo, procedureSeqs.ToList());
                }
            }
        }

        /// <summary>
        /// 执行除了循环外的程序
        /// </summary>
        /// <param name="isDemo"></param>
        /// <param name="procedureSeqs"></param>
        private void ActionProcedureExceptLoop(bool isDemo, List<Seq> procedureSeqs)
        {
            // 因为1个移液步骤有可能是多个seq组成，所以需要按照SeqBelongStepIndex分组
            var stepGroups = procedureSeqs.GroupBy(p => p.SeqBelongStepIndex);
            var stepCount = stepGroups.Count();

            // 当前步骤是否开始计时
            DateTime? waitStepBlockStartSecTime = null;

            for (var stepIndex = 0; stepIndex < stepCount; stepIndex++)
            {
                // 当前步骤
                var stepThis = stepGroups.ElementAt(stepIndex).ToList();
                // 下一个步骤（可能为null）
                var stepNext = stepIndex + 1 >= stepCount ? null : stepGroups.ElementAt(stepIndex + 1).ToList();

                // 当前步骤是否开始计时
                if (stepThis.First().WaitCountdownFromThisStep)
                    waitStepBlockStartSecTime = DateTime.Now;
                // 如果当前步骤为等待且非阻塞，则继续下一个步骤
                if (stepThis.Count == 1)
                {
                    var step = stepThis.First();
                    if (step.StepType == EStepType.Wait && !step.WaitStepBlock)
                        continue;
                }

                // 循环步骤
                if (stepThis.Count == 1 && stepThis.First().StepType == EStepType.Loop)
                {
                    var step = stepThis.First();
                    var stepStartIndex = step.LoopStepRange[0] - 1;
                    var stepStopIndex = step.LoopStepRange[1] - 1;
                    var loopTime = step.LoopTimes;
                    for (var loop = 0; loop < loopTime; loop++)
                    {
                        var isLastLoop = loop == loopTime - 1;
                        LogHelper.Info((string)this.FindResource("LoopStep"), (stepStartIndex + 1) + "~" + (stepStopIndex + 1));
                        var loopSteps = stepGroups.Where(p => p.Key >= stepStartIndex && p.Key <= stepStopIndex);
                        var loopStepCount = loopSteps.Count();
                        for (var index = 0; index < loopStepCount; index++)
                        {
                            var stepLoopThis = loopSteps.ElementAt(index).ToList();
                            var stepLoopNext = index + 1 >= loopStepCount ? null : loopSteps.ElementAt(index + 1).ToList();
                            var lastStepIsWait = loopSteps.Last().Count() == 1 && loopSteps.Last().First().StepType == EStepType.Wait;

                            // 当前步骤是否开始计时
                            if (stepLoopThis.First().WaitCountdownFromThisStep)
                                waitStepBlockStartSecTime = DateTime.Now;
                            // 如果当前步骤为等待且非阻塞，则继续下一个步骤
                            if (stepLoopThis.Count == 1)
                            {
                                var stepLoop = stepLoopThis.First();
                                if (stepLoop.StepType == EStepType.Wait && !stepLoop.WaitStepBlock)
                                    continue;
                            }

                            ActionSeqExceptLoop(isDemo, stepLoopThis, stepLoopNext, isLastLoop && lastStepIsWait, waitStepBlockStartSecTime);
                        }
                    }
                }
                // 其他：移液、等待、搅拌、清洗等
                else
                {
                    ActionSeqExceptLoop(isDemo, stepThis, stepNext, false, waitStepBlockStartSecTime);
                }
            }
        }

        /// <summary>
        /// 执行除了循环外的seq
        /// </summary>
        /// <param name="isDemo"></param>
        /// <param name="stepsThis">当前步骤的seq列表</param>
        /// <param name="stepsNext">下一个步骤的seq列表</param>
        /// <param name="isLastLoopAndLastStepIsWait">循环步骤中，最后一次循环且最后一个步骤为等待</param>
        /// <param name="waitStepBlockStartSecTime">非阻塞等待计时开始时间</param>
        /// <exception cref="ManualStopException"></exception>
        private void ActionSeqExceptLoop(bool isDemo, List<Seq> stepsThis, List<Seq> stepsNext, bool isLastLoopAndLastStepIsWait, DateTime? waitStepBlockStartSecTime)
        {
            foreach (var seq in stepsThis)
            {
                switch (seq.StepType)
                {
                    // 移液
                    case EStepType.LiquidTransfer:
                        #region 移液
                        LogHelper.Info((string)this.FindResource("LiquidTransfer"), "");

                        // 是否不执行该序列
                        if (seq.IsComment)
                            return;

                        // 是否只有特殊指令
                        if (seq.IsCmdOnly)
                        {
                            // 执行特殊指令
                            ParseCmdLine(seq.Cmd, seq.IsTxtLink);
                            return;
                        }

                        // 使用枪头Index
                        var headUsedIndex = seq.HeadUsedIndex;
                        CmdHelper.headStatusList[headUsedIndex].SeqIndex = seq.SeqIndex;

                        // 使用的钢针Index
                        var steelNeedleChoiceIndex = seq.SteelNeedleChoiceIndex;

                        // 源盘
                        var sourceTemplateIndex = seq.SourceTemplateIndex;
                        var sourceTemplateConsumableType = seq.SourceTemplateConsumableType;
                        var sourceHoleIndex = seq.SourceHoleIndex;

                        // 靶盘
                        var targetTemplateIndexList = seq.TargetTemplateIndexList;
                        var targetTemplateConsumableType = seq.TargetTemplateConsumableType;
                        var targetHoleIndexList = seq.TargetHoleIndexList;
                        var volumeEachList = seq.VolumeEachList;

                        // 梯度稀释
                        var serialDilute = seq.SerialDilute;

                        /**
                         * 特殊指令
                         */
                        var cmd = seq.Cmd;

                        // 移液头最大量程
                        var headLiquidRangeMax = ConstantsUtils.LiquidRangeMaxDic[ParamsHelper.HeadList[headUsedIndex].HeadLiquidRange];
                        // 移液头实际量程
                        var headLiquidRangeReal = Convert.ToDecimal((int)ParamsHelper.HeadList[headUsedIndex].HeadLiquidRange);

                        // 靶孔数目（区分一吸一喷还是一吸多喷）
                        var targetHoleIndexCount = targetHoleIndexList.Count;
                        // 是否一吸多喷
                        var isOne2More = targetHoleIndexCount > 1;

                        /**
                         * 当前机器移液量程（x毫升的机器会存在吸喷多于x毫升 或者 一吸多喷多次吸液的情况，所以要采取多吸）
                         */
                        // 分液次数
                        var liquidAbsorbCount = 1;
                        // 每次吸液量
                        var liquidVolumeAbsorbEach = 0m;
                        // 一吸多喷中，超过量程吸喷液信息
                        var One2MoreInfoList = new List<One2More>();
                        // 额外多吸体积
                        var additionalVolume = serialDilute ? 0 : ParamsHelper.CommonSettingList[headUsedIndex].AbsorbAirBeforePercent + ParamsHelper.CommonSettingList[headUsedIndex].AbsorbAirAfterPercent * 0.01m * headLiquidRangeReal;

                        /**
                         * 计算吸液次数和每次吸液体积
                         */
                        // 一吸一喷
                        if (!isOne2More)
                        {
                            // 一共吸取的体积
                            var liquidVolumeTotal = 0m;
                            for (var j = 0; j < targetHoleIndexCount; j++)
                            {
                                // 如果一吸多喷投入体积只有一个，就默认为相同体积，否则体积数必须等于靶孔数
                                if (volumeEachList.Count > 1)
                                    liquidVolumeAbsorbEach += volumeEachList[j];
                                else
                                    liquidVolumeAbsorbEach += volumeEachList[0];
                            }
                            liquidVolumeTotal = liquidVolumeAbsorbEach;

                            // 注意：需要加上空气量再比较，（吸液前后空气量 + 吸液量）是否大于量程的最大值
                            while (liquidVolumeAbsorbEach + additionalVolume > headLiquidRangeMax)
                            {
                                liquidAbsorbCount++;
                                liquidVolumeAbsorbEach = liquidVolumeTotal / liquidAbsorbCount;
                            }
                        }
                        // 一吸多喷
                        else
                        {
                            // 上次移液截取的盘位或孔位开始index
                            var lastStartIndex = 0;
                            // 一吸多喷多吸体积
                            additionalVolume += serialDilute ? 0 : ParamsHelper.CommonSettingList[headUsedIndex].AbsorbLiquidMoreOne2MorePercent * 0.01m * headLiquidRangeReal;
                            for (var index = 0; index < seq.TargetHoleIndexList.Count; index++)
                            {
                                // 当前孔吸液体积
                                var volumeCurrentHole = seq.VolumeEachList.Count == 1
                                    ? seq.VolumeEachList[0]
                                    : seq.VolumeEachList[index];
                                // 下一孔吸液体积
                                var volumeNextHole = 0m;
                                if (index + 1 < seq.TargetHoleIndexList.Count)
                                {
                                    volumeNextHole = seq.VolumeEachList.Count == 1
                                        ? seq.VolumeEachList[0]
                                        : seq.VolumeEachList[index + 1];
                                }

                                // 当前孔吸液体积 > 量程
                                if (volumeCurrentHole + additionalVolume > headLiquidRangeMax)
                                {
                                    var absorbCountInThisHoleOne2More = 1; // 本孔分液次数
                                    liquidVolumeAbsorbEach = volumeCurrentHole;
                                    while (liquidVolumeAbsorbEach + additionalVolume > headLiquidRangeMax)
                                    {
                                        absorbCountInThisHoleOne2More++;
                                        liquidVolumeAbsorbEach = volumeCurrentHole / absorbCountInThisHoleOne2More;
                                    }

                                    for (var absortIndex = 0; absortIndex < absorbCountInThisHoleOne2More; absortIndex++)
                                    {
                                        var one2More = new One2More
                                        {
                                            SourceVolumeAbsorbEach = liquidVolumeAbsorbEach,
                                            TargetTemplateIndexListEach = seq.TargetTemplateIndexList.Count == 1
                                            ? seq.TargetTemplateIndexList
                                            : seq.TargetTemplateIndexList.GetRange(index, 1),
                                            TargetHoleIndexListEach = seq.TargetHoleIndexList.GetRange(index, 1),
                                            TargetVolumeJetListEach = new List<decimal> { liquidVolumeAbsorbEach }
                                        };
                                        One2MoreInfoList.Add(one2More);

                                        liquidAbsorbCount++;
                                    }
                                    liquidVolumeAbsorbEach = 0;
                                    lastStartIndex = index;
                                }
                                else
                                {
                                    var lastVolume = 0m;
                                    liquidVolumeAbsorbEach += volumeCurrentHole;
                                    if (serialDilute) liquidVolumeAbsorbEach = volumeCurrentHole;
                                    lastVolume = volumeCurrentHole;

                                    // 如果叠加体积超过量程，剔除现在的孔
                                    if (liquidVolumeAbsorbEach + additionalVolume > headLiquidRangeMax && !serialDilute)
                                    {
                                        // 前n-1次分液（n为分液次数）
                                        var indexCount = index - lastStartIndex;
                                        var one2More = new One2More
                                        {
                                            SourceVolumeAbsorbEach = liquidVolumeAbsorbEach - lastVolume, // 减去上一孔多余的体积
                                            TargetTemplateIndexListEach = seq.TargetTemplateIndexList.Count == 1
                                                ? seq.TargetTemplateIndexList
                                                : seq.TargetTemplateIndexList.GetRange(lastStartIndex, indexCount),
                                            TargetHoleIndexListEach =
                                                seq.TargetHoleIndexList.GetRange(lastStartIndex, indexCount),
                                            TargetVolumeJetListEach = seq.VolumeEachList.Count == 1
                                                ? seq.VolumeEachList
                                                : seq.VolumeEachList.GetRange(lastStartIndex, indexCount)
                                        };
                                        One2MoreInfoList.Add(one2More);

                                        liquidAbsorbCount++;
                                        liquidVolumeAbsorbEach = 0;
                                        lastStartIndex = index;
                                        index -= 1; // 从上一个靶孔开始检索
                                    }
                                    // 如果下一孔超过量程，处理之前的孔
                                    else if (volumeNextHole + additionalVolume > headLiquidRangeMax && !serialDilute)
                                    {
                                        var indexCount = index - lastStartIndex + 1;
                                        var one2More = new One2More
                                        {
                                            SourceVolumeAbsorbEach = liquidVolumeAbsorbEach,
                                            TargetTemplateIndexListEach = seq.TargetTemplateIndexList.Count == 1
                                                ? seq.TargetTemplateIndexList
                                                : seq.TargetTemplateIndexList.GetRange(lastStartIndex, indexCount),
                                            TargetHoleIndexListEach =
                                                seq.TargetHoleIndexList.GetRange(lastStartIndex, indexCount),
                                            TargetVolumeJetListEach = seq.VolumeEachList.Count == 1
                                                ? seq.VolumeEachList
                                                : seq.VolumeEachList.GetRange(lastStartIndex, indexCount)
                                        };
                                        One2MoreInfoList.Add(one2More);

                                        liquidAbsorbCount++;
                                        liquidVolumeAbsorbEach = 0;
                                        lastStartIndex = index;
                                    }
                                }
                            }

                            //最后一次分液
                            if (liquidVolumeAbsorbEach != 0)
                            {
                                var lastRoundOne2More = new One2More();
                                var lastRoundIndexCount = targetHoleIndexCount - lastStartIndex;
                                lastRoundOne2More.SourceVolumeAbsorbEach = liquidVolumeAbsorbEach;
                                lastRoundOne2More.TargetTemplateIndexListEach = seq.TargetTemplateIndexList.Count == 1
                                    ? seq.TargetTemplateIndexList
                                    : seq.TargetTemplateIndexList.GetRange(lastStartIndex, lastRoundIndexCount);
                                lastRoundOne2More.TargetHoleIndexListEach = seq.TargetHoleIndexList.GetRange(lastStartIndex, lastRoundIndexCount);
                                lastRoundOne2More.TargetVolumeJetListEach = seq.VolumeEachList.Count == 1
                                    ? seq.VolumeEachList
                                    : seq.VolumeEachList.GetRange(lastStartIndex, lastRoundIndexCount);
                                One2MoreInfoList.Add(lastRoundOne2More);
                            }

                            // 再次处理一下吸液次数，预防超出Index范围
                            liquidAbsorbCount = One2MoreInfoList.Count;
                        }

                        /**
                         * 流程动作逻辑：
                         * ①如果体积只有1个且为0，且靶盘位置不为空，直接到靶盘执行特殊指令
                         * ②其余按照正常逻辑进行
                         */
                        // 直接到靶盘执行特殊指令
                        if (volumeEachList.Count == 1 && volumeEachList[0] == 0 && targetTemplateIndexList.Count == 1 && targetHoleIndexList.Count == 1)
                        {
                            // 到靶盘位置喷液
                            CmdHelper.JetSingleLiquid(headUsedIndex, steelNeedleChoiceIndex, targetTemplateConsumableType, null, targetTemplateIndexList[0], targetHoleIndexList[0], seq, liquidVolumeAbsorbEach, isDemo);
                        }
                        else
                        {
                            // 一次吸液
                            if (liquidAbsorbCount == 1)
                            {
                                // 吸液
                                CmdHelper.AbsorbLiquid(headUsedIndex, steelNeedleChoiceIndex, sourceTemplateConsumableType, targetTemplateConsumableType, sourceTemplateIndex, sourceHoleIndex, seq, liquidVolumeAbsorbEach, isOne2More, isDemo);

                                // 一吸一喷
                                if (!isOne2More)
                                {
                                    CmdHelper.JetSingleLiquid(headUsedIndex, steelNeedleChoiceIndex, targetTemplateConsumableType, null, targetTemplateIndexList[0], targetHoleIndexList[0], seq, liquidVolumeAbsorbEach, isDemo);
                                }
                                // 一吸多喷
                                else
                                    CmdHelper.JetMultiLiquid(headUsedIndex, steelNeedleChoiceIndex, sourceTemplateConsumableType, sourceTemplateIndex, sourceHoleIndex, targetTemplateConsumableType, null, targetTemplateIndexList, targetHoleIndexList, seq, volumeEachList, isDemo);
                            }
                            // 多次吸液
                            else
                            {
                                // 一吸一喷
                                if (!isOne2More)
                                {
                                    for (var j = 0; j < liquidAbsorbCount; j++)
                                    {
                                        // 吸液
                                        CmdHelper.AbsorbLiquid(headUsedIndex, steelNeedleChoiceIndex, sourceTemplateConsumableType, targetTemplateConsumableType, sourceTemplateIndex, sourceHoleIndex, seq,
                                            liquidVolumeAbsorbEach, false, isDemo);

                                        // 喷液
                                        CmdHelper.JetSingleLiquid(headUsedIndex, steelNeedleChoiceIndex, targetTemplateConsumableType, sourceTemplateConsumableType, targetTemplateIndexList[0], targetHoleIndexList[0], seq, liquidVolumeAbsorbEach, isDemo);
                                    }
                                }
                                // 一吸多喷
                                else
                                {
                                    for (var j = 0; j < liquidAbsorbCount; j++)
                                    {
                                        var one2MoreInfo = One2MoreInfoList[j];

                                        // 吸液
                                        CmdHelper.AbsorbLiquid(headUsedIndex, steelNeedleChoiceIndex, sourceTemplateConsumableType, targetTemplateConsumableType, sourceTemplateIndex, sourceHoleIndex, seq, one2MoreInfo.SourceVolumeAbsorbEach, true, isDemo);

                                        // 喷液
                                        CmdHelper.JetMultiLiquid(headUsedIndex, steelNeedleChoiceIndex, sourceTemplateConsumableType, sourceTemplateIndex, sourceHoleIndex, targetTemplateConsumableType, sourceTemplateConsumableType, one2MoreInfo.TargetTemplateIndexListEach,
                                            one2MoreInfo.TargetHoleIndexListEach, seq, one2MoreInfo.TargetVolumeJetListEach, isDemo);
                                    }
                                }
                            }
                        }

                        // 执行特殊指令
                        ParseCmdLine(seq.Cmd, seq.IsTxtLink);

                        break;
                    #endregion
                    // 搅拌启动
                    case EStepType.ShakeStart:
                        #region 搅拌启动
                        if (!isDemo)
                        {
                            LogHelper.Info((string)this.FindResource("ShakeStart"), "");
                            var num = seq.SourceTemplateIndex + 1;
                            var shakeSec = seq.Time * 60;

                            CmdHelper.ShakeStart(num, true);
                            if (shakeSec > 0)
                            {
                                Dispatcher.Invoke(() =>
                                {
                                    SetLoadingStatus(ERunStatus.Countdown, false);
                                });
                                // 已经过去的时间
                                var shakeTimePassed = 0;
                                while (shakeTimePassed < shakeSec)
                                {
                                    Thread.Sleep(1000);
                                    shakeTimePassed += 1;

                                    CmdHelper.ManualStop(false);
                                }

                                Dispatcher.Invoke(() =>
                                {
                                    SetLoadingStatus(ERunStatus.Running, false, true);
                                });
                            }
                        }
                        break;
                    #endregion
                    // 搅拌停止
                    case EStepType.ShakeStop:
                        #region 搅拌停止
                        if (!isDemo)
                        {
                            LogHelper.Info((string)this.FindResource("ShakeStop"), "");
                            var num = seq.SourceTemplateIndex + 1;
                            CmdHelper.ShakeStop(num, true);
                        }
                        break;
                    #endregion
                    // 清洗
                    case EStepType.Cleaning:
                        #region 清洗
                        LogHelper.Info((string)this.FindResource("Cleaning"), "");
                        CmdHelper.Cleaning(0, seq.TargetTemplateConsumableType, seq, isDemo);
                        break;
                    #endregion
                    // 等待（阻塞）
                    case EStepType.Wait:
                        #region 等待
                        if (!isDemo && seq.WaitStepBlock && !isLastLoopAndLastStepIsWait)
                        {
                            var waitSec = seq.Time * 60;
                            LogHelper.Info((string)this.FindResource("Wait"), seq.Time + (string)this.FindResource("UnitMin"));
                            Dispatcher.Invoke(() =>
                            {
                                SetLoadingStatus(ERunStatus.Countdown, false);
                                this.LabelIntervalRemain.Content = waitSec;
                                this.StackPanelIntervalRemain.Visibility = Visibility.Visible;
                            });
                            // 已经过去的时间
                            var waitTimePassed = 0;
                            while (waitTimePassed < waitSec)
                            {
                                Thread.Sleep(1000);
                                waitTimePassed += 1;

                                Dispatcher.Invoke(() =>
                                {
                                    var waitRemain = waitSec - waitTimePassed;
                                    this.LabelIntervalRemain.Content = waitRemain;
                                });
                                CmdHelper.ManualStop(false);
                            }

                            Dispatcher.Invoke(() =>
                            {
                                this.StackPanelIntervalRemain.Visibility = Visibility.Collapsed;
                                SetLoadingStatus(ERunStatus.Running, false, true);
                            });
                        }
                        break;
                        #endregion
                }
            }

            // 如果下一个步骤为等待且非阻塞，还没到时间，就继续等待；否则继续运行
            if (stepsNext != null && stepsNext.Count == 1)
            {
                var seqNext = stepsNext.First();
                if (seqNext.StepType == EStepType.Wait && !seqNext.WaitStepBlock && !isLastLoopAndLastStepIsWait && waitStepBlockStartSecTime != null)
                {
                    var waitStepBlockStopSecTime = DateTime.Now;
                    var waitSec = seqNext.Time * 60;
                    // 判断是否已经等待足够时间
                    var waitStepBlockTimeElapsed = Convert.ToInt32((waitStepBlockStopSecTime - (DateTime)waitStepBlockStartSecTime).TotalSeconds);
                    // 还没到时间，继续等待
                    if (waitStepBlockTimeElapsed < waitSec)
                    {
                        var waitSecRemain = waitSec - waitStepBlockTimeElapsed;
                        LogHelper.Info((string)this.FindResource("Wait"), waitSecRemain + (string)this.FindResource("UnitSec"));
                        Dispatcher.Invoke(() =>
                        {
                            SetLoadingStatus(ERunStatus.Countdown, false);
                            this.LabelIntervalRemain.Content = waitSecRemain;
                            this.StackPanelIntervalRemain.Visibility = Visibility.Visible;
                        });
                        // 已经过去的时间
                        var waitTimePassed = 0;
                        while (waitTimePassed < waitSecRemain)
                        {
                            Thread.Sleep(1000);
                            waitTimePassed += 1;

                            Dispatcher.Invoke(() =>
                            {
                                var waitRemain = waitSecRemain - waitTimePassed;
                                this.LabelIntervalRemain.Content = waitRemain;
                            });
                            CmdHelper.ManualStop(false);
                        }

                        Dispatcher.Invoke(() =>
                        {
                            this.StackPanelIntervalRemain.Visibility = Visibility.Collapsed;
                            SetLoadingStatus(ERunStatus.Running, false, true);
                        });
                    }
                }
            }

        }

        /// <summary>
        /// 解析特殊指令
        /// </summary>
        /// <param name="cmd"></param>
        /// <param name="isTxtLink">是否文件链接</param>
        public void ParseCmdLine(string cmd, bool isTxtLink)
        {
            // 拆分行
            string[] lineCmd = cmd.Split(new[] { "\r\n" }, StringSplitOptions.RemoveEmptyEntries);
            foreach (var line in lineCmd)
            {
                // 剔除相关特殊指令
                if (isTxtLink && CheckCmdExist(line))
                    continue;

                // 拆分分号
                string[] semiCmd = line.Split(new[] { ';', '；' }, StringSplitOptions.RemoveEmptyEntries);
                foreach (var semi in semiCmd)
                {
                    // 剔除相关特殊指令
                    if (!isTxtLink && CheckCmdExist(semi))
                        continue;

                    // 等待指令
                    if (semi.ToLower().StartsWith("wait"))
                    {
                        // 剔除“WAIT”
                        var subCmd = semi.Substring(4).Trim();

                        // 一直暂停
                        if (subCmd.Equals(""))
                        {
                            Dispatcher.Invoke(() =>
                            {
                                // 马上停止下一个指令执行
                                CmdHelper.isManualPause = true;
                                CmdHelper.isManualStop = false;
                                // 模拟手动点击“暂停”
                                mMainWindow.SetLoadingStatus(ERunStatus.Pause, false);
                            });
                        }
                        // 暂停，并弹出框提示
                        else if (subCmd.Contains("*"))
                        {
                            Dispatcher.Invoke(() =>
                            {
                                // 马上停止下一个指令执行
                                CmdHelper.isManualPause = true;
                                CmdHelper.isManualStop = false;
                                // 模拟手动点击“暂停”
                                mMainWindow.SetLoadingStatus(ERunStatus.Pause, false);
                                this.LabelPrompt.Content = subCmd.Substring(1);
                            });
                        }
                        // 暂停x毫秒
                        else
                            Thread.Sleep(Int32.Parse(subCmd));
                    }
                    // 普通指令
                    else
                    {
                        // 检测试管摆放指令
                        if (semi.ToLower().Contains(AutoLiquid_Library.Utils.ConstantsUtils.CheckTubeCmd.ToLower()))
                        {
                            var otherCmd = semi.ToLower().Replace(AutoLiquid_Library.Utils.ConstantsUtils.CheckTubeCmd.ToLower(), "");
                            CheckTube();

                            CmdHelper.DoCmd(otherCmd.Trim(), true);
                        }
                        else
                            CmdHelper.DoCmd(semi.Trim(), true);
                    }
                }
            }
        }

        /// <summary>
        /// 检查是否包含特殊指令
        /// </summary>
        /// <param name="cmd"></param>
        /// <returns></returns>
        private bool CheckCmdExist(string cmd)
        {
            // 剔除移液头特殊指令
            if (cmd.ToLower().Contains(AutoLiquid_Library.Utils.ConstantsUtils.Head2Cmd.ToLower()))
                return true;

            // 剔除吸液、喷液前后特殊指令
            if (cmd.ToLower().Contains(AutoLiquid_Library.Utils.ConstantsUtils.AbsorbBeforeCmd.ToLower()) || cmd.ToLower().Contains(AutoLiquid_Library.Utils.ConstantsUtils.AbsorbAfterCmd.ToLower())
                                                                                                           || cmd.ToLower().Contains(AutoLiquid_Library.Utils.ConstantsUtils.JetBeforeCmd.ToLower()) || cmd.ToLower().Contains(AutoLiquid_Library.Utils.ConstantsUtils.JetAfterCmd.ToLower()))
                return true;

            // 剔除指定孔位喷液体积补偿指令
            if (cmd.ToLower().Contains(AutoLiquid_Library.Utils.ConstantsUtils.JetOffsetCmd.ToLower()))
                return true;

            // 剔除重复执行Excel表格
            if (cmd.ToLower().Contains(AutoLiquid_Library.Utils.ConstantsUtils.RepeatCmd.ToLower()))
                return true;

            return false;
        }

        /// <summary>
        /// 检测试管摆放（如没摆放，就等待）
        /// </summary>
        private void CheckTube()
        {
            // 是否检测完成
            var isCheckSuccess = false;
            CmdHelper.frmDAE.IsTubeExist = false;
            // 检测次数
            var trueCheckCount = 3;
            // 连续为true的次数
            var trueContinuousCount = 0;

            /**
             * 判断逻辑：
             * ①快速检测：快速检测10次（检测间隔为20ms），不弹出提示框，如果连续3次true，则继续后面的步骤； 如果false，跳到②普通检测
             * ②普通检测：弹出提示屏幕，检测3次（检测间隔为500ms）
             */
            // 快速检测
            for (var i = 0; i < 10; i++)
            {
                // 发送检测指令
                CmdHelper.CheckTubeExist(false);
                Thread.Sleep(20);
                CmdHelper.ManualStop(false);

                if (CmdHelper.frmDAE.IsTubeExist)
                {
                    trueContinuousCount++;
                    if (trueContinuousCount >= trueCheckCount)
                    {
                        isCheckSuccess = true;
                        break;
                    }
                }
                else
                    trueContinuousCount = 0; // 只要有1次不为true，就复位
            }

            // 普通检测
            if (!isCheckSuccess)
            {
                trueContinuousCount = 0;

                // 弹出框提示
                WindowMessageBox wmb = null;
                Dispatcher.Invoke(() =>
                {
                    wmb = new WindowMessageBox();
                    wmb.Show();
                });

                while (trueContinuousCount < trueCheckCount)
                {
                    // 发送检测指令
                    CmdHelper.CheckTubeExist(false);
                    Thread.Sleep(500);
                    CmdHelper.ManualStop(false);

                    if (CmdHelper.frmDAE.IsTubeExist)
                        trueContinuousCount++;
                    else
                        trueContinuousCount = 0; // 只要有1次不为true，就复位
                }

                Dispatcher.Invoke(() =>
                {
                    wmb?.Close();
                });
            }

            // 复位属性
            CmdHelper.frmDAE.IsTubeExist = false;
        }

        /// <summary>
        /// 显示程序详情
        /// </summary>
        /// <param name="chainIndex"></param>
        private void ShowProcedureDetail(int chainIndex)
        {
            if (ParamsHelper.ProcedureAll.ProcedureIndexChain.Count <= chainIndex)
                return;

            var procedureIndex = ParamsHelper.ProcedureAll.ProcedureIndexChain[chainIndex];
            var procedure = ParamsHelper.ProcedureAll.Procedures[procedureIndex];

            this.TextBlockProcedureName.Text = procedure.Name;

            this.RichTextBoxProcedureDetail.Document.Blocks.Clear();

            var steps = procedure.Steps;

            Paragraph info = new Paragraph { FontSize = 16 };
            // 只有循环
            if (steps.Count() == 1 && steps[0].StepType == EStepType.Loop)
            {
                var stepLoopOnly = steps[0];
                var loopStartIndex = stepLoopOnly.LoopStepRange[0] - 1;
                var loopStopIndex = stepLoopOnly.LoopStepRange[1] - 1;
                var loopStartProcedure = ParamsHelper.ProcedureAll.Procedures[ParamsHelper.ProcedureAll.ProcedureIndexChain[loopStartIndex]];
                var loopStopProcedure = ParamsHelper.ProcedureAll.Procedures[ParamsHelper.ProcedureAll.ProcedureIndexChain[loopStopIndex]];
                info.Inlines.Add(new Bold(new Run((string)this.FindResource("LoopProcedure") + Environment.NewLine)));
                info.Inlines.Add(new Run((string)this.FindResource("Procedure") + "《" + loopStartProcedure.Name + "》" + "~" + (string)this.FindResource("Procedure") + "《" + loopStopProcedure.Name + "》" + Environment.NewLine));
                info.Inlines.Add(new Bold(new Run((string)this.FindResource("LoopTimes") + Environment.NewLine)));
                info.Inlines.Add(new Run(stepLoopOnly.LoopTimes + Environment.NewLine));
            }
            else
            {
                for (var i = 0; i < steps.Count; i++)
                {
                    var step = steps[i];
                    switch (step.StepType)
                    {
                        case EStepType.LiquidTransfer:
                            var fromTemplate = step.SourceTemplateTypeIndex == 0 ? (string)this.FindResource("TemplateFermentation") : (string)this.FindResource("TemplateSample");
                            var toTemplate = step.TargetTemplateTypeIndex == 0 ? (string)this.FindResource("TemplateFermentation") : (string)this.FindResource("TemplateSample");
                            info.Inlines.Add(new Bold(new Run((string)this.FindResource("LiquidTransfer") + Environment.NewLine)));
                            info.Inlines.Add(new Run(fromTemplate + (step.SourceTemplateIndex + 1) + " -> " + toTemplate + (step.TargetTemplateIndex + 1) + "(" + step.VolumeEach + "ml)" + Environment.NewLine));
                            break;
                        case EStepType.ShakeStart:
                            var shakeTimeStr = (string)this.FindResource("Shake") + step.Time + (string)this.FindResource("UnitMin");
                            info.Inlines.Add(new Bold(new Run((string)this.FindResource("ShakeStart") + Environment.NewLine)));
                            info.Inlines.Add(new Run((string)this.FindResource("TemplateFermentation") + (step.SourceTemplateIndex + 1) + shakeTimeStr + Environment.NewLine));
                            break;
                        case EStepType.ShakeStop:
                            info.Inlines.Add(new Bold(new Run((string)this.FindResource("ShakeStop") + Environment.NewLine)));
                            info.Inlines.Add(new Run((string)this.FindResource("TemplateFermentation") + (step.SourceTemplateIndex + 1) + Environment.NewLine));
                            break;
                        case EStepType.Cleaning:
                            info.Inlines.Add(new Bold(new Run((string)this.FindResource("Cleaning") + Environment.NewLine)));
                            break;
                        case EStepType.Wait:
                            info.Inlines.Add(new Bold(new Run((string)this.FindResource("Wait") + Environment.NewLine)));
                            info.Inlines.Add(new Run(step.Time + (string)this.FindResource("UnitMin") + Environment.NewLine));
                            break;
                        case EStepType.Loop:
                            var loopStartIndex = step.LoopStepRange[0] - 1;
                            var loopStopIndex = step.LoopStepRange[1] - 1;
                            info.Inlines.Add(new Bold(new Run((string)this.FindResource("LoopStep") + Environment.NewLine)));
                            info.Inlines.Add(new Run((string)this.FindResource("ProcedureStep") + "《" + procedure.Steps[loopStartIndex].Name + "》" + "~" + (string)this.FindResource("ProcedureStep") + "《" + procedure.Steps[loopStopIndex].Name + "》" + Environment.NewLine));
                            info.Inlines.Add(new Bold(new Run((string)this.FindResource("LoopTimes") + Environment.NewLine)));
                            info.Inlines.Add(new Run(step.LoopTimes.ToString()));
                            break;
                    }
                }
            }

            this.RichTextBoxProcedureDetail.Document.Blocks.Add(info);
        }

        /// <summary>
        /// 检查动作逻辑
        /// 循环：范围不能有误
        /// </summary>
        /// <returns></returns>
        private Error CheckActionLogic()
        {
            var error = new Error();

            // 所有步骤
            var stepAll = ParamsHelper.ProcedureAll.Procedures[ParamsHelper.ProcedureAll.ProcedureSelectedIndex].Steps;

            // 逐个步骤判断逻辑
            for (var i = 0; i < stepAll.Count; i++)
            {
                var step = stepAll[i];
                error.Index = i;

                // 循环：范围不能有误
                if (step.StepType == EStepType.Loop)
                {
                    var stepCount = stepAll.Count;
                    var loopFrom = step.LoopStepRange[0];
                    var loopTo = step.LoopStepRange[1];

                    if (loopFrom < 1 || loopTo < 1 || loopFrom > stepCount || loopTo > stepCount || loopTo < loopFrom)
                    {
                        error.Available = true;
                        error.Info = (string)this.FindResource("Prompt_Logic_Error_Loop_Range");
                        break;
                    }
                }
            }

            return error;
        }
    }
}
