using BNet.Cafe.Server.Ashx;
using BNet.Cafe.Server.ConstantData;
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
    public class InventoryTransactions
    {
        private readonly DBContext DBContext = new DBContext();
        private readonly DBScriptService dBScriptService = new DBScriptService();
        private readonly GridviewPaginationService paginationService = new GridviewPaginationService();

        public DataTable GetAll(string search = "", string transactionType = "", string dateRange = "", int pageIndex = -1, bool isDeleted = false)
        {
            var scripts = new Dictionary<string, string>();
            string DBTransactionType = dBScriptService.CleanUpToUpper(transactionType);
            string DBIsDeleted = isDeleted ? "TRUE" : "FALSE";
            string DBSearch = dBScriptService.CleanUpToUpper(search);
            dBScriptService.AddIfNotNullOrEmpty(scripts, "DBIsDeleted", DBIsDeleted);
            dBScriptService.AddIfNotNullOrEmpty(scripts, "DBSearch", DBSearch);
            string LIMIT = paginationService.SetPagination(pageIndex);

            string DBDateStart = "";
            string DBDateEnd = "";
            if (dateRange.Contains(","))
            {
                var date = dateRange.Split(',');
                DBDateStart = date[0];
                DBDateEnd = date[1];
            }

            dBScriptService.AddIfNotNullOrEmpty(scripts, "DBSearch", DBSearch);
            dBScriptService.AddIfNotNullOrEmpty(scripts, "DBIsDeleted", DBIsDeleted);
            dBScriptService.AddIfNotNullOrEmpty(scripts, "DBTransactionType", DBTransactionType);
            dBScriptService.AddIfNotNullOrEmpty(scripts, "DBDateStart", DBDateStart);
            dBScriptService.AddIfNotNullOrEmpty(scripts, "DBDateEnd", DBDateEnd);
            if (pageIndex >= 0)
            {
                dBScriptService.AddIfNotNullOrEmpty(scripts, "LIMIT", LIMIT);
            }

            Page page = HttpContext.Current.Handler as Page;
            string template = HttpContext.Current.Server.MapPath("~/Databases/Queries/InventoryTransactions/GetAll.sql");
            string sql = dBScriptService.Scripts(scripts, template);

            return DBContext.SqlDataAdapterAsync(sql);
        }

        public DataTable GetById(string id)
        {
            var scripts = new Dictionary<string, string>();
            string DBId = dBScriptService.CleanUpToUpper(id);

            dBScriptService.AddIfNotNullOrEmpty(scripts, "DBId", DBId);

            Page page = HttpContext.Current.Handler as Page;
            string template = HttpContext.Current.Server.MapPath("~/Databases/Queries/InventoryTransactions/GetById.sql");
            string sql = dBScriptService.Scripts(scripts, template);

            return DBContext.SqlDataAdapterAsync(sql);
        }

        public DataTable GetByItemId(string itemId, int pageIndex = -1, bool isDeleted = false)
        {
            var scripts = new Dictionary<string, string>();
            string DBItemId = dBScriptService.CleanUpToUpper(itemId);
            string DBIsDeleted = isDeleted ? "TRUE" : "FALSE";
            string LIMIT = paginationService.SetPagination(pageIndex);

            dBScriptService.AddIfNotNullOrEmpty(scripts, "DBItemId", DBItemId);
            dBScriptService.AddIfNotNullOrEmpty(scripts, "DBIsDeleted", DBIsDeleted);
            if (pageIndex >= 0)
            {
                dBScriptService.AddIfNotNullOrEmpty(scripts, "LIMIT", LIMIT);
            }

            Page page = HttpContext.Current.Handler as Page;
            string template = HttpContext.Current.Server.MapPath("~/Databases/Queries/InventoryTransactions/GetByItemId.sql");
            string sql = dBScriptService.Scripts(scripts, template);

            return DBContext.SqlDataAdapterAsync(sql);
        }

        public bool Create(string itemId, string userId, string transactionType, string quantity)
        {
            var scripts = new Dictionary<string, string>();
            string DBItemId = dBScriptService.CleanUpToUpper(itemId);
            string DBUserId = dBScriptService.CleanUpToUpper(userId);
            string DBTransactionType = dBScriptService.CleanUpToUpper(transactionType);
            string DBQuantity = dBScriptService.CleanUpToUpper(quantity);

            dBScriptService.AddIfNotNullOrEmpty(scripts, "DBItemId", DBItemId);
            dBScriptService.AddIfNotNullOrEmpty(scripts, "DBUserId", DBUserId);
            dBScriptService.AddIfNotNullOrEmpty(scripts, "DBTransactionType", DBTransactionType);
            dBScriptService.AddIfNotNullOrEmpty(scripts, "DBQuantity", DBQuantity);
            dBScriptService.AddIfNotNullOrEmpty(scripts, "DBDateCreated", TimeService.Get().ToString("yyyy-MM-dd"));
            dBScriptService.AddIfNotNullOrEmpty(scripts, "DBTimeCreated", TimeService.Get().ToString("HH:mm:ss"));
            dBScriptService.AddIfNotNullOrEmpty(scripts, "DBIsDeleted", "FALSE");

            Page page = HttpContext.Current.Handler as Page;
            string template = HttpContext.Current.Server.MapPath("~/Databases/Queries/InventoryTransactions/Create.sql");
            string sql = dBScriptService.Scripts(scripts, template);

            return DBContext.SqlExecuteAsync(sql);
        }

        public bool Update(string id, string itemId, string userId, string transactionType, string quantity)
        {
            var scripts = new Dictionary<string, string>();
            string DBId = dBScriptService.CleanUpToUpper(id);
            string DBItemId = dBScriptService.CleanUpToUpper(itemId);
            string DBUserId = dBScriptService.CleanUpToUpper(userId);
            string DBTransactionType = dBScriptService.CleanUpToUpper(transactionType);
            string DBQuantity = dBScriptService.CleanUpToUpper(quantity);

            dBScriptService.AddIfNotNullOrEmpty(scripts, "DBId", DBId);
            dBScriptService.AddIfNotNullOrEmpty(scripts, "DBItemId", DBItemId);
            dBScriptService.AddIfNotNullOrEmpty(scripts, "DBUserId", DBUserId);
            dBScriptService.AddIfNotNullOrEmpty(scripts, "DBTransactionType", DBTransactionType);
            dBScriptService.AddIfNotNullOrEmpty(scripts, "DBQuantity", DBQuantity);

            Page page = HttpContext.Current.Handler as Page;
            string template = HttpContext.Current.Server.MapPath("~/Databases/Queries/InventoryTransactions/Update.sql");
            string sql = dBScriptService.Scripts(scripts, template);

            return DBContext.SqlExecuteAsync(sql);
        }

        public bool Delete(string id)
        {
            var scripts = new Dictionary<string, string>();
            string DBId = dBScriptService.CleanUpToUpper(id);

            dBScriptService.AddIfNotNullOrEmpty(scripts, "DBId", DBId);

            Page page = HttpContext.Current.Handler as Page;
            string template = HttpContext.Current.Server.MapPath("~/Databases/Queries/InventoryTransactions/DeleteById.sql");
            string sql = dBScriptService.Scripts(scripts, template);

            return DBContext.SqlExecuteAsync(sql);
        }

        public CountTransactions GetCount()
        {
            CountTransactions countValue = new CountTransactions();
            var scripts = new Dictionary<string, string>();
            dBScriptService.AddIfNotNullOrEmpty(scripts, "DBIsDeleted", "FALSE");

            Page page = HttpContext.Current.Handler as Page;
            string template = HttpContext.Current.Server.MapPath("~/Databases/Queries/InventoryTransactions/GetAll.sql");
            string sql = dBScriptService.Scripts(scripts, template);
            DataTable dataTable = DBContext.SqlDataAdapterAsync(sql);

            int stockInCount = 0;
            int stockOutCount = 0;

            if (dataTable != null)
            {
                for (int i = 0; i < dataTable.Rows.Count; i++)
                {
                    string type = dataTable.Rows[i]["DBTransactionType"].ToString().ToUpper();
                    if (type == "STOCK_IN")
                    {
                        stockInCount++;
                    }
                    else if (type == "SALE")
                    {
                        stockOutCount++;
                    }
                }
            }

            countValue.Total = dataTable != null ? dataTable.Rows.Count.ToString() : "0";
            countValue.StockIn = stockInCount.ToString();
            countValue.StockOut = stockOutCount.ToString();
            return countValue;
        }

        public bool StockIn(string itemId, string userId, string quantity, string cost)
        {
            // Parse cost and format to 2 decimal places without commas (e.g., 1234.50)
            decimal.TryParse(cost, out decimal parsedCost);
            string formattedCost = parsedCost.ToString("0.00", System.Globalization.CultureInfo.InvariantCulture);

            var scripts = new Dictionary<string, string>();
            dBScriptService.AddIfNotNullOrEmpty(scripts, "DBTransactionType", TransactionType.STOCK_IN);
            dBScriptService.AddIfNotNullOrEmpty(scripts, "DBItemId", dBScriptService.CleanUpToUpper(itemId));
            dBScriptService.AddIfNotNullOrEmpty(scripts, "DBUserId", dBScriptService.CleanUpToUpper(userId));
            dBScriptService.AddIfNotNullOrEmpty(scripts, "DBQuantity", dBScriptService.CleanUpToUpper(quantity));
            dBScriptService.AddIfNotNullOrEmpty(scripts, "DBCost", formattedCost);
            dBScriptService.AddIfNotNullOrEmpty(scripts, "DBDateCreated", TimeService.Get().ToString("yyyy-MM-dd"));
            dBScriptService.AddIfNotNullOrEmpty(scripts, "DBTimeCreated", TimeService.Get().ToString("HH:mm:ss"));
            dBScriptService.AddIfNotNullOrEmpty(scripts, "DBIsDeleted", "FALSE");

            string template = HttpContext.Current.Server.MapPath("~/Databases/Queries/InventoryTransactions/StockIn.sql");
            string sql = dBScriptService.Scripts(scripts, template);

            return DBContext.SqlExecuteAsync(sql);
        }

        public bool StockOut(string itemId, string userId, string quantity, string cost)
        {
            // Parse cost and format to 2 decimal places without commas (e.g., 1234.50)
            decimal.TryParse(cost, out decimal parsedCost);
            string formattedCost = parsedCost.ToString("0.00", System.Globalization.CultureInfo.InvariantCulture);

            var scripts = new Dictionary<string, string>();
            dBScriptService.AddIfNotNullOrEmpty(scripts, "DBTransactionType", TransactionType.SALE);
            dBScriptService.AddIfNotNullOrEmpty(scripts, "DBItemId", dBScriptService.CleanUpToUpper(itemId));
            dBScriptService.AddIfNotNullOrEmpty(scripts, "DBUserId", dBScriptService.CleanUpToUpper(userId));
            dBScriptService.AddIfNotNullOrEmpty(scripts, "DBQuantity", dBScriptService.CleanUpToUpper(quantity));
            dBScriptService.AddIfNotNullOrEmpty(scripts, "DBCost", formattedCost);
            dBScriptService.AddIfNotNullOrEmpty(scripts, "DBDateCreated", TimeService.Get().ToString("yyyy-MM-dd"));
            dBScriptService.AddIfNotNullOrEmpty(scripts, "DBTimeCreated", TimeService.Get().ToString("HH:mm:ss"));
            dBScriptService.AddIfNotNullOrEmpty(scripts, "DBIsDeleted", "FALSE");
            string template = HttpContext.Current.Server.MapPath("~/Databases/Queries/InventoryTransactions/StockOut.sql");
            string sql = dBScriptService.Scripts(scripts, template);

            return DBContext.SqlExecuteAsync(sql);
        }

        public class CountTransactions
        {
            public string Total { get; set; }
            public string StockIn { get; set; }
            public string StockOut { get; set; }
        }

        public SalesCountResult GetSalesCount(string search = "", string dateRange = "", bool isDeleted = false)
        {
            var scripts = new Dictionary<string, string>();
            string DBIsDeleted = isDeleted ? "TRUE" : "FALSE";
            string DBSearch = dBScriptService.CleanUpToUpper(search);

            dBScriptService.AddIfNotNullOrEmpty(scripts, "DBIsDeleted", DBIsDeleted);
            dBScriptService.AddIfNotNullOrEmpty(scripts, "DBSearch", DBSearch);

            // Handle date range splitting (YYYY-MM-DD,YYYY-MM-DD)
            string dateStart = "";
            string dateEnd = "";
            if (!string.IsNullOrEmpty(dateRange) && dateRange.Contains(","))
            {
                string[] dates = dateRange.Split(',');
                dateStart = dates[0].Trim();
                dateEnd = dates[1].Trim();
            }
            dBScriptService.AddIfNotNullOrEmpty(scripts, "DBTransactionType", "SALE");
            dBScriptService.AddIfNotNullOrEmpty(scripts, "DBDateStart", dateStart);
            dBScriptService.AddIfNotNullOrEmpty(scripts, "DBDateEnd", dateEnd);

            string template = HttpContext.Current.Server.MapPath("~/Databases/Queries/InventoryTransactions/GetSalesCount.sql");
            string sql = dBScriptService.Scripts(scripts, template);
            DataTable dt = DBContext.SqlDataAdapterAsync(sql);

            SalesCountResult result = new SalesCountResult();
            if (dt != null && dt.Rows.Count > 0)
            {
                DataRow row = dt.Rows[0];
                result.TotalOrders = row["DBTotalOrders"].ToString();
                result.ItemsSold = row["DBTotalItemsSold"].ToString();
                result.Revenue = "₱" + row["DBTotalRevenue"].ToString();
                result.AvgOrderValue = "₱" + row["DBAvgOrderValue"].ToString();
            }

            return result;
        }

        public class SalesCountResult
        {
            public string TotalOrders { get; set; } = "0";
            public string ItemsSold { get; set; } = "0";
            public string Revenue { get; set; } = "₱0.00";
            public string AvgOrderValue { get; set; } = "₱0.00";
        }
    }
}