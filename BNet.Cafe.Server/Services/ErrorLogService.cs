using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;

namespace BNet.Cafe.Server.Services
{
    public class ErrorLogService
    {
        private static readonly object _lock = new object();

        public static void Save(string message)
        {
            string path = @"C:\BNet.Cafe\ErrorLog.txt";
            try
            {
                Directory.CreateDirectory(Path.GetDirectoryName(path));

                string logMessage = $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] {message}{Environment.NewLine}";

                lock (_lock)
                {
                    string existing = File.Exists(path) ? File.ReadAllText(path) : "";
                    File.WriteAllText(path, logMessage + existing);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Logging failed: " + ex.Message);
            }
        }

    }
}