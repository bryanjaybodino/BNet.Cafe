using System;

namespace BNet.Cafe.Client
{
    public static class EnvironmentHelper
    {
        public static bool IsDevelopment
        {
            get
            {
                string exePath = AppDomain.CurrentDomain.BaseDirectory.ToLower();
                return exePath.Contains(@"\bin");
            }
        }
    }
}