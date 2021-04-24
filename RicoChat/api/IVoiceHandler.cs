

namespace RicoChat.api
{
    interface IVoiceHandler
    {
        bool SendData(byte[] buffer, int bytesRecorded);
        void Dispose();
    }
}
