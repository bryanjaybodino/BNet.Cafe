using BNet.Cafe.Client.Services;
using System;
using System.Collections.Concurrent;

namespace BNet.Cafe.Client.Repositories
{
    public class TimeService
    {
        public static string XTimeZoneArgs = "X-TimeZone";

        // Cache valid TimeZoneInfo objects to avoid repeated OS lookups
        private static readonly ConcurrentDictionary<string, TimeZoneInfo> _timeZoneCache =
            new ConcurrentDictionary<string, TimeZoneInfo>(StringComparer.OrdinalIgnoreCase);

        public static DateTime Get()
        {
            string timeZoneId = ConfigHelper.TimeZone;

            // Retrieve from cache or resolve once
            TimeZoneInfo timeZone = _timeZoneCache.GetOrAdd(timeZoneId, id =>
            {
                try
                {
                    return TimeZoneInfo.FindSystemTimeZoneById(id);
                }
                catch (TimeZoneNotFoundException)
                {
                    // Fallback to UTC if the configured zone is invalid
                    return TimeZoneInfo.Utc;
                }
                catch (InvalidTimeZoneException)
                {
                    return TimeZoneInfo.Utc;
                }
            });

            return TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, timeZone);
        }
    }
}