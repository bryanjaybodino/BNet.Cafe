using BNet.Cafe.Client.Ashx;
using BNet.Cafe.Client.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace BNet.Cafe.Client.Services
{
    internal class CalculateRentalPrice
    {
        public static double CalculatePrice(int totalMinutes)
        {
            if (totalMinutes <= 0) return 0.0;

            // Load rates from local JSON cache (Zero API Overhead)
            List<GetPricingRatesHandlerData> rates = SessionPricingRate.ReadLocalRates();

            if (rates == null || !rates.Any())
            {
                // Fallback default calculation if JSON file is missing
                return totalMinutes * (10.0 / 30.0);
            }

            // Filter active tiers sorted by minutes ascending
            var Tiers = rates
                .Where(r => !r.IsDeleted &&
                            r.CustomerType.Equals("Guest / Walk-in", StringComparison.OrdinalIgnoreCase))
                .OrderBy(r => r.Minutes)
                .ToList();

            if (!Tiers.Any()) return 0.0;

            // Target minutes is lower than lowest tier
            if (totalMinutes <= Tiers.First().Minutes)
            {
                var minTier = Tiers.First();
                return totalMinutes * (minTier.Price / minTier.Minutes);
            }

            // Target minutes exceeds highest tier: calculate highest tier price + excess rate
            var lastTier = Tiers.Last();
            if (totalMinutes >= lastTier.Minutes)
            {
                int extraMins = totalMinutes - lastTier.Minutes;

                // Calculate excess rate based on the last bracket slope (or standard ₱10/hr if only 1 tier exists)
                double excessRatePerMin = Tiers.Count > 1
                    ? (lastTier.Price - Tiers[Tiers.Count - 2].Price) / (lastTier.Minutes - Tiers[Tiers.Count - 2].Minutes)
                    : 10.0 / 60.0;

                return lastTier.Price + (extraMins * excessRatePerMin);
            }

            // Interpolate price between intermediate tiers
            for (int i = 0; i < Tiers.Count - 1; i++)
            {
                var lowerTier = Tiers[i];
                var upperTier = Tiers[i + 1];

                if (totalMinutes > lowerTier.Minutes && totalMinutes <= upperTier.Minutes)
                {
                    double basePrice = lowerTier.Price;
                    int extraMins = totalMinutes - lowerTier.Minutes;
                    double ratePerMin = (upperTier.Price - lowerTier.Price) / (upperTier.Minutes - lowerTier.Minutes);

                    return basePrice + (extraMins * ratePerMin);
                }
            }

            return 0.0;
        }
    }
}