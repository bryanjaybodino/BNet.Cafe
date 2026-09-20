using BNet.Cafe.Client.Services;
using Newtonsoft.Json;
using System;
using System.IO;

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
        // Safe local directory for diskless write access across all Windows users
        private static readonly string SessionDirectory = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData),
            "BNetCafe"
        );

        //C:\ProgramData\BNetCafe
        private static readonly string SessionFilePath = Path.Combine(SessionDirectory, $"{ConfigHelper.GetClientNameFromIP()+"_Session"}.json");

        public static void SaveSession(DateTime createdTime, DateTime endTime, string userId, double amount, bool isAdmin, bool isOpenTime, bool isPaused = false, double remainingSeconds = 0)
        {
            try
            {
                // Ensure target directory exists before saving file
                if (!Directory.Exists(SessionDirectory))
                {
                    Directory.CreateDirectory(SessionDirectory);
                }

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
                File.WriteAllText(SessionFilePath, jsonContent);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error saving session: {ex.Message}");
            }
        }

        public static SessionLoginData ReadSession()
        {
            if (!File.Exists(SessionFilePath)) return null;

            try
            {
                string jsonContent = File.ReadAllText(SessionFilePath);
                return JsonConvert.DeserializeObject<SessionLoginData>(jsonContent);
            }
            catch
            {
                return null;
            }
        }

        public static bool Exists() => File.Exists(SessionFilePath);

        public static void ClearSession()
        {
            if (File.Exists(SessionFilePath))
            {
                try { File.Delete(SessionFilePath); } catch { }
            }
        }
    }
}