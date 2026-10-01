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
    public class InventoryItems
    {
        private readonly DBContext DBContext = new DBContext();
        private readonly DBScriptService dBScriptService = new DBScriptService();
        private readonly GridviewPaginationService paginationService = new GridviewPaginationService();

        public DataTable GetAll(string search = "", int pageIndex = -1, bool isDeleted = false)
        {
            var scripts = new Dictionary<string, string>();
            string DBItemName = dBScriptService.CleanUpToUpper(search);
            string DBIsDeleted = isDeleted ? "TRUE" : "FALSE";
            string LIMIT = paginationService.SetPagination(pageIndex);

            dBScriptService.AddIfNotNullOrEmpty(scripts, "DBIsDeleted", DBIsDeleted);
            dBScriptService.AddIfNotNullOrEmpty(scripts, "DBItemName", DBItemName);
            if (pageIndex >= 0)
            {
                dBScriptService.AddIfNotNullOrEmpty(scripts, "LIMIT", LIMIT);
            }

            Page page = HttpContext.Current.Handler as Page;
            string template = HttpContext.Current.Server.MapPath("~/Databases/Queries/InventoryItems/GetAll.sql");
            string sql = dBScriptService.Scripts(scripts, template);

            return DBContext.SqlDataAdapterAsync(sql);
        }

        public DataTable GetById(string id)
        {
            var scripts = new Dictionary<string, string>();
            string DBId = dBScriptService.CleanUpToUpper(id);

            dBScriptService.AddIfNotNullOrEmpty(scripts, "DBId", DBId);

            Page page = HttpContext.Current.Handler as Page;
            string template = HttpContext.Current.Server.MapPath("~/Databases/Queries/InventoryItems/GetById.sql");
            string sql = dBScriptService.Scripts(scripts, template);

            return DBContext.SqlDataAdapterAsync(sql);
        }

        public string GetIdByName(string name)
        {
            var scripts = new Dictionary<string, string>();
            string DBItemName = dBScriptService.CleanUpToUpper(name);

            dBScriptService.AddIfNotNullOrEmpty(scripts, "DBItemName", DBItemName);

            Page page = HttpContext.Current.Handler as Page;
            string template = HttpContext.Current.Server.MapPath("~/Databases/Queries/InventoryItems/GetIdByName.sql");
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

        public (bool Success, string ItemId) Create(string itemName, string category = "", string unitPrice = "0", string quantityInStock = "0", string reorderLevel = "0")
        {
            var scripts = new Dictionary<string, string>();
            string DBItemName = dBScriptService.CleanUpToUpper(itemName);
            string DBCategory = dBScriptService.CleanUpToUpper(category);
            string DBUnitPrice = dBScriptService.CleanUpToUpper(unitPrice);
            string DBQuantityInStock = dBScriptService.CleanUpToUpper(quantityInStock);
            string DBReorderLevel = dBScriptService.CleanUpToUpper(reorderLevel);

            dBScriptService.AddIfNotNullOrEmpty(scripts, "DBItemName", DBItemName);
            dBScriptService.AddIfNotNullOrEmpty(scripts, "DBCategory", DBCategory);
            dBScriptService.AddIfNotNullOrEmpty(scripts, "DBUnitPrice", DBUnitPrice);
            dBScriptService.AddIfNotNullOrEmpty(scripts, "DBQuantityInStock", DBQuantityInStock);
            dBScriptService.AddIfNotNullOrEmpty(scripts, "DBReorderLevel", DBReorderLevel);
            dBScriptService.AddIfNotNullOrEmpty(scripts, "DBDateCreated", TimeService.Get().ToString("yyyy-MM-dd"));
            dBScriptService.AddIfNotNullOrEmpty(scripts, "DBTimeCreated", TimeService.Get().ToString("HH:mm:ss"));
            dBScriptService.AddIfNotNullOrEmpty(scripts, "DBIsDeleted", "FALSE");

            Page page = HttpContext.Current.Handler as Page;
            string template = HttpContext.Current.Server.MapPath("~/Databases/Queries/InventoryItems/Create.sql");
            string sql = dBScriptService.Scripts(scripts, template);
            var result = DBContext.SqlExecuteReaderAsync(sql);

            int itemNameExist = Convert.ToInt32(result["ItemNameExist"]);
            string newItemId = result.ContainsKey("DBId") ? result["DBId"].ToString() : DBContext.LastInsertedId.ToString();

            bool isSuccess = itemNameExist != 1;
            return (isSuccess, newItemId);
        }

        public bool Update(string id, string itemName, string category = "", string unitPrice = "0", string quantityInStock = "0", string reorderLevel = "0")
        {
            var scripts = new Dictionary<string, string>();
            string DBId = dBScriptService.CleanUpToUpper(id);
            string DBItemName = dBScriptService.CleanUpToUpper(itemName);
            string DBCategory = dBScriptService.CleanUpToUpper(category);
            string DBUnitPrice = dBScriptService.CleanUpToUpper(unitPrice);
            string DBQuantityInStock = dBScriptService.CleanUpToUpper(quantityInStock);
            string DBReorderLevel = dBScriptService.CleanUpToUpper(reorderLevel);

            dBScriptService.AddIfNotNullOrEmpty(scripts, "DBId", DBId);
            dBScriptService.AddIfNotNullOrEmpty(scripts, "DBItemName", DBItemName);
            dBScriptService.AddIfNotNullOrEmpty(scripts, "DBCategory", DBCategory);
            dBScriptService.AddIfNotNullOrEmpty(scripts, "DBUnitPrice", DBUnitPrice);
            dBScriptService.AddIfNotNullOrEmpty(scripts, "DBQuantityInStock", DBQuantityInStock);
            dBScriptService.AddIfNotNullOrEmpty(scripts, "DBReorderLevel", DBReorderLevel);

            Page page = HttpContext.Current.Handler as Page;
            string template = HttpContext.Current.Server.MapPath("~/Databases/Queries/InventoryItems/Update.sql");
            string sql = dBScriptService.Scripts(scripts, template);

            return DBContext.SqlExecuteAsync(sql);
        }

        public bool UpdateStock(string id, string quantityInStock)
        {
            var scripts = new Dictionary<string, string>();
            string DBId = dBScriptService.CleanUpToUpper(id);
            string DBQuantityInStock = dBScriptService.CleanUpToUpper(quantityInStock);

            dBScriptService.AddIfNotNullOrEmpty(scripts, "DBId", DBId);
            dBScriptService.AddIfNotNullOrEmpty(scripts, "DBQuantityInStock", DBQuantityInStock);

            Page page = HttpContext.Current.Handler as Page;
            string template = HttpContext.Current.Server.MapPath("~/Databases/Queries/InventoryItems/UpdateStock.sql");
            string sql = dBScriptService.Scripts(scripts, template);

            return DBContext.SqlExecuteAsync(sql);
        }

        public bool Delete(string id)
        {
            var scripts = new Dictionary<string, string>();
            string DBId = dBScriptService.CleanUpToUpper(id);

            dBScriptService.AddIfNotNullOrEmpty(scripts, "DBId", DBId);

            Page page = HttpContext.Current.Handler as Page;
            string template = HttpContext.Current.Server.MapPath("~/Databases/Queries/InventoryItems/DeleteById.sql");
            string sql = dBScriptService.Scripts(scripts, template);

            return DBContext.SqlExecuteAsync(sql);
        }

        public CountInventoryItems GetCount()
        {
            CountInventoryItems countValue = new CountInventoryItems();
            var scripts = new Dictionary<string, string>();
            dBScriptService.AddIfNotNullOrEmpty(scripts, "DBIsDeleted", "FALSE");

            Page page = HttpContext.Current.Handler as Page;
            string template = HttpContext.Current.Server.MapPath("~/Databases/Queries/InventoryItems/GetAll.sql");
            string sql = dBScriptService.Scripts(scripts, template);
            DataTable dataTable = DBContext.SqlDataAdapterAsync(sql);

            int lowStock = 0;
            int outOfStock = 0;

            if (dataTable != null)
            {
                for (int i = 0; i < dataTable.Rows.Count; i++)
                {
                    int stock = 0;
                    int reorder = 0;

                    int.TryParse(dataTable.Rows[i]["DBQuantityInStock"].ToString(), out stock);
                    int.TryParse(dataTable.Rows[i]["DBReorderLevel"].ToString(), out reorder);

                    if (stock == 0)
                    {
                        outOfStock++;
                    }
                    else if (stock <= reorder)
                    {
                        lowStock++;
                    }
                }
            }

            countValue.Total = dataTable != null ? dataTable.Rows.Count.ToString() : "0";
            countValue.LowStock = lowStock.ToString();
            countValue.OutOfStock = outOfStock.ToString();
            return countValue;
        }

        public class CountInventoryItems
        {
            public string Total { get; set; }
            public string LowStock { get; set; }
            public string OutOfStock { get; set; }
        }
    }
}