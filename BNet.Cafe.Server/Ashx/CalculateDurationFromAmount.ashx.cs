using Newtonsoft.Json;
using System;
using System.Web;
using BNet.Cafe.Server.Repositories;

namespace BNet.Cafe.Server.Ashx
{
    /// <summary>
    /// Handler to calculate time duration (minutes) from currency amount (₱).
    /// Uses database-driven interval pricing with reverse linear interpolation.
    /// </summary>
    public class CalculateDurationFromAmount : IHttpHandler
    {
        public void ProcessRequest(HttpContext context)
        {
            context.Response.ContentType = "application/json";

            try
            {
                // Parse currency amount from QueryString (e.g. ?amount=25) or POST body
                decimal amount = 0m;
                decimal.TryParse(context.Request["amount"], out amount);

                // Get customer type (default: GUEST / WALK-IN)
                string customerType = context.Request["customerType"] ?? "GUEST / WALK-IN";

                // Calculate duration using database intervals
                int totalMinutes = PricingRates.CalculateDurationFromPrice((double)amount, customerType);
                string formattedTime = FormatMinutesToHours(totalMinutes);

                var responsePayload = new
                {
                    success = true,
                    amount = amount,
                    formattedAmount = $"₱ {amount:F2}",
                    totalMinutes = totalMinutes,
                    formattedTime = formattedTime,
                    customerType = customerType
                };

                context.Response.Write(JsonConvert.SerializeObject(responsePayload));
            }
            catch (Exception ex)
            {
                var errorResponse = new
                {
                    success = false,
                    error = "Failed to calculate duration",
                    message = ex.Message
                };

                context.Response.StatusCode = 500;
                context.Response.Write(JsonConvert.SerializeObject(errorResponse));
            }
        }

        public static int CalculateDuration(decimal amount, string customerType = "GUEST / WALK-IN")
        {
            return PricingRates.CalculateDurationFromPrice((double)amount, customerType);
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