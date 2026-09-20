using BNet.Cafe.Client.Services;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace BNet.Cafe.Client.Ashx
{
    internal class GetUserHandler
    {
        private static readonly HttpClient client = new HttpClient();

        /// <summary>
        /// Fetches user data by userId.
        /// </summary>
        public async Task<GetLoginHandlerData> GetByIdAsync(string userId)
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
        public async Task<GetLoginHandlerData> GetByEmailAsync(string email)
        {
            var formData = new Dictionary<string, string>
            {
                { "email", email }
            };

            return await ExecuteRequestAsync(formData);
        }

        private async Task<GetLoginHandlerData> ExecuteRequestAsync(Dictionary<string, string> formData)
        {
            using (var content = new FormUrlEncodedContent(formData))
            {
                string baseUrl = ConfigHelper.AppUrl?.TrimEnd('/');
                string handlerUrl = $"{baseUrl}/Ashx/GetUserHandler.ashx";

                HttpResponseMessage response = await client.PostAsync(handlerUrl, content);
                response.EnsureSuccessStatusCode();

                string jsonString = await response.Content.ReadAsStringAsync();

                return JsonConvert.DeserializeObject<GetLoginHandlerData>(jsonString);
            }
        }
    }
}