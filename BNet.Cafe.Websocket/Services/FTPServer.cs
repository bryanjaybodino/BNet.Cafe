using System;
using System.IO;
using System.Threading.Tasks;

namespace BNet.Cafe.Websocket.Services
{
    internal class FTPServer
    {
        public static Task StartAsync()
        {
            // Define the local directory path for printing documents
            string rootFolder = Path.Combine(
               Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
               "For Printing"
            );

            // Ensure the folder exists on disk before starting the FTP server
            if (!Directory.Exists(rootFolder))
            {
                Directory.CreateDirectory(rootFolder);
            }

            BNet.FTPServer.Commands commands = new BNet.FTPServer.Commands();
            commands.Setup(rootFolder, 21);
            return commands.StartAsync();
        }
    }
}