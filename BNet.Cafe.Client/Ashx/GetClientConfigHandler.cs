using BNet.Cafe.Client.Services;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;

namespace BNet.Cafe.Client.Ashx
{
    internal class GetClientConfigHandler
    {
        private static readonly HttpClient client = new HttpClient();

        /// <summary>
        /// Fetches client configuration by ID.
        /// </summary>
        public async Task<GetClientConfigData> GetByIdAsync(string id)
        {
            var formData = new Dictionary<string, string>
            {
                { "id", id }
            };

            return await ExecuteRequestAsync(formData);
        }

        /// <summary>
        /// Fetches client configuration.
        /// </summary>
        public async Task<GetClientConfigData> GetAllAsync()
        {
            var formData = new Dictionary<string, string>();
            return await ExecuteRequestAsync(formData);
        }

        private async Task<GetClientConfigData> ExecuteRequestAsync(Dictionary<string, string> formData)
        {
            using (var content = new FormUrlEncodedContent(formData))
            {
                string baseUrl = ConfigHelper.AppUrl?.TrimEnd('/');
                string handlerUrl = $"{baseUrl}/Ashx/GetClientConfigHandler.ashx";

                HttpResponseMessage response = await client.PostAsync(handlerUrl, content);
                response.EnsureSuccessStatusCode();

                string jsonString = await response.Content.ReadAsStringAsync();

                return JsonConvert.DeserializeObject<GetClientConfigData>(jsonString);
            }
        }
    }
}