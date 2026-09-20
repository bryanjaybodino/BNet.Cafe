using System;
using System.Data;
using System.Web;
using Newtonsoft.Json;
using BNet.Cafe.Server.Repositories;

namespace BNet.Cafe.Server.Ashx
{
    public class GetPricingRates : IHttpHandler
    {
        public void ProcessRequest(HttpContext context)
        {
            context.Response.ContentType = "application/json";

            try
            {
                // Call Repository
                var repo = new PricingRates();
                DataTable dt = repo.GetAll();

                // Serialize DataTable to JSON
                string jsonResult = JsonConvert.SerializeObject(dt, Formatting.Indented);

                context.Response.StatusCode = 200;
                context.Response.Write(jsonResult);
            }
            catch (Exception ex)
            {
                context.Response.StatusCode = 500;
                context.Response.Write(JsonConvert.SerializeObject(new
                {
                    error = true,
                    message = ex.Message
                }));
            }
        }

        public bool IsReusable => false;
    }
}