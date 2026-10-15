using System;
using System.Collections.Generic;
using System.Linq;
using AutoLiquid_Library.Comm;

namespace AutoLiquid_Library.Utils
{
    public class DataHelper
    {
        public static int ByteToInt(byte b)
        {
            return Convert.ToInt32(b);
        }

        public static byte IntToByte(int value)
        {
            return Convert.ToByte(value);
        }

        public static int ByteArrayToInt(byte[] b)
        {
            int num = 0;
            int num2;
            for (int i = 0; i < b.Length; i = num2 + 1)
            {
                num += (int)b[i];
                num2 = i;
            }
            return num;
        }

        public static byte GetItem(byte[] data, int index)
        {
            return data[index - 1];
        }

        /// <summary>
        /// 获取数据低位
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public static byte GetLowByte(int data)
        {
            return (byte)(data & 255);
        }

        /// <summary>
        /// 获取数据高位
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public static byte GetHighByte(int data)
        {
            return (byte)(data >> 24);
        }

        /// <summary>
        /// 整合数据
        /// </summary>
        /// <param name="frameLength"></param>
        /// <param name="frameData"></param>
        /// <param name="frameCode"></param>
        /// <returns></returns>
        public static byte[] GetCombineData(byte frameLength, ValidData frameData, byte frameCode)
        {
            List<byte> list = new List<byte>();
            list.Add(BaseFrame.FrameHeader01);
            list.Add(BaseFrame.FrameHeader02);
            list.Add(frameLength);
            list.Add(frameData.CMDCode);
            list.AddRange(frameData.ConcreteData);
            list.Add(frameCode);
            return list.OfType<byte>().ToArray<byte>();
        }

        /// <summary>
        /// 获取验证码
        /// </summary>
        /// <param name="validData"></param>
        /// <returns></returns>
        public static int GetFrameCode(ValidData validData)
        {
            int num = BaseFrame.FrameHeader01 + BaseFrame.FrameHeader02 + BaseFrame.FrameLength + DataHelper.ByteToInt(validData.CMDCode) + DataHelper.ByteArrayToInt(validData.ConcreteData);
            if (num > 255)
            {
                num = DataHelper.ByteToInt(DataHelper.GetLowByte(num));
            }
            return num;
        }

        /// <summary>
        /// 检查校验码是否正确
        /// </summary>
        /// <param name="mBaseFrame"></param>
        /// <returns></returns>
        public static bool CheckFrameCodeValidate(BaseFrame mBaseFrame)
        {
            int num = BaseFrame.FrameHeader01 + BaseFrame.FrameHeader02 + BaseFrame.FrameLength + DataHelper.ByteToInt(mBaseFrame.FrameData.CMDCode) + DataHelper.ByteArrayToInt(mBaseFrame.FrameData.ConcreteData);
            if (num > 255)
            {
                num = DataHelper.ByteToInt(DataHelper.GetLowByte(num));
            }
            int num2 = DataHelper.ByteToInt(mBaseFrame.FrameCode);

            return num == num2;
        }
    }
}
