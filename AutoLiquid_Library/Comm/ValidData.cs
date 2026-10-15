using System;

namespace AutoLiquid_Library.Comm
{
    /// <summary>
    /// 有效数据
    /// </summary>
	public class ValidData
	{
		private byte _cmdCode;

		private byte[] _concreteData;

		public byte CMDCode
		{
			get
			{
				return this._cmdCode;
			}
			set
			{
				this._cmdCode = value;
			}
		}

		public byte[] ConcreteData
		{
			get
			{
				return this._concreteData;
			}
			set
			{
				this._concreteData = value;
			}
		}

		public ValidData(byte mCMDCode, byte[] mConcreteData)
		{
			this.CMDCode = mCMDCode;
			this.ConcreteData = mConcreteData;
		}
	}
}
