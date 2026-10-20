using BNet.Cafe.Server.Databases;
using BNet.Cafe.Server.Services;
using System;
using System.Collections.Generic;
using System.Data;
using System.Web;
using System.Web.UI;

namespace BNet.Cafe.Server.Repositories
{
    public class Points
    {
        private readonly DBContext DBContext = new DBContext();
        private readonly DBScriptService dBScriptService = new DBScriptService();
        private readonly GridviewPaginationService paginationService = new GridviewPaginationService();

        public bool Create(string userId, string points, string description)
        {
            var scripts = new Dictionary<string, string>();
            string DBUserId = dBScriptService.CleanUpToUpper(userId);
            string DBDescription = dBScriptService.CleanUpToUpper(description);

            dBScriptService.AddIfNotNullOrEmpty(scripts, "DBUserId", DBUserId);
            dBScriptService.AddIfNotNullOrEmpty(scripts, "DBDescription", DBDescription);
            dBScriptService.AddIfNotNullOrEmpty(scripts, "DBPoints", points);
            dBScriptService.AddIfNotNullOrEmpty(scripts, "DBDateCreated", TimeService.Get().ToString("yyyy-MM-dd"));
            dBScriptService.AddIfNotNullOrEmpty(scripts, "DBTimeCreated", TimeService.Get().ToString("HH:mm:ss"));
            dBScriptService.AddIfNotNullOrEmpty(scripts, "DBIsDeleted", "FALSE");

            Page page = HttpContext.Current.Handler as Page;
            string template = HttpContext.Current.Server.MapPath("~/Databases/Queries/Points/Create.sql");
            string sql = dBScriptService.Scripts(scripts, template);

            return DBContext.SqlExecuteAsync(sql);
        }

        public DataTable GetAll(string search = "", string userId = "", string dateRange = "", int pageIndex = 0, bool isDeleted = false)
        {
            var scripts = new Dictionary<string, string>();
            string DBSearch = dBScriptService.CleanUpToUpper(search);
            string DBUserId = dBScriptService.CleanUpToUpper(userId);
            string DBIsDeleted = isDeleted ? "TRUE" : "FALSE";
            string LIMIT = paginationService.SetPagination(pageIndex);

            string DBDateStart = "";
            string DBDateEnd = "";
            if (dateRange.Contains(","))
            {
                var date = dateRange.Split(',');
                DBDateStart = date[0];
                DBDateEnd = date[1];
            }

            dBScriptService.AddIfNotNullOrEmpty(scripts, "DBIsDeleted", DBIsDeleted);
            dBScriptService.AddIfNotNullOrEmpty(scripts, "DBUserId", DBUserId);
            dBScriptService.AddIfNotNullOrEmpty(scripts, "DBSearch", DBSearch);
            dBScriptService.AddIfNotNullOrEmpty(scripts, "DBDateStart", DBDateStart);
            dBScriptService.AddIfNotNullOrEmpty(scripts, "DBDateEnd", DBDateEnd);
            if (pageIndex >= 0)
            {
                dBScriptService.AddIfNotNullOrEmpty(scripts, "LIMIT", LIMIT);
            }

            Page page = HttpContext.Current.Handler as Page;
            string template = HttpContext.Current.Server.MapPath("~/Databases/Queries/Points/GetAll.sql");
            string sql = dBScriptService.Scripts(scripts, template);

            return DBContext.SqlDataAdapterAsync(sql);
        }

        public CountPoints GetCount(string userId = "", string dateRange = "")
        {
            string DBDateStart = "";
            string DBDateEnd = "";
            if (dateRange.Contains(","))
            {
                var date = dateRange.Split(',');
                DBDateStart = date[0];
                DBDateEnd = date[1];
            }

            CountPoints countPoints = new CountPoints();
            var scripts = new Dictionary<string, string>();
            string DBUserId = dBScriptService.CleanUpToUpper(userId);

            dBScriptService.AddIfNotNullOrEmpty(scripts, "DBIsDeleted", "FALSE");
            dBScriptService.AddIfNotNullOrEmpty(scripts, "DBUserId", DBUserId);
            dBScriptService.AddIfNotNullOrEmpty(scripts, "DBDateStart", DBDateStart);
            dBScriptService.AddIfNotNullOrEmpty(scripts, "DBDateEnd", DBDateEnd);

            Page page = HttpContext.Current.Handler as Page;
            string template = HttpContext.Current.Server.MapPath("~/Databases/Queries/Points/GetCount.sql");
            string sql = dBScriptService.Scripts(scripts, template);
            var data = DBContext.SqlDataAdapterAsync(sql);

            string total = "0";
            string users = "0";
            string totalPoints = "0.00";

            if (data.Rows.Count > 0)
            {
                total = data.Rows[0]["DBTotalTransactions"].ToString();
                users = data.Rows[0]["DBTotalUsers"].ToString();
                totalPoints = data.Rows[0]["DBTotalPointsEarned"].ToString();
            }

            countPoints.Total = total;
            countPoints.Users = users;
            countPoints.TotalPoints = totalPoints;

            return countPoints;
        }

        public double GetPointsByUserId(string id)
        {
            var scripts = new Dictionary<string, string>();
            string DBUserId = dBScriptService.CleanUpToUpper(id);

            dBScriptService.AddIfNotNullOrEmpty(scripts, "DBUserId", DBUserId);

            Page page = HttpContext.Current.Handler as Page;
            string template = HttpContext.Current.Server.MapPath("~/Databases/Queries/Points/GetPointsByUserId.sql");
            string sql = dBScriptService.Scripts(scripts, template);

            DataTable dataTable = DBContext.SqlDataAdapterAsync(sql);
            double totalPoints = 0;
            for (int i = 0; i < dataTable.Rows.Count; i++)
            {
                string points = dataTable.Rows[i]["DBTotalPoints"].ToString();
                totalPoints = Convert.ToDouble(points);
            }
            return totalPoints;
        }

        public class CountPoints
        {
            public string Total { get; set; }
            public string Users { get; set; }
            public string TotalPoints { get; set; }
        }
    }
}