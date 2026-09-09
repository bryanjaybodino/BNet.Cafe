using System.Runtime.InteropServices;

namespace BNet.Cafe.Client
{
    internal static class NativeMethods
    {
        [DllImport("shcore.dll", SetLastError = true)]
        public static extern bool SetProcessDpiAwareness(int awareness);
    }
}