using System.Collections.Generic;


namespace RicoChat.api
{
    class RicoChatClient
    {
        VoicePlayback m_HandOut;
        VoiceInput m_HandIn;

        VoiceClient m_VoiceClient;
        SignalClient m_DataClient;

        public RicoChatClient()
        {

        }

        public Dictionary<string, string> ConnectToServer(string uid, string username, string address, int port = 9000)
        {
            // connect to TCP
            m_DataClient = new SignalClient(address, port + 1);
            if (m_DataClient.Authenticate(uid, username, out int vcid))
            {
                var resp = m_DataClient.Join("default");

                m_VoiceClient = new VoiceClient(address, port);
                if (m_VoiceClient.UdpAuthenticate(vcid, uid))
                    return resp;
            }

            //var hand_save = new TestSoundSave();
            //var pp = new TestSoundIO(voice_client);

            // couldn't connect
            return null;
        }

        public void StartVoiceIO(int InputAudioDevice, int OutputAudioDevice)
        {
            m_HandOut = new VoicePlayback(OutputAudioDevice);
            m_HandIn = new VoiceInput(m_VoiceClient, InputAudioDevice);

            m_VoiceClient.StartReceiving(m_HandOut);
        }

        public bool JoinChannel(string chname)
        {

            return true;
        }

        public void SetDevices(int devin, int devout)
        {

        }

        public void Dispose()
        {
            
            // @TODO: quit from VC & signal
            //m_DataClient.D
        }
    }
}
