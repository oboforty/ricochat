using NAudio.Wave;


namespace RicoChat.api
{
    class VoicePlayback : IVoiceHandler
    {
        private WaveOut recievedStream;
        private BufferedWaveProvider waveProvider;
        public int OutputAudioDevice { get; set; }

        public const int BUFFER_SIZE = 5242880;
        public byte[] Buffer = new byte[BUFFER_SIZE];


        public VoicePlayback(int dev)
        {
            OutputAudioDevice = dev;
            
            // init:
            waveProvider = new BufferedWaveProvider(new WaveFormat(8000, 16, WaveIn.GetCapabilities(OutputAudioDevice).Channels));
            recievedStream = new WaveOut();
            recievedStream.Init(waveProvider);
        }

        public bool SendData(byte[] buffer, int bytesRead)
        {
            waveProvider.AddSamples(buffer, 0, bytesRead);

            //if (waveProvider.BufferedBytes >= 1200)
            //{
                recievedStream.Play();
            //}


            return true;
        }

        public void Dispose()
        {
            // todo later: play exit sound?

            recievedStream.Stop();
        }
    }
}
