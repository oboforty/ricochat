using NAudio.Wave;
using RicoChat.api;
using System;

namespace RicoChat.test
{
    class TestSoundSave : IVoiceHandler
    {
        WaveFileWriter writer;
        WaveFormat format;

        public TestSoundSave()
        {
            byte[] testSequence = new byte[] { 0x1, 0x2, 0xFF, 0xFE };
            format = new WaveFormat(8000, 16, WaveIn.GetCapabilities(0).Channels);

            writer = new WaveFileWriter("D:\\dev\\RicoChat\\RicoChat\\sfx\\save.wav", format);
        }

        public bool SendData(byte[] buffer, int bytesRecorded)
        {
            writer.Write(buffer, 0, bytesRecorded);

            return true;
        }

        public void Close()
        {
            writer.Close();
            //writer.Dispose();
        }

        public void Dispose()
        {
            Close();
        }
    }
}
