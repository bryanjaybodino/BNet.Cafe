using System;
using System.Collections.Generic;
using System.Configuration;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json;

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
                string baseUrl = ConfigurationManager.AppSettings["RegisterUrl"]?.TrimEnd('/');
                string handlerUrl = $"{baseUrl}/Ashx/CreateRentalHandler.ashx";

                HttpResponseMessage response = await client.PostAsync(handlerUrl, content);
                response.EnsureSuccessStatusCode();

                string jsonString = await response.Content.ReadAsStringAsync();

                // Newtonsoft handles lowercase-to-PascalCase mapping automatically by default,
                // but JsonProperty explicit mapping ensures zero edge-case mismatches.
                return JsonConvert.DeserializeObject<ApiResponse>(jsonString);
            }
        }
    }
}