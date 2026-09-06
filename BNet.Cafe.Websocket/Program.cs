using System;
using System.Threading;

namespace BNet.Cafe.Websocket
{
    internal static class Program
    {
        private static readonly AutoResetEvent KeepAliveEvent =
            new AutoResetEvent(false);

        static void Main(string[] args)
        {
            try
            {
                Setup server = new Setup();
                server.StartWebsocket();

                // Keep process alive
                KeepAliveEvent.WaitOne();
            }
            catch (Exception ex)
            {
                // Log the exception somewhere instead of showing a console.
            }
        }
    }
}
