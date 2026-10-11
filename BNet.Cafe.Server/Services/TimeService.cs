using BNet.Cafe.Server.Repositories;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web;

namespace BNet.Cafe.Server.Services
{
    public class TimeService
    {

        public static string XTimeZoneArgs = "X-TimeZone";
        public static DateTime Get()
        {
            ClientConfig clientConfigRepository = new ClientConfig();
            DataTable dt = clientConfigRepository.GetAll();

            // Default to local timezone if not found or empty
            string savedTimeZone = TimeZoneInfo.Local.Id;

            if (dt != null && dt.Rows.Count > 0)
            {
                DataRow row = dt.Rows[0];
                if (row.Table.Columns.Contains("DBTimeZone"))
                {
                    string dbTimeZone = row["DBTimeZone"]?.ToString();
                    if (!string.IsNullOrWhiteSpace(dbTimeZone))
                    {
                        // Case-insensitive search against available system time zones
                        var matchedZone = TimeZoneInfo.GetSystemTimeZones()
                            .FirstOrDefault(z => string.Equals(z.Id, dbTimeZone, StringComparison.OrdinalIgnoreCase));

                        if (matchedZone != null)
                        {
                            savedTimeZone = matchedZone.Id;
                        }
                    }
                }
            }

            return TimeZoneInfo.ConvertTimeBySystemTimeZoneId(DateTime.UtcNow, savedTimeZone);
        }
    }
}