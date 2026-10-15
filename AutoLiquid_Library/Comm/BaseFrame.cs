using System;
using AutoLiquid_Library.Utils;

namespace AutoLiquid_Library.Comm
{
    public class BaseFrame
    {
        public const int FrameHeader01 = 250; // 帧头1

        public const int FrameHeader02 = 175; // 帧头2

        public const int FrameLength = 8; // 帧长

        private ValidData _frameData; // 有效数据

        private byte _frameCode; // 校验码

        private byte[] _totalData; // 整帧数据

        public ValidData FrameData
        {
            get
            {
                return this._frameData;
            }
            set
            {
                this._frameData = value;
            }
        }

        public byte FrameCode
        {
            get
            {
                return this._frameCode;
            }
            set
            {
                this._frameCode = value;
            }
        }

        public byte[] TotalData
        {
            get
            {
                return this._totalData;
            }
            set
            {
                this._totalData = value;
            }
        }

        public BaseFrame()
        {
        }

        public BaseFrame(ValidData _frameData)
        {
            this.FrameData = _frameData;
            this.FrameCode = DataHelper.IntToByte(DataHelper.GetFrameCode(_frameData));
            this.TotalData = DataHelper.GetCombineData(FrameLength, _frameData, _frameCode);
        }

        public BaseFrame(byte[] buffer)
        {
            this.CopyFromBuffer(buffer);
        }

        public void CopyToBuffer(byte[] buffer, int bufferStartIndex)
        {
            Array.Copy(this.TotalData, 0, buffer, bufferStartIndex, this.TotalData.Length);
        }

        public void CopyFromBuffer(byte[] buffer)
        {
            this.TotalData = new byte[buffer.Length];
            Array.Copy(buffer, 0, this.TotalData, 0, buffer.Length);
            this.FrameData = new ValidData(this.TotalData[3], this.CopyFromBuffer(this.TotalData, 4, 3));
            this.FrameCode = this.TotalData[this.TotalData.Length - 1];
        }

        public byte[] CopyFromBuffer(byte[] byteArray, int byteArrayStartIndex, int length)
        {
            byte[] array = new byte[length];
            Array.Copy(byteArray, byteArrayStartIndex, array, 0, length);
            return array;
        }
    }
}
