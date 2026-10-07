using BNet.Cafe.Server.Repositories;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq; // Added System.Linq for sorting
using System.Web;

namespace BNet.Cafe.Server.Ashx
{
    /// <summary>
    /// Summary description for GetChatMessagesHandler
    /// </summary>
    public class GetChatMessagesHandler : IHttpHandler
    {
        private readonly ChatMessages _chatMessagesRepository = new ChatMessages();
        Sessions.User userSession = new Sessions.User();
        public void ProcessRequest(HttpContext context)
        {
            context.Response.ContentType = "application/json";

            try
            {
                string userId = userSession.user_id;
                string computerName = context.Request["computerName"] ?? "";
                string pageIndexStr = context.Request["pageIndex"];
                string isDeletedStr = context.Request["isDeleted"];

                int pageIndex = -1;
                if (!string.IsNullOrWhiteSpace(pageIndexStr))
                {
                    int.TryParse(pageIndexStr, out pageIndex);
                }

                bool isDeleted = false;
                if (!string.IsNullOrWhiteSpace(isDeletedStr))
                {
                    bool.TryParse(isDeletedStr, out isDeleted);
                }

                DataTable dt = _chatMessagesRepository.GetAll(userId, computerName, pageIndex, isDeleted);
                List<ChatMessageItem> list = new List<ChatMessageItem>();

                if (dt != null && dt.Rows.Count > 0)
                {
                    foreach (DataRow row in dt.Rows)
                    {
                        list.Add(new ChatMessageItem
                        {
                            Id = row["DBId"].ToString(),
                            Message = row["DBMessage"].ToString(),
                            ComputerName = row["DBComputerName"].ToString(),
                            UserId = row["DBUserId"].ToString(),
                            DateCreated = row["DBDateCreated"].ToString(),
                            TimeCreated = row["DBTimeCreated"].ToString(),
                            IsDeleted = row["DBIsDeleted"] != DBNull.Value && Convert.ToBoolean(row["DBIsDeleted"])
                        });
                    }

                    // Sort list by DBID in ascending numeric order
                    list = list.OrderBy(x => long.TryParse(x.Id, out long id) ? id : 0).ToList();
                }

                SendJsonResponse(context, true, "Chat messages retrieved successfully.", list);
            }
            catch (Exception ex)
            {
                SendJsonResponse(context, false, $"Server error: {ex.Message}");
            }
        }

        private void SendJsonResponse(HttpContext context, bool success, string message, List<ChatMessageItem> data = null)
        {
            var responseObj = new
            {
                success = success,
                message = message,
                data = data ?? new List<ChatMessageItem>()
            };

            string jsonResponse = JsonConvert.SerializeObject(responseObj);
            context.Response.Write(jsonResponse);
        }

        public bool IsReusable => false;

        public class ChatMessageItem
        {
            [JsonProperty("id")]
            public string Id { get; set; }

            [JsonProperty("message")]
            public string Message { get; set; }

            [JsonProperty("computerName")]
            public string ComputerName { get; set; }

            [JsonProperty("userId")]
            public string UserId { get; set; }

            [JsonProperty("dateCreated")]
            public string DateCreated { get; set; }

            [JsonProperty("timeCreated")]
            public string TimeCreated { get; set; }

            [JsonProperty("isDeleted")]
            public bool IsDeleted { get; set; }
        }
    }
}