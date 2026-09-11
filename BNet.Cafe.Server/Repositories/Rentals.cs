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

        public DataTable GetAll(string search = "", string computerId = "", int pageIndex = 0, bool isDeleted = false)
        {
            var scripts = new Dictionary<string, string>();
            string DBSearch = dBScriptService.CleanUpToUpper(search);
            string DBComputerId = dBScriptService.CleanUpToUpper(computerId);
            string DBIsDeleted = isDeleted ? "TRUE" : "FALSE";
            string LIMIT = paginationService.SetPagination(pageIndex);

            dBScriptService.AddIfNotNullOrEmpty(scripts, "DBIsDeleted", DBIsDeleted);
            dBScriptService.AddIfNotNullOrEmpty(scripts, "DBSearch", DBSearch);
            dBScriptService.AddIfNotNullOrEmpty(scripts, "DBComputerId", DBComputerId);
            dBScriptService.AddIfNotNullOrEmpty(scripts, "LIMIT", LIMIT);

            Page page = HttpContext.Current.Handler as Page;
            string template = page.Server.MapPath("~/Databases/Queries/Rentals/GetAll.sql");
            string sql = dBScriptService.Scripts(scripts, template);

            return DBContext.SqlDataAdapterAsync(sql);
        }

        public DataTable GetByComputerId(string id)
        {
            var scripts = new Dictionary<string, string>();
            string DBComputerId = dBScriptService.CleanUpToUpper(id);

            dBScriptService.AddIfNotNullOrEmpty(scripts, "DBComputerId", DBComputerId);

            Page page = HttpContext.Current.Handler as Page;
            string template = page.Server.MapPath("~/Databases/Queries/Rentals/GetByComputerId.sql");
            string sql = dBScriptService.Scripts(scripts, template);

            return DBContext.SqlDataAdapterAsync(sql);
        }

        public bool Create(string computerId, string userId, string duration, string amount)
        {
            var scripts = new Dictionary<string, string>();
            string DBComputerId = dBScriptService.CleanUpToUpper(computerId);
            string DBUserId = dBScriptService.CleanUpToUpper(userId);

            dBScriptService.AddIfNotNullOrEmpty(scripts, "DBComputerId", DBComputerId);
            dBScriptService.AddIfNotNullOrEmpty(scripts, "DBUserId", DBUserId);
            dBScriptService.AddIfNotNullOrEmpty(scripts, "DBDuration", duration);
            dBScriptService.AddIfNotNullOrEmpty(scripts, "DBAmount", amount);
            dBScriptService.AddIfNotNullOrEmpty(scripts, "DBDateCreated", TimeService.Get().ToString("yyyy-MM-dd"));
            dBScriptService.AddIfNotNullOrEmpty(scripts, "DBTimeCreated", TimeService.Get().ToString("HH:mm:ss"));
            dBScriptService.AddIfNotNullOrEmpty(scripts, "DBIsDeleted", "FALSE");

            Page page = HttpContext.Current.Handler as Page;
            string template = page.Server.MapPath("~/Databases/Queries/Rentals/Create.sql");
            string sql = dBScriptService.Scripts(scripts, template);

            return DBContext.SqlExecuteAsync(sql);
        }

        public bool Update(string id, string computerId, string userId, string duration, string amount)
        {
            var scripts = new Dictionary<string, string>();
            string DBId = dBScriptService.CleanUpToUpper(id);
            string DBComputerId = dBScriptService.CleanUpToUpper(computerId);
            string DBUserId = dBScriptService.CleanUpToUpper(userId);

            dBScriptService.AddIfNotNullOrEmpty(scripts, "DBId", DBId);
            dBScriptService.AddIfNotNullOrEmpty(scripts, "DBComputerId", DBComputerId);
            dBScriptService.AddIfNotNullOrEmpty(scripts, "DBUserId", DBUserId);
            dBScriptService.AddIfNotNullOrEmpty(scripts, "DBDuration", duration);
            dBScriptService.AddIfNotNullOrEmpty(scripts, "DBAmount", amount);

            Page page = HttpContext.Current.Handler as Page;
            string template = page.Server.MapPath("~/Databases/Queries/Rentals/Update.sql");
            string sql = dBScriptService.Scripts(scripts, template);

            return DBContext.SqlExecuteAsync(sql);
        }

        public bool Delete(string id)
        {
            var scripts = new Dictionary<string, string>();
            string DBId = dBScriptService.CleanUpToUpper(id);

            dBScriptService.AddIfNotNullOrEmpty(scripts, "DBId", DBId);

            Page page = HttpContext.Current.Handler as Page;
            string template = page.Server.MapPath("~/Databases/Queries/Rentals/DeleteById.sql");
            string sql = dBScriptService.Scripts(scripts, template);

            return DBContext.SqlExecuteAsync(sql);
        }

        public CountRentals GetCount(string computerId = "")
        {
            CountRentals countRentals = new CountRentals();
            var scripts = new Dictionary<string, string>();
            string DBComputerId = dBScriptService.CleanUpToUpper(computerId);
            dBScriptService.AddIfNotNullOrEmpty(scripts, "DBIsDeleted", "FALSE");
            dBScriptService.AddIfNotNullOrEmpty(scripts, "DBComputerId", DBComputerId);

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