using BNet.Cafe.Client.Services;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Script.Serialization;

namespace BNet.Cafe.Client.Ashx
{
    internal class LoginHandler
    {
        private static readonly HttpClient client = new HttpClient();

        public async Task<LoginApiResponse> LoginAsync(string email, string password)
        {
            var formData = new Dictionary<string, string>
            {
                { "email", email },
                { "password", password }
            };

            using (var content = new FormUrlEncodedContent(formData))
            {
                string baseUrl = ConfigHelper.AppUrl?.TrimEnd('/');
                string handlerUrl = $"{baseUrl}/Ashx/LoginHandler.ashx";

                HttpResponseMessage response = await client.PostAsync(handlerUrl, content);
                response.EnsureSuccessStatusCode();

                string jsonString = await response.Content.ReadAsStringAsync();

                var serializer = new JavaScriptSerializer();
                return serializer.Deserialize<LoginApiResponse>(jsonString);
            }
        }
    }
}