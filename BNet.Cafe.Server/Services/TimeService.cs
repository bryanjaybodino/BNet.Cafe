using BNet.Cafe.Server.Repositories;
using System;
using System.Collections.Concurrent;
using System.Data;
using System.Linq;
using System.Runtime.Caching;

namespace BNet.Cafe.Server.Services
{
    public class TimeService
    {
        public static string XTimeZoneArgs = "X-TimeZone";

        // Cache the TimeZoneInfo to avoid repeated OS lookups
        private static readonly ConcurrentDictionary<string, TimeZoneInfo> _timeZoneCache =
            new ConcurrentDictionary<string, TimeZoneInfo>(StringComparer.OrdinalIgnoreCase);

        // Use ObjectCache to cache the database config for a short period (e.g., 1 minute)
        private static readonly MemoryCache _cache = MemoryCache.Default;
        private const string CacheKey = "DBTimeZone_Config";

        public static DateTime Get()
        {
            string savedTimeZone = GetCachedTimeZoneId();

            TimeZoneInfo timeZone = _timeZoneCache.GetOrAdd(savedTimeZone, id =>
            {
                try
                {
                    return TimeZoneInfo.FindSystemTimeZoneById(id);
                }
                catch (Exception)
                {
                    return TimeZoneInfo.Utc;
                }
            });

            return TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, timeZone);
        }

        private static string GetCachedTimeZoneId()
        {
            if (_cache.Get(CacheKey) is string cachedId)
            {
                return cachedId;
            }

            string timeZoneId = TimeZoneInfo.Local.Id;

            try
            {
                ClientConfig clientConfigRepository = new ClientConfig();
                DataTable dt = clientConfigRepository.GetAll();

                if (dt != null && dt.Rows.Count > 0)
                {
                    DataRow row = dt.Rows[0];
                    if (row.Table.Columns.Contains("DBTimeZone"))
                    {
                        string dbTimeZone = row["DBTimeZone"]?.ToString();
                        if (!string.IsNullOrWhiteSpace(dbTimeZone))
                        {
                            // Validate it exists on the system before caching
                            var matchedZone = TimeZoneInfo.GetSystemTimeZones()
                                .FirstOrDefault(z => string.Equals(z.Id, dbTimeZone, StringComparison.OrdinalIgnoreCase));

                            if (matchedZone != null)
                            {
                                timeZoneId = matchedZone.Id;
                            }
                        }
                    }
                }
            }
            catch
            {
                // Fallback safely if DB call fails
            }

            // Cache the result for 1 month so changes reflect quickly without overloading the DB
            _cache.Set(CacheKey, timeZoneId, DateTimeOffset.Now.AddMonths(1));

            return timeZoneId;
        }
        public static void ClearTimeZoneCache()
        {
            MemoryCache cache = MemoryCache.Default;
            if (cache.Contains("DBTimeZone_Config"))
            {
                cache.Remove("DBTimeZone_Config");
            }
        }
    }
}