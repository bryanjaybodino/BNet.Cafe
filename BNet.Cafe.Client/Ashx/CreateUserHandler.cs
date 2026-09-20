using BNet.Cafe.Client.Services;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Script.Serialization;

namespace BNet.Cafe.Client.Ashx
{
    internal class CreateUserHandler
    {
        private static readonly HttpClient client = new HttpClient();

        public async Task<ApiResponse> CreateUserAsync(string email, string password, string name, string role)
        {
            var formData = new Dictionary<string, string>
            {
                { "email", email },
                { "password", password },
                { "name", name },
                { "role", role }
            };

            using (var content = new FormUrlEncodedContent(formData))
            {
                string baseUrl = ConfigHelper.AppUrl?.TrimEnd('/');
                string handlerUrl = $"{baseUrl}/Ashx/CreateUserHandler.ashx";

                HttpResponseMessage response = await client.PostAsync(handlerUrl, content);
                response.EnsureSuccessStatusCode();

                string jsonString = await response.Content.ReadAsStringAsync();

                var serializer = new JavaScriptSerializer();
                return serializer.Deserialize<ApiResponse>(jsonString);
            }
        }
    }
}