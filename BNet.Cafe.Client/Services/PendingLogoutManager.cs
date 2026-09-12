using BNet.Cafe.Client.Ashx;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BNet.Cafe.Client.Services
{

    public class PendingLogoutData
    {
        public string UserId { get; set; }
        public string MinutesUsed { get; set; }
        public string Amount { get; set; }
        public string ActionType { get; set; }
        public DateTime Timestamp { get; set; }
    }

    public static class PendingLogoutManager
    {
        private static readonly string PendingFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "pending_logout.json");

        // First: Check if the file exists
        public static bool HasPendingLogout()
        {
            return File.Exists(PendingFilePath);
        }

        // Save pending logout data to local JSON file
        public static void SavePendingLogout(string userId, string minutesUsed, string amount, string actionType)
        {
            try
            {
                var data = new PendingLogoutData
                {
                    UserId = userId,
                    MinutesUsed = minutesUsed,
                    Amount = amount,
                    ActionType = actionType,
                    Timestamp = DateTime.Now
                };

                string json = JsonConvert.SerializeObject(data, Formatting.Indented);
                File.WriteAllText(PendingFilePath, json);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error saving pending logout: {ex.Message}");
            }
        }

        // Read pending data
        public static PendingLogoutData ReadPendingLogout()
        {
            if (!HasPendingLogout()) return null;

            try
            {
                string json = File.ReadAllText(PendingFilePath);
                return JsonConvert.DeserializeObject<PendingLogoutData>(json);
            }
            catch
            {
                return null;
            }
        }

        // Fourth: Remove the notepad/file
        public static void ClearPendingLogout()
        {
            if (File.Exists(PendingFilePath))
            {
                try { File.Delete(PendingFilePath); } catch { }
            }
        }

        // Second & Third: Check server connection and sync save to DB
        public static async Task<bool> ProcessPendingLogoutAsync()
        {
            if (!HasPendingLogout()) return true;

            var pendingData = ReadPendingLogout();
            if (pendingData == null)
            {
                ClearPendingLogout();
                return true;
            }

            try
            {
                var createBalanceHandler = new CreateBalanceHandler();

                // Attempt to send balance to the server database
                var response = await createBalanceHandler.CreateBalanceAsync(
                    pendingData.UserId,
                    pendingData.MinutesUsed,
                    pendingData.Amount,
                    pendingData.ActionType
                );

                // Assuming server response returns success or non-null when online
                if (response != null)
                {
                    // Fourth step: remove notepad/file upon successful save
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