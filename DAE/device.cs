using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DAERun
{
    class Device
    {
        private int mPos;
        public string Name { get; set; }
        public int CanAddr { get; set; }
        public long LastReceiveTime { get; set; }
        public long LastSendTime {
            get; set;
        }
        public byte[] LastSendData = new byte[13];
        public byte[] LastReceiveData1 = new byte[13];
        public byte[] LastReceiveData2 = new byte[13];
        private int packageIndex;
        public string LastCmdText="";

        public int Pos {
            get
            {
                return  mPos; }
            set { mPos=value; } }

        public void CleanReceiveData()
        {
             Array.Clear(LastReceiveData1, 0, LastReceiveData1.Length);
             Array.Clear(LastReceiveData2, 0, LastReceiveData2.Length);
        }

        public int PackageIndex
        {
            get
            {
                packageIndex++;
                if (packageIndex > 0xF) packageIndex = 0;
                return packageIndex;
            }

            set
            {
                packageIndex = value;
            }
        }

    }

}
