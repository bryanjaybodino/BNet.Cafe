using BNet.Cafe.Server.Repositories;
using BNet.Cafe.Server.Services;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;

namespace BNet.Cafe.Server.Forms
{
    public partial class Dashboard : System.Web.UI.UserControl
    {
        private readonly Rentals rentals = new Rentals();
        private readonly BNet.Cafe.Server.Repositories.Dashboard dashboardRepository = new BNet.Cafe.Server.Repositories.Dashboard();
        private readonly InventoryTransactions transactionsRepo = new InventoryTransactions();
        private readonly InventoryItems itemsRepo = new InventoryItems();
        private readonly Repositories.Suppliers suppliersRepo = new Repositories.Suppliers();

        protected void Page_PreRender(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                DateTime dateTime = TimeService.Get();
                DateTime firstDayOfMonth = new DateTime(dateTime.Year, dateTime.Month, 1);
                DateTime lastDayOfMonth = firstDayOfMonth.AddMonths(1).AddDays(-1);
                TextBox_DateRage.Text = $"{firstDayOfMonth:yyyy-MM-dd},{lastDayOfMonth:yyyy-MM-dd}";
            }

            string dateRange = TextBox_DateRage.Text;

            // 1. Rental KPI cards
            var count = rentals.GetCount("", dateRange);
            Label_TotalTransactions.Text = count.Total;
            Label_Users.Text = count.Users;
            Label_WalkIn.Text = count.WalkIn;
            Label_Income.Text = count.Income;

            // 2. Product Sales Revenue
            var salesCount = transactionsRepo.GetSalesCount("", dateRange);
            Label_SalesRevenue.Text = salesCount.Revenue;

            // 3. Stock In / Restock Costs
            DataTable stockInLogs = transactionsRepo.GetAll("", ConstantData.TransactionType.STOCK_IN, dateRange);
            decimal totalRestockCost = 0;
            if (stockInLogs != null)
            {
                foreach (DataRow row in stockInLogs.Rows)
                {
                    if (row["DBTotalCost"] != DBNull.Value)
                    {
                        totalRestockCost += Convert.ToDecimal(row["DBTotalCost"]);
                    }
                }
            }
            Label_TotalCosts.Text = "₱" + totalRestockCost.ToString("N2");

            // 4. Inventory Metrics
            var inventoryCount = itemsRepo.GetCount();
            Label_TotalInventory.Text = inventoryCount.Total;
            Label_LowStockCount.Text = inventoryCount.LowStock;

            // 5. Suppliers Count
            var suppliersCount = suppliersRepo.GetCount();
            Label_TotalSuppliers.Text = suppliersCount.Total;

            // 6. Chart & Leaderboard Data
            DataTable revenueTrend = dashboardRepository.GetRevenueTrend(dateRange);
            DataTable computerUsage = dashboardRepository.GetComputerUsage(dateRange);
            DataTable topUpUsers = dashboardRepository.GetTopUpUsers(dateRange);

            // Fetch sales transactions for daily sales calculations
            DataTable salesLogs = transactionsRepo.GetAll("", ConstantData.TransactionType.SALE, dateRange);

            // Calculate total top-up sum for KPI card
            decimal totalTopUpSum = 0;
            foreach (DataRow row in topUpUsers.Rows)
            {
                if (row["TotalTopUpAmount"] != DBNull.Value)
                {
                    totalTopUpSum += Convert.ToDecimal(row["TotalTopUpAmount"]);
                }
            }
            Label_TopUpIncome.Text = "₱" + totalTopUpSum.ToString("N2");

            HiddenField_ChartData.Value = BuildChartDataJson(count, revenueTrend, computerUsage, topUpUsers, salesLogs, stockInLogs);
        }

        private string BuildChartDataJson(
            Rentals.CountRentals count,
            DataTable revenueTrend,
            DataTable computerUsage,
            DataTable topUpUsers,
            DataTable salesLogs,
            DataTable stockInLogs)
        {
            int usersCount = int.TryParse(count.Users, out int u) ? u : 0;
            int walkInCount = int.TryParse(count.WalkIn, out int w) ? w : 0;

            var revenueItems = new List<string>();
            foreach (DataRow row in revenueTrend.Rows)
            {
                string date = row["DBDate"].ToString();
                string amount = row["DBTotalAmount"].ToString().Replace(",", "");
                if (string.IsNullOrEmpty(amount)) amount = "0";

                revenueItems.Add("{\"date\":\"" + EscapeJson(date) + "\",\"amount\":" + amount + "}");
            }

            var usageItems = new List<string>();
            foreach (DataRow row in computerUsage.Rows)
            {
                string pc = row["DBComputerName"] == DBNull.Value ? "Unknown" : row["DBComputerName"].ToString();
                string cnt = row["DBCount"].ToString();

                usageItems.Add("{\"pc\":\"" + EscapeJson(pc) + "\",\"count\":" + cnt + "}");
            }

            var topUserItems = new List<string>();
            foreach (DataRow row in topUpUsers.Rows)
            {
                string name = row["MemberName"] == DBNull.Value ? "Unknown" : row["MemberName"].ToString();
                string cnt = row["TopUpCount"].ToString();
                string amt = Convert.ToDecimal(row["TotalTopUpAmount"]).ToString("0.00");

                topUserItems.Add("{\"name\":\"" + EscapeJson(name) + "\",\"count\":" + cnt + ",\"amount\":" + amt + "}");
            }

            var customerTypeItems = new List<string>
            {
                "{\"label\":\"Members\",\"value\":" + usersCount + ",\"color\":\"#10b981\"}",
                "{\"label\":\"Walk-In\",\"value\":" + walkInCount + ",\"color\":\"#3b82f6\"}"
            };

            // Build Daily Sales vs Cost dataset
            var dailyData = new SortedDictionary<string, (decimal Sales, decimal Cost)>();

            if (salesLogs != null)
            {
                foreach (DataRow row in salesLogs.Rows)
                {
                    string date = Convert.ToDateTime(row["DBDateCreated"]).ToString("MM/dd");
                    decimal totalCost = row["DBTotalCost"] != DBNull.Value ? Convert.ToDecimal(row["DBTotalCost"]) : 0;

                    if (!dailyData.ContainsKey(date)) dailyData[date] = (0, 0);
                    dailyData[date] = (dailyData[date].Sales + totalCost, dailyData[date].Cost);
                }
            }

            if (stockInLogs != null)
            {
                foreach (DataRow row in stockInLogs.Rows)
                {
                    string date = Convert.ToDateTime(row["DBDateCreated"]).ToString("MM/dd");
                    decimal totalCost = row["DBTotalCost"] != DBNull.Value ? Convert.ToDecimal(row["DBTotalCost"]) : 0;

                    if (!dailyData.ContainsKey(date)) dailyData[date] = (0, 0);
                    dailyData[date] = (dailyData[date].Sales, dailyData[date].Cost + totalCost);
                }
            }

            var salesVsCostItems = dailyData.Select(d =>
                "{\"date\":\"" + EscapeJson(d.Key) + "\",\"sales\":" + d.Value.Sales.ToString("0.00") + ",\"cost\":" + d.Value.Cost.ToString("0.00") + "}"
            );

            StringBuilder json = new StringBuilder();
            json.Append("{");
            json.Append("\"revenueTrend\":[").Append(string.Join(",", revenueItems)).Append("],");
            json.Append("\"computerUsage\":[").Append(string.Join(",", usageItems)).Append("],");
            json.Append("\"customerTypes\":[").Append(string.Join(",", customerTypeItems)).Append("],");
            json.Append("\"topUsers\":[").Append(string.Join(",", topUserItems)).Append("],");
            json.Append("\"salesVsCost\":[").Append(string.Join(",", salesVsCostItems)).Append("]");
            json.Append("}");

            return json.ToString();
        }

        private string EscapeJson(string input)
        {
            return string.IsNullOrEmpty(input)
                ? ""
                : input.Replace("\\", "\\\\").Replace("\"", "\\\"");
        }
    }
}