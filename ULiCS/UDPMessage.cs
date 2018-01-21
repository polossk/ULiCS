using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;

namespace polossk.Universal.Net
{
    public class UDPMessage
    {
        private volatile bool _shouldStopBroadcast;

        public void OnBroadcast(int threadID, int port, string ver)
        {
            UdpClient bcHost = new UdpClient(new IPEndPoint(IPAddress.Any, 0));
            IPEndPoint bcTarget = new IPEndPoint(IPAddress.Broadcast, port);
            byte[] buf = Encoding.Unicode.GetBytes(ver);
            _shouldStopBroadcast = false;
            while (!_shouldStopBroadcast)
            {
                DateTime now = DateTime.Now;
                bcHost.Send(buf, buf.Length, bcTarget);
                Thread.Sleep(500);
            }
        }

        public void RequestStopBroadcast()
        {
            _shouldStopBroadcast = true;
        }

        public IPAddress OnListenBroadcast(int threadID, int port, string ver)
        {
            IPAddress ret = IPAddress.Any;
            OnListenBroadcast(threadID, port, ver, out ret);
            return ret;
        }

        public void OnListenBroadcast(int threadID, int port, string ver, out IPAddress serverIP)
        {
            using (UdpClient client = new UdpClient(new IPEndPoint(IPAddress.Any, port)))
            {
                IPEndPoint endpoint = new IPEndPoint(IPAddress.Any, 0);
                int testCnt = 0;
                while (true)
                {
                    byte[] buf = client.Receive(ref endpoint);
                    string msg = Encoding.Unicode.GetString(buf);
                    if (msg != ver) continue;
                    else testCnt++;
                    DateTime now = DateTime.Now;
                    Console.WriteLine("{0} [client {1}]: receive message from [{2}:{3}]",
                        now, threadID, endpoint.Address.ToString(), endpoint.Port.ToString());
                    Console.WriteLine("{0} [client {1}]: receive message [{2}]",
                        now, threadID, msg);
                    if (testCnt == 5) { serverIP = endpoint.Address; break; }
                }
            }
            Console.WriteLine("{0} [client {1}]: Success deceted server IP.", DateTime.Now, threadID);
        }

    }
}
