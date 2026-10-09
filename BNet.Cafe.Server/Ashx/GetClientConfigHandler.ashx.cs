using BNet.Cafe.Server.Repositories;
using Newtonsoft.Json;
using System;
using System.Data;
using System.Web;

namespace BNet.Cafe.Server.Ashx
{
    /// <summary>
    /// Summary description for GetClientConfigHandler
    /// </summary>
    public class GetClientConfigHandler : IHttpHandler
    {
        private readonly ClientConfig _clientConfigRepository = new ClientConfig();

        public void ProcessRequest(HttpContext context)
        {
            context.Response.ContentType = "application/json";

            try
            {
                string id = context.Request["id"];
                DataTable dt = null;

                if (!string.IsNullOrWhiteSpace(id))
                {
                    dt = _clientConfigRepository.GetById(id);
                }
                else
                {
                    dt = _clientConfigRepository.GetAll();
                }

                if (dt != null && dt.Rows.Count > 0)
                {
                    DataRow row = dt.Rows[0];

                    var configData = new GetClientConfigData
                    {
                        Id = row["DBId"].ToString(),
                        AccountCreationAllowed = Convert.ToBoolean(row["DBAccountCreationAllowed"]),
                        AutoShutDownInterval = Convert.ToInt32(row["DBAutoShutDownInterval"]),
                        DesktopSlideShow = Convert.ToBoolean(row["DBDesktopSlideShow"]),
                        ResetShutdownCountdown = Convert.ToBoolean(row["DBResetShutdownCountdown"]),
                        DateCreated = row["DBDateCreated"].ToString(),
                        TimeCreated = row["DBTimeCreated"].ToString(),
                        IsDeleted = Convert.ToBoolean(row["DBIsDeleted"])
                    };

                    SendJsonResponse(context, true, "Client configuration retrieved successfully.", configData);
                }
                else
                {
                    SendJsonResponse(context, false, "Client configuration not found.");
                }
            }
            catch (Exception ex)
            {
                SendJsonResponse(context, false, $"Server error: {ex.Message}");
            }
        }

        private void SendJsonResponse(HttpContext context, bool success, string message, GetClientConfigData data = null)
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

        public class GetClientConfigData
        {
            [JsonProperty("id")]
            public string Id { get; set; }

            [JsonProperty("accountCreationAllowed")]
            public bool AccountCreationAllowed { get; set; }

            [JsonProperty("autoShutDownInterval")]
            public int AutoShutDownInterval { get; set; }

            [JsonProperty("desktopSlideShow")]
            public bool DesktopSlideShow { get; set; }

            [JsonProperty("resetShutdownCountdown")]
            public bool ResetShutdownCountdown { get; set; }

            [JsonProperty("dateCreated")]
            public string DateCreated { get; set; }

            [JsonProperty("timeCreated")]
            public string TimeCreated { get; set; }

            [JsonProperty("isDeleted")]
            public bool IsDeleted { get; set; }
        }
    }
}