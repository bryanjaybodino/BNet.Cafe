using BNet.Cafe.Server.Databases;
using BNet.Cafe.Server.Services;
using System;
using System.Collections.Generic;
using System.Data;
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

        public DataTable GetAll(string customerType = "", int pageIndex = 0, bool isDeleted = false)
        {
            var scripts = new Dictionary<string, string>();
            string DBIsDeleted = isDeleted ? "TRUE" : "FALSE";
            string LIMIT = paginationService.SetPagination(pageIndex);
            string DBCustomerType = dBScriptService.CleanUpToUpper(customerType);

            dBScriptService.AddIfNotNullOrEmpty(scripts, "DBCustomerType", DBCustomerType);
            dBScriptService.AddIfNotNullOrEmpty(scripts, "DBIsDeleted", DBIsDeleted);
            dBScriptService.AddIfNotNullOrEmpty(scripts, "LIMIT", LIMIT);

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

            return DBContext.SqlExecuteAsync(sql);
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
        /// </summary>
        public static double CalculatePriceFromDuration(int totalMinutes, string customerType = "GUEST")
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

                // If minutes exceed maximum, return highest price
                if (totalMinutes > pricingPoints.Last().Minutes)
                {
                    return pricingPoints.Last().Price;
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
        /// Calculate duration (minutes) from price using database intervals
        /// Uses reverse linear interpolation
        /// </summary>
        public static int CalculateDurationFromPrice(double amount, string customerType = "GUEST")
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

                // Convert to list and sort by minutes
                var pricingPoints = new List<PricingPoint>();
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

                // If amount exceeds maximum price, return maximum minutes
                if (amount > pricingPoints.Last().Price)
                {
                    return pricingPoints.Last().Minutes;
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

        // Helper class for pricing points
        private class PricingPoint
        {
            public int Minutes { get; set; }
            public double Price { get; set; }
        }
    }
}