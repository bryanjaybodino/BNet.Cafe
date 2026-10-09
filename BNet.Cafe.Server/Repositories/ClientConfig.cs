using BNet.Cafe.Server.Ashx;
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
    public class ClientConfig
    {
        private readonly DBContext DBContext = new DBContext();
        private readonly DBScriptService dBScriptService = new DBScriptService();
        private readonly GridviewPaginationService paginationService = new GridviewPaginationService();

        public DataTable GetAll(int pageIndex = -1, bool isDeleted = false)
        {
            var scripts = new Dictionary<string, string>();
            string DBIsDeleted = isDeleted ? "TRUE" : "FALSE";
            string LIMIT = paginationService.SetPagination(pageIndex);

            dBScriptService.AddIfNotNullOrEmpty(scripts, "DBIsDeleted", DBIsDeleted);
            if (pageIndex >= 0)
            {
                dBScriptService.AddIfNotNullOrEmpty(scripts, "LIMIT", LIMIT);
            }

            Page page = HttpContext.Current.Handler as Page;
            string template = HttpContext.Current.Server.MapPath("~/Databases/Queries/ClientConfig/GetAll.sql");
            string sql = dBScriptService.Scripts(scripts, template);

            return DBContext.SqlDataAdapterAsync(sql);
        }

        public DataTable GetById(string id)
        {
            var scripts = new Dictionary<string, string>();
            string DBId = dBScriptService.CleanUpToUpper(id);

            dBScriptService.AddIfNotNullOrEmpty(scripts, "DBId", DBId);

            Page page = HttpContext.Current.Handler as Page;
            string template = HttpContext.Current.Server.MapPath("~/Databases/Queries/ClientConfig/GetById.sql");
            string sql = dBScriptService.Scripts(scripts, template);

            return DBContext.SqlDataAdapterAsync(sql);
        }

        public bool Create(string accountCreationAllowed = "TRUE", string autoShutDownInterval = "300", string desktopSlideShow = "FALSE", string resetShutdownCountdown = "FALSE")
        {
            var scripts = new Dictionary<string, string>();
            string DBAccountCreationAllowed = dBScriptService.CleanUpToUpper(accountCreationAllowed);
            string DBAutoShutDownInterval = dBScriptService.CleanUpToUpper(autoShutDownInterval);
            string DBDesktopSlideShow = dBScriptService.CleanUpToUpper(desktopSlideShow);
            string DBResetShutdownCountdown = dBScriptService.CleanUpToUpper(resetShutdownCountdown);

            dBScriptService.AddIfNotNullOrEmpty(scripts, "DBAccountCreationAllowed", DBAccountCreationAllowed);
            dBScriptService.AddIfNotNullOrEmpty(scripts, "DBAutoShutDownInterval", DBAutoShutDownInterval);
            dBScriptService.AddIfNotNullOrEmpty(scripts, "DBDesktopSlideShow", DBDesktopSlideShow);
            dBScriptService.AddIfNotNullOrEmpty(scripts, "DBResetShutdownCountdown", DBResetShutdownCountdown);
            dBScriptService.AddIfNotNullOrEmpty(scripts, "DBDateCreated", TimeService.Get().ToString("yyyy-MM-dd"));
            dBScriptService.AddIfNotNullOrEmpty(scripts, "DBTimeCreated", TimeService.Get().ToString("HH:mm:ss"));
            dBScriptService.AddIfNotNullOrEmpty(scripts, "DBIsDeleted", "FALSE");

            Page page = HttpContext.Current.Handler as Page;
            string template = HttpContext.Current.Server.MapPath("~/Databases/Queries/ClientConfig/Create.sql");
            string sql = dBScriptService.Scripts(scripts, template);

            return DBContext.SqlExecuteAsync(sql);
        }

        public bool Update(string id, string accountCreationAllowed, string autoShutDownInterval, string desktopSlideShow, string resetShutdownCountdown)
        {
            var scripts = new Dictionary<string, string>();
            string DBId = dBScriptService.CleanUpToUpper(id);
            string DBAccountCreationAllowed = dBScriptService.CleanUpToUpper(accountCreationAllowed);
            string DBAutoShutDownInterval = dBScriptService.CleanUpToUpper(autoShutDownInterval);
            string DBDesktopSlideShow = dBScriptService.CleanUpToUpper(desktopSlideShow);
            string DBResetShutdownCountdown = dBScriptService.CleanUpToUpper(resetShutdownCountdown);

            dBScriptService.AddIfNotNullOrEmpty(scripts, "DBId", DBId);
            dBScriptService.AddIfNotNullOrEmpty(scripts, "DBAccountCreationAllowed", DBAccountCreationAllowed);
            dBScriptService.AddIfNotNullOrEmpty(scripts, "DBAutoShutDownInterval", DBAutoShutDownInterval);
            dBScriptService.AddIfNotNullOrEmpty(scripts, "DBDesktopSlideShow", DBDesktopSlideShow);
            dBScriptService.AddIfNotNullOrEmpty(scripts, "DBResetShutdownCountdown", DBResetShutdownCountdown);

            Page page = HttpContext.Current.Handler as Page;
            string template = HttpContext.Current.Server.MapPath("~/Databases/Queries/ClientConfig/Update.sql");
            string sql = dBScriptService.Scripts(scripts, template);

            return DBContext.SqlExecuteAsync(sql);
        }

        public bool Delete(string id)
        {
            var scripts = new Dictionary<string, string>();
            string DBId = dBScriptService.CleanUpToUpper(id);

            dBScriptService.AddIfNotNullOrEmpty(scripts, "DBId", DBId);

            Page page = HttpContext.Current.Handler as Page;
            string template = HttpContext.Current.Server.MapPath("~/Databases/Queries/ClientConfig/DeleteById.sql");
            string sql = dBScriptService.Scripts(scripts, template);

            return DBContext.SqlExecuteAsync(sql);
        }
    }
}