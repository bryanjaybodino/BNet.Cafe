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
    internal class UserHandler
    {
        private static readonly HttpClient client = new HttpClient();

        public async Task<LoginApiResponse> UserAsync(string userId)
        {
            var formData = new Dictionary<string, string>
            {
                { "userId", userId },
            };

            using (var content = new FormUrlEncodedContent(formData))
            {
                string baseUrl = ConfigurationManager.AppSettings["AppUrl"]?.TrimEnd('/');
                string handlerUrl = $"{baseUrl}/Ashx/UserHandler.ashx";

                HttpResponseMessage response = await client.PostAsync(handlerUrl, content);
                response.EnsureSuccessStatusCode();

                string jsonString = await response.Content.ReadAsStringAsync();

                return JsonConvert.DeserializeObject<LoginApiResponse>(jsonString);
            }
        }
    }
}