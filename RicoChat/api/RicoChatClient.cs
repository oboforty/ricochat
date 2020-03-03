using System;
using System.Collections.Generic;
using System.Text;

namespace RicoChat.api
{
    class RicoChatClient
    {
        VoicePlayback m_HandOut;
        VoiceInput m_HandIn;

        VoiceClient m_VoiceClient;
        DataClient m_DataClient;

        public RicoChatClient()
        {

        }

        public Dictionary<int, string> ConnectToServer(string username, string address, int port = 9000)
        {
            // connect to TCP
            // m_DataClient = new DataClient(username, address, port+1);

            //var hand_save = new TestSoundSave();
            //var pp = new TestSoundIO(voice_client);


            // Right now the chat plays back what you say
            m_VoiceClient = new VoiceClient(address, port);
            if (m_VoiceClient.UdpAuthenticate(username))
            {
                return m_VoiceClient.Join("default");
            }

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

        internal void Dispose()
        {
            //m_DataClient.D
        }
    }
}
