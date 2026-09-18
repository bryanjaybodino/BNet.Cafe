using BNet.Cafe.Client.Models;
using BNet.Cafe.Client.Services;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;

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
            return JsonConvert.DeserializeObject<List<PricingRateItem>>(jsonString);
        }
    }
}