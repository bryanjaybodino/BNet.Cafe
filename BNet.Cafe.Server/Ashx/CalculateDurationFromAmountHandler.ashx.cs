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
    public class CalculateDurationFromAmountHandler : IHttpHandler
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

                var responsePayload = new CalculateDurationFromAmountData
                {
                    Amount = amount,
                    FormattedAmount = $"₱ {amount:F2}",
                    TotalMinutes = totalMinutes,
                    FormattedTime = formattedTime,
                    CustomerType = customerType
                };

                SendJsonResponse(context, true, "Duration calculated successfully.", responsePayload);
            }
            catch (Exception ex)
            {
                SendJsonResponse(context, false, $"Failed to calculate duration: {ex.Message}");
            }
        }

        private void SendJsonResponse(HttpContext context, bool success, string message, CalculateDurationFromAmountData data = null)
        {
            var responseObj = new
            {
                success = success,
                message = message,
                data = data
            };

            string jsonResponse = JsonConvert.SerializeObject(responseObj);
            context.Response.Write(jsonResponse);
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

        public class CalculateDurationFromAmountData
        {
            [JsonProperty("amount")]
            public decimal Amount { get; set; }

            [JsonProperty("formattedAmount")]
            public string FormattedAmount { get; set; }

            [JsonProperty("totalMinutes")]
            public int TotalMinutes { get; set; }

            [JsonProperty("formattedTime")]
            public string FormattedTime { get; set; }

            [JsonProperty("customerType")]
            public string CustomerType { get; set; }
        }
    }
}