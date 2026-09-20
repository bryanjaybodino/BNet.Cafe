using BNet.Cafe.Client.Services;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Script.Serialization;

namespace BNet.Cafe.Client.Ashx
{
    internal class CreateRentalHandler
    {
        private static readonly HttpClient client = new HttpClient();

        public async Task<ApiResponse> CreateRentalAsync(string computerName, string userId, string duration, string amount)
        {

            var formData = new Dictionary<string, string>
            {
                { "computerName", computerName },
                { "userId", userId },
                { "duration", duration },
                { "amount", amount }
            };

            using (var content = new FormUrlEncodedContent(formData))
            {
                string baseUrl = ConfigHelper.AppUrl?.TrimEnd('/');
                string handlerUrl = $"{baseUrl}/Ashx/CreateRentalHandler.ashx";

                HttpResponseMessage response = await client.PostAsync(handlerUrl, content);
                response.EnsureSuccessStatusCode();

                string jsonString = await response.Content.ReadAsStringAsync();

                var serializer = new JavaScriptSerializer();
                return serializer.Deserialize<ApiResponse>(jsonString);
            }
        }
    }
}