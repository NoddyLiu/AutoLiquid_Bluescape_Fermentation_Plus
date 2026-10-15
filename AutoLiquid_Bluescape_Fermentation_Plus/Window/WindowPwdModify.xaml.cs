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
    /// 密码修改窗体
    /// </summary>
    public partial class WindowPwdModify : MetroWindow
    {
        // 是否修改成功
        public bool isSuccessful = false;

        public WindowPwdModify()
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
                 * ② 原密码是否正确
                 * ③ 新密码 和 确认密码是否一致
                 * ④ 新密码 和 原密码 一样
                 */
                var pwdOld = this.PasswordBoxOld.Password.Trim();
                var pwdNew = this.PasswordBoxNew.Password.Trim();
                var pwdConfirm = this.PasswordBoxConfirm.Password.Trim();
                if (pwdOld.Equals("") || pwdNew.Equals("") || pwdConfirm.Equals(""))
                {
                    isSuccessful = false;
                    this.LabelError.Content = (string)this.FindResource("Prompt_Pls_Input_Pwd_First");
                }
                else if (!PermissionHelper.ValidatePassword(pwdOld, ParamsHelper.Permission.PwdHash))
                {
                    isSuccessful = false;
                    this.LabelError.Content = (string)this.FindResource("Prompt_Pwd_Old_Not_Correct");
                }
                else if (!pwdNew.Equals(pwdConfirm))
                {
                    isSuccessful = false;
                    this.LabelError.Content = (string)this.FindResource("Prompt_Pwd_New_Not_Same");
                }
                else if (pwdOld.Equals(pwdNew))
                {
                    isSuccessful = false;
                    this.LabelError.Content = (string)this.FindResource("Prompt_Pwd_New_Same_As_Old");
                }
                else
                {
                    isSuccessful = true;
                    ParamsHelper.Permission.PwdHash = PermissionHelper.CreateHash(pwdNew);
                    FileUtils.SavePermission(ParamsHelper.Permission);
                    this.Close();
                    MessageBox.Show((string) this.FindResource("Prompt_Pwd_Modify_Successfully"));
                }
            }
        }
    }
}
