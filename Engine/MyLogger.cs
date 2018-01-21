using System;
using System.IO;
using System.Text;

namespace SPA.Engine
{
    public class MyLogger : ILogger
    {
        public void Log(string message)
        {
            //throw new NotImplementedException();
            string filePath = @"loglog.log";

            StringBuilder sb = new StringBuilder();
            sb.AppendLine($"{DateTime.UtcNow} - {message}");

            File.AppendAllTextAsync(filePath, sb.ToString());
        }
    }
}
