using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using System.Net;
using System.IO;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Xml.Serialization;
using DAERun.CallBack;


namespace DAERun
{
    public partial class FrmMain : Form
    {
        // tcp还是udp
        private bool isTcp = true;

        const int cmdExeTimeOut = 60000; //指令允许最长的执行时间，单位：毫秒 ///

        // Scale值保存文件
        private string conStrXmlFileName = "workplate.xml";

        // 目标IP
        public string remoteIP = "192.168.1.3";

        private System.Diagnostics.Stopwatch cmdWatch = new System.Diagnostics.Stopwatch();
        private System.Diagnostics.Stopwatch sysWatch = new System.Diagnostics.Stopwatch();
        private string[] deviceName = new string[] {
            "X",   "Y",  "Z",  "M",  "N",  "P",  "Q",  "V",  "W",
            "T1", "T2","PO1","PO2","PO3","PO4","PO5","A","O","PO6","PO7","PO8",
            "XX", "YY", "ZZ", "MM", "NN", "PP", "QQ", "VV", "WW",
            "Z1", "Z2", "Z3", "Z4", "Z5", "Z6", "Z7", "Z8",
            "P1", "P2", "P3", "P4", "P5", "P6", "P7", "P8"
        };
        private int[] deviceAddr = new int[] {
            11,   12,    13,  16,   17,   21,   22,    23,   14,
            31,   32 ,  41,   42,   43 ,  44,   45, 99,  50,  46,   47,    48,
            1,    2,    3,    6,    7,    8,    9,    10,    4,
            201,202,203,204,205,206,207,208,
            221,222,223,224,225,226,227,228
        };
        private List<Device> deviceList = new List<Device>();
        public Socket socketSend;
        private int listenPort = 4000;
        private long listenIp = 0;

        private IPEndPoint localEP;
        private IPEndPoint serverEP;

        //  bool isDo = false;  
        private FrmTip frmTip;
        private string cmdText;
        private string cmdTextOrg;
        private byte[] socketRecBuffer = new byte[20 * 13];
        private int seqPos = -1;
        private bool isReceiveFromNet = true;
        List<Device> deviceToControl = new List<Device>();
        public Thread thWork;
        public WorkPlate workPlate = new WorkPlate();
        public MotorPosition MotorPos;

        // 接收数据回调
        private ReceivedCallBack mCmdReceivedCallBack;

        // 指令是否被拦截（可用于判断开门停止）2019-08-31 liu
        private MyCallBack mCmdInterceptedCallBack; // 拦截回调函数 
        public string CurrentOperationCmd; // 当前执行的动作指令（预防拦截后继续，当前指令没有被执行）
        public bool IsDoorClosed; // 仓门是否已经关闭

        // 温控运行状态查询 2020-03-24 liu
        private string TempControlModuleStartCmd = "AU"; // 启动温控模块
        public bool IsTempControlModuleRunning; // 温控模块是否运行中
        public decimal TemperatureResult = 0;

        // 查询指令列表 2019-09-02 liu
        private List<string> mQueryCmdList = new List<string> { "AD1" };


        // 机器的设备id
        public string mDeviceId = "";

        // 试管是否存在
        public bool IsTubeExist;

        // 程序是否运行中（包括运行中和停止两种情况）
        public bool isRunning;
        // 清洗槽状态（是否有清洗液）
        // public bool IsCleaningFilled = false;


        /// <summary>
        /// 置true 隐藏提示窗口
        /// </summary>
        public bool IsShowFormTip { get; set; } = true;

        /// <summary>
        ///置true人工取消DoSeq的执行
        /// </summary>
        public bool IsManualStop { get; set; } = false;

        /// <summary>
        /// 置true： 人工对DoSeq暂停 ，置false 取消人工暂停
        /// </summary>
        public bool IsManualPause { get; set; } = false;

        /// <summary>
        /// 0:No Error  1:No anwser   2:Command executed time out  3:Wrong Command  4:Manual Stop  5:Oher Error
        /// </summary>
        public int LastErrorType { get; set; }
        /// <summary>
        /// 错误信息
        /// </summary>
        public string LastErrorMsg { get; set; }

        public FrmMain(string remoteIp, int localPort, string xmlFileName) : this()
        {
            InitializeComponent();

            this.remoteIP = remoteIp;
            // tcp端口默认是4001
            this.listenPort = isTcp ? localPort + 1 : localPort;
            this.conStrXmlFileName = xmlFileName;

            // 加载Scale值
            if (!this.conStrXmlFileName.Equals(""))
                ReadConfig();

            try
            {
                localEP = new IPEndPoint(listenIp, listenPort);
                serverEP = new IPEndPoint(IPAddress.Parse(this.remoteIP), listenPort);

                if (this.isTcp)
                {
                    socketSend = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                    socketSend.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.KeepAlive, true);
                    socketSend.Ttl = 128;
                    // socketSend.Connect(serverEP);
                    IAsyncResult result = socketSend.BeginConnect(serverEP.Address, serverEP.Port, null, null);
                    // 2s连接超时
                    result.AsyncWaitHandle.WaitOne(2000, true);
                    if (!socketSend.Connected)
                    {
                        socketSend.Close();
                        MessageBox.Show("Tcp连接异常！");
                        return;
                    }
                }
                else
                {
                    socketSend = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
                    socketSend.Ttl = 128;
                    socketSend.Bind(localEP);
                }

                thWork = new Thread(thSocketReceive);
                thWork.Priority = ThreadPriority.Highest;
                thWork.IsBackground = true;
                thWork.Start();
            }
            catch (SocketException ex)
            {

                MessageBox.Show("网络异常，请检查是否已经连好网线口！");
            }
        }

        public FrmMain()
        {
            InitializeComponent();
            MotorPos = new MotorPosition(workPlate);
            Form.CheckForIllegalCrossThreadCalls = false;
            cmdWatch.Start();
            sysWatch.Start();

            for (int i = 0; i < deviceName.Length; i++)
            {
                deviceList.Add(
                    new Device()
                    {
                        Name = deviceName[i],
                        CanAddr = deviceAddr[i],
                        Pos = 0,
                    }
                    );
            }

            contextMenuStrip1.Items[0].Click += delegate
            {
                richTextBoxCmdTmp.Copy();
            };

            contextMenuStrip1.Items[1].Click += delegate
            {
                richTextBoxCmdTmp.Paste();
            };

            contextMenuStrip2.Items[0].Click += delegate
            {
                richTextBoxCmd.Copy();
            };

            contextMenuStrip2.Items[1].Click += delegate
            {
                richTextBoxCmd.Paste();
            };

            try
            {
                richTextBoxCmdTmp.LoadFile(Application.StartupPath + @"\temp.txt", RichTextBoxStreamType.PlainText);

            }
            catch (Exception e1)
            {


            }
        }

        /// <summary>
        /// 测试网络是否连通
        /// </summary>
        /// <param name="ip"></param>
        /// <returns></returns>
        public bool TestNet()
        {
            Ping ping = new Ping();
            for (int i = 0; i < 5; i++)
            {
                try
                {
                    if (ping.Send(this.remoteIP, 500).Status == IPStatus.Success) return true;
                }
                catch (PingException ex)
                {
                    return false;
                }
            }
            return false;
        }

        /// <summary>
        /// 加载xml scale参数
        /// </summary>
        private void ReadConfig()
        {
            XmlSerializer s = new XmlSerializer(typeof(WorkPlate));
            try
            {
                workPlate = (WorkPlate)s.Deserialize(new StreamReader(conStrXmlFileName));
            }
            catch (Exception ex)
            {
                workPlate = new WorkPlate();
                using (StreamWriter sr = new StreamWriter(conStrXmlFileName))
                {
                    s.Serialize(sr, workPlate);
                    sr.Flush();
                    sr.Close();
                }
            }
        }

        /// <summary>
        /// 断连连接
        /// </summary>
        public bool Disconnect()
        {
            try
            {
                if (socketSend != null && socketSend.Connected)
                {
                    socketSend.Disconnect(true);
                    socketSend.Close();
                }

                return true;
            }
            catch (Exception e)
            {
                return false;
            }
        }

        private void Form1_Load(object sender, EventArgs e)
        {
        }



        void btnExit_Click(object sender, EventArgs e)
        {
            IsManualStop = true;
        }


        private string getFileStr()
        {
            return "config.xml";
        }



        private void btnCan1_Click(object sender, EventArgs e)
        {
            textBoxCanAddr.Text = (sender as Button).Tag.ToString();
            textBoxDevice.Text = (sender as Button).Text;
        }

        private void button9_Click(object sender, EventArgs e)
        {
            DoCmd(textBoxDevice.Text + (sender as Button).Text);
        }

        private string checkMotionValue(string singleCmd)
        {
            string axi = singleCmd.Substring(0, 1).ToUpper();
            string actType = singleCmd.Substring(1, 1).ToUpper();
            decimal ints = 0;

            if (singleCmd.Contains('.'))  //如果值中含有小数点，则数字单位为毫米，需要转换成步数
            {
                if (singleCmd.ToUpper().StartsWith("WAIT"))
                {
                    decimal value = decimal.Parse(singleCmd.Substring(4));
                    ints = value * 10;
                    singleCmd = "WAIT " + (int)ints;

                }
                else if (actType == "A" || actType == "S")
                {
                    decimal value = decimal.Parse(singleCmd.Substring(2));
                    if (axi == "X") ints = (value) * workPlate.Xcale;
                    else if (axi == "Y") ints = (value) * workPlate.Ycale;
                    else if (axi == "Z") ints = (value) * workPlate.Zcale;
                    else if (axi == "W") ints = (value) * workPlate.Wcale;
                    // 线性关系修正参数（一般线性关系公式是y=kx+b）
                    else if (axi == "P")
                    {
                        if (workPlate.Pk != 0 && value != 0)
                            ints = workPlate.Pk * (value + workPlate.Pb);
                        else
                            ints = value * workPlate.Pcale;
                    }
                    else if (axi == "Q") ints = value * workPlate.Qcale;
                    else if (axi == "M") ints = value * workPlate.Mcale;

                    singleCmd = axi + actType + " " + (int)ints;
                }
            }

            switch (axi)
            {
                case "X":
                    if (actType == "I") MotorPos.XPos = 0;
                    if (actType == "A") MotorPos.XPos = (int)ints;
                    if (actType == "S") MotorPos.XPos += (int)ints;
                    break;
                case "Y":
                    if (actType == "I") MotorPos.YPos = 0;
                    if (actType == "A") MotorPos.YPos = (int)ints;
                    if (actType == "S") MotorPos.YPos += (int)ints;
                    break;
                case "Z":
                    if (actType == "I") MotorPos.ZPos = 0;
                    if (actType == "A") MotorPos.ZPos = (int)ints;
                    if (actType == "S") MotorPos.ZPos += (int)ints;
                    break;
                case "P":
                    if (actType == "I") MotorPos.PPos = 0;
                    if (actType == "A") MotorPos.PPos = (int)ints;
                    if (actType == "S") MotorPos.PPos += (int)ints;
                    break;
                case "Q":
                    if (actType == "I") MotorPos.QPos = 0;
                    if (actType == "A") MotorPos.QPos = (int)ints;
                    if (actType == "S") MotorPos.QPos += (int)ints;
                    break;
                case "W":
                    if (actType == "I") MotorPos.WPos = 0;
                    if (actType == "A") MotorPos.WPos = (int)ints;
                    if (actType == "S") MotorPos.WPos += (int)ints;
                    break;
                default:
                    break;
            }

            return singleCmd;
        }

        /// <summary>
        /// 设置指令被拦截回调
        /// </summary>
        /// <param name="myCallBack"></param>
        public void SetReceivedCallBack(ReceivedCallBack myCallBack)
        {
            mCmdReceivedCallBack = myCallBack;
        }

        /// <summary>
        /// 设置指令被拦截回调
        /// </summary>
        /// <param name="myCallBack"></param>
        public void SetInterceptedCallBack(MyCallBack myCallBack)
        {
            mCmdInterceptedCallBack = myCallBack;
        }

        public Boolean DoCmd(string cText, bool waitAnswer = true)
        {
            if (!IsQueryCmd(cText))
                CurrentOperationCmd = cText;

            if (cText == null)
            {
                LastErrorType = 3;
                LastErrorMsg = "Wrong Command";
                return false;
            }

            try
            {
                this.Enabled = false;
                timerHideTip.Enabled = false;
                cmdTextOrg = cText;
                cmdText = cmdPreProcess(cmdTextOrg);
                if (cmdText.Length == 0)
                {
                    LastErrorType = 3;
                    LastErrorMsg = "Wrong Command";
                    return false;
                }
                thSendCmd(waitAnswer);
            }
            catch (ExceptionCmdNoAnwser ex)
            {
                LastErrorType = 1;
                LastErrorMsg = ex.Message;
                return false;
            }
            catch (ExceptionCmdTimeOut ex)
            {
                LastErrorType = 2;
                LastErrorMsg = ex.Message;
                return false;
            }
            catch (ExceptionManulStop ex)
            {
                LastErrorType = 4;
                LastErrorMsg = ex.Message;
                return false;
            }
            catch (Exception ex)
            {
                LastErrorType = 5;
                LastErrorMsg = ex.Message;
                return false;
            }
            finally
            {
                timerHideTip.Enabled = true;

            }
            Thread.Sleep(10);

            return true;
        }

        /// <summary>
        /// 指令是否为查询指令
        /// </summary>
        /// <param name="cmd"></param>
        /// <returns></returns>
        private bool IsQueryCmd(string cmd)
        {
            return mQueryCmdList.Contains(cmd.ToUpper());
        }

        private int getCanAddrByName(string name)
        {
            try
            {
                return deviceList.First(p => p.Name == name).CanAddr;
            }
            catch (Exception)
            {
                try
                {
                    frmTip.TopMost = false;
                }
                catch (Exception)
                {
                }

                MessageBox.Show("没有" + name + "设备！");
                return 0;
            }
        }
        private string cmdPreProcess(string cmdTextOrg)
        {

            cmdTextOrg = cmdTextOrg.Trim();
            //  if (cmdTextOrg.Substring(0, 2) == "//" || cmdTextOrg.Substring(0, 2) == "==") return ""; //如果是正行注解，则返回空
            // 找// 或--
            int pos = 0;
            int pos1 = cmdTextOrg.IndexOf("//");
            int pos2 = cmdTextOrg.IndexOf("--");
            pos1 = (pos1 >= 0) ?
                 pos1 : int.MaxValue;
            pos2 = (pos2 >= 0) ?
                  pos2 : int.MaxValue;
            pos = Math.Min(pos1, pos2);
            if (pos == int.MaxValue) return cmdTextOrg;
            return cmdTextOrg.Substring(0, pos).Trim();
        }

        int rCount2 = 0;
        private void thSocketReceive()
        {

            EndPoint ep = (EndPoint)new IPEndPoint(IPAddress.Any, 0);
            Device device = null;

            socketSend.ReceiveTimeout = 1000;
            while (isReceiveFromNet)
            {
                try
                {
                    int c = this.isTcp ? socketSend.Receive(socketRecBuffer, 0, socketRecBuffer.Length, SocketFlags.None) : socketSend.ReceiveFrom(socketRecBuffer, 0, socketRecBuffer.Length, SocketFlags.None, ref ep);
                    Console.WriteLine("接收到的数据数目：" + c);
                    // if (c % 13 != 0) continue;
                    //      lock (this)
                    //     {
                    for (int findex = 0; findex < c / 13; findex++)
                    {
                        try
                        {
                            byte addr = socketRecBuffer[findex * 13 + 3];
                            device = deviceList.First(p => p.CanAddr == addr);   //找到相应设备
                            device.LastReceiveTime = sysWatch.ElapsedTicks;

                            // Console.WriteLine("接收到的can地址" + addr);

                            if (socketRecBuffer[findex * 13 + 5] == 0)
                            {
                                Array.Copy(socketRecBuffer, findex * 13, device.LastReceiveData1, 0, 13);
                            }
                            else
                            {
                                Array.Copy(socketRecBuffer, findex * 13, device.LastReceiveData2, 0, 13);

                                if (addr == 14)
                                {
                                    Console.WriteLine("接收到的can地址：" + addr + "  device.LastReceiveData2[6]：" + device.LastReceiveData2[6]);
                                }

                                // 接收数据回调
                                // mCmdReceivedCallBack?.Callback(device.LastReceiveData2);


                                // 指令是否被下位机拦截（证明仓门打开中）
                                if (device.LastReceiveData2[5] == 0x80)
                                {
                                    if (IsDoorClosed)
                                    {
                                        IsDoorClosed = false;
                                        // MessageBox.Show("指令被下位机拦截了！！！");
                                        mCmdInterceptedCallBack?.Callback();
                                    }
                                    break;
                                }
                   

                                // 仓门状态
                                if (device.LastReceiveData2[6] == 'D' && device.LastReceiveData2[7] == '1')
                                {
                                    if (device.LastReceiveData2[8] == '1') // 仓门关闭
                                        IsDoorClosed = true;
                                    else if (device.LastReceiveData2[8] == '0') // 仓门打开
                                        IsDoorClosed = false;
                                }

                                // 试管是否存在
                                if (device.LastReceiveData2[6] == 'A')
                                {
                                    if (device.LastReceiveData2[7] == '1') // 试管存在
                                        IsTubeExist = true;
                                    else if (device.LastReceiveData2[7] == '0') // 试管不存在
                                        IsTubeExist = false;

                                    // Console.WriteLine("试管是否存在：" + IsTubeExist);
                                }

                                // 温控模块
                                if (device.LastReceiveData2[6] == 'T')
                                {
                                    if (device.LastCmdText.Equals("AT29999"))
                                    {
                                        var tr = System.Text.Encoding.UTF8.GetString(device.LastReceiveData2).Substring(7);
                                        //decimal d = (device.LastReceiveData2[7] - byte.Parse("0")) * 100 + (device.LastReceiveData2[8] - byte.Parse("0")) * 10 + (device.LastReceiveData2[9] - byte.Parse("0"));
                                        decimal t = decimal.Parse(tr) / 10;
                                        // TemperatureResult = t;
                                        // Console.WriteLine("温控模块1：" + TemperatureResult);

                                        TemperatureResult = t >= 2000 ? t - 2000 : t + 2000;
                                        Console.WriteLine("温控模块1：" + TemperatureResult);
                                    }
                                }

                                // // 清洗槽加液完成
                                // if (device.CanAddr == 14 && device.LastReceiveData2[6] == 'S')
                                // // if ((device.CanAddr == 14 && isRunning) || (device.CanAddr == 14 && device.LastReceiveData2[6] == 'S'))
                                // {
                                //     IsCleaningFilled = true;
                                //     Console.WriteLine("------------------------------------清洗槽加液完成------------------------------------");
                                // }
                                //
                                // // 清洗槽排液完成
                                // if (device.CanAddr == 22 && device.LastReceiveData2[6] == 'S')
                                // // if ((device.CanAddr == 22 && isRunning) || (device.CanAddr == 22 && device.LastReceiveData2[6] == 'S'))
                                // {
                                //     IsCleaningFilled = false;
                                //     Console.WriteLine("------------------------------------清洗槽排液完成------------------------------------");
                                // }
                            }

                        }
                        catch (Exception err)
                        {
                        }
                    }

                    // Array.Clear(this.socketRecBuffer, 0, this.socketRecBuffer.Length);
                    //    }
                }
                catch (Exception ex)
                {
                    //  buffPos = 0;
                }
            }
        }

        //private int getCmdAnsPacketCount()
        //{
        //    int count = 0;
        //    for (int i = 0; i < buffPos / 13; i++)
        //    {
        //        if (socketRecBuffer[i * 13] != 0 && socketRecBuffer[i * 13 + 5] == 0)
        //        {
        //            count++;
        //        }
        //    }
        //    return count;
        //}


        //private int getFinishedAnsPacketCount()
        //{
        //    int count = 0;
        //    for (int i = 0; i < buffPos / 13; i++)
        //    {
        //        if (socketRecBuffer[i * 13] != 0 && socketRecBuffer[i * 13 + 5] != 0)
        //        {
        //            count++;
        //        }
        //    }
        //    return count;
        //}

        private int getMoveCmdCount(string[] cmdlist)
        {
            int count = 0;
            foreach (var onecmd in cmdlist)
            {
                if (onecmd.Trim().Substring(0, 1).ToUpper() != "F")
                {
                    string tmp = onecmd.Trim().Substring(1, 1).ToUpper();
                    if (tmp == "I" || tmp == "S" || tmp == "A" || tmp == "V" || tmp == "O") count++;
                }
                //   if (onecmd.Substring(0, 2) == "PO") count++;
            }
            return count;
        }

        public void SetRemoteIP(string strIP)
        {
            this.txtip.Text = strIP;
        }

        public void SetRemotePort(string strPort)
        {
            this.txtport.Text = strPort;
        }


        private void thSendCmd(bool waitAnswer = true)
        {
            IsManualStop = false;
            try
            {
                if (frmTip == null || frmTip.IsDisposed)
                {
                    frmTip = new FrmTip();
                    frmTip.btnExit.Click += btnExit_Click;
                    frmTip.Deactivate += delegate { this.Enabled = true; };
                }
                if (frmTip.Visible == false && IsShowFormTip) frmTip.Show(this);
            }
            catch (Exception ex)
            {

            }

            // 温控模块启动
            if (cmdText.ToUpper().Equals(TempControlModuleStartCmd))
            {
                IsTempControlModuleRunning = true;
            }

            if (cmdText.ToUpper().StartsWith("WAIT"))
            {
                cmdText = checkMotionValue(cmdText);
                cmdWatch.Reset();
                cmdWatch.Start();
                frmTip.labelTip.Text = (seqPos >= 0 ? ((seqPos + 1).ToString() + " ") : "") + cmdText;
                Application.DoEvents();
                while (cmdWatch.ElapsedMilliseconds < int.Parse(cmdText.Substring(4)) / 10)
                {
                    Thread.Sleep(10);
                }
                return;
            }

            frmTip.labelTip.Text = (seqPos >= 0 ? ((seqPos + 1).ToString() + " ") : "") + "正在发送指令: " + cmdTextOrg;
            Application.DoEvents();

            byte[] byteMessage = null;
            EndPoint ep = (EndPoint)new IPEndPoint(IPAddress.Any, 0);
            string[] cmdList = null;
            try
            {
                cmdList = cmdText.Split(new string[] { "," }, StringSplitOptions.RemoveEmptyEntries);

                int cmdIndex = 0;

                deviceToControl.Clear();
                Device device = null;

                for (int i = 0; i < cmdList.Length; i++)
                {
                    string oneCmd0 = cmdList[i].Trim().ToUpper();
                    string oneCmd = checkMotionValue(oneCmd0);

                    byteMessage = getSendData(oneCmd);

                    device = deviceList.First(p => p.CanAddr == byteMessage[4]);
                    deviceToControl.Add(device);

                    device.CleanReceiveData();
                    device.LastSendTime = sysWatch.ElapsedTicks;
                    device.LastCmdText = oneCmd;

                    if (this.isTcp)
                        socketSend.Send(byteMessage);
                    else
                        socketSend.SendTo(byteMessage, SocketFlags.None, serverEP);

                    cmdIndex++;

                    if (oneCmd.Trim().Length >= 2 &&
                        oneCmd.Trim().Substring(1, 1).ToUpper() == "T")  //如果是温度指令，则延时后跳过检测
                    {
                        Thread.Sleep(50);
                        continue;
                    }

                    cmdWatch.Reset();
                    cmdWatch.Start();
                    bool isAns = false;
                    while (cmdWatch.ElapsedMilliseconds < 2000)
                    {
                        Thread.Sleep(5);
                        // if (IsStop) return;
                        if (IsManualStop) throw new ExceptionManulStop();

                        if (isDeviceAnswer(device))
                        {
                            isAns = true;
                            break;
                        }
                    }

                    if (!isAns)
                    {
                        if (frmTip != null) this.frmTip.TopMost = false;
                        if (IsShowFormTip) MessageBox.Show("对于" + oneCmd + "设备没有回应！" + (seqPos >= 0 ? ("行号:" + (seqPos + 1).ToString() + " ") : ""));
                        IsManualStop = true;
                        throw new ExceptionCmdNoAnwser();
                    }
                    Thread.Sleep(5);
                    // if (isPositionCmd(oneCmd)) updatePos(oneCmd);
                }

                cmdWatch.Reset();
                cmdWatch.Start();

                frmTip.labelTip.Text = (seqPos >= 0 ? ((seqPos + 1).ToString() + " ") : "") + "正在执行：" + cmdText;
                frmTip.Update();

                bool isOk = false;

                if (waitAnswer)
                {
                    //  int mCount = getMoveCmdCount(cmdList);
                    if (cmdList[0].Trim().ToUpper().StartsWith("AF"))
                    {
                        while (cmdWatch.ElapsedMilliseconds < 120 * 1000) //120秒
                        {
                            Application.DoEvents();
                            Thread.Sleep(5);
                            if (isOtherCmdFinished(deviceToControl))
                            {
                                isOk = true;
                                break;
                            }
                        }
                    }
                    else if (cmdList[0].Trim().ToUpper().StartsWith("PP")
                             || cmdList[0].Trim().ToUpper().StartsWith("PO"))
                    {
                        while (cmdWatch.ElapsedMilliseconds < 3600 * 1000) //3600秒
                        {
                            Application.DoEvents();
                            Thread.Sleep(5);
                            if (isOtherCmdFinished(deviceToControl))
                            {
                                isOk = true;
                                break;
                            }
                        }

                    }
                    else
                    {
                        // while (cmdWatch.ElapsedMilliseconds < cmdExeTimeOut + cmdList.Length * 10000)
                        // {
                        //     Application.DoEvents();
                        //     // if (IsStop) return;
                        //     if (IsManualStop) throw new ExceptionManulStop();
                        //     Thread.Sleep(5);
                        //
                        //     if (isAllMotionCmdFinished(deviceToControl))
                        //     {
                        //         isOk = true;
                        //         break;
                        //     }
                        //
                        // }
                        // 2026-03-04：改成不频繁调用 Application.DoEvents()，避免造成卡顿，改成每500ms 才刷新一次 UI，而不是每 5ms
                        int uiRefreshCounter = 0;
                        while (cmdWatch.ElapsedMilliseconds < cmdExeTimeOut + cmdList.Length * 10000)
                        {
                            if (IsManualStop) throw new ExceptionManulStop();
                            Thread.Sleep(5);
                            uiRefreshCounter += 5;
                            if (uiRefreshCounter >= 500)
                            {
                                Application.DoEvents();
                                uiRefreshCounter = 0;
                            }
                            if (isAllMotionCmdFinished(deviceToControl))
                            {
                                isOk = true;
                                break;
                            }
                        }
                    }

                    if (!isOk)
                    {
                        if (frmTip != null) frmTip.TopMost = false;
                        if (IsShowFormTip)
                            MessageBox.Show("执行指令失败！ " + (seqPos >= 0 ? ("行号: " + (seqPos + 1).ToString() + " ") : ""));

                        throw new ExceptionCmdTimeOut();
                    }
                }
            }
            finally
            {

            }


        }

        private bool isAllMotionCmdFinished(List<Device> deviceL)
        {

            foreach (var device in deviceL)
            {
                if (isActionCmd(device.LastCmdText))
                {
                    if (!isDeviceFinishAnswer(device)) return false;
                }

            }
            return true;
        }

        private bool isOtherCmdFinished(List<Device> deviceL)
        {

            foreach (var device in deviceL)
            {
                if (!isDeviceFinishAnswer(device)) return false;

            }
            return true;
        }



        private bool isDeviceAnswer(Device device)
        {
            return device.LastReceiveTime > device.LastSendTime
                && device.LastReceiveData1[0] != 0
                && device.LastReceiveData1[5] == 0;

        }

        private bool isDeviceFinishAnswer(Device device)
        {
            return device.LastReceiveTime > device.LastSendTime
                && device.LastReceiveData2[0] != 0
                //                && device.LastReceiveData2[5] == device.LastSendData[5]
                && device.LastReceiveData2[6] == device.LastSendData[6];
        }

        /// <summary>
        ///  是否位置类型指令，这类指令影响运动位置
        /// </summary>
        /// <param name="oneCmd"></param>
        /// <returns></returns>
        private bool isPositionCmd(string oneCmd)
        {
            string tmp = oneCmd.Trim().Substring(1, 1).ToUpper();
            return tmp == "I" || tmp == "S" || tmp == "A";
        }

        /// <summary>
        /// 是否动作类型指令，执行这类指令要等待返回完成通讯包
        /// </summary>
        /// <param name="oneCmd"></param>
        /// <returns></returns>
        private bool isActionCmd(string oneCmd)
        {
            if (oneCmd.Trim().Substring(0, 1).ToUpper() == "F") return false;
            if (oneCmd.Trim().Substring(1, 1).ToUpper() == "F") return false;

            string tmp = oneCmd.Trim().Substring(1, 1).ToUpper();
            return tmp == "I" || tmp == "S" || tmp == "A" || tmp == "O" || tmp == "V" || tmp == "P";
        }

        private void updatePos(string oneCmd)
        {
            string d = oneCmd.Substring(0, 1).ToUpper();
            string b = oneCmd.Substring(1, 1).ToUpper();
            int c = 0;

            switch (b)
            {
                case "I":
                    deviceList.First(p => p.Name == d).Pos = 0;
                    break;
                case "A":
                    c = int.Parse(oneCmd.Substring(3));
                    deviceList.First(p => p.Name == d).Pos = c;
                    break;
                case "S":
                    c = int.Parse(oneCmd.Substring(3));
                    deviceList.First(p => p.Name == d).Pos += c;
                    break;
                default:
                    break;
            }
            switch (d)
            {
                case "X":
                    toolStripStatusLabelX.Text = "X: " + deviceList.First(p => p.Name == d).Pos;
                    break;
                case "Y":
                    toolStripStatusLabelY.Text = "Y: " + deviceList.First(p => p.Name == d).Pos;
                    break;
                case "Z":
                    toolStripStatusLabelZ.Text = "Z: " + deviceList.First(p => p.Name == d).Pos;
                    break;
                case "M":
                    toolStripStatusLabelM.Text = "M: " + deviceList.First(p => p.Name == d).Pos;
                    break;
                case "N":
                    toolStripStatusLabelN.Text = "N: " + deviceList.First(p => p.Name == d).Pos;
                    break;
                case "P":
                    toolStripStatusLabelP.Text = "P: " + deviceList.First(p => p.Name == d).Pos;
                    break;
                case "U":
                    toolStripStatusLabelU.Text = "U: " + deviceList.First(p => p.Name == d).Pos;
                    break;
                case "V":
                    toolStripStatusLabelV.Text = "V: " + deviceList.First(p => p.Name == d).Pos;
                    break;
                case "W":
                    toolStripStatusLabelW.Text = "W: " + deviceList.First(p => p.Name == d).Pos;
                    break;
                default:
                    break;




            }
        }


        private bool comPareMessage(byte[] byteMessage, byte[] byteMessageR2)
        {
            return true;

            int fcount = byteMessage.Length / 13;
            //    if (fcount*13!=byteMessage.Length) fcount++;
            fcount = 1;
            for (int i = 0; i < fcount; i++)
            {
                for (int j = 5; j < 7; j++)
                {
                    int offset = i * 13 + j;
                    if (offset > byteMessage.Length - 1) break;
                    if (byteMessage[offset] != byteMessageR2[offset]) return false;
                }
            }
            return true;
        }

        private byte[] getSendData(string onecmd)
        {
            onecmd = onecmd.Trim().ToUpper();
            //取命令的设备名称
            string dname = "";
            if (!onecmd.Contains("@"))
            {
                if (onecmd.Substring(0, 1) == "F")
                {
                    dname = onecmd.Substring(1, 1);
                    onecmd = "FX" + onecmd.Substring(2);
                }
                else
                {
                    dname = onecmd.Substring(0, 1);
                    onecmd = "X" + onecmd.Substring(1);
                    //onecmd = onecmd.Substring(0).Trim();
                }
            }
            else
            {
                int tmp = onecmd.IndexOf("@");
                dname = onecmd.Substring(tmp + 1).Trim();
                onecmd = onecmd.Substring(0, tmp).Trim();
            }

            int fcount = onecmd.Length / 8;
            if (onecmd.Length != fcount * 8) fcount++;

            byte[] buff = new byte[13 * fcount];
            byte[] byteP = Encoding.UTF8.GetBytes(onecmd);
            int cIndex = 0;

            for (int findex = 0; findex < fcount; findex++)
            {
                if (findex == fcount - 1)
                {
                    if (onecmd.Length == fcount * 8)
                        buff[findex * 13 + 0] = (byte)(0x80 + 8);
                    else
                        buff[findex * 13 + 0] = (byte)(0x80 + (onecmd.Length % 8));
                }
                else
                    buff[findex * 13 + 0] = (byte)(0x80 + 8);

                buff[findex * 13 + 1] = 0;
                Device device = null;

                buff[findex * 13 + 2] = (byte)((fcount << 4) + (findex + 1));
                buff[findex * 13 + 3] = 0;
                try
                {
                    // buff[findex * 13 + 4] = (byte)int.Parse(textBoxCanAddr.Text);
                    byte addr = (byte)getCanAddrByName(dname);
                    buff[findex * 13 + 4] = addr;
                    device = deviceList.First(p => p.CanAddr == addr);
                    //  buff[findex * 13 + 1] = (byte)device.PackageIndex;

                }
                catch (Exception)
                {
                    try
                    {
                        frmTip.TopMost = false;
                    }
                    catch (Exception)
                    {
                    }
                    MessageBox.Show("Can地址不正确，请检查是否为空（黄色文本框）！");
                }


                for (int i = 5; i < 13; i++)
                {
                    buff[findex * 13 + i] = (cIndex < onecmd.Length) ?
                                            byteP[cIndex]
                                            : (byte)0;
                    cIndex++;
                }

                if (findex == 0) Array.Copy(buff, device.LastSendData, 13); //每次发送记录第一个Can帧
            }
            return buff;
        }

        private void btnRun1_Click(object sender, EventArgs e)
        {
            DoCmd(textBoxCmd1.Text);
        }

        private void btnRun2_Click(object sender, EventArgs e)
        {
            DoCmd(textBoxCmd2.Text);
        }

        private void timerDoDelay_Tick(object sender, EventArgs e)
        {
            timerDoDelay.Enabled = false;
        }

        private void buttonNewTest_Click(object sender, EventArgs e)
        {

            //doCmd("XI,YI,ZI,PI");
            //disA = 1;
            //for (int i = 0; i < 1000; i++)
            //{
            //    buttonNewTest.Text = (i + 1).ToString();
            //    this.Update();

            //    doCmd("XS " + (disA > 0 ? 6000 : -6000).ToString()
            //        + ",YS  " + (disA > 0 ? 2000 : -2000).ToString()
            //          + ",ZS  " + (disA > 0 ? 2000 : -2000).ToString()
            //           + ",PS  " + (disA > 0 ? 9000 : -9000).ToString()
            //        );
            //    disA *= -1;
            //    //  doCmd("XA 100");
            //    if (isStop)
            //    {
            //        MessageBox.Show("停止测试！");
            //        return;
            //    }
            //    Thread.Sleep(500);
            //}

        }

        int disA = -500;

        private void timerNewTest_Tick(object sender, EventArgs e)
        {
            //disA *= -1;
            //sendCount2++;
            //buttonNewTest.Text = "send: " + sendCount2.ToString() + " / rec: " + rCount2.ToString();
            //doCmd("XA " + (disA > 0 ? 200 : 0).ToString());
        }

        private void btnRun_Click(object sender, EventArgs e)
        {
            DoSeq(richTextBoxCmdTmp.Lines);
        }

        private void toolStripStatusLabelX_Click(object sender, EventArgs e)
        {

        }

        private void toolStripStatusLabel1_Click(object sender, EventArgs e)
        {

        }

        private void toolStripStatusLabel2_Click(object sender, EventArgs e)
        {

        }

        private void contextMenuStrip1_Opening(object sender, CancelEventArgs e)
        {

        }

        private void toolStripMenuItem4_Click(object sender, EventArgs e)
        {

        }

        private void toolStripStatusLabelY_Click(object sender, EventArgs e)
        {

        }

        private void textBoxDevice_TextChanged(object sender, EventArgs e)
        {

        }

        private void textBoxCmd1_TextChanged(object sender, EventArgs e)
        {

        }

        private void textBoxCmd2_TextChanged(object sender, EventArgs e)
        {

        }

        private void textBoxCanAddr_TextChanged(object sender, EventArgs e)
        {

        }

        private void richTextBoxCmdTmp_TextChanged(object sender, EventArgs e)
        {

        }

        private void but_run1_Click(object sender, EventArgs e)
        {
            DoSeq(richTextBoxCmd.Lines);

        }

        public Boolean DoSeq(string[] seq)
        {
            seqPos = 0;
            IsManualPause = false;

            List<helper> helpList = getLoopList(seq);

            int loopLevel = 0;

            while (seqPos < seq.Length)
            {
                string cmdLine = seq[seqPos];
                if (cmdLine == null) continue;
                cmdLine = cmdLine.Trim();
                cmdLine = cmdPreProcess(cmdLine);
                Thread.Sleep(50);
                //Application.DoEvents();
                if (cmdLine == "" || cmdLine.Contains("="))
                {
                    seqPos++;
                    continue;
                }
                if (cmdLine.Contains(":"))  //如果是循环标签
                {
                    loopLevel++;
                    helpList.First(p => p.LineNo > seqPos && p.Label == cmdLine.Replace(":", "")).LoopLevel = loopLevel;
                    seqPos++;
                    continue;
                }

                if (cmdLine.ToUpper().StartsWith("LOOP"))
                {
                    helpList[seqPos].Loopcount--;
                    if (helpList[seqPos].Loopcount <= 0)
                    {
                        loopLevel--;
                        seqPos++;
                        continue;
                    }
                    else
                    {
                        var loopStart = helpList.Last(p => p.LineNo < seqPos && p.Label == helpList[seqPos].Label);
                        var innerLoops = helpList.Where(p => p.Label.Length > 0
                                      && p.OrgLoopcount > 0
                                      && p.LineNo > loopStart.LineNo
                                      && p.LineNo < seqPos
                                      && p.LoopLevel == loopLevel + 1);
                        foreach (var item in innerLoops)
                        {
                            item.Loopcount = item.OrgLoopcount;
                        }
                        seqPos = loopStart.LineNo + 1;
                        continue;
                    }
                }
                cmdLine = macroReplace(cmdLine, helpList, seqPos);
                bool result = DoCmd(cmdLine);

                if (!result)
                {
                    seqPos = -1;
                    return false;
                }
                while (IsManualPause)
                {
                    Application.DoEvents();
                    Thread.Sleep(5);
                }
                Application.DoEvents();

                if (IsManualStop)
                {
                    if (IsShowFormTip) MessageBox.Show("放弃继续执行！");
                    seqPos = -1;
                    LastErrorType = 4;
                    LastErrorMsg = "Manual Stop";
                    return false;
                }

                seqPos++;
            }
            seqPos = -1;
            if (IsShowFormTip) MessageBox.Show("全部指令执行完毕！");
            return true;
        }

        private string macroReplace(string cL, List<helper> helpList, int beforePos)
        {
            string[] wrds = cL.Split(new string[] { " ", "," }, StringSplitOptions.RemoveEmptyEntries);
            foreach (var wrd in wrds)
            {
                var h = helpList.LastOrDefault(p => p.orgWord == wrd && p.LineNo < beforePos);
                if (h != null)
                {
                    cL = cL.Replace(wrd, h.tarWord);
                }
            }
            return cL;
        }

        private List<helper> getLoopList(string[] seq)
        {
            List<helper> tmpL = new List<helper>();
            for (int i = 0; i < seq.Length; i++)
            {
                helper ll = new helper();
                ll.LineNo = i;
                ll.Label = "";
                ll.Loopcount = 0;
                ll.OrgLoopcount = 0;
                ll.LoopLevel = 0;
                ll.orgWord = "";
                ll.tarWord = "";
                string txt = seq[i];
                txt = cmdPreProcess(txt.Trim());
                if (txt.StartsWith("\\") || txt.StartsWith("--")) continue;
                if (txt.Contains(":"))
                {
                    ll.Label = txt.Trim().Replace(":", "");  // label 不带冒号
                }
                else if (txt.ToUpper().StartsWith("LOOP"))
                {
                    int posDot = txt.IndexOf(',');
                    ll.OrgLoopcount = ll.Loopcount = int.Parse(txt.Substring(posDot + 1));
                    int posSpace = txt.IndexOf(" ");
                    string la = txt.Substring(posSpace, posDot - posSpace).Trim();
                    ll.Label = la;
                    if (ll.OrgLoopcount <= 0) ll.OrgLoopcount = 1;
                    // tmpL.First(p => p.Label == la).Loopcount = ll.Loopcount;
                }
                else if (txt.Contains("="))
                {
                    string[] ts = txt.Split(new string[] { "=", " " }, StringSplitOptions.RemoveEmptyEntries);
                    ll.orgWord = ts[0];
                    ll.tarWord = ts[1];
                }
                tmpL.Add(ll);
            }
            return tmpL;
        }

        private void richTextBox1_TextChanged(object sender, EventArgs e)
        {

        }

        private void but_open_Click(object sender, EventArgs e)
        {
            OpenFileDialog file1 = new OpenFileDialog();
            file1.InitialDirectory = Application.StartupPath;
            //file1.InitialDirectory = "D:\\path";
            //  file1.Filter = "文本文件|*.txt|RTF文件|*.RTF|所有文件|*.*";
            if (file1.ShowDialog() == DialogResult.OK)
            {
                //System.Diagnostics.Process.Start(file1.FileName);d
                // richTextBoxCmd.r
                labelFileName.Text = file1.FileName;

                richTextBoxCmd.LoadFile(file1.FileName, RichTextBoxStreamType.PlainText);

            }


        }

        private void but_copy_Click(object sender, EventArgs e)
        {

            richTextBoxCmd.Copy();


        }

        private void but_paste_Click(object sender, EventArgs e)
        {
            richTextBoxCmd.Paste();

        }

        private void but_save_Click(object sender, EventArgs e)
        {
            SaveFileDialog s = new SaveFileDialog();
            s.InitialDirectory = Application.StartupPath;
            if (s.ShowDialog() == DialogResult.OK)
            {
                richTextBoxCmd.SaveFile(s.FileName, RichTextBoxStreamType.PlainText);
                //     richTextBoxCmd.LoadFile()
            }
        }

        private void buttonCopyTmp_Click(object sender, EventArgs e)
        {
            richTextBoxCmdTmp.Copy();
        }

        private void buttonPasteTmp_Click(object sender, EventArgs e)
        {
            richTextBoxCmdTmp.Paste();
        }

        private void FrmDAECanTest_FormClosing(object sender, FormClosingEventArgs e)
        {
            SaveTempBox();
            if (this.Tag != null)
            {
                e.Cancel = true;
                this.Hide();
                return;
            }

        }

        private void SaveTempBox()
        {
            isReceiveFromNet = false;

            try
            {
                richTextBoxCmdTmp.SaveFile(Application.StartupPath + @"\temp.txt", RichTextBoxStreamType.PlainText);

            }
            catch (Exception e1)
            {

            }
        }

        private void timerSave_Tick(object sender, EventArgs e)
        {
            SaveTempBox();
        }

        private void timerHideTip_Tick(object sender, EventArgs e)
        {
            this.timerHideTip.Enabled = false;
            this.Enabled = true;
            try
            {
                frmTip.Close();
            }
            catch (Exception)
            {
            }
        }

        private void btnOfflineDo_Click(object sender, EventArgs e)
        {
            // OldUploadMethod();
            byte[] data = Encoding.UTF8.GetBytes(richTextBoxCmd.Text);
            System.IO.BinaryReader bin = new System.IO.BinaryReader(new System.IO.MemoryStream(data));
            TFTP.TFTPClient tftpCli = new TFTP.TFTPClient(this.txtip.Text);
            tftpCli.Put("seq.txt", bin, TFTP.TFTPClient.Modes.Octet);

        }

        private void OldUploadMethod()
        {
            bool isOk = false;
            string errMsg = "";
            IPEndPoint localEPDl = new IPEndPoint(0, 4001);
            Socket socketDl = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
            try
            {
                socketDl.Bind(localEPDl);

                string ip = this.txtip.Text;
                string port = this.txtport.Text;
                IPAddress serverIp = IPAddress.Parse(ip);
                int serverPort = Convert.ToInt32(port) + 1;
                IPEndPoint iep = new IPEndPoint(serverIp, serverPort);
                socketDl.SendTo(new byte[] { 0xFC }, iep);
                try
                {
                    byte[] data = Encoding.UTF8.GetBytes(richTextBoxCmd.Text);
                    int packetSize = 512;
                    int offset = 0;
                    for (int i = 0; i < data.Length / packetSize; i++)
                    {
                        socketDl.SendTo(data, offset, packetSize, SocketFlags.None, iep);
                        offset += packetSize;
                    }

                    if (data.Length % packetSize != 0)
                    {
                        socketDl.SendTo(data, offset, data.Length % packetSize, SocketFlags.None, iep);
                    }
                    isOk = true;
                }
                catch (Exception ex1)
                {
                    errMsg = ex1.Message;
                }
                finally
                {
                    socketDl.SendTo(new byte[] { 0xFF }, iep);
                }
            }
            finally
            {
                socketDl.Close();
            }
            if (isOk)
            {
                MessageBox.Show("上传成功！");
            }
            else
            {
                MessageBox.Show("失败！ 原因： " + errMsg);
            }
        }
    }
}
