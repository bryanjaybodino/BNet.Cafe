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
    public class CalculateAmountFromDuration : IHttpHandler
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

                var responsePayload = new
                {
                    Success = true,
                    TotalMinutes = totalMinutes,
                    FormattedTime = formattedTime,
                    TotalAmount = Math.Round(totalAmount, 2),
                    FormattedAmount = $"₱ {totalAmount:F2}",
                    CustomerType = customerType
                };

                context.Response.Write(JsonConvert.SerializeObject(responsePayload));
            }
            catch (Exception ex)
            {
                var errorResponse = new
                {
                    Success = false,
                    Error = "Failed to calculate price",
                    Message = ex.Message
                };

                context.Response.StatusCode = 500;
                context.Response.Write(JsonConvert.SerializeObject(errorResponse));
            }
        }
        public static double CalculatePrice(int totalMinutes, string customerType= "GUEST / WALK-IN")
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
    }
}