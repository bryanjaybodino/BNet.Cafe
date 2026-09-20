using BNet.Cafe.Server.Databases;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Script.Serialization;

namespace BNet.Cafe.Server.Ashx
{
    /// <summary>
    /// Summary description for CreateRentalHandler
    /// </summary>
    public class CreateRentalHandler : IHttpHandler
    {

        public void ProcessRequest(HttpContext context)
        {
            context.Response.ContentType = "application/json";

            // Enforce POST method only
            if (!context.Request.HttpMethod.Equals("POST", StringComparison.OrdinalIgnoreCase))
            {
                context.Response.StatusCode = 405; // Method Not Allowed
                SendResponse(context, false, "Invalid request method. Only POST is allowed.");
                return;
            }

            try
            {
                // Read from Request.Form for POST payload
                string computerName = context.Request.Form["computerName"];
                string userId = context.Request.Form["userId"];
                string duration = context.Request.Form["duration"];
                string amount = context.Request.Form["amount"];

                if (string.IsNullOrEmpty(computerName))
                {
                    SendResponse(context, false, "Computer ID is required.");
                    return;
                }

                bool success = Create(context, computerName, userId, duration, amount);

                if (success)
                {
                    SendResponse(context, true, "Rental created successfully.");
                }
                else
                {
                    SendResponse(context, false, "Failed to create rental record.");
                }
            }
            catch (Exception ex)
            {
                context.Response.StatusCode = 500;
                SendResponse(context, false, "Error: " + ex.Message);
            }
        }

        public bool Create(HttpContext context, string computerName, string userId, string duration, string amount)
        {
            var dBScriptService = new DBScriptService();
            var DBContext = new DBContext();
            var scripts = new System.Collections.Generic.Dictionary<string, string>();


            Repositories.Computers computers = new Repositories.Computers();
            string DBComputerId = computers.GetIdByName(dBScriptService.CleanUpToUpper(computerName));
            string DBUserId = dBScriptService.CleanUpToUpper(userId);

            scripts.Add("DBUserId", DBUserId);
            dBScriptService.AddIfNotNullOrEmpty(scripts, "DBComputerId", DBComputerId);
            dBScriptService.AddIfNotNullOrEmpty(scripts, "DBDuration", duration);
            dBScriptService.AddIfNotNullOrEmpty(scripts, "DBAmount", amount);
            dBScriptService.AddIfNotNullOrEmpty(scripts, "DBDateCreated", DateTime.Now.ToString("yyyy-MM-dd"));
            dBScriptService.AddIfNotNullOrEmpty(scripts, "DBTimeCreated", DateTime.Now.ToString("HH:mm:ss"));
            dBScriptService.AddIfNotNullOrEmpty(scripts, "DBIsDeleted", "FALSE");

            string template = context.Server.MapPath("~/Databases/Queries/Rentals/Create.sql");
            string sql = dBScriptService.Scripts(scripts, template);

            return DBContext.SqlExecuteAsync(sql);
        }

        private void SendResponse(HttpContext context, bool success, string message)
        {
            var serializer = new JavaScriptSerializer();
            var responseData = new { Success = success, Message = message };
            context.Response.Write(serializer.Serialize(responseData));
        }

        public bool IsReusable => false;
    }
}