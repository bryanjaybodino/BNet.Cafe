using BNet.Cafe.Server.Repositories;
using Newtonsoft.Json;
using System;
using System.Data;
using System.Web;

namespace BNet.Cafe.Server.Ashx
{
    public class LoginHandler : IHttpHandler
    {
        private readonly Users _usersRepository = new Users();

        public void ProcessRequest(HttpContext context)
        {
            context.Response.ContentType = "application/json";

            try
            {
                string email = context.Request["email"];
                string password = context.Request["password"];

                if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
                {
                    SendJsonResponse(context, false, "Email and password are required.");
                    return;
                }

                DataTable dt = _usersRepository.GetLogin(email, password);

                if (dt != null && dt.Rows.Count > 0)
                {
                    DataRow row = dt.Rows[0];

                    var userData = new UserData
                    {
                        Id = row["DBId"].ToString(),
                        Name = row["DBName"].ToString(),
                        Email = row["DBEmail"].ToString(),
                        Role = row["DBRole"].ToString(),
                        DateCreated = row["DBDateCreated"].ToString(),
                        TimeCreated = row["DBTimeCreated"].ToString(),
                        IsDeleted = Convert.ToBoolean(row["DBIsDeleted"]),
                        TotalDuration = Convert.ToInt32(row["DBTotalDuration"]),
                        FormattedTotalDuration = row["DBFormattedTotalDuration"].ToString()
                    };

                    SendJsonResponse(context, true, "Login successful.", userData);
                }
                else
                {
                    SendJsonResponse(context, false, "Invalid email or password.");
                }
            }
            catch (Exception ex)
            {
                SendJsonResponse(context, false, $"Server error: {ex.Message}");
            }
        }

        private void SendJsonResponse(HttpContext context, bool success, string message, UserData data = null)
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

        public bool IsReusable => false;

        public class UserData
        {
            [JsonProperty("id")]
            public string Id { get; set; }

            [JsonProperty("name")]
            public string Name { get; set; }

            [JsonProperty("email")]
            public string Email { get; set; }

            [JsonProperty("role")]
            public string Role { get; set; }

            [JsonProperty("dateCreated")]
            public string DateCreated { get; set; }

            [JsonProperty("timeCreated")]
            public string TimeCreated { get; set; }

            [JsonProperty("isDeleted")]
            public bool IsDeleted { get; set; }

            [JsonProperty("totalDuration")]
            public int TotalDuration { get; set; }

            [JsonProperty("formattedTotalDuration")]
            public string FormattedTotalDuration { get; set; }
        }
    }
}