using System;
using System.IO;
using System.Linq;
using System.Text;
using MahApps.Metro.Controls;
using System.Windows;
using System.Windows.Media.Imaging;
using AutoLiquid_Bluescape_Fermentation_Plus.Utils;

namespace AutoLiquid_Bluescape_Fermentation_Plus.Window
{
    /// <summary>
    /// "关于"弹出框
    /// </summary>
    public partial class WindowAbout : MetroWindow
    {
        // 更新日志文件路径
        private string logFilePath = AppDomain.CurrentDomain.BaseDirectory +
                                     Path.DirectorySeparatorChar +
                                     AutoLiquid_Library.Utils.ConstantsUtils.FILE_UPDATE_LOG;

        public WindowAbout()
        {
            InitializeComponent();

            var logoFIle = AppDomain.CurrentDomain.BaseDirectory + Path.DirectorySeparatorChar + "logo.png";
            if (File.Exists(logoFIle))
            {
                this.Icon = new BitmapImage(new Uri(logoFIle));
                this.ImageLogo.Source = new BitmapImage(new Uri(logoFIle));
            }

            try
            {
                InitAboutInfo();
                this.LabelVersion.Content = File.ReadLines(logFilePath).First().Split(new char[] { ':', '：' }).ElementAt(1);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }

            // 更新日志
            UpdateLog();

            // 控件事件
            ControlEvent();
        }

        /// <summary>
        /// 关于信息
        /// </summary>
        private void InitAboutInfo()
        {
            var logoTxtFIle = AppDomain.CurrentDomain.BaseDirectory + Path.DirectorySeparatorChar + "logo.txt";
            // 判断是否存在logo.txt文件，如存在，就读取里面的内容
            if (File.Exists(logoTxtFIle))
            {
                var logoTxtContent = File.ReadAllText(logoTxtFIle);
                if (!logoTxtContent.Equals(""))
                    this.TextBlockAboutInfo.Text = logoTxtContent;
            }
        }

        /// <summary>
        /// 更新日志
        /// </summary>
        private void UpdateLog()
        {
            StringBuilder logContent = new StringBuilder();
            string line;
            try
            {
                StreamReader file = new StreamReader(logFilePath);
                while ((line = file.ReadLine()) != null)
                {
                    logContent.AppendLine(line);
                }

                this.TextBoxUpdateLog.Text = logContent.ToString();

                file.Close();
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        private void ControlEvent()
        {
            this.BtnConfirm.Click += BtnClick;
        }

        private void BtnClick(object sender, RoutedEventArgs e)
        {
            if (sender.Equals(this.BtnConfirm))
                this.Close();
        }
    }
}
