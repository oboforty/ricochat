using RicoChat.test;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;


namespace RicoChat.api
{
    class SignalClient
    {
        private readonly Socket clientSocket;
        private IPEndPoint remoteEndPoint;

        private Thread tcpReceiveThread;

        private string tcpSubscriber;
        private bool tcpConnectionActive;

        public SignalClient(string ip, int port)
        {
            clientSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            remoteEndPoint = new IPEndPoint(IPAddress.Parse(ip), port);
        }

        public bool Authenticate(string uid, string username, out int vcid)
        {
            // authenticate
            try
            {
                var ep = remoteEndPoint as EndPoint;
                byte[] recbytes = new byte[64];

                // Connect
                clientSocket.Connect(ep);
                clientSocket.ReceiveTimeout = 5000;

                // send SCON
                byte[] bytes = Encoding.ASCII.GetBytes("scon " + uid + " " + username);
                clientSocket.SendTo(bytes, 0, bytes.Length, SocketFlags.None, ep);

                // receive SOKE - validate authentication
                recbytes = new byte[64];
                var bytesRead = clientSocket.ReceiveFrom(recbytes, ref ep);
                //byte[] client_bytes = new byte[8];
                //Buffer.BlockCopy(recbytes, 0, client_bytes, 0, 8);
                //int client_id = BitConverter.ToInt32(client_bytes, 0);

                // parse rest of message:
                string authResp = Encoding.ASCII.GetString(recbytes, 0, bytesRead);
                if (authResp.StartsWith("soke"))
                {
                    vcid = int.Parse(authResp.Split(' ')[1]);

                    tcpConnectionActive = true;

                    clientSocket.ReceiveTimeout = 0;

                    return true;
                }
                else
                {
                    throw new Exception("Authentication error");
                }
            }
            catch (SocketException e)
            {
                ErrorLog.Write("AUTHENTICATE", e);
            }
            catch (Exception e)
            {
                ErrorLog.Write("AUTHENTICATE", e);
            }

            vcid = -1;
            return false;
        }

        public Dictionary<string, string> Join(string chname)
        {
            Dictionary<string, string> users = new Dictionary<string, string>();

            try
            {
                var ep = remoteEndPoint as EndPoint;
                byte[] bytes = Encoding.ASCII.GetBytes("join " + chname);
                clientSocket.SendTo(bytes, 0, bytes.Length, SocketFlags.None, ep);

                // get userlist
                byte[] recbytes = new byte[4096];
                clientSocket.ReceiveTimeout = 5000;

                // headers + client_id
                int bytesRead = clientSocket.ReceiveFrom(recbytes, ref ep);
                string users_string = Encoding.ASCII.GetString(recbytes, 0, bytesRead);

                foreach (string usr in users_string.Split('|'))
                {
                    var u = usr.Split(':');
                    users.Add(u[0], u[1]);
                }

                clientSocket.ReceiveTimeout = 0;
            }
            catch (SocketException e)
            {
                ErrorLog.Write("JOINCH", e);
                return null;
            }
            catch (Exception e)
            {
                ErrorLog.Write("JOINCH", e);
                return null;
            }

            return users;
        }
        
        public void StartReceiving(IVoiceHandler v0)
        {
            tcpReceiveThread = new Thread(ReceiveThread);
            tcpReceiveThread.Start();
        }

        public bool SendData(byte[] buffer, int bytesRecorded)
        {
            // send voice data from voice user to tcp server
            try
            {
                if (!tcpConnectionActive)
                {
                    return false;
                }
                var ep = remoteEndPoint as EndPoint;
                clientSocket.SendTo(buffer, 0, bytesRecorded, SocketFlags.None, ep);
            }
            catch (Exception e)
            {
                ErrorLog.Write("TCPSEND", e);
                return false;
            }

            return true;
        }

        private void ReceiveThread()
        {
            var state = new StateObject { WorkSocket = clientSocket };
            var ep = remoteEndPoint as EndPoint;

            try
            {
                while (tcpConnectionActive)
                {
                    var bytesRead = clientSocket.ReceiveFrom(state.Buffer, 0, StateObject.BUFFER_SIZE, SocketFlags.None, ref ep);

                    Console.WriteLine(bytesRead);
                }
            }
            catch (Exception e)
            {
                ErrorLog.Write("TCPRECEIVE", e);
                return;
            }
        }

        public void Dispose()
        {
            clientSocket.Close();
            tcpConnectionActive = false;

            //tcpReceiveThread.Abort();
        }

    }
}
