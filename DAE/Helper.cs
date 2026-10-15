using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DAERun
{
    class helper
    {
        public int LineNo{ get; set; }
        public string Label { get; set; }
        public int Loopcount { get; set; }
        public int OrgLoopcount { get; set; }
        public int LoopLevel { get; set; }
        public string orgWord { get; set; }  //原来词
        public string tarWord { get; set; }   //替换成
    }
}
