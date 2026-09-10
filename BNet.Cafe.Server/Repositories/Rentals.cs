using BNet.Cafe.Server.Databases;
using BNet.Cafe.Server.Services;
using System;
using System.Collections.Generic;
using System.Data;
using System.Web;
using System.Web.UI;

namespace BNet.Cafe.Server.Repositories
{
    public class Rentals
    {
        private readonly DBContext DBContext = new DBContext();
        private readonly DBScriptService dBScriptService = new DBScriptService();
        private readonly GridviewPaginationService paginationService = new GridviewPaginationService();

        public DataTable GetAll(string search = "", int pageIndex = 0, bool isDeleted = false)
        {
            var scripts = new Dictionary<string, string>
            {
                { "DBIsDeleted", isDeleted ? "TRUE" : "FALSE" },
                { "DBSearch", dBScriptService.CleanUpToUpper(search) },
                { "LIMIT", $"{paginationService.SetPagination(pageIndex)}" }
            };

            Page page = HttpContext.Current.Handler as Page;
            string template = page.Server.MapPath("~/Databases/Queries/Rentals/GetAll.sql");
            string sql = dBScriptService.Scripts(scripts, template);

            return DBContext.SqlDataAdapterAsync(sql);
        }

        public DataTable GetByComputerId(string id)
        {
            var scripts = new Dictionary<string, string>
            {
                { "DBComputerId", id },
            };

            Page page = HttpContext.Current.Handler as Page;
            string template = page.Server.MapPath("~/Databases/Queries/Rentals/GetByComputerId.sql");
            string sql = dBScriptService.Scripts(scripts, template);

            return DBContext.SqlDataAdapterAsync(sql);
        }

        public bool Create(string computerId, string customerId, string duration, string amount)
        {
            var scripts = new Dictionary<string, string>
            {
                { "DBComputerId", computerId },
                { "DBUserId", customerId },
                { "DBDuration", duration },
                { "DBAmount", amount },
                { "DBDateCreated", TimeService.Get().ToString("yyyy-MM-dd") },
                { "DBTimeCreated", TimeService.Get().ToString("HH:mm:ss") },
                { "DBIsDeleted", "FALSE" }
            };

            Page page = HttpContext.Current.Handler as Page;
            string template = page.Server.MapPath("~/Databases/Queries/Rentals/Create.sql");
            string sql = dBScriptService.Scripts(scripts, template);

            return DBContext.SqlExecuteAsync(sql);
        }

        public bool Update(string id, string computerId, string customerId, string duration, string amount)
        {
            var scripts = new Dictionary<string, string>
            {
                { "DBId", id },
                { "DBComputerId", computerId },
                { "DBUserId", customerId },
                { "DBDuration", duration },
                { "DBAmount", amount }
            };

            Page page = HttpContext.Current.Handler as Page;
            string template = page.Server.MapPath("~/Databases/Queries/Rentals/Update.sql");
            string sql = dBScriptService.Scripts(scripts, template);

            return DBContext.SqlExecuteAsync(sql);
        }

        public bool Delete(string id)
        {
            var scripts = new Dictionary<string, string>
            {
                { "DBId", id }
            };

            Page page = HttpContext.Current.Handler as Page;
            string template = page.Server.MapPath("~/Databases/Queries/Rentals/DeleteById.sql");
            string sql = dBScriptService.Scripts(scripts, template);

            return DBContext.SqlExecuteAsync(sql);
        }




        public CountRentals GetCount()
        {
            CountRentals countRentals = new CountRentals();
            var scripts = new Dictionary<string, string>
            {
                { "DBIsDeleted", "FALSE" },
            };
            Page page = HttpContext.Current.Handler as Page;
            string template = page.Server.MapPath("~/Databases/Queries/Rentals/GetCount.sql");
            string sql = dBScriptService.Scripts(scripts, template);
            var data = DBContext.SqlDataAdapterAsync(sql);

            string total = "0";
            string users = "0";
            string walkIn = "0";
            string income = "0";
            string duration = "0";

            for (int i = 0; i < data.Rows.Count; i++)
            {
                total = data.Rows[i]["DBTotalTransactions"].ToString();
                users = data.Rows[i]["DBTotalUsers"].ToString();
                walkIn = data.Rows[i]["DBTotalWalkIn"].ToString();
                income = data.Rows[i]["DBTotalIncome"].ToString();
                duration = data.Rows[i]["DBTotalFormattedDuration"].ToString();
            }
            countRentals.Total = total;
            countRentals.Users = users;
            countRentals.WalkIn = walkIn;
            countRentals.Income = income;
            countRentals.Duration = duration;
            return countRentals;
        }
        public class CountRentals
        {
            public string Total { get; set; }
            public string Users { get; set; }
            public string WalkIn { get; set; }
            public string Income { get; set; }
            public string Duration { get; set; }
        }
    }
}