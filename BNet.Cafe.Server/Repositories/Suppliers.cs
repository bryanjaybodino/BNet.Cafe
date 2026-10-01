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
    public class Suppliers
    {
        private readonly DBContext DBContext = new DBContext();
        private readonly DBScriptService dBScriptService = new DBScriptService();
        private readonly GridviewPaginationService paginationService = new GridviewPaginationService();

        public DataTable GetAll(string search = "", int pageIndex = -1, bool isDeleted = false)
        {
            var scripts = new Dictionary<string, string>();
            string DBSupplierName = dBScriptService.CleanUpToUpper(search);
            string DBIsDeleted = isDeleted ? "TRUE" : "FALSE";
            string LIMIT = paginationService.SetPagination(pageIndex);

            dBScriptService.AddIfNotNullOrEmpty(scripts, "DBIsDeleted", DBIsDeleted);
            dBScriptService.AddIfNotNullOrEmpty(scripts, "DBSupplierName", DBSupplierName);
            if (pageIndex >= 0)
            {
                dBScriptService.AddIfNotNullOrEmpty(scripts, "LIMIT", LIMIT);
            }

            Page page = HttpContext.Current.Handler as Page;
            string template = HttpContext.Current.Server.MapPath("~/Databases/Queries/Suppliers/GetAll.sql");
            string sql = dBScriptService.Scripts(scripts, template);

            return DBContext.SqlDataAdapterAsync(sql);
        }

        public DataTable GetById(string id)
        {
            var scripts = new Dictionary<string, string>();
            string DBId = dBScriptService.CleanUpToUpper(id);

            dBScriptService.AddIfNotNullOrEmpty(scripts, "DBId", DBId);

            Page page = HttpContext.Current.Handler as Page;
            string template = HttpContext.Current.Server.MapPath("~/Databases/Queries/Suppliers/GetById.sql");
            string sql = dBScriptService.Scripts(scripts, template);

            return DBContext.SqlDataAdapterAsync(sql);
        }

        public string GetIdByName(string name)
        {
            var scripts = new Dictionary<string, string>();
            string DBSupplierName = dBScriptService.CleanUpToUpper(name);

            dBScriptService.AddIfNotNullOrEmpty(scripts, "DBSupplierName", DBSupplierName);

            Page page = HttpContext.Current.Handler as Page;
            string template = HttpContext.Current.Server.MapPath("~/Databases/Queries/Suppliers/GetIdByName.sql");
            string sql = dBScriptService.Scripts(scripts, template);

            DataTable dataTable = DBContext.SqlDataAdapterAsync(sql);

            if (dataTable != null && dataTable.Rows.Count > 0)
            {
                return dataTable.Rows[0]["DBId"].ToString();
            }
            else
            {
                return name;
            }
        }

        public bool Create(string supplierName, string contactPerson = "", string phone = "", string email = "", string address = "")
        {
            var scripts = new Dictionary<string, string>();
            string DBSupplierName = dBScriptService.CleanUpToUpper(supplierName);
            string DBContactPerson = dBScriptService.CleanUpToUpper(contactPerson);
            string DBPhone = dBScriptService.CleanUpToUpper(phone);
            string DBEmail = dBScriptService.CleanUpToUpper(email);
            string DBAddress = dBScriptService.CleanUpToUpper(address);

            dBScriptService.AddIfNotNullOrEmpty(scripts, "DBSupplierName", DBSupplierName);
            dBScriptService.AddIfNotNullOrEmpty(scripts, "DBContactPerson", DBContactPerson);
            dBScriptService.AddIfNotNullOrEmpty(scripts, "DBPhone", DBPhone);
            dBScriptService.AddIfNotNullOrEmpty(scripts, "DBEmail", DBEmail);
            dBScriptService.AddIfNotNullOrEmpty(scripts, "DBAddress", DBAddress);
            dBScriptService.AddIfNotNullOrEmpty(scripts, "DBDateCreated", TimeService.Get().ToString("yyyy-MM-dd"));
            dBScriptService.AddIfNotNullOrEmpty(scripts, "DBTimeCreated", TimeService.Get().ToString("HH:mm:ss"));
            dBScriptService.AddIfNotNullOrEmpty(scripts, "DBIsDeleted", "FALSE");

            Page page = HttpContext.Current.Handler as Page;
            string template = HttpContext.Current.Server.MapPath("~/Databases/Queries/Suppliers/Create.sql");
            string sql = dBScriptService.Scripts(scripts, template);
            var result = DBContext.SqlExecuteReaderAsync(sql);
            int supplierNameExist = Convert.ToInt32(result["SupplierNameExist"]);

            bool name = supplierNameExist != 1;
            return name;
        }

        public bool Update(string id, string supplierName, string contactPerson = "", string phone = "", string email = "", string address = "")
        {
            var scripts = new Dictionary<string, string>();
            string DBId = dBScriptService.CleanUpToUpper(id);
            string DBSupplierName = dBScriptService.CleanUpToUpper(supplierName);
            string DBContactPerson = dBScriptService.CleanUpToUpper(contactPerson);
            string DBPhone = dBScriptService.CleanUpToUpper(phone);
            string DBEmail = dBScriptService.CleanUpToUpper(email);
            string DBAddress = dBScriptService.CleanUpToUpper(address);

            dBScriptService.AddIfNotNullOrEmpty(scripts, "DBId", DBId);
            dBScriptService.AddIfNotNullOrEmpty(scripts, "DBSupplierName", DBSupplierName);
            dBScriptService.AddIfNotNullOrEmpty(scripts, "DBContactPerson", DBContactPerson);
            dBScriptService.AddIfNotNullOrEmpty(scripts, "DBPhone", DBPhone);
            dBScriptService.AddIfNotNullOrEmpty(scripts, "DBEmail", DBEmail);
            dBScriptService.AddIfNotNullOrEmpty(scripts, "DBAddress", DBAddress);

            Page page = HttpContext.Current.Handler as Page;
            string template = HttpContext.Current.Server.MapPath("~/Databases/Queries/Suppliers/Update.sql");
            string sql = dBScriptService.Scripts(scripts, template);

            return DBContext.SqlExecuteAsync(sql);
        }

        public bool Delete(string id)
        {
            var scripts = new Dictionary<string, string>();
            string DBId = dBScriptService.CleanUpToUpper(id);

            dBScriptService.AddIfNotNullOrEmpty(scripts, "DBId", DBId);

            Page page = HttpContext.Current.Handler as Page;
            string template = HttpContext.Current.Server.MapPath("~/Databases/Queries/Suppliers/DeleteById.sql");
            string sql = dBScriptService.Scripts(scripts, template);

            return DBContext.SqlExecuteAsync(sql);
        }

        public CountSuppliers GetCount()
        {
            CountSuppliers countValue = new CountSuppliers();
            var scripts = new Dictionary<string, string>();
            dBScriptService.AddIfNotNullOrEmpty(scripts, "DBIsDeleted", "FALSE");

            Page page = HttpContext.Current.Handler as Page;
            string template = HttpContext.Current.Server.MapPath("~/Databases/Queries/Suppliers/GetAll.sql");
            string sql = dBScriptService.Scripts(scripts, template);
            DataTable dataTable = DBContext.SqlDataAdapterAsync(sql);

            countValue.Total = dataTable.Rows.Count.ToString();
            return countValue;
        }

        public class CountSuppliers
        {
            public string Total { get; set; }
        }
    }
}