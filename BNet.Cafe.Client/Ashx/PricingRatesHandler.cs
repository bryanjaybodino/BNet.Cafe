using BNet.Cafe.Client.Services;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Script.Serialization;

namespace BNet.Cafe.Client.Ashx
{
    internal class PricingRatesHandler
    {
        private static readonly HttpClient client = new HttpClient();

        public async Task<List<PricingRateItem>> GetPricingRatesAsync()
        {
            string baseUrl = ConfigHelper.AppUrl?.TrimEnd('/');
            string handlerUrl = $"{baseUrl}/Ashx/GetPricingRates.ashx";

            HttpResponseMessage response = await client.GetAsync(handlerUrl);
            response.EnsureSuccessStatusCode();

            string jsonString = await response.Content.ReadAsStringAsync();
            var serializer = new JavaScriptSerializer();
            return serializer.Deserialize<List<PricingRateItem>>(jsonString);
        }
    }
}