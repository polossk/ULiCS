using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Net;
using System.Net.Sockets;

namespace polossk.Universal.Net
{
    public partial class TcpClientP : TcpClient
    {
        /// <summary>
        /// 构造函数
        /// </summary>
        public TcpClientP() : base() { }

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="family">IP地址的地址族</param>
        public TcpClientP(AddressFamily family) : base(family) { }

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="localEP">将网络端点表示为 IP 地址和端口号</param>
        public TcpClientP(IPEndPoint localEP) : base(localEP) { }

        /// <summary>
        /// 释放资源
        /// </summary>
        /// <param name="disposing">
        ///     true：释放托管资源和非托管资源
        ///     false：仅释放非托管资源
        /// </param>
        protected override void Dispose(bool disposing)
        {
            // 终止独立的通信线程
            ThreadTaskAbort();

            // 调用基类函数释放资源
            base.Dispose(disposing);
        }

        /// <summary>
        /// 析构函数
        /// </summary>
        ~TcpClientP()
        {   // 仅释放非托管资源
            Dispose(false);
        }
    }



    /// <summary>
    /// 异步连接状态对象
    /// </summary>
    internal class AsyncConnectStateObject
    {
        public ManualResetEvent eventDone;
        public TcpClient client;
        public Exception exception;
    }

    public partial class TcpClientP
    {
        /// <summary>
        /// 信道空闲等待时间，默认1秒
        /// </summary>
        private const Int32 IdleTimeout = 1000;

        // 委托声明
        public delegate void ThreadTaskRequest(object sender, EventArgs e);

        // 定义一个委托类型的事件
        public event ThreadTaskRequest OnThreadTaskRequest;

        /// <summary>
        /// 独立的通信线程处理器
        /// </summary>
        protected Thread _TaskThread;

        /// <summary>
        /// 信道空闲事件
        /// </summary>
        protected ManualResetEvent _ChannelIdleEvent;

        /// <summary>
        /// 任务到达事件
        /// </summary>
        protected ManualResetEvent _TaskArrivedEvent;

        /// <summary>
        /// 独立通信线程结束信号
        /// </summary>
        private volatile Boolean _shouldStop;

        /// <summary>
        /// 启动独立的通信线程
        /// </summary>
        /// <param name="action">线程任务处理函数</param>
        public void ThreadTaskStart()
        {
            if (_TaskThread == null)
            {
                _ChannelIdleEvent = new ManualResetEvent(true);     // 初始化信道空闲
                _TaskArrivedEvent = new ManualResetEvent(false);    // 初始化任务空闲
                _shouldStop = false;

                // 创建并启动独立的通信线程
                _TaskThread = new Thread(new ThreadStart(ThreadTaskAction));
                _TaskThread.Start();
            }
        }

        /// <summary>
        /// 终止独立的通信线程
        /// </summary>
        public void ThreadTaskAbort()
        {   // 终止独立通信线程
            if (_TaskThread != null)
            {
                _shouldStop = true;         // 设置线程结束信号
                _TaskArrivedEvent.Set();    // 设置任务到达事件
            }

            // 关闭信道空闲事件
            if (_ChannelIdleEvent != null)
            {
                _ChannelIdleEvent.Close();
                _ChannelIdleEvent = null;
            }

            // 关闭任务到达事件
            if (_TaskArrivedEvent != null)
            {
                _TaskArrivedEvent.Close();
                _TaskArrivedEvent = null;
            }
        }

        /// <summary>
        /// 独立通信线程任务派发
        /// </summary>
        /// <returns>
        ///     true：任务派发成功
        ///     false：任务派发失败
        /// </returns>
        /// <remarks>
        ///     执行OnThreadTaskRequest关联的事件
        /// </remarks>
        public Boolean ThreadTaskAllocation()
        {   // 启动独立的通信线程
            if (_TaskThread == null)
            {
                ThreadTaskStart();
            }

            // 等待信道空闲
            if (_ChannelIdleEvent.WaitOne(IdleTimeout, false))
            {
                _ChannelIdleEvent.Reset();   // 设置信道忙
                _TaskArrivedEvent.Set();     // 设置任务到达
                return true;    // 任务派发成功
            }
            else
            {
                return false;   // 任务派发失败
            }
        }

        /// <summary>
        /// 独立通信线程任务派发
        /// </summary>
        /// <param name="task">要派发的任务请求</param>
        /// <returns>
        ///     true：任务派发成功
        ///     false：任务派发失败
        /// </returns>
        /// <remarks>
        ///     更新OnThreadTaskRequest为当前任务并执行
        /// </remarks>
        public Boolean ThreadTaskAllocation(ThreadTaskRequest task)
        {   // 启动独立的通信线程
            if (_TaskThread == null)
            {
                ThreadTaskStart();
            }

            // 等待信道空闲
            if (_ChannelIdleEvent.WaitOne(IdleTimeout, false))
            {   // 设置信道忙
                _ChannelIdleEvent.Reset();

                // 清空事件调用列表
                if (OnThreadTaskRequest != null)
                {
                    foreach (Delegate d in OnThreadTaskRequest.GetInvocationList())
                    {
                        OnThreadTaskRequest -= (ThreadTaskRequest)d;
                    }
                }

                // 更新事件调用列表
                OnThreadTaskRequest += task;

                // 设置任务到达
                _TaskArrivedEvent.Set();
                return true;    // 任务派发成功
            }
            else
            {
                return false;   // 任务派发失败
            }
        }

        /// <summary>
        /// 独立通信线程处理器
        /// </summary>
        private void ThreadTaskAction()
        {
            try
            {
                while (true)
                {   // 等待任务到达
                    if (_TaskArrivedEvent.WaitOne())
                    {   // 检测线程结束信号
                        if (_shouldStop) break;

                        try
                        {   // 执行任务
                            if (OnThreadTaskRequest != null)
                            {
                                OnThreadTaskRequest(this, EventArgs.Empty);
                            }
                        }

                        catch
                        {
                            // 阻止异常抛出                
                        }

                        // 等待新的任务
                        if (_TaskArrivedEvent != null) _TaskArrivedEvent.Reset();

                        // 设置信道空闲 
                        if (_ChannelIdleEvent != null) _ChannelIdleEvent.Set();

                        // 再次检测线程结束信号
                        if (_shouldStop) break;
                    }
                } // End While
            }

            catch
            {
                // 阻止异常抛出
            }

            // 保证线程资源释放
            finally
            {   // 线程关闭
                _TaskThread = null;

                // 关闭信道空闲事件
                if (_ChannelIdleEvent != null)
                {
                    _ChannelIdleEvent.Close();
                    _ChannelIdleEvent = null;
                }

                // 关闭任务到达事件
                if (_TaskArrivedEvent != null)
                {
                    _TaskArrivedEvent.Close();
                    _TaskArrivedEvent = null;
                }
            }
        }
    }

    /// <summary>
    /// 实现TcpClient的异步连接
    /// </summary>
    public partial class TcpClientP
    {
        /// <summary>
        /// 设置连接超时值
        /// </summary>
        public Int32 ConnectTimeout = Timeout.Infinite;

        /// <summary>
        /// 异步连接
        /// </summary>
        /// <param name="hostname">主机名</param>
        /// <param name="port">端口号</param>
        public void AsyncConnect(String hostname, Int32 port)
        {
            // 用户定义对象
            AsyncConnectStateObject State = new AsyncConnectStateObject
            {   // 将事件状态设置为非终止状态，导致线程阻止
                eventDone = new ManualResetEvent(false),
                client = this,
                exception = null
            };

            // 开始一个对远程主机连接的异步请求
            BeginConnect(hostname, port, new AsyncCallback(AsyncConnectCallback), State);

            // 等待操作完成信号
            if (State.eventDone.WaitOne(ConnectTimeout, false))
            {   // 接收到信号
                if (State.exception != null) throw State.exception;
            }
            else
            {   // 超时异常
                Close();
                throw new TimeoutException();
            }
        }

        /// <summary>
        /// 异步连接
        /// </summary>
        /// <param name="address">IP地址</param>
        /// <param name="port">端口号</param>
        public void AsyncConnect(IPAddress address, Int32 port)
        {
            // 用户定义对象
            AsyncConnectStateObject State = new AsyncConnectStateObject
            {   // 将事件状态设置为非终止状态，导致线程阻止
                eventDone = new ManualResetEvent(false),
                client = this,
                exception = null
            };

            // 开始一个对远程主机连接的异步请求
            BeginConnect(address, port, new AsyncCallback(AsyncConnectCallback), State);

            // 等待操作完成信号
            if (State.eventDone.WaitOne(ConnectTimeout, false))
            {   // 接收到信号
                if (State.exception != null) throw State.exception;
            }
            else
            {   // 超时异常
                Close();
                throw new TimeoutException();
            }
        }

        /// <summary>
        /// 异步连接回调函数
        /// </summary>
        /// <param name="ar">异步操作结果</param>
        private static void AsyncConnectCallback(IAsyncResult ar)
        {
            AsyncConnectStateObject State = ar.AsyncState as AsyncConnectStateObject;
            try
            {   // 异步接受传入的连接尝试
                State.client.EndConnect(ar);
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


}
