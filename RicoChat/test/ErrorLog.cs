using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace RicoChat.test
{
    class ErrorLog
    {
        public static void Write(string label, Exception exception)
        {
            string logfile = String.Empty;
            try
            {
                File.AppendAllText("log.txt", label+ " -- "+ exception.Message);
            }
            catch (Exception e)
            {
                throw;
            }
        }

    }
}
