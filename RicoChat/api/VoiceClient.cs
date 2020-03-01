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

        public VoiceClient(string address, IVoiceHandler c0)
        {
            var splittedAddress = address.Split(':');
            var ip = splittedAddress[0];
            var port = splittedAddress[1];

            playback = c0;

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

            remoteEndPoint = new IPEndPoint(IPAddress.Parse(ip), Int32.Parse(port));

            // authenticate
            try
            {
                var ep = remoteEndPoint as EndPoint;
                byte[] bytes = Encoding.ASCII.GetBytes("vcon oboforty");
                clientSocket.SendTo(bytes, 0, bytes.Length, SocketFlags.None, ep);

                // validate authentication
                byte[] recbytes = new byte[64];
                clientSocket.ReceiveTimeout = 5000;
                var bytesRead = clientSocket.ReceiveFrom(recbytes, ref ep);


                // parse header bytes:
                byte[] client_bytes = new byte[8];
                Buffer.BlockCopy(recbytes, 0, client_bytes, 0, 8);
                int client_id = BitConverter.ToInt32(client_bytes, 0);

                // parse rest of message:
                string authResp = Encoding.ASCII.GetString(recbytes, 8, bytesRead - 8);
                if (authResp == "voke")
                {
                    udpConnectionActive = true;

                    clientSocket.ReceiveTimeout = 0;
                }
                else
                {
                    throw new Exception("Authentication error");
                }
            }
            catch (SocketException e)
            {
                ErrorLog.Write("AUTHENTICATE", e);
                return;
            }
            catch (Exception e)
            {
                ErrorLog.Write("AUTHENTICATE", e);
                return;
            }


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

                //byte[] bytes = Encoding.ASCII.GetBytes("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");
                //clientSocket.SendTo(bytes, 0, bytes.Length, SocketFlags.None, ep);

                //// temporal: random bytes
                //Byte[] b = new Byte[32];
                //rnd.NextBytes(b);
                //clientSocket.SendTo(b, 0, 1600, SocketFlags.None, ep);
                //Random rnd = new Random();

                //if (rnd.NextDouble() > 0.75)
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

                    // trim off first 8 bytes
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
