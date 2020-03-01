using NAudio.Wave;
using RicoChat.api;
using System;
using System.IO;
using System.Threading;

namespace RicoChat.test
{
    class TestSoundIO
    {
        private WaveInEvent sourceStream;

        private IVoiceHandler hand;

        public TestSoundIO(IVoiceHandler c0)
        {
            hand = c0;

            Thread tt = new Thread(TestThread);
            tt.Start();
        }

        private void TestThread()
        {
            int BufferMilliseconds = 20;
            var format = new WaveFormat(8000, 16, WaveIn.GetCapabilities(0).Channels);
            int chunksize = BufferMilliseconds * format.AverageBytesPerSecond / 1000;

            if (chunksize % format.BlockAlign != 0)
            {
                chunksize -= chunksize % format.BlockAlign;
            }

            using (WaveFileReader reader = new WaveFileReader("D:\\dev\\RicoChat\\RicoChat\\sfx\\trumpet.wav"))
            {
                byte[] bytesBuffer = new byte[reader.Length];
                int read = reader.Read(bytesBuffer, 0, bytesBuffer.Length);
                int j = 0;

                for (int i = 0; i < bytesBuffer.Length - chunksize; i += chunksize)
                {
                    byte[] b2 = new byte[chunksize];
                    Buffer.BlockCopy(bytesBuffer, i, b2, 0, chunksize);

                    hand.SendData(b2, chunksize);

                    Thread.Sleep(BufferMilliseconds);

                    //j++;
                    //if (j > 10)
                    //    break;
                }
            }
        }
    }
}
