using Newtonsoft.Json;
using System;
using System.Web;

namespace BNet.Cafe.Server.Ashx
{
    /// <summary>
    /// Handler to calculate monetary amount (₱) from time duration (minutes).
    /// </summary>
    public class CalculateAmountFromDuration : IHttpHandler
    {
        public void ProcessRequest(HttpContext context)
        {
            context.Response.ContentType = "application/json";

            // Parse total minutes from QueryString (e.g. ?minutes=120) or POST body
            int totalMinutes = 0;
            int.TryParse(context.Request["minutes"], out totalMinutes);

            double totalAmount = CalculatePrice(totalMinutes);
            string formattedTime = FormatMinutesToHours(totalMinutes);

            var responsePayload = new
            {
                totalMinutes = totalMinutes,
                formattedTime = formattedTime,
                totalAmount = Math.Round(totalAmount, 2),
                formattedAmount = $"₱ {totalAmount:F2}"
            };

            context.Response.Write(JsonConvert.SerializeObject(responsePayload));
        }

        public static double CalculatePrice(int totalMinutes)
        {
            if (totalMinutes <= 0) return 0.0;

            // 0 to 30 mins: Linear scale based on ₱10.00 / 30 mins (₱0.3333/min)
            if (totalMinutes <= 30)
            {
                return totalMinutes * (10.0 / 30.0);
            }
            // 30 to 60 mins (1 hr): Interpolate from ₱10.00 up to ₱15.00
            else if (totalMinutes <= 60)
            {
                double basePrice = 10.0;
                int extraMins = totalMinutes - 30;
                double ratePerMin = (15.0 - 10.0) / 30.0;
                return basePrice + (extraMins * ratePerMin);
            }
            // 60 to 120 mins (2 hrs): Interpolate from ₱15.00 up to ₱25.00
            else if (totalMinutes <= 120)
            {
                double basePrice = 15.0;
                int extraMins = totalMinutes - 60;
                double ratePerMin = (25.0 - 15.0) / 60.0;
                return basePrice + (extraMins * ratePerMin);
            }
            // 120 to 180 mins (3 hrs): Interpolate from ₱25.00 up to ₱40.00
            else if (totalMinutes <= 180)
            {
                double basePrice = 25.0;
                int extraMins = totalMinutes - 120;
                double ratePerMin = (40.0 - 25.0) / 60.0;
                return basePrice + (extraMins * ratePerMin);
            }
            // 180 to 240 mins (4 hrs): Interpolate from ₱40.00 up to ₱50.00
            else if (totalMinutes <= 240)
            {
                double basePrice = 40.0;
                int extraMins = totalMinutes - 180;
                double ratePerMin = (50.0 - 40.0) / 60.0;
                return basePrice + (extraMins * ratePerMin);
            }
            // Beyond 4 Hours (240+ mins): ₱50.00 + ₱10.00/hr (₱0.1667/min)
            else
            {
                double basePrice = 50.0;
                int extraMins = totalMinutes - 240;
                double ratePerMin = 10.0 / 60.0;
                return basePrice + (extraMins * ratePerMin);
            }
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