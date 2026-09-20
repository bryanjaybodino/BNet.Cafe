using Newtonsoft.Json;
using System;
using System.Web;
using BNet.Cafe.Server.Repositories;

namespace BNet.Cafe.Server.Ashx
{
    /// <summary>
    /// Handler to calculate monetary amount (₱) from time duration (minutes).
    /// Uses database-driven interval pricing with linear interpolation.
    /// </summary>
    public class CalculateAmountFromDurationHandler : IHttpHandler
    {
        public void ProcessRequest(HttpContext context)
        {
            context.Response.ContentType = "application/json";

            try
            {
                // Parse total minutes from QueryString (e.g. ?minutes=120) or POST body
                int totalMinutes = 0;
                int.TryParse(context.Request["minutes"], out totalMinutes);

                // Get customer type (default: GUEST / WALK-IN)
                string customerType = context.Request["customerType"] ?? "GUEST / WALK-IN";

                // Calculate price using database intervals
                double totalAmount = PricingRates.CalculatePriceFromDuration(totalMinutes, customerType);
                string formattedTime = FormatMinutesToHours(totalMinutes);

                var responsePayload = new CalculateAmountFromDurationData
                {
                    TotalMinutes = totalMinutes,
                    FormattedTime = formattedTime,
                    TotalAmount = Math.Round(totalAmount, 2),
                    FormattedAmount = $"₱ {totalAmount:F2}",
                    CustomerType = customerType
                };

                SendJsonResponse(context, true, "Price calculated successfully.", responsePayload);
            }
            catch (Exception ex)
            {
                SendJsonResponse(context, false, $"Failed to calculate price: {ex.Message}");
            }
        }

        private void SendJsonResponse(HttpContext context, bool success, string message, CalculateAmountFromDurationData data = null)
        {
            string jsonResponse = JsonConvert.SerializeObject(data);
            context.Response.Write(jsonResponse);
        }

        public static double CalculatePrice(int totalMinutes, string customerType = "GUEST / WALK-IN")
        {
            return PricingRates.CalculatePriceFromDuration(totalMinutes, customerType);
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

        public class CalculateAmountFromDurationData
        {
            [JsonProperty("totalMinutes")]
            public int TotalMinutes { get; set; }

            [JsonProperty("formattedTime")]
            public string FormattedTime { get; set; }

            [JsonProperty("totalAmount")]
            public double TotalAmount { get; set; }

            [JsonProperty("formattedAmount")]
            public string FormattedAmount { get; set; }

            [JsonProperty("customerType")]
            public string CustomerType { get; set; }
        }
    }
}