using BNet.Cafe.Server.Repositories;
using BNet.Cafe.Server.Services;
using System;
using System.Collections.Generic;
using System.Data;
using System.Text;

namespace BNet.Cafe.Server.Forms
{
    public partial class Dashboard : System.Web.UI.UserControl
    {
        private readonly Rentals rentals = new Rentals();
        private readonly BNet.Cafe.Server.Repositories.Dashboard dashboardRepository = new BNet.Cafe.Server.Repositories.Dashboard();

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

            // KPI cards
            var count = rentals.GetCount("", dateRange);
            Label_TotalTransactions.Text = count.Total;
            Label_Users.Text = count.Users;
            Label_WalkIn.Text = count.WalkIn;
            Label_Income.Text = count.Income;

            // Chart data
            DataTable revenueTrend = dashboardRepository.GetRevenueTrend(dateRange);
            DataTable computerUsage = dashboardRepository.GetComputerUsage(dateRange);

            HiddenField_ChartData.Value = BuildChartDataJson(count, revenueTrend, computerUsage);
        }

        // Builds the JSON payload consumed client-side by initBNetDashboardCharts()
        // in Dashboard.ascx. Kept as a plain HiddenField value (not an inline
        // <script> block) so it updates correctly across UpdatePanel async
        // postbacks — the client re-reads it via add_endRequest.
        private string BuildChartDataJson(Rentals.CountRentals count, DataTable revenueTrend, DataTable computerUsage)
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

            var customerTypeItems = new List<string>
            {
                "{\"label\":\"Members\",\"value\":" + usersCount + ",\"color\":\"#10b981\"}",
                "{\"label\":\"Walk-In\",\"value\":" + walkInCount + ",\"color\":\"#3b82f6\"}"
            };

            // string.Join never leaves a trailing comma before the closing
            // bracket — the previous version appended "," after every item
            // unconditionally, which produced invalid JSON like "[...,]" and
            // made JSON.parse() throw (silently, inside the client try/catch),
            // so nothing ever rendered.
            StringBuilder json = new StringBuilder();
            json.Append("{");
            json.Append("\"revenueTrend\":[").Append(string.Join(",", revenueItems)).Append("],");
            json.Append("\"computerUsage\":[").Append(string.Join(",", usageItems)).Append("],");
            json.Append("\"customerTypes\":[").Append(string.Join(",", customerTypeItems)).Append("]");
            json.Append("}");

            return json.ToString();
        }

        private string EscapeJson(string input)
        {
            return string.IsNullOrEmpty(input)
                ? ""
                : input.Replace("\\", "\\\\").Replace("\"", "\\\"");
        }

        protected void LinkButton_Refresh_Click(object sender, EventArgs e)
        {
            // Handles explicit refresh trigger without re-executing stale postback actions
        }

        protected void TextBox_DateRage_TextChanged(object sender, EventArgs e)
        {
            // Page_PreRender re-reads TextBox_DateRage.Text and refreshes KPIs + chart JSON
        }
    }
}