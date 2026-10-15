namespace DAERun
{
    partial class FrmTip
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.labelTip = new System.Windows.Forms.Label();
            this.btnExit = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // labelTip
            // 
            this.labelTip.Location = new System.Drawing.Point(31, 70);
            this.labelTip.Margin = new System.Windows.Forms.Padding(8, 0, 8, 0);
            this.labelTip.Name = "labelTip";
            this.labelTip.Size = new System.Drawing.Size(1182, 95);
            this.labelTip.TabIndex = 0;
            this.labelTip.Text = "labelTip";
            // 
            // btnExit
            // 
            this.btnExit.Location = new System.Drawing.Point(1009, 236);
            this.btnExit.Margin = new System.Windows.Forms.Padding(8, 8, 8, 8);
            this.btnExit.Name = "btnExit";
            this.btnExit.Size = new System.Drawing.Size(188, 58);
            this.btnExit.TabIndex = 1;
            this.btnExit.Text = "退出";
            this.btnExit.UseVisualStyleBackColor = true;
            this.btnExit.Click += new System.EventHandler(this.btnExit_Click);
            // 
            // FrmTip
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(15F, 30F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1250, 340);
            this.Controls.Add(this.btnExit);
            this.Controls.Add(this.labelTip);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.Margin = new System.Windows.Forms.Padding(8, 8, 8, 8);
            this.MaximizeBox = false;
            this.Name = "FrmTip";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "提示窗口";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.FrmTip_FormClosing);
            this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.FrmTip_FormClosed);
            this.ResumeLayout(false);

        }

        #endregion

        public System.Windows.Forms.Label labelTip;
        public System.Windows.Forms.Button btnExit;

    }
}