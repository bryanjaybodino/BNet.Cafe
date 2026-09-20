using BNet.Cafe.Client.Ashx;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using System.Web.Script.Serialization;

namespace BNet.Cafe.Client.Services
{
    public static class SessionPricingRate
    {
        private static readonly string StorageDirectory = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData),
            "BNetCafe"
        );

        private static readonly string FilePath = Path.Combine(StorageDirectory, "PricingRates.json");

        public static bool HasLocalRates()
        {
            return File.Exists(FilePath);
        }

        // Fetch once from API and cache locally
        public static async Task<List<PricingRateItem>> InitializeRatesAsync(bool forceRefresh = true)
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
                var handler = new PricingRatesHandler();
                List<PricingRateItem> remoteRates = await handler.GetPricingRatesAsync();

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
            return ReadLocalRates() ?? new List<PricingRateItem>();
        }

        public static void SaveLocalRates(List<PricingRateItem> rates)
        {
            try
            {
                if (!Directory.Exists(StorageDirectory))
                {
                    Directory.CreateDirectory(StorageDirectory);
                }

                var serializer = new JavaScriptSerializer();
                string json = serializer.Serialize(rates);
                File.WriteAllText(FilePath, json);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error saving local pricing rates: {ex.Message}");
            }
        }

        public static List<PricingRateItem> ReadLocalRates()
        {
            if (!HasLocalRates()) return null;

            try
            {
                string json = File.ReadAllText(FilePath);
                var serializer = new JavaScriptSerializer();
                return serializer.Deserialize<List<PricingRateItem>>(json);
            }
            catch
            {
                return null;
            }
        }
    }
}