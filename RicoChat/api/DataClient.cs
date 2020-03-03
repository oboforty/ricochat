using RicoChat.test;
using System;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Windows;


namespace RicoChat.api
{
    class DataClient
    {
        private readonly TcpClient server;
        private readonly IPEndPoint localEndPoint;

        private Thread tcpRecieveThread;

        public DataClient(string username, string serverAddress, int port)
        {
            server = new TcpClient(serverAddress, port);

            var bytes = Encoding.Unicode.GetBytes("connect||" + username);
            server.Client.Send(bytes);
        }
    }
}
