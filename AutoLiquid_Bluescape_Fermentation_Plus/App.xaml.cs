using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using AutoLiquid_Bluescape_Fermentation_Plus.Utils;
using Serilog;
using Serilog.Events;

namespace AutoLiquid_Bluescape_Fermentation_Plus
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private void Application_Startup(object sender, StartupEventArgs e)
        {
            // 日志保存路径
            var logInfoFilePath =
                Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), (string)this.FindResource("ProcessesName")) +
                Path.DirectorySeparatorChar + "Logs" + Path.DirectorySeparatorChar + DateTime.Now.ToString("yyyy-MM-dd") + Path.DirectorySeparatorChar;
            var logErrorFilePath =
                Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), (string)this.FindResource("ProcessesName")) +
                Path.DirectorySeparatorChar + "Errors" + Path.DirectorySeparatorChar + DateTime.Now.ToString("yyyy-MM-dd") + Path.DirectorySeparatorChar;

            // 开启日志
            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Verbose()
                .WriteTo.Logger(c => c.Filter.ByIncludingOnly(ev => ev.Level == LogEventLevel.Information)
                    .WriteTo.File(logInfoFilePath + "log.txt",
                        outputTemplate: "[{Timestamp:yyyy-MM-dd HH:mm:ss,fff}] [{Level:u3}] {Message:lj}{NewLine}{Exception}", // 输出日期格式
                        retainedFileCountLimit: null))
                .WriteTo.Logger(c => c.Filter.ByIncludingOnly(ev => ev.Level == LogEventLevel.Error)
                        .WriteTo.File(logErrorFilePath + "error.txt",
                            outputTemplate: "[{Timestamp:yyyy-MM-dd HH:mm:ss,fff}] [{Level:u3}] {Message:lj}{NewLine}{Exception}", // 输出日期格式
                            retainedFileCountLimit: null))
                .CreateLogger();


            // 避免重复开启软件
            var procName = (string)this.FindResource("ProcessesName");
            Process[] processes = Process.GetProcessesByName(procName);
            if (processes.Length > 1)
            {
                MessageBox.Show((string)this.FindResource("Prompt_Pls_Dont_Open_Software_Again"));
                Application.Current.Shutdown();
            }
            else
            {
                MainWindow main = new MainWindow();
                main.Show();

                // 未捕获异常记录入log.txt
                AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;
            }
        }
        protected override void OnStartup(StartupEventArgs e)
        {
            //设置一个值，此值指示数据绑定 TextBox 是否应显示与源的 Text 属性值一致的字符串 为false
            System.Windows.FrameworkCompatibilityPreferences.KeepTextBoxDisplaySynchronizedWithTextProperty = false;
            base.OnStartup(e);
        }

        private void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            try
            {
                LogHelper.Error(e.ExceptionObject.ToString());
            }
            catch
            {
            }
        }
    }
}
