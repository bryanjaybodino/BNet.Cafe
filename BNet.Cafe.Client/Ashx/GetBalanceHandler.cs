using BNet.Cafe.Client.Services;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;

namespace BNet.Cafe.Client.Ashx
{
    internal class GetBalanceHandler
    {
        private static readonly HttpClient client = new HttpClient();

        public async Task<GetBalanceHandlerData> GetBalanceAsync(string userId)
        {
            var formData = new Dictionary<string, string>
            {
                { "userId", userId }
            };

            using (var content = new FormUrlEncodedContent(formData))
            {
                string baseUrl = ConfigHelper.AppUrl?.TrimEnd('/');
                string handlerUrl = $"{baseUrl}/Ashx/GetBalanceHandler.ashx";

                HttpResponseMessage response = await client.PostAsync(handlerUrl, content);
                response.EnsureSuccessStatusCode();

                string jsonString = await response.Content.ReadAsStringAsync();

                return JsonConvert.DeserializeObject<GetBalanceHandlerData>(jsonString);
            }
        }
    }
}