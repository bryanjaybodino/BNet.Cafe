using System;
using System.IO;

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
        public bool IsPaused { get; set; } // Add IsPaused property
    }

    public static class SessionManager
    {
        private static readonly string SessionFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "session.txt");

        public static void SaveSession(DateTime createdTime, DateTime endTime, string userId, double amount, bool isOpenTime, bool isPaused = false)
        {
            try
            {
                string content = $"{createdTime.Ticks}|{endTime.Ticks}|{userId}|{amount}|{isOpenTime}|{isPaused}";
                File.WriteAllText(SessionFilePath, content);
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
                string[] parts = File.ReadAllText(SessionFilePath).Split('|');
                if (parts.Length >= 4 &&
                    long.TryParse(parts[0], out long createdTicks) &&
                    long.TryParse(parts[1], out long endTicks))
                {
                    string userId = parts[2];
                    double.TryParse(parts[3], out double amount);
                    bool isOpenTime = parts.Length >= 5 && bool.TryParse(parts[4], out bool parsed) && parsed;
                    bool isPaused = parts.Length >= 6 && bool.TryParse(parts[5], out bool p) && p;

                    return new SessionData
                    {
                        CreatedTime = new DateTime(createdTicks),
                        EndTime = new DateTime(endTicks),
                        UserId = userId,
                        Amount = amount,
                        IsOpenTime = isOpenTime,
                        IsAdministrator = (userId == "Administrator" && (amount == 0 || endTicks - createdTicks > TimeSpan.FromDays(1).Ticks)),
                        IsPaused = isPaused
                    };
                }
            }
            catch { }

            return null;
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