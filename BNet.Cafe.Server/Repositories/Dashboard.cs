using BNet.Cafe.Server.Databases;
using System;
using System.Collections.Generic;
using System.Data;
using System.Web;
using System.Web.UI;

namespace BNet.Cafe.Server.Repositories
{
    public class Dashboard
    {
        private readonly DBContext DBContext = new DBContext();
        private readonly DBScriptService dBScriptService = new DBScriptService();

        // Powers the Revenue Trend line chart
        public DataTable GetRevenueTrend(string dateRange = "", bool isDeleted = false)
        {
            var scripts = new Dictionary<string, string>();
            string DBIsDeleted = isDeleted ? "TRUE" : "FALSE";

            string DBDateStart = "";
            string DBDateEnd = "";
            if (dateRange.Contains(","))
            {
                var date = dateRange.Split(',');
                DBDateStart = date[0];
                DBDateEnd = date[1];
            }

            dBScriptService.AddIfNotNullOrEmpty(scripts, "DBIsDeleted", DBIsDeleted);
            dBScriptService.AddIfNotNullOrEmpty(scripts, "DBDateStart", DBDateStart);
            dBScriptService.AddIfNotNullOrEmpty(scripts, "DBDateEnd", DBDateEnd);

            Page page = HttpContext.Current.Handler as Page;
            string template = HttpContext.Current.Server.MapPath("~/Databases/Queries/Dashboard/RevenueTrend.sql");
            string sql = dBScriptService.Scripts(scripts, template);

            return DBContext.SqlDataAdapterAsync(sql);
        }

        // Powers the Computer Usage bar chart
        public DataTable GetComputerUsage(string dateRange = "", bool isDeleted = false)
        {
            var scripts = new Dictionary<string, string>();
            string DBIsDeleted = isDeleted ? "TRUE" : "FALSE";

            string DBDateStart = "";
            string DBDateEnd = "";
            if (dateRange.Contains(","))
            {
                var date = dateRange.Split(',');
                DBDateStart = date[0];
                DBDateEnd = date[1];
            }

            dBScriptService.AddIfNotNullOrEmpty(scripts, "DBIsDeleted", DBIsDeleted);
            dBScriptService.AddIfNotNullOrEmpty(scripts, "DBDateStart", DBDateStart);
            dBScriptService.AddIfNotNullOrEmpty(scripts, "DBDateEnd", DBDateEnd);

            Page page = HttpContext.Current.Handler as Page;
            string template = HttpContext.Current.Server.MapPath("~/Databases/Queries/Dashboard/ComputerUsage.sql");
            string sql = dBScriptService.Scripts(scripts, template);

            return DBContext.SqlDataAdapterAsync(sql);
        }
    }
}