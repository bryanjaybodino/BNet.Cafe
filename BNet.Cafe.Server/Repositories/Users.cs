using BNet.Cafe.Server.Databases;
using BNet.Cafe.Server.Services;
using System;
using System.Collections.Generic;
using System.Data;
using System.Web;
using System.Web.UI;

namespace BNet.Cafe.Server.Repositories
{
    public class Users
    {
        private readonly DBContext DBContext = new DBContext();
        private readonly DBScriptService dBScriptService = new DBScriptService();
        private readonly GridviewPaginationService paginationService = new GridviewPaginationService();

        public DataTable GetAll(string search = "", int pageIndex = 0, bool isDeleted = false)
        {
            var scripts = new Dictionary<string, string>();
            string DBSearch = dBScriptService.CleanUpToUpper(search);
            string DBIsDeleted = isDeleted ? "TRUE" : "FALSE";
            string LIMIT = paginationService.SetPagination(pageIndex);

            dBScriptService.AddIfNotNullOrEmpty(scripts, "DBIsDeleted", DBIsDeleted);
            dBScriptService.AddIfNotNullOrEmpty(scripts, "DBSearch", DBSearch);
            dBScriptService.AddIfNotNullOrEmpty(scripts, "LIMIT", LIMIT);

            Page page = HttpContext.Current.Handler as Page;
            string template = HttpContext.Current.Server.MapPath("~/Databases/Queries/Users/GetAll.sql");
            string sql = dBScriptService.Scripts(scripts, template);

            return DBContext.SqlDataAdapterAsync(sql);
        }

        public DataTable GetById(string id)
        {
            var scripts = new Dictionary<string, string>();
            string DBId = dBScriptService.CleanUpToUpper(id);

            dBScriptService.AddIfNotNullOrEmpty(scripts, "DBId", DBId);


            string template = HttpContext.Current.Server.MapPath("~/Databases/Queries/Users/GetById.sql");
            string sql = dBScriptService.Scripts(scripts, template);

            return DBContext.SqlDataAdapterAsync(sql);
        }

        public DataTable GetLogin(string email, string password)
        {
            var scripts = new Dictionary<string, string>();
            string DBEmail = dBScriptService.CleanUpToUpper(email);
            string DBPassword = dBScriptService.CleanUpToUpper(password);

            dBScriptService.AddIfNotNullOrEmpty(scripts, "DBEmail", DBEmail);
            dBScriptService.AddIfNotNullOrEmpty(scripts, "DBPassword", DBPassword);

            // Use HttpContext.Current.Server directly instead of casting Handler to Page
            string template = HttpContext.Current.Server.MapPath("~/Databases/Queries/Users/GetLogin.sql");
            string sql = dBScriptService.Scripts(scripts, template);

            return DBContext.SqlDataAdapterAsync(sql);
        }



        public DataTable GetByEmail(string email)
        {
            var scripts = new Dictionary<string, string>();
            string DBEmail = dBScriptService.CleanUpToUpper(email);

            dBScriptService.AddIfNotNullOrEmpty(scripts, "DBEmail", DBEmail);

            string template = HttpContext.Current.Server.MapPath("~/Databases/Queries/Users/GetByEmail.sql");
            string sql = dBScriptService.Scripts(scripts, template);

            return DBContext.SqlDataAdapterAsync(sql);
        }


        public bool Create(string email, string password, string name, string role)
        {
            var scripts = new Dictionary<string, string>();
            string DBEmail = dBScriptService.CleanUpToUpper(email);
            string DBPassword = dBScriptService.CleanUpToUpper(password);
            string DBName = dBScriptService.CleanUpToUpper(name);
            string DBRole = dBScriptService.CleanUpToUpper(role);

            dBScriptService.AddIfNotNullOrEmpty(scripts, "DBEmail", DBEmail);
            dBScriptService.AddIfNotNullOrEmpty(scripts, "DBPassword", DBPassword);
            dBScriptService.AddIfNotNullOrEmpty(scripts, "DBName", DBName);
            dBScriptService.AddIfNotNullOrEmpty(scripts, "DBRole", DBRole);
            dBScriptService.AddIfNotNullOrEmpty(scripts, "DBDateCreated", TimeService.Get().ToString("yyyy-MM-dd"));
            dBScriptService.AddIfNotNullOrEmpty(scripts, "DBTimeCreated", TimeService.Get().ToString("HH:mm:ss"));
            dBScriptService.AddIfNotNullOrEmpty(scripts, "DBIsDeleted", "FALSE");

            Page page = HttpContext.Current.Handler as Page;
            string template = HttpContext.Current.Server.MapPath("~/Databases/Queries/Users/Create.sql");
            string sql = dBScriptService.Scripts(scripts, template);
            var result = DBContext.SqlExecuteReaderAsync(sql);
            int emailExist = Convert.ToInt32(result["EmailExist"]);

            return emailExist != 1;
        }

        public bool Update(string id, string email, string password, string name, string role)
        {
            var scripts = new Dictionary<string, string>();
            string DBId = dBScriptService.CleanUpToUpper(id);
            string DBEmail = dBScriptService.CleanUpToUpper(email);
            string DBPassword = dBScriptService.CleanUpToUpper(password);
            string DBName = dBScriptService.CleanUpToUpper(name);
            string DBRole = dBScriptService.CleanUpToUpper(role);

            dBScriptService.AddIfNotNullOrEmpty(scripts, "DBId", DBId);
            dBScriptService.AddIfNotNullOrEmpty(scripts, "DBEmail", DBEmail);
            dBScriptService.AddIfNotNullOrEmpty(scripts, "DBPassword", DBPassword);
            dBScriptService.AddIfNotNullOrEmpty(scripts, "DBName", DBName);
            dBScriptService.AddIfNotNullOrEmpty(scripts, "DBRole", DBRole);

            Page page = HttpContext.Current.Handler as Page;
            string template = HttpContext.Current.Server.MapPath("~/Databases/Queries/Users/Update.sql");
            string sql = dBScriptService.Scripts(scripts, template);

            return DBContext.SqlExecuteAsync(sql);
        }

        public bool Delete(string id)
        {
            var scripts = new Dictionary<string, string>();
            string DBId = dBScriptService.CleanUpToUpper(id);

            dBScriptService.AddIfNotNullOrEmpty(scripts, "DBId", DBId);

            Page page = HttpContext.Current.Handler as Page;
            string template = HttpContext.Current.Server.MapPath("~/Databases/Queries/Users/DeleteById.sql");
            string sql = dBScriptService.Scripts(scripts, template);

            return DBContext.SqlExecuteAsync(sql);
        }
    }
}