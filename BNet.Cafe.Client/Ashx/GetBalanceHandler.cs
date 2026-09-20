using BNet.Cafe.Client.Services;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Script.Serialization;

namespace BNet.Cafe.Client.Ashx
{
    internal class GetBalanceHandler
    {
        private static readonly HttpClient client = new HttpClient();

        public async Task<BalanceApiResponse> GetBalanceAsync(string userId)
        {
            var formData = new Dictionary<string, string>
            {
                { "userId", userId }
            };

            using (var content = new FormUrlEncodedContent(formData))
            {
                string baseUrl = ConfigHelper.AppUrl?.TrimEnd('/');
                string handlerUrl = $"{baseUrl}/Ashx/GetBalance.ashx";

                HttpResponseMessage response = await client.PostAsync(handlerUrl, content);
                response.EnsureSuccessStatusCode();

                string jsonString = await response.Content.ReadAsStringAsync();

                var serializer = new JavaScriptSerializer();
                return serializer.Deserialize<BalanceApiResponse>(jsonString);
            }
        }
    }
}