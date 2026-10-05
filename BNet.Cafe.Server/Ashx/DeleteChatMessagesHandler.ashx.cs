using BNet.Cafe.Server.Repositories;
using Newtonsoft.Json;
using System;
using System.Web;

namespace BNet.Cafe.Server.Ashx
{
    /// <summary>
    /// Handler to mark a chat message as deleted or permanently delete it.
    /// </summary>
    public class DeleteChatMessagesHandler : IHttpHandler
    {
        private readonly ChatMessages _chatMessagesRepository = new ChatMessages();

        public void ProcessRequest(HttpContext context)
        {
            context.Response.ContentType = "application/json";

            try
            {
                string id = context.Request["id"] ?? context.Request["DBId"];

                if (string.IsNullOrWhiteSpace(id))
                {
                    SendJsonResponse(context, false, "Message ID is required.");
                    return;
                }

                // Execute the Delete repository method
                _chatMessagesRepository.Delete(id);

                SendJsonResponse(context, true, "Chat message deleted successfully.");
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
                success = success,
                message = message
            };

            string jsonResponse = JsonConvert.SerializeObject(responseObj);
            context.Response.Write(jsonResponse);
        }

        public bool IsReusable => false;
    }
}