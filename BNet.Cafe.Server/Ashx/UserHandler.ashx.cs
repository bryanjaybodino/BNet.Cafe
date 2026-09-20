using BNet.Cafe.Server.Repositories;
using Newtonsoft.Json;
using System;
using System.Data;
using System.Web;

namespace BNet.Cafe.Server.Ashx
{
    /// <summary>
    /// Summary description for UserHandler
    /// </summary>
    public class UserHandler : IHttpHandler
    {

        private readonly Users _usersRepository = new Users();

        public void ProcessRequest(HttpContext context)
        {
            context.Response.ContentType = "application/json";

            try
            {
                string id = context.Request["userId"];
                string email = context.Request["email"];

                DataTable dt = null;

                if (!string.IsNullOrWhiteSpace(id))
                {
                    dt = _usersRepository.GetById(id);
                }
                else if (!string.IsNullOrWhiteSpace(email))
                {
                    dt = _usersRepository.GetByEmail(email);
                }
                else
                {
                    SendJsonResponse(context, false, "User id or email is required.");
                    return;
                }

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
                        FormattedTotalDuration = row["DBFormattedTotalDuration"].ToString(),
                        Password = row["DBPassword"].ToString()
                    };

                    SendJsonResponse(context, true, "User found successfully.", userData);
                }
                else
                {
                    SendJsonResponse(context, false, "User not found.");
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
                Success = success,
                Message = message,
                Data = data
            };

            string jsonResponse = JsonConvert.SerializeObject(responseObj);
            context.Response.Write(jsonResponse);
        }

        public bool IsReusable => false;


        public class UserData
        {
            public string Id { get; set; }
            public string Name { get; set; }
            public string Email { get; set; }
            public string Role { get; set; }
            public string DateCreated { get; set; }
            public string TimeCreated { get; set; }
            public bool IsDeleted { get; set; }
            public int TotalDuration { get; set; }
            public string FormattedTotalDuration { get; set; }
            public string Password { get; set; }
        }
    }
}