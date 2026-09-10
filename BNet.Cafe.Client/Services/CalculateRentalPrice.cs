using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BNet.Cafe.Client.Services
{
    internal class CalculateRentalPrice
    {
        public static double CalculatePrice(int totalMinutes)
        {
            if (totalMinutes <= 0) return 0.0;

            // 0 to 30 mins: Linear scale based on ₱10.00 / 30 mins (₱0.3333/min)
            if (totalMinutes <= 30)
            {
                return totalMinutes * (10.0 / 30.0);
            }
            // 30 to 60 mins (1 hr): Interpolate from ₱10.00 up to ₱15.00
            else if (totalMinutes <= 60)
            {
                double basePrice = 10.0;
                int extraMins = totalMinutes - 30;
                double ratePerMin = (15.0 - 10.0) / 30.0;
                return basePrice + (extraMins * ratePerMin);
            }
            // 60 to 120 mins (2 hrs): Interpolate from ₱15.00 up to ₱25.00
            else if (totalMinutes <= 120)
            {
                double basePrice = 15.0;
                int extraMins = totalMinutes - 60;
                double ratePerMin = (25.0 - 15.0) / 60.0;
                return basePrice + (extraMins * ratePerMin);
            }
            // 120 to 180 mins (3 hrs): Interpolate from ₱25.00 up to ₱40.00
            else if (totalMinutes <= 180)
            {
                double basePrice = 25.0;
                int extraMins = totalMinutes - 120;
                double ratePerMin = (40.0 - 25.0) / 60.0;
                return basePrice + (extraMins * ratePerMin);
            }
            // 180 to 240 mins (4 hrs): Interpolate from ₱40.00 up to ₱50.00
            else if (totalMinutes <= 240)
            {
                double basePrice = 40.0;
                int extraMins = totalMinutes - 180;
                double ratePerMin = (50.0 - 40.0) / 60.0;
                return basePrice + (extraMins * ratePerMin);
            }
            // Beyond 4 Hours (240+ mins): ₱50.00 + ₱10.00/hr (₱0.1667/min)
            else
            {
                double basePrice = 50.0;
                int extraMins = totalMinutes - 240;
                double ratePerMin = 10.0 / 60.0;
                return basePrice + (extraMins * ratePerMin);
            }
        }
    }
}
