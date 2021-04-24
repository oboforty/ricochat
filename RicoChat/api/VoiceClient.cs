using RicoChat.test;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;


namespace RicoChat.api
{
    public class StateObject
    {
        // Client  socket.
        public Socket WorkSocket = null;
        // Size of receive buffer.
        public const int BUFFER_SIZE = 5242880;
        // Receive buffer.
        public byte[] Buffer = new byte[BUFFER_SIZE];
        // Received data string.
        public StringBuilder Sb = new StringBuilder();
    }

    class VoiceClient : IVoiceHandler
    {
        private readonly Socket clientSocket;
        private IPEndPoint remoteEndPoint;

        private Thread udpReceiveThread;

        private string udpSubscriber;
        private bool udpConnectionActive;

        public string ClientAddress { get; set; }

        public string ServerAddress { get; set; }
        public string ServerName { get; set; }

        private IVoiceHandler playback;

        private bool _AllowUdpThread = true;

        public VoiceClient(string ip, int port)
        {
            clientSocket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
            //clientSocket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);

            // Get local endpoint
            //var host = Dns.GetHostEntry(Dns.GetHostName());
            //var ipAddress = host.AddressList.FirstOrDefault(ip => ip.AddressFamily == AddressFamily.InterNetwork);
            //if (ipAddress != null)
            //{
            //    var random = new Random();
            //    var endPoint = new IPEndPoint(ipAddress, random.Next(65000, 65536));
            //    ClientAddress = $"{endPoint.Address}:{endPoint.Port}";
            //    //localEndPoint = endPoint;
            //}

            remoteEndPoint = new IPEndPoint(IPAddress.Parse(ip), port);

        }

        public bool UdpAuthenticate(int vcid, string uid)
        {
            // authenticate
            try
            {
                var ep = remoteEndPoint as EndPoint;
                byte[] bytes = Encoding.ASCII.GetBytes($"vcon {vcid} {uid}");
                clientSocket.SendTo(bytes, 0, bytes.Length, SocketFlags.None, ep);

                // validate authentication
                byte[] recbytes = new byte[64];
                clientSocket.ReceiveTimeout = 5000;
                var bytesRead = clientSocket.ReceiveFrom(recbytes, ref ep);

                // parse rest of message:
                string authResp = Encoding.ASCII.GetString(recbytes, 0, bytesRead);
                if (authResp == "voke")
                {
                    udpConnectionActive = true;

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

            return false;
        }


        public void StartReceiving(IVoiceHandler v0)
        {
            playback = v0;

            // Start voice connection
            // todo: binding causes error: The requested address is not valid in its context
            //if (!clientSocket.IsBound)
            //    clientSocket.Bind(localEndPoint);

            udpReceiveThread = new Thread(ReceiveThread);
            udpReceiveThread.Start();
        }

        public bool SendData(byte[] buffer, int bytesRecorded)
        {
            // send voice data from voice user to udp server
            try
            {
                if (!udpConnectionActive)
                {
                    return false;
                }
                var ep = remoteEndPoint as EndPoint;
                clientSocket.SendTo(buffer, 0, bytesRecorded, SocketFlags.None, ep);
            }
            catch (Exception e)
            {
                ErrorLog.Write("VOICESEND", e);
                return false;
            }

            return true;
        }

        private void ReceiveThread()
        {
            var state = new StateObject { WorkSocket = clientSocket };
            var ep = remoteEndPoint as EndPoint;
            //var ep = clientSocket.LocalEndPoint as EndPoint;

            try
            {
                while (_AllowUdpThread && udpConnectionActive)
                {
                    var bytesRead = clientSocket.ReceiveFrom(state.Buffer, 0, StateObject.BUFFER_SIZE, SocketFlags.None, ref ep);

                    // trim off first 8 bytes (7 vcid & 1 qos)
                    int L = bytesRead - 8;
                    byte[] audio_data = new byte[L];
                    Buffer.BlockCopy(state.Buffer, 8, audio_data, 0, L);

                    playback.SendData(audio_data, L);
                }
            } catch (Exception e)
            {
                ErrorLog.Write("RECEIVE", e);
                return;
            }

            // old code:
            //try
            //{
            //    // start receiving
            //    var state = new StateObject { WorkSocket = clientSocket };
            //    var ep = remoteEndPoint as EndPoint;
            //    clientSocket.BeginReceiveFrom(state.Buffer, 0, StateObject.BUFFER_SIZE, SocketFlags.None, ref ep, OnRecieve, state);
            //}
            //catch (Exception e)
            //{
            //    udpConnectionActive = false;
            //}
        }

        public void Dispose()
        {
            _AllowUdpThread = false;
            clientSocket.Close();
            udpConnectionActive = false;

            //udpReceiveThread.Abort();
        }

        //public void OnRecieve(IAsyncResult ar)
        //{
        //    var state = ar.AsyncState as StateObject;
        //    if (state == null)
        //        return;

        //    try
        //    {
        //        // playback sound
        //        var bytesRead = clientSocket.EndReceive(ar);
        //        playback.SendData(state.Buffer, bytesRead);

        //        // receive new data
        //        var ep = (EndPoint)localEndPoint;
        //        if (udpConnectionActive)
        //            clientSocket.BeginReceiveFrom(state.Buffer, 0, StateObject.BUFFER_SIZE, SocketFlags.None, ref ep, OnRecieve, state);
        //    }
        //    catch (Exception e)
        //    {
        //        Console.WriteLine("GRRR");
        //        udpConnectionActive = false;
        //    }
        //}

    }
}
