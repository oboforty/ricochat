using NAudio.Wave;


namespace RicoChat.api
{
    class VoiceInput
    {
        private WaveInEvent sourceStream;

        public int InputAudioDevice { get; set; }

        private IVoiceHandler micHandler;

        public VoiceInput(IVoiceHandler c0, int devin)
        {
            micHandler = c0;
            InputAudioDevice = devin;

            sourceStream = new WaveInEvent
            {
                DeviceNumber = InputAudioDevice,
                WaveFormat = new WaveFormat(8000, 16, WaveIn.GetCapabilities(0).Channels),
                BufferMilliseconds = 30
            };

            sourceStream.DataAvailable += sourceStream_DataAvailable;
            sourceStream.StartRecording();
        }


        private void sourceStream_DataAvailable(object sender, WaveInEventArgs e)
        {
            if (sourceStream == null)
                return;

            // send voice packet to handler
            if (!micHandler.SendData(e.Buffer, e.BytesRecorded))
            {
                //sourceStream.StopRecording();
            }
        }

        public void Dispose()
        {
            sourceStream.StopRecording();
        }
    }
}
