using System;
using System.Collections.Generic;
using System.Configuration;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json;

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
                string baseUrl = ConfigurationManager.AppSettings["RegisterUrl"]?.TrimEnd('/');
                string handlerUrl = $"{baseUrl}/Ashx/LoginHandler.ashx";

                HttpResponseMessage response = await client.PostAsync(handlerUrl, content);
                response.EnsureSuccessStatusCode();

                string jsonString = await response.Content.ReadAsStringAsync();

                return JsonConvert.DeserializeObject<LoginApiResponse>(jsonString);
            }
        }
    }

    public class LoginApiResponse
    {
        [JsonProperty("success")]
        public bool Success { get; set; }

        [JsonProperty("message")]
        public string Message { get; set; }

        [JsonProperty("data")]
        public UserData Data { get; set; }
    }

    public class UserData
    {
        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("email")]
        public string Email { get; set; }

        [JsonProperty("role")]
        public string Role { get; set; }

        [JsonProperty("dateCreated")]
        public string DateCreated { get; set; }

        [JsonProperty("timeCreated")]
        public string TimeCreated { get; set; }

        [JsonProperty("isDeleted")]
        public bool IsDeleted { get; set; }

        [JsonProperty("totalDuration")]
        public int TotalDuration { get; set; }

        [JsonProperty("formattedTotalDuration")]
        public string FormattedTotalDuration { get; set; }
    }
}