using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Net;
using System.Net.Sockets;

namespace polossk.Universal.Net
{
    public partial class TcpListenerP : TcpListener
    {
        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="localEP">本地终结点</param>
        public TcpListenerP(IPEndPoint localEP) : base(localEP)
        {
            Thread listenThread = new Thread(new ThreadStart(ListenThreadAction));
            listenThread.Start();
        }

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="localaddr">本地IP地址</param>
        /// <param name="port">侦听端口</param>
        public TcpListenerP(IPAddress localaddr, Int32 port) : base(localaddr, port)
        {
            Thread listenThread = new Thread(new ThreadStart(ListenThreadAction));
            listenThread.Start();
        }

        /// <summary>
        /// 析构函数
        /// </summary>
        ~TcpListenerP() { Stop(); }
    }

    public partial class TcpListenerP : TcpListener
    {
        /// <summary>
        /// 委托声明
        /// </summary>
        /// <param name="sender">事件发送者</param>
        /// <param name="e">事件参数</param>
        public delegate void ThreadTaskRequest(object sender, EventArgs e);

        /// <summary>
        /// 线程任务请求事件
        /// </summary>
        public event ThreadTaskRequest OnThreadTaskRequest;

        // 已接受的Tcp连接列表
        protected List<TcpClient> _tcpClients;

        /// <summary>
        /// 连接列表操作互斥量
        /// </summary>
        private Mutex _mutexClients;

        /// <summary>
        /// 获取所有连接上的IP地址
        /// </summary>
        /// <param name="res">输出 获取的IP地址列表</param>
        public void GetIPList(out List<IPAddress> res)
        {
            res = new List<IPAddress>();
            _mutexClients.WaitOne();
            // 复制地址
            foreach (var c in _tcpClients)
            {
                IPEndPoint where = c.Client.RemoteEndPoint as IPEndPoint;
                res.Add(where.Address);
            }
            _mutexClients.ReleaseMutex();
        }


        /// <summary>
        /// 侦听连接线程
        /// </summary>
        private void ListenThreadAction()
        {   // 启动侦听
            Start();

            // 初始化连接列表和互斥量
            _tcpClients = new List<TcpClient>();
            _mutexClients = new Mutex();
            Console.WriteLine("{0} [host {1}]: On Listen...", DateTime.Now, 6666);
            // 接受连接
            while (true)
            {
                TcpClient tcpClient = null;
                try
                {   // 接受挂起的连接请求
                    tcpClient = AcceptTcpClient();
                    IPEndPoint where = tcpClient.Client.RemoteEndPoint as IPEndPoint;
                    Console.WriteLine("{0} [host {1}]: connected with [{2}:{3}]",
                        DateTime.Now, 6666, where.Address.ToString(), where.Port.ToString());
                    // 将该连接通信加入线程池队列
                    ThreadPool.QueueUserWorkItem(ThreadPoolCallback, tcpClient);

                    // 连接加入列表
                    _mutexClients.WaitOne();
                    _tcpClients.Add(tcpClient);
                    _mutexClients.ReleaseMutex();
                } catch (SocketException ex) {
                    // 结束侦听线程
                    Console.WriteLine("{0} [host {1}]: ERROR {2}", DateTime.Now, 6666, ex.Message);
                    break;
                } catch (Exception ex) {
                    // 加入队列失败
                    Console.WriteLine("{0} [host {1}]: ERROR {2}", DateTime.Now, 6666, ex.Message);
                    if (tcpClient != null) tcpClient.Close();
                }
            }
        }

        /// <summary>
        /// 线程池回调方法
        /// </summary>
        /// <param name="state">回调方法要使用的信息对象</param>
        private void ThreadPoolCallback(Object state)
        {   // 如果无法进行转换，则 as 返回 null 而非引发异常
            TcpClient tcpClient = state as TcpClient;
            try
            {   // 执行任务
                if (OnThreadTaskRequest != null)
                {
                    OnThreadTaskRequest(tcpClient, EventArgs.Empty);
                }
            }

            catch
            {
                // 阻止异常抛出
            }

            finally
            {   // 关闭连接
                tcpClient.Close();

                // 从列表中移除连接
                _mutexClients.WaitOne();
                if (_tcpClients != null)
                {
                    _tcpClients.Remove(tcpClient);
                }
                _mutexClients.ReleaseMutex();
            }
        }

        /// <summary>
        /// 关闭侦听器
        /// </summary>
        /// <remarks>显示隐藏从基类继承的成员</remarks>
        public new void Stop()
        {   // 检测是否已开启侦听
            if (Active)
            {
                // 关闭侦听器
                base.Stop();

                // 关闭已建立的连接
                _mutexClients.WaitOne();
                if (_tcpClients != null)
                {
                    foreach (TcpClient client in _tcpClients)
                    {
                        client.Close();
                    }

                    // 清空连接列表
                    _tcpClients.Clear();
                    _tcpClients = null;
                }
                _mutexClients.ReleaseMutex();
            }
        }
    }
}
