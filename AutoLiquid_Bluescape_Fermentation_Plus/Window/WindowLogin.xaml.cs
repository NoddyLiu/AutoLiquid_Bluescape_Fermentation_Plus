using System;
using System.Collections.Generic;
using System.IO;
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
using AutoLiquid_Bluescape_Fermentation_Plus.Utils;
using MahApps.Metro.Controls;

namespace AutoLiquid_Bluescape_Fermentation_Plus.Window
{
    /// <summary>
    /// 密码登录窗体
    /// </summary>
    public partial class WindowLogin : MetroWindow
    {
        // 是否登录成功
        public bool isSuccessful = false;

        public WindowLogin()
        {
            InitializeComponent();

            var logoFIle = AppDomain.CurrentDomain.BaseDirectory + Path.DirectorySeparatorChar + "logo.png";
            if (File.Exists(logoFIle))
                this.Icon = new BitmapImage(new Uri(logoFIle));

            // 控件事件
            ControlEvent();
        }

        private void ControlEvent()
        {
            this.BtnCancel.Click += BtnClick;
            this.BtnConfirm.Click += BtnClick;
        }

        private void BtnClick(object sender, RoutedEventArgs e)
        {
            if (sender.Equals(this.BtnCancel))
            {
                isSuccessful = false;
                this.Close();
            }
            else if (sender.Equals(this.BtnConfirm))
            {
                /**
                 * 逻辑判断
                 * ① 是否为空
                 * ② 密码是否正确
                 */
                var pwd = this.PasswordBox.Password.Trim();
                if (pwd.Equals(""))
                {
                    isSuccessful = false;
                    this.LabelError.Content = (string)this.FindResource("Prompt_Pls_Input_Pwd_First");
                }
                else if (!PermissionHelper.ValidatePassword(pwd, ParamsHelper.Permission.PwdHash))
                {
                    isSuccessful = false;
                    this.LabelError.Content = (string)this.FindResource("Prompt_Pwd_Not_Correct");
                }
                else
                {
                    isSuccessful = true;
                    this.Close();
                }
            }
        }
    }
}
