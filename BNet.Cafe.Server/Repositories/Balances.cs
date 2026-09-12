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
    public class Balances
    {
        private readonly DBContext DBContext = new DBContext();
        private readonly DBScriptService dBScriptService = new DBScriptService();
        private readonly GridviewPaginationService paginationService = new GridviewPaginationService();

        public bool Create(string userId, string duration, string amount, string description)
        {
            var scripts = new Dictionary<string, string>();
            string DBUserId = dBScriptService.CleanUpToUpper(userId);
            string DBDescription = dBScriptService.CleanUpToUpper(description);

            dBScriptService.AddIfNotNullOrEmpty(scripts, "DBUserId", DBUserId);
            dBScriptService.AddIfNotNullOrEmpty(scripts, "DBDescription", DBDescription);
            dBScriptService.AddIfNotNullOrEmpty(scripts, "DBDuration", duration);
            dBScriptService.AddIfNotNullOrEmpty(scripts, "DBAmount", amount);
            dBScriptService.AddIfNotNullOrEmpty(scripts, "DBDateCreated", TimeService.Get().ToString("yyyy-MM-dd"));
            dBScriptService.AddIfNotNullOrEmpty(scripts, "DBTimeCreated", TimeService.Get().ToString("HH:mm:ss"));
            dBScriptService.AddIfNotNullOrEmpty(scripts, "DBIsDeleted", "FALSE");

            Page page = HttpContext.Current.Handler as Page;
            string template = HttpContext.Current.Server.MapPath("~/Databases/Queries/Balances/Create.sql");
            string sql = dBScriptService.Scripts(scripts, template);

            return DBContext.SqlExecuteAsync(sql);
        }

        public double GetBalanceByUserId(string id)
        {
            var scripts = new Dictionary<string, string>();
            string DBUserId = dBScriptService.CleanUpToUpper(id);

            dBScriptService.AddIfNotNullOrEmpty(scripts, "DBUserId", DBUserId);

            Page page = HttpContext.Current.Handler as Page;
            string template = HttpContext.Current.Server.MapPath("~/Databases/Queries/Balances/GetBalanceByUserId.sql");
            string sql = dBScriptService.Scripts(scripts, template);

            DataTable dataTable = DBContext.SqlDataAdapterAsync(sql);
            double totalBalance = 0;
            for (int i = 0; i < dataTable.Rows.Count; i++)
            {
                string balance = dataTable.Rows[i]["DBTotalDuration"].ToString();
                totalBalance = Convert.ToDouble(balance);
            }
            return totalBalance;
        }
    }
}