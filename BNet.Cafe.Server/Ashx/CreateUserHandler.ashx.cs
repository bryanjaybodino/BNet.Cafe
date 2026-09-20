using BNet.Cafe.Server.Repositories;
using Newtonsoft.Json;
using System;
using System.Web;

namespace BNet.Cafe.Server.Ashx
{
    public class CreateUserHandler : IHttpHandler
    {
        private readonly Users _usersRepository = new Users();

        public void ProcessRequest(HttpContext context)
        {
            context.Response.ContentType = "application/json";

            try
            {
                string email = context.Request["email"];
                string password = context.Request["password"];
                string name = context.Request["name"];
                string role = context.Request["role"];

                if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password) || string.IsNullOrWhiteSpace(name) || string.IsNullOrWhiteSpace(role))
                {
                    SendJsonResponse(context, false, "email, password, name, and role are required parameters.");
                    return;
                }

                bool success = _usersRepository.Create(email, password, name, role);

                if (success)
                {
                    SendJsonResponse(context, true, "User created successfully.");
                }
                else
                {
                    SendJsonResponse(context, false, "Failed to create user. Email may already exist.");
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