using BNet.Cafe.Client.Ashx;
using BNet.Cafe.Client.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace BNet.Cafe.Client.Services
{
    public static class SessionPricingRate
    {
        // Dynamic path pointing to persistent secondary drive (or fallback)
        private static string StorageDirectory => DirectoryHelper.GetAppDataFolderPath("Data");

        private static string FilePath => Path.Combine(StorageDirectory, "PricingRates.json");

        public static bool HasLocalRates()
        {
            return File.Exists(FilePath);
        }

        // Fetch once from API and cache locally
        public static async Task<List<GetPricingRatesHandlerData>> InitializeRatesAsync(bool forceRefresh = true)
        {
            if (!forceRefresh && HasLocalRates())
            {
                var cachedRates = ReadLocalRates();
                if (cachedRates != null && cachedRates.Count > 0)
                {
                    return cachedRates;
                }
            }

            try
            {
                var handler = new GetPricingRatesHandler();
                List<GetPricingRatesHandlerData> remoteRates = await handler.GetPricingRatesAsync();

                if (remoteRates != null && remoteRates.Count > 0)
                {
                    SaveLocalRates(remoteRates);
                    return remoteRates;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to fetch rates from server: {ex.Message}");
            }

            // Fallback to local file if server call failed
            return ReadLocalRates() ?? new List<GetPricingRatesHandlerData>();
        }

        public static void SaveLocalRates(List<GetPricingRatesHandlerData> rates)
        {
            try
            {
                if (!Directory.Exists(StorageDirectory))
                {
                    Directory.CreateDirectory(StorageDirectory);
                }

                string json = JsonConvert.SerializeObject(rates, Formatting.Indented);
                File.WriteAllText(FilePath, json);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error saving local pricing rates: {ex.Message}");
            }
        }

        public static List<GetPricingRatesHandlerData> ReadLocalRates()
        {
            if (!HasLocalRates()) return null;

            try
            {
                string json = File.ReadAllText(FilePath);
                return JsonConvert.DeserializeObject<List<GetPricingRatesHandlerData>>(json);
            }
            catch
            {
                return null;
            }
        }
    }
}