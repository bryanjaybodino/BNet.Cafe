using System;
using System.IO;
using Newtonsoft.Json;

namespace BNet.Cafe.Client
{
    public class SessionData
    {
        public DateTime CreatedTime { get; set; }
        public DateTime EndTime { get; set; }
        public string UserId { get; set; } = string.Empty;
        public double Amount { get; set; }
        public bool IsOpenTime { get; set; }
        public bool IsAdministrator { get; set; }
        public bool IsPaused { get; set; }
        public double RemainingSeconds { get; set; }
    }

    public static class SessionManager
    {
        private static readonly string SessionFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "session.json");

        public static void SaveSession(DateTime createdTime, DateTime endTime, string userId, double amount, bool isOpenTime, bool isPaused = false, double remainingSeconds = 0)
        {
            try
            {
                var session = new SessionData
                {
                    CreatedTime = createdTime,
                    EndTime = endTime,
                    UserId = userId,
                    Amount = amount,
                    IsOpenTime = isOpenTime,
                    IsAdministrator = (userId == "Administrator" && (amount == 0 || (endTime - createdTime).TotalDays > 1)),
                    IsPaused = isPaused,
                    RemainingSeconds = remainingSeconds
                };

                // Formatting.Indented creates clear, readable line breaks in Notepad
                string jsonContent = JsonConvert.SerializeObject(session, Formatting.Indented);
                File.WriteAllText(SessionFilePath, jsonContent);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error saving session: {ex.Message}");
            }
        }

        public static SessionData ReadSession()
        {
            if (!File.Exists(SessionFilePath)) return null;

            try
            {
                string jsonContent = File.ReadAllText(SessionFilePath);
                return JsonConvert.DeserializeObject<SessionData>(jsonContent);
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