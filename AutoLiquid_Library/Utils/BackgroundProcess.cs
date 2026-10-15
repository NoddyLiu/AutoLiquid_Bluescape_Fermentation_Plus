using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace AutoLiquid_Library.Utils
{
    /// <summary>
    /// BackgroundWorker线程处理类
    /// </summary>
    public class BackgroundProcess : BackgroundWorker
    {
        /// <summary>
        /// 执行动作处理线程
        /// </summary>
        /// <param name="doWorkAction">DoWork执行方法</param>
        /// <param name="runWorkerCompletedCallback">RunWorkerCompleted执行动作</param>
        public static void RunAsync(Action doWorkAction, Action runWorkerCompletedCallback)
        {
            BackgroundWorker worker = new BackgroundWorker();
            worker.DoWork += delegate (object sender, DoWorkEventArgs e)
            {
                doWorkAction?.Invoke();
            };
            worker.RunWorkerCompleted += delegate (object sender, RunWorkerCompletedEventArgs e)
            {
                runWorkerCompletedCallback?.Invoke();
            };
            worker.RunWorkerAsync();
        }

        /// <summary>
        /// 执行动作处理线程(含返回值)
        /// </summary>
        /// <typeparam name="TResult">DoWork返回值</typeparam>
        /// <param name="doWorkFunction">DoWork执行方法</param>
        /// <param name="runWorkerCompletedCallback">RunWorkerCompleted执行动作</param>
        public static void RunAsync<TResult>(Func<TResult> doWorkFunction, Action<TResult> runWorkerCompletedCallback)
        {
            BackgroundWorker worker = new BackgroundWorker();
            worker.DoWork += delegate (object sender, DoWorkEventArgs e)
            {
                e.Result = doWorkFunction();
            };
            worker.RunWorkerCompleted += delegate (object sender, RunWorkerCompletedEventArgs e)
            {
                TResult result = (TResult)e.Result;
                runWorkerCompletedCallback(result);
            };
            worker.RunWorkerAsync();
        }
    }
}
