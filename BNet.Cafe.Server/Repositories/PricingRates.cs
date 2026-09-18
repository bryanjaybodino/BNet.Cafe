using BNet.Cafe.Server.Databases;
using BNet.Cafe.Server.Services;
using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Web;
using System.Web.UI;

namespace BNet.Cafe.Server.Repositories
{
    public class PricingRates
    {
        private readonly DBContext DBContext = new DBContext();
        private readonly DBScriptService dBScriptService = new DBScriptService();
        private readonly GridviewPaginationService paginationService = new GridviewPaginationService();

        public DataTable GetAll(string customerType = "", int pageIndex = -1, bool isDeleted = false)
        {
            var scripts = new Dictionary<string, string>();
            string DBIsDeleted = isDeleted ? "TRUE" : "FALSE";
            string LIMIT = paginationService.SetPagination(pageIndex);
            string DBCustomerType = dBScriptService.CleanUpToUpper(customerType);

            dBScriptService.AddIfNotNullOrEmpty(scripts, "DBCustomerType", DBCustomerType);
            dBScriptService.AddIfNotNullOrEmpty(scripts, "DBIsDeleted", DBIsDeleted);
            if (pageIndex >= 0)
            {
                dBScriptService.AddIfNotNullOrEmpty(scripts, "LIMIT", LIMIT);
            }


            Page page = HttpContext.Current.Handler as Page;
            string template = HttpContext.Current.Server.MapPath("~/Databases/Queries/PricingRates/GetAll.sql");
            string sql = dBScriptService.Scripts(scripts, template);

            return DBContext.SqlDataAdapterAsync(sql);
        }

        public DataTable GetById(string id)
        {
            var scripts = new Dictionary<string, string>();
            string DBId = dBScriptService.CleanUpToUpper(id);

            dBScriptService.AddIfNotNullOrEmpty(scripts, "DBId", DBId);

            Page page = HttpContext.Current.Handler as Page;
            string template = HttpContext.Current.Server.MapPath("~/Databases/Queries/PricingRates/GetById.sql");
            string sql = dBScriptService.Scripts(scripts, template);

            return DBContext.SqlDataAdapterAsync(sql);
        }

        public bool Create(string customerType, string minutes, string price)
        {
            var scripts = new Dictionary<string, string>();
            string DBCustomerType = dBScriptService.CleanUpToUpper(customerType);
            string DBMinutes = dBScriptService.CleanUpToUpper(minutes);
            string DBPrice = dBScriptService.CleanUpToUpper(price);

            dBScriptService.AddIfNotNullOrEmpty(scripts, "DBCustomerType", DBCustomerType);
            dBScriptService.AddIfNotNullOrEmpty(scripts, "DBMinutes", DBMinutes);
            dBScriptService.AddIfNotNullOrEmpty(scripts, "DBPrice", DBPrice);
            dBScriptService.AddIfNotNullOrEmpty(scripts, "DBDateCreated", TimeService.Get().ToString("yyyy-MM-dd"));
            dBScriptService.AddIfNotNullOrEmpty(scripts, "DBTimeCreated", TimeService.Get().ToString("HH:mm:ss"));
            dBScriptService.AddIfNotNullOrEmpty(scripts, "DBIsDeleted", "FALSE");

            Page page = HttpContext.Current.Handler as Page;
            string template = HttpContext.Current.Server.MapPath("~/Databases/Queries/PricingRates/Create.sql");
            string sql = dBScriptService.Scripts(scripts, template);
            var result = DBContext.SqlExecuteReaderAsync(sql);
            int pricingRateExist = Convert.ToInt32(result["PricingRateExist"]);

            bool name = pricingRateExist != 1;
            return name;
        }

        public bool Update(string id, string customerType, string minutes, string price)
        {
            var scripts = new Dictionary<string, string>();
            string DBId = dBScriptService.CleanUpToUpper(id);
            string DBCustomerType = dBScriptService.CleanUpToUpper(customerType);
            string DBMinutes = dBScriptService.CleanUpToUpper(minutes);
            string DBPrice = dBScriptService.CleanUpToUpper(price);

            dBScriptService.AddIfNotNullOrEmpty(scripts, "DBId", DBId);
            dBScriptService.AddIfNotNullOrEmpty(scripts, "DBCustomerType", DBCustomerType);
            dBScriptService.AddIfNotNullOrEmpty(scripts, "DBMinutes", DBMinutes);
            dBScriptService.AddIfNotNullOrEmpty(scripts, "DBPrice", DBPrice);

            Page page = HttpContext.Current.Handler as Page;
            string template = HttpContext.Current.Server.MapPath("~/Databases/Queries/PricingRates/Update.sql");
            string sql = dBScriptService.Scripts(scripts, template);

            return DBContext.SqlExecuteAsync(sql);
        }

        public bool Delete(string id)
        {
            var scripts = new Dictionary<string, string>();
            string DBId = dBScriptService.CleanUpToUpper(id);

            dBScriptService.AddIfNotNullOrEmpty(scripts, "DBId", DBId);

            Page page = HttpContext.Current.Handler as Page;
            string template = HttpContext.Current.Server.MapPath("~/Databases/Queries/PricingRates/DeleteById.sql");
            string sql = dBScriptService.Scripts(scripts, template);

            return DBContext.SqlExecuteAsync(sql);
        }

        /// <summary>
        /// Calculate price from duration (minutes) using database intervals
        /// Uses linear interpolation between price points
        /// 
        /// SOLUTION OPTIONS for pricing beyond maximum interval:
        /// - OPTION 1: Linear Extrapolation (continues pricing at the same rate)
        /// - OPTION 2: Price Cap (stops at maximum price)
        /// - OPTION 3: Hourly Rate After Cap (fixed price + per-minute rate)
        /// </summary>
        public static double CalculatePriceFromDuration(int totalMinutes, string customerType = "GUEST / WALK-IN", int pricingModel = 1)
        {
            if (totalMinutes <= 0) return 0.0;

            try
            {
                // Get pricing intervals from database
                PricingRates repo = new PricingRates();
                DataTable dt = repo.GetAll(customerType);

                if (dt == null || dt.Rows.Count == 0)
                {
                    // Fallback if no pricing found
                    return 0.0;
                }

                // Convert to list and sort by minutes
                var pricingPoints = new List<PricingPoint>();
                pricingPoints.Add(new PricingPoint { Minutes = 0, Price = 0 });
                foreach (DataRow row in dt.Rows)
                {
                    if (int.TryParse(row["DBMinutes"].ToString(), out int mins) &&
                        double.TryParse(row["DBPrice"].ToString(), out double price))
                    {
                        pricingPoints.Add(new PricingPoint { Minutes = mins, Price = price });
                    }
                }

                pricingPoints = pricingPoints.OrderBy(p => p.Minutes).ToList();

                if (pricingPoints.Count == 0)
                    return 0.0;

                // If exact match found
                var exactMatch = pricingPoints.FirstOrDefault(p => p.Minutes == totalMinutes);
                if (exactMatch != null)
                    return exactMatch.Price;

                // If minutes exceed maximum
                if (totalMinutes > pricingPoints.Last().Minutes)
                {
                    return HandlePricingBeyondMaximum(totalMinutes, pricingPoints, pricingModel);
                }

                // Linear interpolation between two points
                for (int i = 0; i < pricingPoints.Count - 1; i++)
                {
                    if (totalMinutes >= pricingPoints[i].Minutes && totalMinutes < pricingPoints[i + 1].Minutes)
                    {
                        double x1 = pricingPoints[i].Minutes;
                        double y1 = pricingPoints[i].Price;
                        double x2 = pricingPoints[i + 1].Minutes;
                        double y2 = pricingPoints[i + 1].Price;

                        // Linear interpolation formula: y = y1 + (x - x1) * (y2 - y1) / (x2 - x1)
                        double interpolatedPrice = y1 + (totalMinutes - x1) * (y2 - y1) / (x2 - x1);
                        return Math.Round(interpolatedPrice, 2);
                    }
                }

                // Fallback: return the closest price
                return pricingPoints[0].Price;
            }
            catch
            {
                return 0.0;
            }
        }

        /// <summary>
        /// Handle pricing for minutes beyond the maximum interval
        /// </summary>
        /// <param name="totalMinutes">Total minutes used</param>
        /// <param name="pricingPoints">Sorted pricing points</param>
        /// <param name="model">1 = Linear Extrapolation, 2 = Price Cap, 3 = Hourly Rate</param>
        private static double HandlePricingBeyondMaximum(int totalMinutes, List<PricingPoint> pricingPoints, int model)
        {
            switch (model)
            {
                case 1: // OPTION 1: Linear Extrapolation
                    // Continue pricing at the same rate as the last interval
                    return LinearExtrapolation(totalMinutes, pricingPoints);

                case 2: // OPTION 2: Price Cap (original behavior)
                    return pricingPoints.Last().Price;

                case 3: // OPTION 3: Hourly Rate After Cap
                    // Fixed price for max interval + per-minute rate for additional minutes
                    return PriceCapWithHourlyRate(totalMinutes, pricingPoints);

                default:
                    return pricingPoints.Last().Price;
            }
        }

        /// <summary>
        /// OPTION 1: Linear Extrapolation
        /// Continues the pricing rate from the last two intervals
        /// Example: If 300min=200 and 360min=250, rate is 0.833/min
        /// For 420min: 250 + (420-360) * 0.833 = 299.99
        /// </summary>
        private static double LinearExtrapolation(int totalMinutes, List<PricingPoint> pricingPoints)
        {
            if (pricingPoints.Count < 2)
                return pricingPoints.Last().Price;

            // Get the rate from the last two points
            double lastMinutes = pricingPoints.Last().Minutes;
            double lastPrice = pricingPoints.Last().Price;
            double prevMinutes = pricingPoints[pricingPoints.Count - 2].Minutes;
            double prevPrice = pricingPoints[pricingPoints.Count - 2].Price;

            // Calculate the rate (price per minute)
            double ratePerMinute = (lastPrice - prevPrice) / (lastMinutes - prevMinutes);

            // Extrapolate beyond maximum
            double extrapolatedPrice = lastPrice + (totalMinutes - lastMinutes) * ratePerMinute;
            return Math.Round(extrapolatedPrice, 2);
        }

        /// <summary>
        /// OPTION 3: Price Cap with Hourly Rate
        /// Charge maximum price for the included time, then add per-minute rate
        /// Example: If cap is 6hrs=250, add 0.50 pesos per minute after
        /// For 7hrs (420min): 250 + (420-360) * ratePerMinute
        /// </summary>
        private static double PriceCapWithHourlyRate(int totalMinutes, List<PricingPoint> pricingPoints)
        {
            double maxPrice = pricingPoints.Last().Price;
            double maxMinutes = pricingPoints.Last().Minutes;

            if (pricingPoints.Count < 2)
                return maxPrice;

            // Calculate hourly rate from the last interval
            double prevMinutes = pricingPoints[pricingPoints.Count - 2].Minutes;
            double prevPrice = pricingPoints[pricingPoints.Count - 2].Price;
            double ratePerMinute = (maxPrice - prevPrice) / (maxMinutes - prevMinutes);

            // Add the overage charges
            int overtimeMinutes = totalMinutes - (int)maxMinutes;
            double overtimeCharge = overtimeMinutes * ratePerMinute;

            double totalPrice = maxPrice + overtimeCharge;
            return Math.Round(totalPrice, 2);
        }

        /// <summary>
        /// Calculate duration (minutes) from price using database intervals
        /// Uses reverse linear interpolation
        /// </summary>
        public static int CalculateDurationFromPrice(double amount, string customerType = "GUEST / WALK-IN", int pricingModel = 1)
        {
            if (amount <= 0) return 0;

            try
            {
                // Get pricing intervals from database
                PricingRates repo = new PricingRates();
                DataTable dt = repo.GetAll(customerType);

                if (dt == null || dt.Rows.Count == 0)
                {
                    return 0;
                }

                // Convert to list and sort by price
                var pricingPoints = new List<PricingPoint>();
                pricingPoints.Add(new PricingPoint { Minutes = 0, Price = 0 });
                foreach (DataRow row in dt.Rows)
                {
                    if (int.TryParse(row["DBMinutes"].ToString(), out int mins) &&
                        double.TryParse(row["DBPrice"].ToString(), out double price))
                    {
                        pricingPoints.Add(new PricingPoint { Minutes = mins, Price = price });
                    }
                }

                pricingPoints = pricingPoints.OrderBy(p => p.Price).ToList();

                if (pricingPoints.Count == 0)
                    return 0;

                // If exact match found
                var exactMatch = pricingPoints.FirstOrDefault(p => Math.Abs(p.Price - amount) < 0.01);
                if (exactMatch != null)
                    return exactMatch.Minutes;

                // If amount exceeds maximum price
                if (amount > pricingPoints.Last().Price)
                {
                    return HandleDurationBeyondMaxPrice(amount, pricingPoints, pricingModel);
                }

                // Reverse linear interpolation between two points
                for (int i = 0; i < pricingPoints.Count - 1; i++)
                {
                    if (amount >= pricingPoints[i].Price && amount < pricingPoints[i + 1].Price)
                    {
                        double y1 = pricingPoints[i].Price;
                        double x1 = pricingPoints[i].Minutes;
                        double y2 = pricingPoints[i + 1].Price;
                        double x2 = pricingPoints[i + 1].Minutes;

                        // Reverse interpolation: x = x1 + (y - y1) * (x2 - x1) / (y2 - y1)
                        double interpolatedMinutes = x1 + (amount - y1) * (x2 - x1) / (y2 - y1);
                        return (int)Math.Round(interpolatedMinutes);
                    }
                }

                // Fallback: return the first interval
                return pricingPoints[0].Minutes;
            }
            catch
            {
                return 0;
            }
        }

        /// <summary>
        /// Handle duration calculation for prices beyond the maximum
        /// </summary>
        private static int HandleDurationBeyondMaxPrice(double amount, List<PricingPoint> pricingPoints, int pricingModel)
        {
            switch (pricingModel)
            {
                case 1: // Linear Extrapolation
                    return ReverseLinearExtrapolation(amount, pricingPoints);

                case 2: // Price Cap (original behavior)
                    return (int)pricingPoints.Last().Minutes;

                case 3: // Hourly Rate After Cap
                    return ReversePriceCapWithHourlyRate(amount, pricingPoints);

                default:
                    return (int)pricingPoints.Last().Minutes;
            }
        }

        private static int ReverseLinearExtrapolation(double amount, List<PricingPoint> pricingPoints)
        {
            if (pricingPoints.Count < 2)
                return (int)pricingPoints.Last().Minutes;

            double lastMinutes = pricingPoints.Last().Minutes;
            double lastPrice = pricingPoints.Last().Price;
            double prevMinutes = pricingPoints[pricingPoints.Count - 2].Minutes;
            double prevPrice = pricingPoints[pricingPoints.Count - 2].Price;

            double ratePerMinute = (lastPrice - prevPrice) / (lastMinutes - prevMinutes);

            double extrapolatedMinutes = lastMinutes + (amount - lastPrice) / ratePerMinute;
            return (int)Math.Round(extrapolatedMinutes);
        }

        private static int ReversePriceCapWithHourlyRate(double amount, List<PricingPoint> pricingPoints)
        {
            double maxPrice = pricingPoints.Last().Price;
            double maxMinutes = pricingPoints.Last().Minutes;

            if (amount <= maxPrice)
                return (int)maxMinutes;

            if (pricingPoints.Count < 2)
                return (int)maxMinutes;

            double prevMinutes = pricingPoints[pricingPoints.Count - 2].Minutes;
            double prevPrice = pricingPoints[pricingPoints.Count - 2].Price;
            double ratePerMinute = (maxPrice - prevPrice) / (maxMinutes - prevMinutes);

            double overtimeMinutes = (amount - maxPrice) / ratePerMinute;
            double totalMinutes = maxMinutes + overtimeMinutes;

            return (int)Math.Round(totalMinutes);
        }

        // Helper class for pricing points
        private class PricingPoint
        {
            public int Minutes { get; set; }
            public double Price { get; set; }
        }
    }
}