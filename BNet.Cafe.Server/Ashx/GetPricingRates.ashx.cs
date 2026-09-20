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
                            Minutes = row["DBMinutes"].ToString(),
                            Price = row["DBPrice"].ToString(),
                            DateCreated = row["DBDateCreated"].ToString(),
                            TimeCreated = row["DBTimeCreated"].ToString(),
                            IsDeleted = row["DBIsDeleted"].ToString()
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
            public int Id { get; set; }
            public string CustomerType { get; set; }
            public string Minutes { get; set; }
            public string Price { get; set; }
            public string DateCreated { get; set; }
            public string TimeCreated { get; set; }
            public string IsDeleted { get; set; }
        }
    }
}