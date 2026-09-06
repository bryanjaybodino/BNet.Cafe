using BNet.Cafe.Server.Databases;
using System;
using System.Collections.Generic;
using System.Data;
using System.Web;
using System.Web.UI;

namespace BNet.Cafe.Server.Services
{
    public class ComputerService
    {
        private readonly DBContext DBContext = new DBContext();
        private readonly DBScriptService dBScriptService = new DBScriptService();

        public DataTable GetAll(string search = "", bool isDeleted = false, int limit = 100)
        {
            var scripts = new Dictionary<string, string>
            {
                { "DBIsDeleted", isDeleted ? "TRUE" : "FALSE" },
                { "DBComputerName", dBScriptService.CleanUpToUpper(search) },
                { "LIMIT", $"{limit}" }
            };

            Page page = HttpContext.Current.Handler as Page;
            string template = page.Server.MapPath("~/Databases/Queries/Computers/GetAll.sql");
            string sql = dBScriptService.Scripts(scripts, template);

            return DBContext.SqlDataAdapterAsync(sql);
        }

        public bool Create(string computerName)
        {
            var scripts = new Dictionary<string, string>
            {
                { "DBComputerName", dBScriptService.CleanUpToUpper(computerName) },
                { "DBDateCreated", TimeService.Get().ToString("yyyy-MM-dd") },
                { "DBTimeCreated", TimeService.Get().ToString("HH:mm:ss") },
                { "DBIsDeleted", "FALSE" }
            };

            Page page = HttpContext.Current.Handler as Page;
            string template = page.Server.MapPath("~/Databases/Queries/Computers/Create.sql");
            string sql = dBScriptService.Scripts(scripts, template);
            var result = DBContext.SqlExecuteReaderAsync(sql);
            int computerNameExist = Convert.ToInt32(result["ComputerNameExist"]);

            bool name = computerNameExist != 1;
            return (name);

        }

        public bool Update(int id, string computerName)
        {
            var scripts = new Dictionary<string, string>
            {
                { "DBId", id.ToString() },
                { "DBComputerName", dBScriptService.CleanUpToUpper(computerName) }
            };

            Page page = HttpContext.Current.Handler as Page;
            string template = page.Server.MapPath("~/Databases/Queries/Computers/Update.sql");
            string sql = dBScriptService.Scripts(scripts, template);

            return DBContext.SqlExecuteAsync(sql);
        }

        public bool Delete(int id)
        {
            var scripts = new Dictionary<string, string>
            {
                { "DBId", id.ToString() }
            };

            Page page = HttpContext.Current.Handler as Page;
            string template = page.Server.MapPath("~/Databases/Queries/Computers/DeleteById.sql");
            string sql = dBScriptService.Scripts(scripts, template);

            return DBContext.SqlExecuteAsync(sql);
        }
    }
}