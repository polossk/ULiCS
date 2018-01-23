using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Net;
using System.Net.Sockets;


namespace polossk.Universal.Net
{
    /// <summary>
    /// 异步读状态对象
    /// </summary>
    internal class AsyncReadStateObjectC
    {
        public ManualResetEvent eventDone;
        public NetworkStream stream;
        public Exception exception;
        public Int32 numberOfBytesRead;
    }

    /// <summary>
    /// 实现TcpClient的异步接收
    /// </summary>
    public partial class TcpClientP
    {
        /// <summary>
        /// 设置读超时等待值
        /// </summary>
        public Int32 ReadTimeout = Timeout.Infinite;

        /// <summary>
        /// 异步接收
        /// </summary>
        /// <param name="data">接收到的字节数组</param>
        public void Read(out Byte[] data)
        {
            // 获取网络数据流
            NetworkStream netStream = GetStream();

            // 用户定义对象
            AsyncReadStateObjectC State = new AsyncReadStateObjectC
            {   // 将事件状态设置为非终止状态，导致线程阻止
                eventDone = new ManualResetEvent(false),
                stream = netStream,
                exception = null,
                numberOfBytesRead = -1
            };

            Byte[] Buffer = new Byte[ReceiveBufferSize];
            using (MemoryStream memStream = new MemoryStream(ReceiveBufferSize))
            {
                Int32 TotalBytes = 0;       // 总共需要接收的字节数
                Int32 ReceivedBytes = 0;    // 当前已接收的字节数
                while (true)
                {
                    // 将事件状态设置为非终止状态，导致线程阻止
                    State.eventDone.Reset();

                    // 异步读取网络数据流
                    netStream.BeginRead(Buffer, 0, Buffer.Length, new AsyncCallback(AsyncReadCallback), State);

                    // 等待操作完成信号
                    if (State.eventDone.WaitOne(ReadTimeout, false))
                    {   // 接收到信号
                        if (State.exception != null) throw State.exception;

                        if (State.numberOfBytesRead == 0)
                        {   // 连接已经断开
                            throw new SocketException();
                        }
                        else if (State.numberOfBytesRead > 0)
                        {
                            if (TotalBytes == 0)
                            {   // 提取流头部字节长度信息
                                TotalBytes = BitConverter.ToInt32(Buffer, 0);

                                // 保存剩余信息
                                memStream.Write(Buffer, 4, State.numberOfBytesRead - 4);
                            }
                            else
                            {
                                memStream.Write(Buffer, 0, State.numberOfBytesRead);
                            }

                            ReceivedBytes += State.numberOfBytesRead;
                            if (ReceivedBytes >= TotalBytes) break;
                        }
                    }
                    else
                    {   // 超时异常
                        throw new TimeoutException();
                    }
                }

                data = (memStream.Length > 0) ? memStream.ToArray() : null;
            }
        }

        /// <summary>
        /// 异步接收
        /// </summary>
        /// <param name="answer">接收到的字符串</param>
        /// <param name="codePage">代码页</param>
        /// <remarks>
        /// 代码页：
        ///     936：简体中文GB2312
        ///     54936：简体中文GB18030
        ///     950：繁体中文BIG5
        ///     1252：西欧字符CP1252
        ///     65001：UTF-8编码
        /// </remarks>
        public void Read(out String answer, Int32 codePage = 65001)
        {
            Byte[] data;
            Read(out data);
            answer = (data != null) ? Encoding.GetEncoding(codePage).GetString(data) : null;
        }

        /// <summary>
        /// 异步读取回调函数
        /// </summary>
        /// <param name="ar">异步操作结果</param>
        private static void AsyncReadCallback(IAsyncResult ar)
        {
            AsyncReadStateObjectC State = ar.AsyncState as AsyncReadStateObjectC;
            try
            {   // 异步写入结束
                State.numberOfBytesRead = State.stream.EndRead(ar);
            }

            catch (Exception e)
            {   // 异步连接异常
                State.exception = e;
            }

            finally
            {   // 将事件状态设置为终止状态，线程继续
                State.eventDone.Set();
            }
        }
    }

    /// <summary>
    /// 异步写状态对象
    /// </summary>
    internal class AsyncWriteStateObjectC
    {
        public ManualResetEvent eventDone;
        public NetworkStream stream;
        public Exception exception;
    }

    /// <summary>
    /// 实现TcpClient的异步发送
    /// </summary>
    public partial class TcpClientP
    {
        /// <summary>
        /// 设置写超时等待值
        /// </summary>
        public Int32 WriteTimeout = Timeout.Infinite;

        /// <summary>
        /// 异步发送
        /// </summary>
        /// <param name="buffer">字节数组</param>
        /// <param name="offset">起始偏移量</param>
        /// <param name="size">字节数</param>
        public void Write(Byte[] buffer, Int32 offset, Int32 size)
        {
            // 获取网络数据流
            NetworkStream netStream = GetStream();

            // 用户定义对象
            AsyncWriteStateObjectC State = new AsyncWriteStateObjectC
            {   // 将事件状态设置为非终止状态，导致线程阻止
                eventDone = new ManualResetEvent(false),
                stream = netStream,
                exception = null
            };

            Byte[] BytesArray;
            // 在数据前插入长度信息
            Int32 Length = buffer.Length + 4;
            BytesArray = new Byte[Length];
            Array.Copy(BitConverter.GetBytes(Length), BytesArray, 4);
            Array.Copy(buffer, 0, BytesArray, 4, buffer.Length);


            // 写入加长度信息头的数据
            netStream.BeginWrite(BytesArray, 0, BytesArray.Length, new AsyncCallback(AsyncWriteCallback), State);

            // 等待操作完成信号
            if (State.eventDone.WaitOne(WriteTimeout, false))
            {   // 接收到信号
                if (State.exception != null) throw State.exception;
            }
            else
            {   // 超时异常
                throw new TimeoutException();
            }
        }

        /// <summary>
        /// 异步发送
        /// </summary>
        /// <param name="data">字节数组</param>
        public void Write(Byte[] data)
        {
            Write(data, 0, data.Length);
        }

        /// <summary>
        /// 异步发送
        /// </summary>
        /// <param name="command">字符串</param>
        /// <param name="codePage">代码页</param>
        /// <remarks>
        /// 代码页：
        ///     936：简体中文GB2312
        ///     54936：简体中文GB18030
        ///     950：繁体中文BIG5
        ///     1252：西欧字符CP1252
        ///     65001：UTF-8编码
        /// </remarks>
        public void Write(String command, Int32 codePage = 65001)
        {
            Write(Encoding.GetEncoding(codePage).GetBytes(command));
        }

        /// <summary>
        /// 异步写入回调函数
        /// </summary>
        /// <param name="ar">异步操作结果</param>
        private static void AsyncWriteCallback(IAsyncResult ar)
        {
            AsyncWriteStateObjectC State = ar.AsyncState as AsyncWriteStateObjectC;
            try
            {   // 异步写入结束
                State.stream.EndWrite(ar);
            }

            catch (Exception e)
            {   // 异步连接异常
                State.exception = e;
            }

            finally
            {   // 将事件状态设置为终止状态，线程继续
                State.eventDone.Set();
            }
        }
    }

    /// <summary>
    /// 实现TcpClient的异步查询
    /// </summary>
    public partial class TcpClientP
    {
        /// <summary>
        /// 异步查询
        /// </summary>
        /// <param name="command">发送数据</param>
        /// <param name="offset">起始偏移量</param>
        /// <param name="size">字节数</param>
        /// <param name="answer">接收数据</param>
        public void Query(Byte[] command, Int32 offset, Int32 size, out Byte[] answer)
        {
            if (command != null)
            {   // 发送数据
                Write(command, offset, size);
            }

            // 接收数据
            Read(out answer);
        }

        /// <summary>
        /// 异步查询
        /// </summary>
        /// <param name="command">发送数据</param>
        /// <param name="offset">起始偏移量</param>
        /// <param name="size">字节数</param>
        /// <param name="answer">接收数据</param>
        /// <param name="codePage">代码页</param>
        /// <remarks>
        /// 代码页：
        ///     简体中文GB2312      936
        ///     简体中文GB18030     54936
        ///     繁体中文BIG5        950
        ///     西欧字符CP1252      1252
        ///     UTF-8               65001
        /// </remarks>
        public void Query(Byte[] command, Int32 offset, Int32 size, out String answer, Int32 codePage = 65001)
        {
            if (command != null)
            {   // 发送数据
                Write(command, offset, size);
            }

            // 接收数据
            Read(out answer, codePage);
        }

        /// <summary>
        /// 异步查询
        /// </summary>
        /// <param name="command">发送数据</param>
        /// <param name="answer">接收数据</param>
        public void Query(Byte[] command, out Byte[] answer)
        {
            if (command != null)
            {   // 发送数据
                Write(command);
            }

            // 接收数据
            Read(out answer);
        }

        /// <summary>
        /// 异步查询
        /// </summary>
        /// <param name="command">发送数据</param>
        /// <param name="answer">接收数据</param>
        /// <param name="codePage">代码页</param>
        /// <remarks>
        /// 代码页：
        ///     简体中文GB2312      936
        ///     简体中文GB18030     54936
        ///     繁体中文BIG5        950
        ///     西欧字符CP1252      1252
        ///     UTF-8               65001
        /// </remarks>
        public void Query(Byte[] command, out String answer, Int32 codePage = 65001)
        {
            if (command != null)
            {   // 发送数据
                Write(command);
            }

            // 接收数据
            Read(out answer, codePage);
        }

        /// <summary>
        /// 异步查询
        /// </summary>
        /// <param name="command">发送数据</param>
        /// <param name="answer">接收数据</param>
        /// <param name="codePage">代码页</param>
        /// <remarks>
        /// 代码页：
        ///     简体中文GB2312      936
        ///     简体中文GB18030     54936
        ///     繁体中文BIG5        950
        ///     西欧字符CP1252      1252
        ///     UTF-8               65001
        /// </remarks>
        public void Query(String command, out Byte[] answer, Int32 codePage = 65001)
        {
            if (!String.IsNullOrEmpty(command))
            {   // 发送数据
                Write(command, codePage);
            }

            // 接收数据
            Read(out answer);
        }

        /// <summary>
        /// 异步查询
        /// </summary>
        /// <param name="command">发送数据</param>
        /// <param name="answer">接收数据</param>
        /// <param name="codePage">代码页</param>
        /// <remarks>
        /// 代码页：
        ///     简体中文GB2312      936
        ///     简体中文GB18030     54936
        ///     繁体中文BIG5        950
        ///     西欧字符CP1252      1252
        ///     UTF-8               65001
        /// </remarks>
        public void Query(String command, out String answer, Int32 codePage = 65001)
        {
            if (!String.IsNullOrEmpty(command))
            {   // 发送数据
                Write(command, codePage);
            }

            // 接收数据
            Read(out answer, codePage);
        }
    }

}
