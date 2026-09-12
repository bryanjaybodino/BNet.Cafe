using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace BNet.Cafe.Server.Ashx
{
    /// <summary>
    /// Summary description for CalculateDurationFromAmount
    /// </summary>
    public class CalculateDurationFromAmount : IHttpHandler
    {

        public void ProcessRequest(HttpContext context)
        {
            context.Response.ContentType = "application/json";

            // Parse currency amount from QueryString (e.g. ?amount=25) or POST body
            decimal amount = 0m;
            decimal.TryParse(context.Request["amount"], out amount);

            int totalMinutes = CalculateDuration(amount);
            string formattedTime = FormatMinutesToHours(totalMinutes);

            var responsePayload = new
            {
                amount = amount,
                formattedAmount = $"₱ {amount:F2}",
                totalMinutes = totalMinutes,
                formattedTime = formattedTime
            };

            context.Response.Write(JsonConvert.SerializeObject(responsePayload));
        }

        public static int CalculateDuration(decimal amount)
        {
            if (amount <= 0) return 0;

            if (amount <= 5) return (int)Math.Round((amount / 5m) * 15m);
            if (amount <= 10) return 15 + (int)Math.Round(((amount - 5m) / 5m) * 15m);
            if (amount <= 15) return 30 + (int)Math.Round(((amount - 10m) / 5m) * 30m);
            if (amount <= 25) return 60 + (int)Math.Round(((amount - 15m) / 10m) * 60m);
            if (amount <= 40) return 120 + (int)Math.Round(((amount - 25m) / 15m) * 60m);
            if (amount <= 50) return 180 + (int)Math.Round(((amount - 40m) / 10m) * 60m);

            decimal extraAmount = amount - 50m;
            int extraHoursInMins = (int)Math.Round((extraAmount / 10m) * 60m);
            return 240 + extraHoursInMins;
        }

        private string FormatMinutesToHours(int totalMinutes)
        {
            if (totalMinutes <= 0) return "0 hrs 0 mins";

            int hours = totalMinutes / 60;
            int minutes = totalMinutes % 60;

            string hrLabel = hours == 1 ? "hr" : "hrs";
            string minLabel = minutes == 1 ? "min" : "mins";

            if (hours > 0 && minutes > 0)
                return $"{hours} {hrLabel} {minutes} {minLabel}";
            if (hours > 0)
                return $"{hours} {hrLabel}";

            return $"{minutes} {minLabel}";
        }

        public bool IsReusable => false;
    }
}