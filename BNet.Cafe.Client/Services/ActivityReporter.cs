using BNet.Cafe.Client.Models;
using BNet.Cafe.Client.Repositories;
using BNet.Cafe.Client.Services;
using Newtonsoft.Json;
using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace BNet.Cafe.Client
{
    public class ActivityReporter
    {
        private static readonly UTF8Encoding Utf8 = new UTF8Encoding(false);
        private string _lastSentTitle = string.Empty;

        public void ClearTitleCache()
        {
            _lastSentTitle = string.Empty;
        }

        public async Task SendActivityInfoAsync(AgentSession session, UserActivity.ActiveWindowInfo info, TextContent deviceInfo)
        {
            if (session == null || !session.IsOpen || info == null) return;
            if (info.WindowTitle == _lastSentTitle) return;

            _lastSentTitle = info.WindowTitle;
            string timeStart = string.Empty;
            string timeEnd = string.Empty;

            string sessionFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "session.txt");
            if (File.Exists(sessionFilePath))
            {
                try
                {
                    string content = File.ReadAllText(sessionFilePath);
                    string[] parts = content.Split('|');

                    if (parts.Length >= 4
                        && long.TryParse(parts[0], out long createdTicks)
                        && long.TryParse(parts[1], out long endTicks))
                    {
                        timeStart = new DateTime(createdTicks).ToString("yyyy-MM-dd HH:mm:ss");
                        timeEnd = new DateTime(endTicks).ToString("yyyy-MM-dd HH:mm:ss");
                    }
                }
                catch { }
            }

            var payloadObj = new
            {
                clientName = deviceInfo.ClientName,
                appName = info.AppName,
                processName = info.ProcessName,
                windowTitle = info.WindowTitle,
                url = info.Url,
                isBrowser = info.IsBrowser,
                capturedAt = info.CapturedAt.ToString("o"),
                timeStart = timeStart,
                timeEnd = timeEnd,

                windows = deviceInfo.Windows,
                windowsVersion = deviceInfo.WindowsVersion,
                osArchitecture = deviceInfo.OSArchitecture,
                serialNumber = deviceInfo.SerialNumber,
                machineName = deviceInfo.MachineName,
                workGroup = deviceInfo.WorkGroup,
                osVersion = deviceInfo.OSVersion,
                processorCount = deviceInfo.ProcessorCount,
                screenCount = ScreenCaptured.GetScreenCount().ToString(),
                IPAddress = NetworkUtility.GetLocalIPAddress()
            };

            string json = JsonConvert.SerializeObject(payloadObj);
            byte[] jsonBytes = Utf8.GetBytes(json);
            byte[] frame = new byte[1 + jsonBytes.Length];
            frame[0] = 0x03;
            Buffer.BlockCopy(jsonBytes, 0, frame, 1, jsonBytes.Length);
            await session.SendAsync(frame, System.Net.WebSockets.WebSocketMessageType.Binary);
        }
    }
}