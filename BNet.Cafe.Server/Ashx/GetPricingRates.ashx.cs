using BNet.Cafe.Server.Repositories;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data;
using System.Web;

namespace BNet.Cafe.Server.Ashx
{
    /// <summary>
    /// Handler to fetch pricing rates without DB prefixes in response model
    /// </summary>
    public class GetPricingRates : IHttpHandler
    {
        private readonly PricingRates _pricingRatesRepository = new PricingRates();

        public void ProcessRequest(HttpContext context)
        {
            context.Response.ContentType = "application/json";

            try
            {
                DataTable dt = _pricingRatesRepository.GetAll();

                if (dt != null && dt.Rows.Count > 0)
                {
                    var ratesList = new List<PricingRateData>();

                    foreach (DataRow row in dt.Rows)
                    {
                        ratesList.Add(new PricingRateData
                        {
                            Id = Convert.ToInt32(row["DBId"]),
                            CustomerType = row["DBCustomerType"].ToString(),
                            Minutes = Convert.ToInt32(row["DBMinutes"]),
                            Price = Convert.ToDouble(row["DBPrice"]),
                            DateCreated = row["DBDateCreated"].ToString(),
                            TimeCreated = row["DBTimeCreated"].ToString(),
                            IsDeleted = Convert.ToBoolean(row["DBIsDeleted"])
                        });
                    }

                    SendJsonResponse(context, true, "Pricing rates retrieved successfully.", ratesList);
                }
                else
                {
                    SendJsonResponse(context, false, "No pricing rates found.");
                }
            }
            catch (Exception ex)
            {
                SendJsonResponse(context, false, $"Server error: {ex.Message}");
            }
        }

        private void SendJsonResponse(HttpContext context, bool success, string message, List<PricingRateData> data = null)
        {
            var responseObj = new
            {
                Success = success,
                Message = message,
                Data = data
            };

            string jsonResponse = JsonConvert.SerializeObject(responseObj, Formatting.Indented);
            context.Response.Write(jsonResponse);
        }

        public bool IsReusable => false;

        public class PricingRateData
        {
            [JsonProperty("id")]
            public int Id { get; set; }

            [JsonProperty("customerType")]
            public string CustomerType { get; set; }

            [JsonProperty("minutes")]
            public int Minutes { get; set; }

            [JsonProperty("price")]
            public double Price { get; set; }

            [JsonProperty("dateCreated")]
            public string DateCreated { get; set; }

            [JsonProperty("timeCreated")]
            public string TimeCreated { get; set; }

            [JsonProperty("isDeleted")]
            public bool IsDeleted { get; set; }
        }
    }
}