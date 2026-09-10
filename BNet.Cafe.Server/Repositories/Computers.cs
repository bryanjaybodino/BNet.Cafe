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
        public DataTable GetAll(string search = "", int pageIndex = 0, bool isDeleted = false)
        {
            var scripts = new Dictionary<string, string>
            {
                { "DBIsDeleted", isDeleted ? "TRUE" : "FALSE" },
                { "DBComputerName", dBScriptService.CleanUpToUpper(search) },
                { "LIMIT", $"{paginationService.SetPagination(pageIndex)}" }
            };

            Page page = HttpContext.Current.Handler as Page;
            string template = page.Server.MapPath("~/Databases/Queries/Computers/GetAll.sql");
            string sql = dBScriptService.Scripts(scripts, template);

            return DBContext.SqlDataAdapterAsync(sql);
        }

        public DataTable GetById(string id)
        {
            var scripts = new Dictionary<string, string>
            {
                { "DBId", id },
            };

            Page page = HttpContext.Current.Handler as Page;
            string template = page.Server.MapPath("~/Databases/Queries/Computers/GetById.sql");
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

        public bool Update(string id, string computerName)
        {
            var scripts = new Dictionary<string, string>
            {
                { "DBId", id },
                { "DBComputerName", dBScriptService.CleanUpToUpper(computerName) }
            };

            Page page = HttpContext.Current.Handler as Page;
            string template = page.Server.MapPath("~/Databases/Queries/Computers/Update.sql");
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
            string template = page.Server.MapPath("~/Databases/Queries/Computers/DeleteById.sql");
            string sql = dBScriptService.Scripts(scripts, template);

            return DBContext.SqlExecuteAsync(sql);
        }


        public CountComputers Count()
        {
            CountComputers countValue = new CountComputers();
            var scripts = new Dictionary<string, string>
            {
                { "DBIsDeleted", "FALSE" }
            };
            Page page = HttpContext.Current.Handler as Page;
            string template = page.Server.MapPath("~/Databases/Queries/Computers/GetAll.sql");
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