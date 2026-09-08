using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace BNet.Cafe.Client.Repositories
{
    public class TimeService
    {
        private const string DefaultTimeZone = "Taipei Standard Time";
        public static string XTimeZoneArgs = "X-TimeZone";
        public static DateTime Get()
        {
            return TimeZoneInfo.ConvertTimeBySystemTimeZoneId(DateTime.UtcNow, DefaultTimeZone);
        }
    }
}