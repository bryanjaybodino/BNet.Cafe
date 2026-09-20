using BNet.Cafe.Client.Models;
using BNet.Cafe.Client.Services;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;

namespace BNet.Cafe.Client.Ashx
{
    internal class GetPricingRatesHandler
    {
        private static readonly HttpClient client = new HttpClient();

        public async Task<List<GetPricingRatesHandlerData>> GetPricingRatesAsync()
        {
            string baseUrl = ConfigHelper.AppUrl?.TrimEnd('/');
            string handlerUrl = $"{baseUrl}/Ashx/GetPricingRatesHandler.ashx";

            HttpResponseMessage response = await client.GetAsync(handlerUrl);
            response.EnsureSuccessStatusCode();

            string jsonString = await response.Content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<List<GetPricingRatesHandlerData>>(jsonString);
        }
    }
}