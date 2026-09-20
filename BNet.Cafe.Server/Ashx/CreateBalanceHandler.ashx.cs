using BNet.Cafe.Server.Repositories;
using Newtonsoft.Json;
using System;
using System.Web;

namespace BNet.Cafe.Server.Ashx
{
    public class CreateBalanceHandler : IHttpHandler
    {
        private readonly Balances _balancesRepository = new Balances();

        public void ProcessRequest(HttpContext context)
        {
            context.Response.ContentType = "application/json";

            try
            {
                string userId = context.Request["userId"];
                string duration = context.Request["duration"];
                string amount = context.Request["amount"];
                string description = context.Request["description"];

                if (string.IsNullOrWhiteSpace(userId) || string.IsNullOrWhiteSpace(duration) || string.IsNullOrWhiteSpace(amount))
                {
                    SendJsonResponse(context, false, "userId, duration, and amount are required parameters.");
                    return;
                }

                bool success = _balancesRepository.Create(userId, duration, amount, description ?? "");

                if (success)
                {
                    SendJsonResponse(context, true, "Balance entry created successfully.");
                }
                else
                {
                    SendJsonResponse(context, false, "Failed to create balance entry.");
                }
            }
            catch (Exception ex)
            {
                SendJsonResponse(context, false, $"Server error: {ex.Message}");
            }
        }

        private void SendJsonResponse(HttpContext context, bool success, string message)
        {
            var responseObj = new
            {
                Success = success,
                Message = message
            };

            string jsonResponse = JsonConvert.SerializeObject(responseObj);
            context.Response.Write(jsonResponse);
        }

        public bool IsReusable => false;
    }
}