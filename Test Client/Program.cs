using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Net;
using System.Net.Sockets;


namespace Test_Client
{
    class Program
    {


        static void OlD_Main(string[] args)
        {
            Socket clientSocket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);

            var ip = "80.217.114.215";
            var port = 9001;
            var remoteEndPoint = new IPEndPoint(IPAddress.Parse(ip), port);

            var ep = remoteEndPoint as EndPoint;
            byte[] bytes = Encoding.ASCII.GetBytes("vcon oboforty");
            clientSocket.SendTo(bytes, 0, bytes.Length, SocketFlags.None, ep);

            // validate authentication
            byte[] recbytes = new byte[64];
            var bytesRead = clientSocket.ReceiveFrom(recbytes, ref ep);

            // parse header bytes:
            byte[] client_bytes = new byte[8];
            Buffer.BlockCopy(recbytes, 0, client_bytes, 0, 8);
            int client_id = BitConverter.ToInt32(client_bytes, 0);

            Console.WriteLine(client_id);

            // parse rest of message:
            string authResp = Encoding.ASCII.GetString(recbytes, 8, bytesRead-8);
            if (authResp != "voke")
            {
                throw new Exception("Authentication error");
            }
            else
            {
                Console.WriteLine("OK");
            }

            //byte[] bytes2 = Encoding.ASCII.GetBytes("LORD JESUS");
            //clientSocket.SendTo(bytes2, 0, bytes2.Length, SocketFlags.None, ep);
            //var br = clientSocket.ReceiveFrom(recbytes, ref ep);
            //Console.WriteLine(Encoding.ASCII.GetString(recbytes, 0, br));

            //byte[] bytes3 = Encoding.ASCII.GetBytes("LORD JESUS");
            //clientSocket.SendTo(bytes3, 0, bytes3.Length, SocketFlags.None, ep);
            //var br2 = clientSocket.ReceiveFrom(recbytes, ref ep);
            //Console.WriteLine(Encoding.ASCII.GetString(recbytes, 0, br2));

        }
    }
}
