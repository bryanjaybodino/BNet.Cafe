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
    public class PointsRates
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
            string template = HttpContext.Current.Server.MapPath("~/Databases/Queries/PointsRates/GetAll.sql");
            string sql = dBScriptService.Scripts(scripts, template);

            return DBContext.SqlDataAdapterAsync(sql);
        }

        public DataTable GetById(string id)
        {
            var scripts = new Dictionary<string, string>();
            string DBId = dBScriptService.CleanUpToUpper(id);

            dBScriptService.AddIfNotNullOrEmpty(scripts, "DBId", DBId);

            Page page = HttpContext.Current.Handler as Page;
            string template = HttpContext.Current.Server.MapPath("~/Databases/Queries/PointsRates/GetById.sql");
            string sql = dBScriptService.Scripts(scripts, template);

            return DBContext.SqlDataAdapterAsync(sql);
        }

        public bool Create(string customerType, string minutes, string points)
        {
            var scripts = new Dictionary<string, string>();
            string DBCustomerType = dBScriptService.CleanUpToUpper(customerType);
            string DBMinutes = dBScriptService.CleanUpToUpper(minutes);
            string DBPoints = dBScriptService.CleanUpToUpper(points);

            dBScriptService.AddIfNotNullOrEmpty(scripts, "DBCustomerType", DBCustomerType);
            dBScriptService.AddIfNotNullOrEmpty(scripts, "DBMinutes", DBMinutes);
            dBScriptService.AddIfNotNullOrEmpty(scripts, "DBPoints", DBPoints);
            dBScriptService.AddIfNotNullOrEmpty(scripts, "DBDateCreated", TimeService.Get().ToString("yyyy-MM-dd"));
            dBScriptService.AddIfNotNullOrEmpty(scripts, "DBTimeCreated", TimeService.Get().ToString("HH:mm:ss"));
            dBScriptService.AddIfNotNullOrEmpty(scripts, "DBIsDeleted", "FALSE");

            Page page = HttpContext.Current.Handler as Page;
            string template = HttpContext.Current.Server.MapPath("~/Databases/Queries/PointsRates/Create.sql");
            string sql = dBScriptService.Scripts(scripts, template);
            var result = DBContext.SqlExecuteReaderAsync(sql);
            int customerPointsExist = Convert.ToInt32(result["CustomerPointsExist"]);

            bool success = customerPointsExist != 1;
            return success;
        }

        public bool Update(string id, string customerType, string minutes, string points)
        {
            var scripts = new Dictionary<string, string>();
            string DBId = dBScriptService.CleanUpToUpper(id);
            string DBCustomerType = dBScriptService.CleanUpToUpper(customerType);
            string DBMinutes = dBScriptService.CleanUpToUpper(minutes);
            string DBPoints = dBScriptService.CleanUpToUpper(points);

            dBScriptService.AddIfNotNullOrEmpty(scripts, "DBId", DBId);
            dBScriptService.AddIfNotNullOrEmpty(scripts, "DBCustomerType", DBCustomerType);
            dBScriptService.AddIfNotNullOrEmpty(scripts, "DBMinutes", DBMinutes);
            dBScriptService.AddIfNotNullOrEmpty(scripts, "DBPoints", DBPoints);

            Page page = HttpContext.Current.Handler as Page;
            string template = HttpContext.Current.Server.MapPath("~/Databases/Queries/PointsRates/Update.sql");
            string sql = dBScriptService.Scripts(scripts, template);

            return DBContext.SqlExecuteAsync(sql);
        }

        public bool Delete(string id)
        {
            var scripts = new Dictionary<string, string>();
            string DBId = dBScriptService.CleanUpToUpper(id);

            dBScriptService.AddIfNotNullOrEmpty(scripts, "DBId", DBId);

            Page page = HttpContext.Current.Handler as Page;
            string template = HttpContext.Current.Server.MapPath("~/Databases/Queries/PointsRates/DeleteById.sql");
            string sql = dBScriptService.Scripts(scripts, template);

            return DBContext.SqlExecuteAsync(sql);
        }

        /// <summary>
        /// Calculates points from duration (minutes) using database intervals, 
        /// supporting linear interpolation for partial times (e.g. 30 mins = 0.5 points if 60 mins = 1 point)
        /// </summary>
        public static double CalculatePointsFromDuration(int totalMinutes, string customerType = "GUEST / WALK-IN")
        {
            if (totalMinutes <= 0) return 0.0;

            try
            {
                PointsRates repo = new PointsRates();
                DataTable dt = repo.GetAll(customerType);

                if (dt == null || dt.Rows.Count == 0)
                {
                    return 0.0;
                }

                var pointsPoints = new List<PointsPoint>();
                pointsPoints.Add(new PointsPoint { Minutes = 0, Points = 0 });
                foreach (DataRow row in dt.Rows)
                {
                    if (int.TryParse(row["DBMinutes"].ToString(), out int mins) &&
                        double.TryParse(row["DBPoints"].ToString(), out double pts))
                    {
                        pointsPoints.Add(new PointsPoint { Minutes = mins, Points = pts });
                    }
                }

                pointsPoints = pointsPoints.OrderBy(p => p.Minutes).ToList();

                if (pointsPoints.Count == 0)
                    return 0.0;

                // Exact match check
                var exactMatch = pointsPoints.FirstOrDefault(p => p.Minutes == totalMinutes);
                if (exactMatch != null)
                    return exactMatch.Points;

                // Beyond max limit rule (linear extrapolation or cap)
                if (totalMinutes > pointsPoints.Last().Minutes)
                {
                    return LinearExtrapolationPoints(totalMinutes, pointsPoints);
                }

                // Linear interpolation for intermediate durations (e.g., 30 mins)
                for (int i = 0; i < pointsPoints.Count - 1; i++)
                {
                    if (totalMinutes >= pointsPoints[i].Minutes && totalMinutes < pointsPoints[i + 1].Minutes)
                    {
                        double x1 = pointsPoints[i].Minutes;
                        double y1 = pointsPoints[i].Points;
                        double x2 = pointsPoints[i + 1].Minutes;
                        double y2 = pointsPoints[i + 1].Points;

                        double interpolatedPoints = y1 + (totalMinutes - x1) * (y2 - y1) / (x2 - x1);
                        return Math.Round(interpolatedPoints, 2);
                    }
                }

                return pointsPoints[0].Points;
            }
            catch
            {
                return 0.0;
            }
        }

        private static double LinearExtrapolationPoints(int totalMinutes, List<PointsPoint> pointsPoints)
        {
            if (pointsPoints.Count < 2)
                return pointsPoints.Last().Points;

            double lastMinutes = pointsPoints.Last().Minutes;
            double lastPoints = pointsPoints.Last().Points;
            double prevMinutes = pointsPoints[pointsPoints.Count - 2].Minutes;
            double prevPoints = pointsPoints[pointsPoints.Count - 2].Points;

            double ratePerMinute = (lastPoints - prevPoints) / (lastMinutes - prevMinutes);
            double extrapolatedPoints = lastPoints + (totalMinutes - lastMinutes) * ratePerMinute;
            return Math.Round(extrapolatedPoints, 2);
        }

        private class PointsPoint
        {
            public int Minutes { get; set; }
            public double Points { get; set; }
        }
    }
}