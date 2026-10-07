using BNet.Cafe.Server.Repositories;
using BNet.Cafe.Server.Sessions;
using System;
using System.IO;
using System.Web;
using System.Web.Script.Serialization;

namespace BNet.Cafe.Server.Ashx
{
    public class SaveChatMessageHandler : IHttpHandler
    {
        public void ProcessRequest(HttpContext context)
        {
            context.Response.ContentType = "application/json";

            try
            {
                string jsonString;
                using (var reader = new StreamReader(context.Request.InputStream))
                {
                    jsonString = reader.ReadToEnd();
                }

                JavaScriptSerializer serializer = new JavaScriptSerializer();
                var data = serializer.Deserialize<SaveMessageRequest>(jsonString);

                if (data != null && !string.IsNullOrEmpty(data.Message))
                {
                    User userSession = new User();
                    string resolvedUserId = !string.IsNullOrEmpty(data.UserId)
                        ? data.UserId
                        : (userSession.count > 0 ? userSession.user_id : ConstantData.UserType.Guest);

                    ChatMessages repo = new ChatMessages();
                    repo.Create(data.Message, data.ComputerName, resolvedUserId);

                    context.Response.Write(serializer.Serialize(new { success = true }));
                    return;
                }
            }
            catch (Exception ex)
            {
                JavaScriptSerializer serializer = new JavaScriptSerializer();
                context.Response.Write(serializer.Serialize(new { success = false, error = ex.Message }));
                return;
            }

            JavaScriptSerializer errSerializer = new JavaScriptSerializer();
            context.Response.Write(errSerializer.Serialize(new { success = false, error = "Invalid message data" }));
        }

        public bool IsReusable => false;

        private class SaveMessageRequest
        {
            public string Message { get; set; }
            public string ComputerName { get; set; }
            public string UserId { get; set; }
        }
    }
}