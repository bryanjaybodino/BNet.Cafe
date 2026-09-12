using System;
using System.Collections.Generic;
using System.Configuration;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace BNet.Cafe.Client.Ashx
{
    internal class CreateBalanceHandler
    {
        private static readonly HttpClient client = new HttpClient();

        public async Task<ApiResponse> CreateBalanceAsync(string userId, string duration, string amount, string description)
        {
            var formData = new Dictionary<string, string>
            {
                { "userId", userId },
                { "duration", duration },
                { "amount", amount },
                { "description", description }
            };

            using (var content = new FormUrlEncodedContent(formData))
            {
                string baseUrl = ConfigurationManager.AppSettings["AppUrl"]?.TrimEnd('/');
                string handlerUrl = $"{baseUrl}/Ashx/CreateBalanceHandler.ashx";

                HttpResponseMessage response = await client.PostAsync(handlerUrl, content);
                response.EnsureSuccessStatusCode();

                string jsonString = await response.Content.ReadAsStringAsync();

                return JsonConvert.DeserializeObject<ApiResponse>(jsonString);
            }
        }
    }
}