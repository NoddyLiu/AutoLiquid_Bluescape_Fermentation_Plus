using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DAERun.CallBack
{
    /// <summary>
    /// 接收回调
    /// </summary>
    public abstract class ReceivedCallBack
    {
        public abstract void Callback(byte[] bytes);
    }
}
