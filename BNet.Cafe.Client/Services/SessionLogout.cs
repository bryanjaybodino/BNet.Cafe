using BNet.Cafe.Client.Ashx;
using Newtonsoft.Json;
using System;
using System.IO;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace BNet.Cafe.Client.Services
{
    public class SessionLogoutData
    {
        public string UserId { get; set; }
        public string MinutesUsed { get; set; }
        public string Amount { get; set; }
        public string ActionType { get; set; }
        public DateTime Timestamp { get; set; }
    }

    public static class SessionLogout
    {
        private static string GetFtpLogoutUrl()
        {
            string ftpBasePath = ConfigHelper.FtpServerPath; // ftp://192.168.1.2/
            if (string.IsNullOrEmpty(ftpBasePath)) return null;

            if (!ftpBasePath.EndsWith("/"))
            {
                ftpBasePath += "/";
            }

            string clientName = ConfigHelper.GetClientNameFromIP();
            return $"{ftpBasePath}{clientName}_Logout.json";
        }

        public static bool HasPendingLogout()
        {
            return ReadPendingLogout() != null;
        }

        public static void SavePendingLogout(string userId, string minutesUsed, string amount, string actionType)
        {
            try
            {
                string ftpUrl = GetFtpLogoutUrl();
                if (string.IsNullOrEmpty(ftpUrl)) return;

                var data = new SessionLogoutData
                {
                    UserId = userId,
                    MinutesUsed = minutesUsed,
                    Amount = amount,
                    ActionType = actionType,
                    Timestamp = DateTime.Now
                };

                string json = JsonConvert.SerializeObject(data, Formatting.Indented);
                byte[] fileContents = Encoding.UTF8.GetBytes(json);

                FtpWebRequest request = (FtpWebRequest)WebRequest.Create(ftpUrl);
                request.Method = WebRequestMethods.Ftp.UploadFile;
                request.UseBinary = true;
                request.KeepAlive = false;

                using (Stream requestStream = request.GetRequestStream())
                {
                    requestStream.Write(fileContents, 0, fileContents.Length);
                }

                using (FtpWebResponse response = (FtpWebResponse)request.GetResponse()) { }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error saving pending logout to FTP: {ex.Message}");
            }
        }

        public static SessionLogoutData ReadPendingLogout()
        {
            string ftpUrl = GetFtpLogoutUrl();
            if (string.IsNullOrEmpty(ftpUrl)) return null;

            try
            {
                FtpWebRequest request = (FtpWebRequest)WebRequest.Create(ftpUrl);
                request.Method = WebRequestMethods.Ftp.DownloadFile;
                request.UseBinary = true;
                request.KeepAlive = false;

                using (FtpWebResponse response = (FtpWebResponse)request.GetResponse())
                using (Stream responseStream = response.GetResponseStream())
                using (StreamReader reader = new StreamReader(responseStream, Encoding.UTF8))
                {
                    string json = reader.ReadToEnd();
                    if (string.IsNullOrWhiteSpace(json)) return null;
                    return JsonConvert.DeserializeObject<SessionLogoutData>(json);
                }
            }
            catch
            {
                return null;
            }
        }

        public static void ClearPendingLogout()
        {
            string ftpUrl = GetFtpLogoutUrl();
            if (string.IsNullOrEmpty(ftpUrl)) return;

            try
            {
                FtpWebRequest request = (FtpWebRequest)WebRequest.Create(ftpUrl);
                request.Method = WebRequestMethods.Ftp.DeleteFile;
                request.KeepAlive = false;

                using (FtpWebResponse response = (FtpWebResponse)request.GetResponse()) { }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error clearing pending logout from FTP: {ex.Message}");
            }
        }

        public static async Task<bool> ProcessPendingLogoutAsync()
        {
            var pendingData = ReadPendingLogout();
            if (pendingData == null) return true;

            try
            {
                var createBalanceHandler = new CreateBalanceHandler();

                var response = await createBalanceHandler.CreateBalanceAsync(
                    pendingData.UserId,
                    pendingData.MinutesUsed,
                    pendingData.Amount,
                    pendingData.ActionType
                );

                if (response != null)
                {
                    ClearPendingLogout();
                    return true;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Server unavailable or error syncing logout: {ex.Message}");
            }

            return false;
        }
    }
}