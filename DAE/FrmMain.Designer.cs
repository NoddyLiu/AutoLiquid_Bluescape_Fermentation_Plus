namespace DAERun
{
    partial class FrmMain
    {
        /// <summary>
        /// 必需的设计器变量。
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// 清理所有正在使用的资源。hhh
        /// </summary>
        /// <param name="disposing">如果应释放托管资源，为 true；否则为 false。</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows 窗体设计器生成的代码

        /// <summary>
        /// 设计器支持所需的方法 - 不要
        /// 使用代码编辑器修改此方法的内容。
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            this.txtip = new System.Windows.Forms.TextBox();
            this.txtport = new System.Windows.Forms.TextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.timer1 = new System.Windows.Forms.Timer(this.components);
            this.btnCan1 = new System.Windows.Forms.Button();
            this.button2 = new System.Windows.Forms.Button();
            this.button3 = new System.Windows.Forms.Button();
            this.button4 = new System.Windows.Forms.Button();
            this.button5 = new System.Windows.Forms.Button();
            this.button6 = new System.Windows.Forms.Button();
            this.button7 = new System.Windows.Forms.Button();
            this.button8 = new System.Windows.Forms.Button();
            this.textBoxCanAddr = new System.Windows.Forms.TextBox();
            this.textBoxCmd1 = new System.Windows.Forms.TextBox();
            this.btnRun1 = new System.Windows.Forms.Button();
            this.button9 = new System.Windows.Forms.Button();
            this.btnRun2 = new System.Windows.Forms.Button();
            this.btnXS1 = new System.Windows.Forms.Button();
            this.textBoxCmd2 = new System.Windows.Forms.TextBox();
            this.btnXS2 = new System.Windows.Forms.Button();
            this.btnXA0 = new System.Windows.Forms.Button();
            this.timerDoDelay = new System.Windows.Forms.Timer(this.components);
            this.buttonNewTest = new System.Windows.Forms.Button();
            this.timerNewTest = new System.Windows.Forms.Timer(this.components);
            this.textBoxDevice = new System.Windows.Forms.TextBox();
            this.richTextBoxCmdTmp = new System.Windows.Forms.RichTextBox();
            this.contextMenuStrip1 = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.toolStripMenuItem1 = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItem2 = new System.Windows.Forms.ToolStripMenuItem();
            this.btnRun = new System.Windows.Forms.Button();
            this.statusStrip1 = new System.Windows.Forms.StatusStrip();
            this.toolStripStatusLabelX = new System.Windows.Forms.ToolStripStatusLabel();
            this.toolStripStatusLabelY = new System.Windows.Forms.ToolStripStatusLabel();
            this.toolStripStatusLabelZ = new System.Windows.Forms.ToolStripStatusLabel();
            this.toolStripStatusLabelM = new System.Windows.Forms.ToolStripStatusLabel();
            this.toolStripStatusLabelN = new System.Windows.Forms.ToolStripStatusLabel();
            this.toolStripStatusLabelP = new System.Windows.Forms.ToolStripStatusLabel();
            this.toolStripStatusLabelU = new System.Windows.Forms.ToolStripStatusLabel();
            this.toolStripStatusLabelV = new System.Windows.Forms.ToolStripStatusLabel();
            this.toolStripStatusLabelW = new System.Windows.Forms.ToolStripStatusLabel();
            this.richTextBoxCmd = new System.Windows.Forms.RichTextBox();
            this.contextMenuStrip2 = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.toolStripMenuItem3 = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItem4 = new System.Windows.Forms.ToolStripMenuItem();
            this.but_run1 = new System.Windows.Forms.Button();
            this.but_open = new System.Windows.Forms.Button();
            this.but_copy = new System.Windows.Forms.Button();
            this.but_paste = new System.Windows.Forms.Button();
            this.but_save = new System.Windows.Forms.Button();
            this.labelFileName = new System.Windows.Forms.Label();
            this.label4 = new System.Windows.Forms.Label();
            this.buttonCopyTmp = new System.Windows.Forms.Button();
            this.buttonPasteTmp = new System.Windows.Forms.Button();
            this.timerSave = new System.Windows.Forms.Timer(this.components);
            this.timerHideTip = new System.Windows.Forms.Timer(this.components);
            this.btnOfflineDo = new System.Windows.Forms.Button();
            this.contextMenuStrip1.SuspendLayout();
            this.statusStrip1.SuspendLayout();
            this.contextMenuStrip2.SuspendLayout();
            this.SuspendLayout();
            // 
            // txtip
            // 
            this.txtip.Location = new System.Drawing.Point(228, 24);
            this.txtip.Margin = new System.Windows.Forms.Padding(6);
            this.txtip.Name = "txtip";
            this.txtip.Size = new System.Drawing.Size(262, 42);
            this.txtip.TabIndex = 3;
            this.txtip.Text = "192.168.1.3";
            // 
            // txtport
            // 
            this.txtport.Location = new System.Drawing.Point(644, 24);
            this.txtport.Margin = new System.Windows.Forms.Padding(6);
            this.txtport.Name = "txtport";
            this.txtport.Size = new System.Drawing.Size(156, 42);
            this.txtport.TabIndex = 3;
            this.txtport.Text = "4000";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(86, 24);
            this.label2.Margin = new System.Windows.Forms.Padding(6, 0, 6, 0);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(163, 30);
            this.label2.TabIndex = 4;
            this.label2.Text = "net can ip";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(574, 30);
            this.label3.Margin = new System.Windows.Forms.Padding(6, 0, 6, 0);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(73, 30);
            this.label3.TabIndex = 5;
            this.label3.Text = "port";
            // 
            // btnCan1
            // 
            this.btnCan1.Location = new System.Drawing.Point(12, 80);
            this.btnCan1.Margin = new System.Windows.Forms.Padding(6);
            this.btnCan1.Name = "btnCan1";
            this.btnCan1.Size = new System.Drawing.Size(70, 52);
            this.btnCan1.TabIndex = 27;
            this.btnCan1.Tag = "11";
            this.btnCan1.Text = "X";
            this.btnCan1.UseVisualStyleBackColor = true;
            this.btnCan1.Click += new System.EventHandler(this.btnCan1_Click);
            // 
            // button2
            // 
            this.button2.Location = new System.Drawing.Point(82, 80);
            this.button2.Margin = new System.Windows.Forms.Padding(6);
            this.button2.Name = "button2";
            this.button2.Size = new System.Drawing.Size(70, 52);
            this.button2.TabIndex = 27;
            this.button2.Tag = "12";
            this.button2.Text = "Y";
            this.button2.UseVisualStyleBackColor = true;
            this.button2.Click += new System.EventHandler(this.btnCan1_Click);
            // 
            // button3
            // 
            this.button3.Location = new System.Drawing.Point(152, 80);
            this.button3.Margin = new System.Windows.Forms.Padding(6);
            this.button3.Name = "button3";
            this.button3.Size = new System.Drawing.Size(70, 52);
            this.button3.TabIndex = 27;
            this.button3.Tag = "13";
            this.button3.Text = "Z";
            this.button3.UseVisualStyleBackColor = true;
            this.button3.Click += new System.EventHandler(this.btnCan1_Click);
            // 
            // button4
            // 
            this.button4.Location = new System.Drawing.Point(222, 80);
            this.button4.Margin = new System.Windows.Forms.Padding(6);
            this.button4.Name = "button4";
            this.button4.Size = new System.Drawing.Size(70, 52);
            this.button4.TabIndex = 27;
            this.button4.Tag = "16";
            this.button4.Text = "M";
            this.button4.UseVisualStyleBackColor = true;
            this.button4.Click += new System.EventHandler(this.btnCan1_Click);
            // 
            // button5
            // 
            this.button5.Location = new System.Drawing.Point(292, 80);
            this.button5.Margin = new System.Windows.Forms.Padding(6);
            this.button5.Name = "button5";
            this.button5.Size = new System.Drawing.Size(70, 52);
            this.button5.TabIndex = 27;
            this.button5.Tag = "17";
            this.button5.Text = "N";
            this.button5.UseVisualStyleBackColor = true;
            this.button5.Click += new System.EventHandler(this.btnCan1_Click);
            // 
            // button6
            // 
            this.button6.Location = new System.Drawing.Point(362, 80);
            this.button6.Margin = new System.Windows.Forms.Padding(6);
            this.button6.Name = "button6";
            this.button6.Size = new System.Drawing.Size(70, 52);
            this.button6.TabIndex = 27;
            this.button6.Tag = "21";
            this.button6.Text = "P";
            this.button6.UseVisualStyleBackColor = true;
            this.button6.Click += new System.EventHandler(this.btnCan1_Click);
            // 
            // button7
            // 
            this.button7.Location = new System.Drawing.Point(432, 80);
            this.button7.Margin = new System.Windows.Forms.Padding(6);
            this.button7.Name = "button7";
            this.button7.Size = new System.Drawing.Size(70, 52);
            this.button7.TabIndex = 27;
            this.button7.Text = "8";
            this.button7.UseVisualStyleBackColor = true;
            this.button7.Visible = false;
            this.button7.Click += new System.EventHandler(this.btnCan1_Click);
            // 
            // button8
            // 
            this.button8.Location = new System.Drawing.Point(502, 80);
            this.button8.Margin = new System.Windows.Forms.Padding(6);
            this.button8.Name = "button8";
            this.button8.Size = new System.Drawing.Size(70, 52);
            this.button8.TabIndex = 27;
            this.button8.Text = "9";
            this.button8.UseVisualStyleBackColor = true;
            this.button8.Visible = false;
            this.button8.Click += new System.EventHandler(this.btnCan1_Click);
            // 
            // textBoxCanAddr
            // 
            this.textBoxCanAddr.BackColor = System.Drawing.Color.DarkKhaki;
            this.textBoxCanAddr.Location = new System.Drawing.Point(1074, 84);
            this.textBoxCanAddr.Margin = new System.Windows.Forms.Padding(6);
            this.textBoxCanAddr.Name = "textBoxCanAddr";
            this.textBoxCanAddr.Size = new System.Drawing.Size(64, 42);
            this.textBoxCanAddr.TabIndex = 28;
            this.textBoxCanAddr.Text = "11";
            this.textBoxCanAddr.TextChanged += new System.EventHandler(this.textBoxCanAddr_TextChanged);
            // 
            // textBoxCmd1
            // 
            this.textBoxCmd1.Location = new System.Drawing.Point(15, 148);
            this.textBoxCmd1.Margin = new System.Windows.Forms.Padding(6);
            this.textBoxCmd1.Name = "textBoxCmd1";
            this.textBoxCmd1.Size = new System.Drawing.Size(496, 42);
            this.textBoxCmd1.TabIndex = 29;
            this.textBoxCmd1.Text = "XA 2000";
            this.textBoxCmd1.TextChanged += new System.EventHandler(this.textBoxCmd1_TextChanged);
            // 
            // btnRun1
            // 
            this.btnRun1.Location = new System.Drawing.Point(525, 146);
            this.btnRun1.Margin = new System.Windows.Forms.Padding(6);
            this.btnRun1.Name = "btnRun1";
            this.btnRun1.Size = new System.Drawing.Size(94, 46);
            this.btnRun1.TabIndex = 30;
            this.btnRun1.Text = "Do";
            this.btnRun1.UseVisualStyleBackColor = true;
            this.btnRun1.Click += new System.EventHandler(this.btnRun1_Click);
            // 
            // button9
            // 
            this.button9.Location = new System.Drawing.Point(654, 82);
            this.button9.Margin = new System.Windows.Forms.Padding(6);
            this.button9.Name = "button9";
            this.button9.Size = new System.Drawing.Size(72, 48);
            this.button9.TabIndex = 31;
            this.button9.Text = "I";
            this.button9.UseVisualStyleBackColor = true;
            this.button9.Click += new System.EventHandler(this.button9_Click);
            // 
            // btnRun2
            // 
            this.btnRun2.Location = new System.Drawing.Point(1118, 146);
            this.btnRun2.Margin = new System.Windows.Forms.Padding(6);
            this.btnRun2.Name = "btnRun2";
            this.btnRun2.Size = new System.Drawing.Size(92, 46);
            this.btnRun2.TabIndex = 30;
            this.btnRun2.Text = "Do";
            this.btnRun2.UseVisualStyleBackColor = true;
            this.btnRun2.Click += new System.EventHandler(this.btnRun2_Click);
            // 
            // btnXS1
            // 
            this.btnXS1.Location = new System.Drawing.Point(730, 82);
            this.btnXS1.Margin = new System.Windows.Forms.Padding(6);
            this.btnXS1.Name = "btnXS1";
            this.btnXS1.Size = new System.Drawing.Size(104, 48);
            this.btnXS1.TabIndex = 32;
            this.btnXS1.Text = "S 100";
            this.btnXS1.UseVisualStyleBackColor = true;
            this.btnXS1.Click += new System.EventHandler(this.button9_Click);
            // 
            // textBoxCmd2
            // 
            this.textBoxCmd2.Location = new System.Drawing.Point(658, 148);
            this.textBoxCmd2.Margin = new System.Windows.Forms.Padding(6);
            this.textBoxCmd2.Name = "textBoxCmd2";
            this.textBoxCmd2.Size = new System.Drawing.Size(450, 42);
            this.textBoxCmd2.TabIndex = 29;
            this.textBoxCmd2.Text = "FX 100 1000 3";
            this.textBoxCmd2.TextChanged += new System.EventHandler(this.textBoxCmd2_TextChanged);
            // 
            // btnXS2
            // 
            this.btnXS2.Location = new System.Drawing.Point(836, 82);
            this.btnXS2.Margin = new System.Windows.Forms.Padding(6);
            this.btnXS2.Name = "btnXS2";
            this.btnXS2.Size = new System.Drawing.Size(114, 48);
            this.btnXS2.TabIndex = 32;
            this.btnXS2.Text = "S -100";
            this.btnXS2.UseVisualStyleBackColor = true;
            this.btnXS2.Click += new System.EventHandler(this.button9_Click);
            // 
            // btnXA0
            // 
            this.btnXA0.Location = new System.Drawing.Point(954, 82);
            this.btnXA0.Margin = new System.Windows.Forms.Padding(6);
            this.btnXA0.Name = "btnXA0";
            this.btnXA0.Size = new System.Drawing.Size(114, 48);
            this.btnXA0.TabIndex = 32;
            this.btnXA0.Text = "A 0";
            this.btnXA0.UseVisualStyleBackColor = true;
            this.btnXA0.Click += new System.EventHandler(this.button9_Click);
            // 
            // timerDoDelay
            // 
            this.timerDoDelay.Interval = 4000;
            this.timerDoDelay.Tick += new System.EventHandler(this.timerDoDelay_Tick);
            // 
            // buttonNewTest
            // 
            this.buttonNewTest.Location = new System.Drawing.Point(1115, 778);
            this.buttonNewTest.Margin = new System.Windows.Forms.Padding(6);
            this.buttonNewTest.Name = "buttonNewTest";
            this.buttonNewTest.Size = new System.Drawing.Size(158, 58);
            this.buttonNewTest.TabIndex = 33;
            this.buttonNewTest.Text = "NewTest";
            this.buttonNewTest.UseVisualStyleBackColor = true;
            this.buttonNewTest.Visible = false;
            this.buttonNewTest.Click += new System.EventHandler(this.buttonNewTest_Click);
            // 
            // timerNewTest
            // 
            this.timerNewTest.Interval = 1000;
            this.timerNewTest.Tick += new System.EventHandler(this.timerNewTest_Tick);
            // 
            // textBoxDevice
            // 
            this.textBoxDevice.BackColor = System.Drawing.Color.Yellow;
            this.textBoxDevice.Location = new System.Drawing.Point(578, 82);
            this.textBoxDevice.Margin = new System.Windows.Forms.Padding(6);
            this.textBoxDevice.Name = "textBoxDevice";
            this.textBoxDevice.Size = new System.Drawing.Size(64, 42);
            this.textBoxDevice.TabIndex = 34;
            this.textBoxDevice.Text = "X";
            this.textBoxDevice.TextChanged += new System.EventHandler(this.textBoxDevice_TextChanged);
            // 
            // richTextBoxCmdTmp
            // 
            this.richTextBoxCmdTmp.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left)));
            this.richTextBoxCmdTmp.ContextMenuStrip = this.contextMenuStrip1;
            this.richTextBoxCmdTmp.Location = new System.Drawing.Point(15, 254);
            this.richTextBoxCmdTmp.Margin = new System.Windows.Forms.Padding(6);
            this.richTextBoxCmdTmp.Name = "richTextBoxCmdTmp";
            this.richTextBoxCmdTmp.Size = new System.Drawing.Size(475, 578);
            this.richTextBoxCmdTmp.TabIndex = 35;
            this.richTextBoxCmdTmp.Text = "";
            this.richTextBoxCmdTmp.TextChanged += new System.EventHandler(this.richTextBoxCmdTmp_TextChanged);
            // 
            // contextMenuStrip1
            // 
            this.contextMenuStrip1.ImageScalingSize = new System.Drawing.Size(20, 20);
            this.contextMenuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripMenuItem1,
            this.toolStripMenuItem2});
            this.contextMenuStrip1.Name = "contextMenuStrip1";
            this.contextMenuStrip1.Size = new System.Drawing.Size(155, 92);
            this.contextMenuStrip1.Opening += new System.ComponentModel.CancelEventHandler(this.contextMenuStrip1_Opening);
            // 
            // toolStripMenuItem1
            // 
            this.toolStripMenuItem1.Name = "toolStripMenuItem1";
            this.toolStripMenuItem1.Size = new System.Drawing.Size(154, 44);
            this.toolStripMenuItem1.Text = "复制";
            // 
            // toolStripMenuItem2
            // 
            this.toolStripMenuItem2.Name = "toolStripMenuItem2";
            this.toolStripMenuItem2.Size = new System.Drawing.Size(154, 44);
            this.toolStripMenuItem2.Text = "粘贴";
            // 
            // btnRun
            // 
            this.btnRun.Location = new System.Drawing.Point(502, 268);
            this.btnRun.Margin = new System.Windows.Forms.Padding(6);
            this.btnRun.Name = "btnRun";
            this.btnRun.Size = new System.Drawing.Size(121, 46);
            this.btnRun.TabIndex = 36;
            this.btnRun.Text = "运行";
            this.btnRun.UseVisualStyleBackColor = true;
            this.btnRun.Click += new System.EventHandler(this.btnRun_Click);
            // 
            // statusStrip1
            // 
            this.statusStrip1.ImageScalingSize = new System.Drawing.Size(20, 20);
            this.statusStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripStatusLabelX,
            this.toolStripStatusLabelY,
            this.toolStripStatusLabelZ,
            this.toolStripStatusLabelM,
            this.toolStripStatusLabelN,
            this.toolStripStatusLabelP,
            this.toolStripStatusLabelU,
            this.toolStripStatusLabelV,
            this.toolStripStatusLabelW});
            this.statusStrip1.Location = new System.Drawing.Point(0, 848);
            this.statusStrip1.Name = "statusStrip1";
            this.statusStrip1.Padding = new System.Windows.Forms.Padding(2, 0, 28, 0);
            this.statusStrip1.Size = new System.Drawing.Size(1282, 38);
            this.statusStrip1.TabIndex = 37;
            this.statusStrip1.Text = "statusStrip1";
            // 
            // toolStripStatusLabelX
            // 
            this.toolStripStatusLabelX.AutoSize = false;
            this.toolStripStatusLabelX.Name = "toolStripStatusLabelX";
            this.toolStripStatusLabelX.Size = new System.Drawing.Size(60, 33);
            this.toolStripStatusLabelX.Text = "X: ";
            this.toolStripStatusLabelX.Click += new System.EventHandler(this.toolStripStatusLabelX_Click);
            // 
            // toolStripStatusLabelY
            // 
            this.toolStripStatusLabelY.AutoSize = false;
            this.toolStripStatusLabelY.Name = "toolStripStatusLabelY";
            this.toolStripStatusLabelY.Size = new System.Drawing.Size(60, 33);
            this.toolStripStatusLabelY.Text = "Y: ";
            this.toolStripStatusLabelY.Click += new System.EventHandler(this.toolStripStatusLabelY_Click);
            // 
            // toolStripStatusLabelZ
            // 
            this.toolStripStatusLabelZ.AutoSize = false;
            this.toolStripStatusLabelZ.Name = "toolStripStatusLabelZ";
            this.toolStripStatusLabelZ.Size = new System.Drawing.Size(60, 33);
            this.toolStripStatusLabelZ.Text = "Z:";
            this.toolStripStatusLabelZ.Click += new System.EventHandler(this.toolStripStatusLabel1_Click);
            // 
            // toolStripStatusLabelM
            // 
            this.toolStripStatusLabelM.AutoSize = false;
            this.toolStripStatusLabelM.Name = "toolStripStatusLabelM";
            this.toolStripStatusLabelM.Size = new System.Drawing.Size(60, 33);
            this.toolStripStatusLabelM.Text = "M:";
            // 
            // toolStripStatusLabelN
            // 
            this.toolStripStatusLabelN.AutoSize = false;
            this.toolStripStatusLabelN.Name = "toolStripStatusLabelN";
            this.toolStripStatusLabelN.Size = new System.Drawing.Size(60, 33);
            this.toolStripStatusLabelN.Text = "N:";
            // 
            // toolStripStatusLabelP
            // 
            this.toolStripStatusLabelP.AutoSize = false;
            this.toolStripStatusLabelP.Name = "toolStripStatusLabelP";
            this.toolStripStatusLabelP.Size = new System.Drawing.Size(60, 33);
            this.toolStripStatusLabelP.Text = "P:";
            // 
            // toolStripStatusLabelU
            // 
            this.toolStripStatusLabelU.AutoSize = false;
            this.toolStripStatusLabelU.Name = "toolStripStatusLabelU";
            this.toolStripStatusLabelU.RightToLeft = System.Windows.Forms.RightToLeft.Yes;
            this.toolStripStatusLabelU.Size = new System.Drawing.Size(60, 33);
            this.toolStripStatusLabelU.Text = "U:";
            this.toolStripStatusLabelU.Click += new System.EventHandler(this.toolStripStatusLabel2_Click);
            // 
            // toolStripStatusLabelV
            // 
            this.toolStripStatusLabelV.AutoSize = false;
            this.toolStripStatusLabelV.Name = "toolStripStatusLabelV";
            this.toolStripStatusLabelV.Size = new System.Drawing.Size(60, 33);
            this.toolStripStatusLabelV.Text = "V:";
            // 
            // toolStripStatusLabelW
            // 
            this.toolStripStatusLabelW.AutoSize = false;
            this.toolStripStatusLabelW.Name = "toolStripStatusLabelW";
            this.toolStripStatusLabelW.Size = new System.Drawing.Size(60, 33);
            this.toolStripStatusLabelW.Text = "W:";
            // 
            // richTextBoxCmd
            // 
            this.richTextBoxCmd.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left)));
            this.richTextBoxCmd.ContextMenuStrip = this.contextMenuStrip2;
            this.richTextBoxCmd.Location = new System.Drawing.Point(644, 254);
            this.richTextBoxCmd.Margin = new System.Windows.Forms.Padding(6);
            this.richTextBoxCmd.Name = "richTextBoxCmd";
            this.richTextBoxCmd.Size = new System.Drawing.Size(464, 580);
            this.richTextBoxCmd.TabIndex = 35;
            this.richTextBoxCmd.Text = "";
            this.richTextBoxCmd.TextChanged += new System.EventHandler(this.richTextBox1_TextChanged);
            // 
            // contextMenuStrip2
            // 
            this.contextMenuStrip2.ImageScalingSize = new System.Drawing.Size(20, 20);
            this.contextMenuStrip2.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripMenuItem3,
            this.toolStripMenuItem4});
            this.contextMenuStrip2.Name = "contextMenuStrip1";
            this.contextMenuStrip2.Size = new System.Drawing.Size(155, 92);
            // 
            // toolStripMenuItem3
            // 
            this.toolStripMenuItem3.Name = "toolStripMenuItem3";
            this.toolStripMenuItem3.Size = new System.Drawing.Size(154, 44);
            this.toolStripMenuItem3.Text = "复制";
            // 
            // toolStripMenuItem4
            // 
            this.toolStripMenuItem4.Name = "toolStripMenuItem4";
            this.toolStripMenuItem4.Size = new System.Drawing.Size(154, 44);
            this.toolStripMenuItem4.Text = "粘贴";
            this.toolStripMenuItem4.Click += new System.EventHandler(this.toolStripMenuItem4_Click);
            // 
            // but_run1
            // 
            this.but_run1.Location = new System.Drawing.Point(1120, 280);
            this.but_run1.Margin = new System.Windows.Forms.Padding(6);
            this.but_run1.Name = "but_run1";
            this.but_run1.Size = new System.Drawing.Size(148, 46);
            this.but_run1.TabIndex = 38;
            this.but_run1.Text = "运行";
            this.but_run1.UseVisualStyleBackColor = true;
            this.but_run1.Click += new System.EventHandler(this.but_run1_Click);
            // 
            // but_open
            // 
            this.but_open.Location = new System.Drawing.Point(1120, 380);
            this.but_open.Margin = new System.Windows.Forms.Padding(6);
            this.but_open.Name = "but_open";
            this.but_open.Size = new System.Drawing.Size(148, 46);
            this.but_open.TabIndex = 39;
            this.but_open.Text = "打开";
            this.but_open.UseVisualStyleBackColor = true;
            this.but_open.Click += new System.EventHandler(this.but_open_Click);
            // 
            // but_copy
            // 
            this.but_copy.Location = new System.Drawing.Point(1120, 470);
            this.but_copy.Margin = new System.Windows.Forms.Padding(6);
            this.but_copy.Name = "but_copy";
            this.but_copy.Size = new System.Drawing.Size(148, 46);
            this.but_copy.TabIndex = 40;
            this.but_copy.Text = "复制";
            this.but_copy.UseVisualStyleBackColor = true;
            this.but_copy.Click += new System.EventHandler(this.but_copy_Click);
            // 
            // but_paste
            // 
            this.but_paste.Location = new System.Drawing.Point(1118, 554);
            this.but_paste.Margin = new System.Windows.Forms.Padding(6);
            this.but_paste.Name = "but_paste";
            this.but_paste.Size = new System.Drawing.Size(148, 46);
            this.but_paste.TabIndex = 41;
            this.but_paste.Text = "粘贴";
            this.but_paste.UseVisualStyleBackColor = true;
            this.but_paste.Click += new System.EventHandler(this.but_paste_Click);
            // 
            // but_save
            // 
            this.but_save.Location = new System.Drawing.Point(1118, 642);
            this.but_save.Margin = new System.Windows.Forms.Padding(6);
            this.but_save.Name = "but_save";
            this.but_save.Size = new System.Drawing.Size(148, 46);
            this.but_save.TabIndex = 42;
            this.but_save.Text = "保存";
            this.but_save.UseVisualStyleBackColor = true;
            this.but_save.Click += new System.EventHandler(this.but_save_Click);
            // 
            // labelFileName
            // 
            this.labelFileName.AutoSize = true;
            this.labelFileName.Location = new System.Drawing.Point(668, 216);
            this.labelFileName.Margin = new System.Windows.Forms.Padding(6, 0, 6, 0);
            this.labelFileName.Name = "labelFileName";
            this.labelFileName.Size = new System.Drawing.Size(133, 30);
            this.labelFileName.TabIndex = 44;
            this.labelFileName.Text = "文件名：";
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(19, 214);
            this.label4.Margin = new System.Windows.Forms.Padding(6, 0, 6, 0);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(73, 30);
            this.label4.TabIndex = 45;
            this.label4.Text = "临时";
            // 
            // buttonCopyTmp
            // 
            this.buttonCopyTmp.Location = new System.Drawing.Point(502, 362);
            this.buttonCopyTmp.Margin = new System.Windows.Forms.Padding(6);
            this.buttonCopyTmp.Name = "buttonCopyTmp";
            this.buttonCopyTmp.Size = new System.Drawing.Size(121, 46);
            this.buttonCopyTmp.TabIndex = 40;
            this.buttonCopyTmp.Text = "复制";
            this.buttonCopyTmp.UseVisualStyleBackColor = true;
            this.buttonCopyTmp.Click += new System.EventHandler(this.buttonCopyTmp_Click);
            // 
            // buttonPasteTmp
            // 
            this.buttonPasteTmp.Location = new System.Drawing.Point(500, 446);
            this.buttonPasteTmp.Margin = new System.Windows.Forms.Padding(6);
            this.buttonPasteTmp.Name = "buttonPasteTmp";
            this.buttonPasteTmp.Size = new System.Drawing.Size(121, 46);
            this.buttonPasteTmp.TabIndex = 41;
            this.buttonPasteTmp.Text = "粘贴";
            this.buttonPasteTmp.UseVisualStyleBackColor = true;
            this.buttonPasteTmp.Click += new System.EventHandler(this.buttonPasteTmp_Click);
            // 
            // timerSave
            // 
            this.timerSave.Interval = 20000;
            this.timerSave.Tick += new System.EventHandler(this.timerSave_Tick);
            // 
            // timerHideTip
            // 
            this.timerHideTip.Tick += new System.EventHandler(this.timerHideTip_Tick);
            // 
            // btnOfflineDo
            // 
            this.btnOfflineDo.Location = new System.Drawing.Point(1120, 700);
            this.btnOfflineDo.Margin = new System.Windows.Forms.Padding(4);
            this.btnOfflineDo.Name = "btnOfflineDo";
            this.btnOfflineDo.Size = new System.Drawing.Size(148, 60);
            this.btnOfflineDo.TabIndex = 46;
            this.btnOfflineDo.Text = "上传并执行";
            this.btnOfflineDo.UseVisualStyleBackColor = true;
            this.btnOfflineDo.Click += new System.EventHandler(this.btnOfflineDo_Click);
            // 
            // FrmMain
            // 
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Inherit;
            this.ClientSize = new System.Drawing.Size(1282, 886);
            this.Controls.Add(this.btnOfflineDo);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.labelFileName);
            this.Controls.Add(this.but_save);
            this.Controls.Add(this.buttonPasteTmp);
            this.Controls.Add(this.but_paste);
            this.Controls.Add(this.buttonCopyTmp);
            this.Controls.Add(this.but_copy);
            this.Controls.Add(this.but_open);
            this.Controls.Add(this.but_run1);
            this.Controls.Add(this.richTextBoxCmd);
            this.Controls.Add(this.statusStrip1);
            this.Controls.Add(this.btnRun);
            this.Controls.Add(this.richTextBoxCmdTmp);
            this.Controls.Add(this.textBoxDevice);
            this.Controls.Add(this.buttonNewTest);
            this.Controls.Add(this.btnXA0);
            this.Controls.Add(this.btnXS2);
            this.Controls.Add(this.btnXS1);
            this.Controls.Add(this.button9);
            this.Controls.Add(this.btnRun2);
            this.Controls.Add(this.btnRun1);
            this.Controls.Add(this.textBoxCmd2);
            this.Controls.Add(this.textBoxCmd1);
            this.Controls.Add(this.textBoxCanAddr);
            this.Controls.Add(this.button8);
            this.Controls.Add(this.button7);
            this.Controls.Add(this.button6);
            this.Controls.Add(this.button5);
            this.Controls.Add(this.button4);
            this.Controls.Add(this.button3);
            this.Controls.Add(this.button2);
            this.Controls.Add(this.btnCan1);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.txtport);
            this.Controls.Add(this.txtip);
            this.Margin = new System.Windows.Forms.Padding(6);
            this.Name = "FrmMain";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "MoTest";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.FrmDAECanTest_FormClosing);
            this.Load += new System.EventHandler(this.Form1_Load);
            this.contextMenuStrip1.ResumeLayout(false);
            this.statusStrip1.ResumeLayout(false);
            this.statusStrip1.PerformLayout();
            this.contextMenuStrip2.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TextBox txtip;
        private System.Windows.Forms.TextBox txtport;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Timer timer1;
        private System.Windows.Forms.Button btnCan1;
        private System.Windows.Forms.Button button2;
        private System.Windows.Forms.Button button3;
        private System.Windows.Forms.Button button4;
        private System.Windows.Forms.Button button5;
        private System.Windows.Forms.Button button6;
        private System.Windows.Forms.Button button7;
        private System.Windows.Forms.Button button8;
        private System.Windows.Forms.TextBox textBoxCanAddr;
        private System.Windows.Forms.TextBox textBoxCmd1;
        private System.Windows.Forms.Button btnRun1;
        private System.Windows.Forms.Button button9;
        private System.Windows.Forms.Button btnRun2;
        private System.Windows.Forms.Button btnXS1;
        private System.Windows.Forms.TextBox textBoxCmd2;
        private System.Windows.Forms.Button btnXS2;
        private System.Windows.Forms.Button btnXA0;
        private System.Windows.Forms.Timer timerDoDelay;
        private System.Windows.Forms.Button buttonNewTest;
        private System.Windows.Forms.Timer timerNewTest;
        private System.Windows.Forms.TextBox textBoxDevice;
        private System.Windows.Forms.RichTextBox richTextBoxCmdTmp;
        private System.Windows.Forms.Button btnRun;
        private System.Windows.Forms.StatusStrip statusStrip1;
        private System.Windows.Forms.ToolStripStatusLabel toolStripStatusLabelX;
        private System.Windows.Forms.ToolStripStatusLabel toolStripStatusLabelY;
        private System.Windows.Forms.ContextMenuStrip contextMenuStrip1;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItem1;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItem2;
        private System.Windows.Forms.ToolStripStatusLabel toolStripStatusLabelZ;
        private System.Windows.Forms.ToolStripStatusLabel toolStripStatusLabelM;
        private System.Windows.Forms.ToolStripStatusLabel toolStripStatusLabelN;
        private System.Windows.Forms.ToolStripStatusLabel toolStripStatusLabelP;
        private System.Windows.Forms.ToolStripStatusLabel toolStripStatusLabelU;
        private System.Windows.Forms.ToolStripStatusLabel toolStripStatusLabelV;
        private System.Windows.Forms.ToolStripStatusLabel toolStripStatusLabelW;
        private System.Windows.Forms.RichTextBox richTextBoxCmd;
        private System.Windows.Forms.Button but_run1;
        private System.Windows.Forms.Button but_open;
        private System.Windows.Forms.Button but_copy;
        private System.Windows.Forms.Button but_paste;
        private System.Windows.Forms.Button but_save;
        private System.Windows.Forms.ContextMenuStrip contextMenuStrip2;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItem3;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItem4;
        private System.Windows.Forms.Label labelFileName;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Button buttonCopyTmp;
        private System.Windows.Forms.Button buttonPasteTmp;
        private System.Windows.Forms.Timer timerSave;
        private System.Windows.Forms.Timer timerHideTip;
        private System.Windows.Forms.Button btnOfflineDo;
    }
}

