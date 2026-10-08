using BNet.Cafe.Client.Services;
using Newtonsoft.Json;
using System;
using System.IO;
using System.Net;
using System.Text;

namespace BNet.Cafe.Client
{
    public class SessionLoginData
    {
        public DateTime CreatedTime { get; set; }
        public DateTime EndTime { get; set; }
        public string UserId { get; set; } = string.Empty;
        public double Amount { get; set; }
        public bool IsOpenTime { get; set; }
        public bool isAdmin { get; set; }
        public bool IsPaused { get; set; }
        public double RemainingSeconds { get; set; }
    }

    public static class SessionLogin
    {
        private static string GetFtpSessionUrl()
        {
            string ftpBasePath = ConfigHelper.FtpServerPath; // Returns ftp://192.168.1.2/
            if (string.IsNullOrEmpty(ftpBasePath)) return null;

            if (!ftpBasePath.EndsWith("/"))
            {
                ftpBasePath += "/";
            }

            string clientName = ConfigHelper.GetClientNameFromIP();
            return $"{ftpBasePath}{clientName}_Session.json";
        }

        public static void SaveSession(DateTime createdTime, DateTime endTime, string userId, double amount, bool isAdmin, bool isOpenTime, bool isPaused = false, double remainingSeconds = 0)
        {
            try
            {
                string ftpUrl = GetFtpSessionUrl();
                if (string.IsNullOrEmpty(ftpUrl)) return;

                var session = new SessionLoginData
                {
                    CreatedTime = createdTime,
                    EndTime = endTime,
                    UserId = userId,
                    Amount = amount,
                    IsOpenTime = isOpenTime,
                    isAdmin = isAdmin,
                    IsPaused = isPaused,
                    RemainingSeconds = remainingSeconds
                };

                string jsonContent = JsonConvert.SerializeObject(session, Formatting.Indented);
                byte[] fileContents = Encoding.UTF8.GetBytes(jsonContent);

                // Upload JSON payload directly to the FTP Server
                FtpWebRequest request = (FtpWebRequest)WebRequest.Create(ftpUrl);
                request.Method = WebRequestMethods.Ftp.UploadFile;
                request.UseBinary = true;
                request.KeepAlive = false;

                using (Stream requestStream = request.GetRequestStream())
                {
                    requestStream.Write(fileContents, 0, fileContents.Length);
                }

                using (FtpWebResponse response = (FtpWebResponse)request.GetResponse())
                {
                    // Successfully saved to FTP
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error saving session to FTP: {ex.Message}");
            }
        }

        public static SessionLoginData ReadSession()
        {
            string ftpUrl = GetFtpSessionUrl();
            if (string.IsNullOrEmpty(ftpUrl)) return null;

            try
            {
                // Download JSON directly from the FTP Server
                FtpWebRequest request = (FtpWebRequest)WebRequest.Create(ftpUrl);
                request.Method = WebRequestMethods.Ftp.DownloadFile;
                request.UseBinary = true;
                request.KeepAlive = false;

                using (FtpWebResponse response = (FtpWebResponse)request.GetResponse())
                using (Stream responseStream = response.GetResponseStream())
                using (StreamReader reader = new StreamReader(responseStream, Encoding.UTF8))
                {
                    string jsonContent = reader.ReadToEnd();
                    if (string.IsNullOrWhiteSpace(jsonContent)) return null;

                    return JsonConvert.DeserializeObject<SessionLoginData>(jsonContent);
                }
            }
            catch
            {
                // File does not exist on FTP server or connection failed
                return null;
            }
        }

        public static bool Exists()
        {
            return ReadSession() != null;
        }

        public static void ClearSession()
        {
            string ftpUrl = GetFtpSessionUrl();
            if (string.IsNullOrEmpty(ftpUrl)) return;

            try
            {
                // Delete JSON file from FTP Server on logout/timeout
                FtpWebRequest request = (FtpWebRequest)WebRequest.Create(ftpUrl);
                request.Method = WebRequestMethods.Ftp.DeleteFile;
                request.KeepAlive = false;

                using (FtpWebResponse response = (FtpWebResponse)request.GetResponse())
                {
                    // Successfully removed
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error deleting session from FTP: {ex.Message}");
            }
        }
    }
}