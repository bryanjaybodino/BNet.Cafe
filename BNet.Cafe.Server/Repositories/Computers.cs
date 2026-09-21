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
    public class Computers
    {
        private readonly DBContext DBContext = new DBContext();
        private readonly DBScriptService dBScriptService = new DBScriptService();
        private readonly GridviewPaginationService paginationService = new GridviewPaginationService();

        public DataTable GetAll(string search = "", int pageIndex = -1, bool isDeleted = false)
        {
            var scripts = new Dictionary<string, string>();
            string DBComputerName = dBScriptService.CleanUpToUpper(search);
            string DBIsDeleted = isDeleted ? "TRUE" : "FALSE";
            string LIMIT = paginationService.SetPagination(pageIndex);

            dBScriptService.AddIfNotNullOrEmpty(scripts, "DBIsDeleted", DBIsDeleted);
            dBScriptService.AddIfNotNullOrEmpty(scripts, "DBComputerName", DBComputerName);
            if (pageIndex >= 0)
            {
                dBScriptService.AddIfNotNullOrEmpty(scripts, "LIMIT", LIMIT);
            }
            Page page = HttpContext.Current.Handler as Page;
            string template = HttpContext.Current.Server.MapPath("~/Databases/Queries/Computers/GetAll.sql");
            string sql = dBScriptService.Scripts(scripts, template);

            return DBContext.SqlDataAdapterAsync(sql);
        }

        public DataTable GetById(string id)
        {
            var scripts = new Dictionary<string, string>();
            string DBId = dBScriptService.CleanUpToUpper(id);

            dBScriptService.AddIfNotNullOrEmpty(scripts, "DBId", DBId);

            Page page = HttpContext.Current.Handler as Page;
            string template = HttpContext.Current.Server.MapPath("~/Databases/Queries/Computers/GetById.sql");
            string sql = dBScriptService.Scripts(scripts, template);

            return DBContext.SqlDataAdapterAsync(sql);
        }


        public string GetIdByName(string name)
        {
            var scripts = new Dictionary<string, string>();
            string DBComputerName = dBScriptService.CleanUpToUpper(name);

            dBScriptService.AddIfNotNullOrEmpty(scripts, "DBComputerName", DBComputerName);

            Page page = HttpContext.Current.Handler as Page;
            string template = HttpContext.Current.Server.MapPath("~/Databases/Queries/Computers/GetIdByName.sql");
            string sql = dBScriptService.Scripts(scripts, template);

            DataTable dataTable = DBContext.SqlDataAdapterAsync(sql);

            if (dataTable != null)
            {
                return dataTable.Rows[0]["DBId"].ToString();
            }
            else
            {
                return name;
            }
        }



        public bool Create(string computerName)
        {
            var scripts = new Dictionary<string, string>();
            string DBComputerName = dBScriptService.CleanUpToUpper(computerName);

            dBScriptService.AddIfNotNullOrEmpty(scripts, "DBComputerName", DBComputerName);
            dBScriptService.AddIfNotNullOrEmpty(scripts, "DBDateCreated", TimeService.Get().ToString("yyyy-MM-dd"));
            dBScriptService.AddIfNotNullOrEmpty(scripts, "DBTimeCreated", TimeService.Get().ToString("HH:mm:ss"));
            dBScriptService.AddIfNotNullOrEmpty(scripts, "DBIsDeleted", "FALSE");

            Page page = HttpContext.Current.Handler as Page;
            string template = HttpContext.Current.Server.MapPath("~/Databases/Queries/Computers/Create.sql");
            string sql = dBScriptService.Scripts(scripts, template);
            var result = DBContext.SqlExecuteReaderAsync(sql);
            int computerNameExist = Convert.ToInt32(result["ComputerNameExist"]);

            bool name = computerNameExist != 1;
            return name;
        }

        public bool Update(string id, string computerName)
        {
            var scripts = new Dictionary<string, string>();
            string DBId = dBScriptService.CleanUpToUpper(id);
            string DBComputerName = dBScriptService.CleanUpToUpper(computerName);

            dBScriptService.AddIfNotNullOrEmpty(scripts, "DBId", DBId);
            dBScriptService.AddIfNotNullOrEmpty(scripts, "DBComputerName", DBComputerName);

            Page page = HttpContext.Current.Handler as Page;
            string template = HttpContext.Current.Server.MapPath("~/Databases/Queries/Computers/Update.sql");
            string sql = dBScriptService.Scripts(scripts, template);

            return DBContext.SqlExecuteAsync(sql);
        }

        public bool UpdatePosition(string id, string x, string y)
        {
            var scripts = new Dictionary<string, string>();
            string DBId = dBScriptService.CleanUpToUpper(id);
            string DBPosX = dBScriptService.CleanUpToUpper(x);
            string DBPosY = dBScriptService.CleanUpToUpper(y);

            dBScriptService.AddIfNotNullOrEmpty(scripts, "DBId", DBId);
            dBScriptService.AddIfNotNullOrEmpty(scripts, "DBPosX", DBPosX);
            dBScriptService.AddIfNotNullOrEmpty(scripts, "DBPosY", DBPosY);

            Page page = HttpContext.Current.Handler as Page;
            string template = HttpContext.Current.Server.MapPath("~/Databases/Queries/Computers/Update.sql");
            string sql = dBScriptService.Scripts(scripts, template);

            return DBContext.SqlExecuteAsync(sql);
        }

        public bool Delete(string id)
        {
            var scripts = new Dictionary<string, string>();
            string DBId = dBScriptService.CleanUpToUpper(id);

            dBScriptService.AddIfNotNullOrEmpty(scripts, "DBId", DBId);

            Page page = HttpContext.Current.Handler as Page;
            string template = HttpContext.Current.Server.MapPath("~/Databases/Queries/Computers/DeleteById.sql");
            string sql = dBScriptService.Scripts(scripts, template);

            return DBContext.SqlExecuteAsync(sql);
        }

        public CountComputers GetCount()
        {
            CountComputers countValue = new CountComputers();
            var scripts = new Dictionary<string, string>();
            dBScriptService.AddIfNotNullOrEmpty(scripts, "DBIsDeleted", "FALSE");

            Page page = HttpContext.Current.Handler as Page;
            string template = HttpContext.Current.Server.MapPath("~/Databases/Queries/Computers/GetAll.sql");
            string sql = dBScriptService.Scripts(scripts, template);
            DataTable dataTable = DBContext.SqlDataAdapterAsync(sql);

            ClientData clientData = new ClientData();
            var liveData = clientData.FetchData();

            int occupied = 0;
            int available = 0;
            int offline = 0;

            for (int i = 0; i < dataTable.Rows.Count; i++)
            {
                string DBComputerName = dataTable.Rows[i]["DBComputerName"].ToString();
                var fetchData = liveData.FirstOrDefault(x => x.ClientName == DBComputerName);
                if (fetchData != null)
                {
                    if (!string.IsNullOrEmpty(fetchData.TimeStart) && DateTime.TryParse(fetchData.TimeStart, out DateTime start))
                    {
                        occupied++;
                    }
                    else
                    {
                        available++;
                    }
                }
                else
                {
                    offline++;
                }
            }

            countValue.Total = dataTable.Rows.Count.ToString();
            countValue.Available = available.ToString();
            countValue.Occupied = occupied.ToString();
            countValue.Offline = offline.ToString();
            return countValue;
        }

        public class CountComputers
        {
            public string Total { get; set; }
            public string Occupied { get; set; }
            public string Available { get; set; }
            public string Offline { get; set; }
        }
    }
}