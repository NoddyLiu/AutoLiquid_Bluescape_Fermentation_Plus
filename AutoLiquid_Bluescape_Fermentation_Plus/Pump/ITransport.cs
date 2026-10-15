using System;

namespace AutoLiquid_Bluescape_Fermentation_Plus.Pump
{
    /// <summary>
    /// 传输层抽象（可注入任何串口/网络实现）。
    ///
    /// 字节语义：open/write/read 均按"裸字节"处理，不解析帧。
    /// 帧解析在 <see cref="OemCodec"/> 完成。
    /// </summary>
    public interface ITransport
    {
        bool IsOpen { get; }
        void Open();
        void Close();
        void Write(byte[] data);

        /// <summary>
        /// 读取一段字节。timeoutMs 内尽量收集最多 maxLen 字节返回，
        /// 到时无数据则返回空数组。
        /// </summary>
        byte[] Read(int maxLen = 256, int timeoutMs = 200);
    }

    /// <summary>
    /// 标识传输层支持"发送前清空内部接收队列"的能力。
    /// 协议客户端在 send 前会主动调用，避免上一帧的粘连字节混入下一帧解析。
    /// </summary>
    public interface ISupportsPreSendClear
    {
        void ClearRxQueue();
    }
}
