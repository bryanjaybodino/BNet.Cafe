using BNet.Cafe.Client.Repositories;
using BNet.Cafe.Client.Services;
using System;

namespace BNet.Cafe.Client
{
    public static class DisplayFormatter
    {
        public static string FormatPurchasedTime(double totalSeconds)
        {
            TimeSpan time = TimeSpan.FromSeconds(Math.Max(0, totalSeconds));
            int totalHours = (int)time.TotalHours;
            int minutes = time.Minutes;

            string hourText = totalHours == 1 ? "1hr" : $"{totalHours}hrs";
            string minText = minutes == 1 ? "1min" : $"{minutes}mins";

            if (totalHours < 1) return minText;
            return minutes > 0 ? $"{hourText} | {minText}" : hourText;
        }

        public static string FormatTimeoutDisplay(DateTime targetEndTime)
        {
            if (targetEndTime.Date > TimeService.Get().Date)
            {
                return $"Timeout : {targetEndTime:hh:mm tt (MMM dd)}";
            }
            return $"Timeout : {targetEndTime:hh:mm tt}";
        }
    }
}