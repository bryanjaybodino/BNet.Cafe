using BNet.Cafe.Client.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace BNet.Cafe.Client.Repositories
{
    public class TimeService
    {
        public static string XTimeZoneArgs = "X-TimeZone";
        public static DateTime Get()
        {
            string timeZoneId = ConfigHelper.TimeZone;

            // Perform a case-insensitive match against system time zones
            var matchedZone = TimeZoneInfo.GetSystemTimeZones()
                .FirstOrDefault(z => string.Equals(z.Id, timeZoneId, StringComparison.OrdinalIgnoreCase));

            // Use the correctly cased system ID if found, otherwise fall back to the original value
            string validId = matchedZone?.Id ?? timeZoneId;

            return TimeZoneInfo.ConvertTimeBySystemTimeZoneId(DateTime.UtcNow, validId);
        }
    }
}