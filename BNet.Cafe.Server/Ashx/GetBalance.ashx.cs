using BNet.Cafe.Server.Repositories;
using Newtonsoft.Json;
using System;
using System.Web;

namespace BNet.Cafe.Server.Ashx
{
    /// <summary>
    /// Summary description for GetBalance
    /// </summary>
    public class GetBalance : IHttpHandler
    {
        private readonly Balances _balancesRepository = new Balances();

        public void ProcessRequest(HttpContext context)
        {
            context.Response.ContentType = "application/json";

            try
            {
                string userId = context.Request["userId"];

                if (string.IsNullOrWhiteSpace(userId))
                {
                    SendJsonResponse(context, false, "User ID is required.");
                    return;
                }

                double balance = _balancesRepository.GetBalanceByUserId(userId);

                SendJsonResponse(context, true, "Balance retrieved successfully.", new { userId = userId, balance = balance });
            }
            catch (Exception ex)
            {
                SendJsonResponse(context, false, $"Server error: {ex.Message}");
            }
        }

        private void SendJsonResponse(HttpContext context, bool success, string message, object data = null)
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

        public bool IsReusable
        {
            get
            {
                return false;
            }
        }
    }
}