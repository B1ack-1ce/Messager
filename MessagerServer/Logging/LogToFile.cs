using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MessagerServer.Logging
{
    public class LogToFile
    {
        private static object _lock = new object();
        public static void WriteToFile(string content)
        {
            var path = Path.Combine(Directory.GetCurrentDirectory(), "log.txt");

            lock (_lock)
            {
                File.AppendAllText(path, $"{content}\n\n");
            }
        }
    }
}
