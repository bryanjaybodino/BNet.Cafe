using BNet.Cafe.Client.Services;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Script.Serialization;

namespace BNet.Cafe.Client.Ashx
{
    internal class UserHandler
    {
        private static readonly HttpClient client = new HttpClient();

        /// <summary>
        /// Fetches user data by userId.
        /// </summary>
        public async Task<LoginApiResponse> GetByIdAsync(string userId)
        {
            var formData = new Dictionary<string, string>
            {
                { "userId", userId }
            };

            return await ExecuteRequestAsync(formData);
        }

        /// <summary>
        /// Fetches user data by email.
        /// </summary>
        public async Task<LoginApiResponse> GetByEmailAsync(string email)
        {
            var formData = new Dictionary<string, string>
            {
                { "email", email }
            };

            return await ExecuteRequestAsync(formData);
        }

        private async Task<LoginApiResponse> ExecuteRequestAsync(Dictionary<string, string> formData)
        {
            using (var content = new FormUrlEncodedContent(formData))
            {
                string baseUrl = ConfigHelper.AppUrl?.TrimEnd('/');
                string handlerUrl = $"{baseUrl}/Ashx/UserHandler.ashx";

                HttpResponseMessage response = await client.PostAsync(handlerUrl, content);
                response.EnsureSuccessStatusCode();

                string jsonString = await response.Content.ReadAsStringAsync();

                var serializer = new JavaScriptSerializer();
                return serializer.Deserialize<LoginApiResponse>(jsonString);
            }
        }
    }
}