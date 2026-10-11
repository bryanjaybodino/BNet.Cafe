using BNet.Cafe.Server.Services;
using System;
using System.Web;

namespace BNet.Cafe.Server.Ashx
{
    public class GetTimeServiceHandler : IHttpHandler
    {
        public void ProcessRequest(HttpContext context)
        {
            context.Response.ContentType = "application/json";

            // Get the current server time using the optimized TimeService
            DateTime serverTime = TimeService.Get();

            // Return as JSON format so JavaScript can easily parse it
            string jsonResponse = $"{{\"serverTime\":\"{serverTime:o}\"}}";
            context.Response.Write(jsonResponse);
        }

        public bool IsReusable => false;
    }
}