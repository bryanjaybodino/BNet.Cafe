using BNet.Cafe.Client.Models;
using BNet.Cafe.Client.Repositories;
using System;
using System.Configuration;
using System.Linq;
using System.Management;
using System.Threading.Tasks;

namespace BNet.Cafe.Client
{
    public static class DeviceInfoCollector
    {
        public static async Task<TextContent> GatherDeviceInfoAsync()
        {
            string caption = "", version = "", arch = "", serial = "";
            try
            {
                var wmi = new ManagementObjectSearcher("select * from Win32_OperatingSystem")
                    .Get().Cast<ManagementObject>().First();
                caption = ((string)wmi["Caption"])?.Trim() ?? "";
                version = (string)wmi["Version"] ?? "";
                arch = (string)wmi["OSArchitecture"] ?? "";
                serial = (string)wmi["SerialNumber"] ?? "";
            }
            catch { }

            await Task.CompletedTask;
            return new TextContent
            {
                Windows = caption,
                WindowsVersion = version,
                OSArchitecture = arch,
                SerialNumber = serial,
                MachineName = Environment.MachineName,
                ClientName = (ConfigurationManager.AppSettings["ClientName"] ?? "").ToUpper().Replace(" ", ""),
                WorkGroup = Environment.UserDomainName,
                OSVersion = Environment.OSVersion.VersionString,
                ProcessorCount = Environment.ProcessorCount.ToString(),
                ScreenCount = ScreenCaptured.GetScreenCount().ToString(),
            };
        }
    }
}