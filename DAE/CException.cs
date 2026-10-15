using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DAERun
{
    public class ExceptionCmdTimeOut:Exception
    {
        public override string Message => "Command executed time out";
    }

    public class ExceptionCmdNoAnwser: Exception
    {
        public override string Message => "No anwser";
    }

    public class ExceptionManulStop : Exception
    {
        public override string Message => "Manual Stop";
    }

}
