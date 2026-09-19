<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="Dashboard.ascx.cs" Inherits="BNet.Cafe.Server.Forms.Dashboard" %>
<link href="Assets/BNetChart/BNetChart.css" rel="stylesheet" />

<asp:UpdatePanel ID="UpdatePanel1" runat="server">
    <ContentTemplate>
        <asp:LinkButton ID="LinkButton_Refresh" OnClick="LinkButton_Refresh_Click" runat="server"></asp:LinkButton>
        <asp:HiddenField ID="HiddenField_ChartData" runat="server" />

        <div class="bnet-table-toolbar dashboard-toolbar">
            <h2 class="dashboard-heading">Dashboard</h2>
            <asp:TextBox ID="TextBox_DateRage" AutoPostBack="true" CssClass="bnet-datepicker" data-mode="range"
                runat="server" OnTextChanged="TextBox_DateRage_TextChanged"></asp:TextBox>
        </div>

        <!-- KPI Cards -->
        <div class="cards-grid cards-grid-compact">
            <div class="card card-compact">
                <div class="card-inline-content">
                    <div class="card-icon-sm">
                        <i class="fa-solid fa-receipt"></i>
                    </div>
                    <div class="card-details">
                        <span class="card-title-sm">Total Transactions</span>
                        <b>
                            <asp:Label ID="Label_TotalTransactions" runat="server" Text="0"></asp:Label>
                        </b>
                    </div>
                </div>
                <span class="bnet-badge-status blue">Overall</span>
            </div>

            <div class="card card-compact">
                <div class="card-inline-content">
                    <div class="card-icon-sm">
                        <i class="fa-solid fa-users"></i>
                    </div>
                    <div class="card-details">
                        <span class="card-title-sm">Users</span>
                        <b>
                            <asp:Label ID="Label_Users" runat="server" Text="0"></asp:Label>
                        </b>
                    </div>
                </div>
                <span class="bnet-badge-status green">Members</span>
            </div>

            <div class="card card-compact">
                <div class="card-inline-content">
                    <div class="card-icon-sm">
                        <i class="fa-solid fa-user-clock"></i>
                    </div>
                    <div class="card-details">
                        <span class="card-title-sm">Walk In</span>
                        <b>
                            <asp:Label ID="Label_WalkIn" runat="server" Text="0"></asp:Label>
                        </b>
                    </div>
                </div>
                <span class="bnet-badge-status gray">Guests</span>
            </div>

            <div class="card card-compact">
                <div class="card-inline-content">
                    <div class="card-icon-sm">
                        <i class="fa-solid fa-peso-sign"></i>
                    </div>
                    <div class="card-details">
                        <span class="card-title-sm">Income</span>
                        <b>
                            <asp:Label ID="Label_Income" runat="server" Text="₱0.00"></asp:Label>
                        </b>
                    </div>
                </div>
                <span class="bnet-badge-status yellow">Revenue</span>
            </div>

            <!-- Top-Up Load KPI -->
            <div class="card card-compact">
                <div class="card-inline-content">
                    <div class="card-icon-sm">
                        <i class="fa-solid fa-wallet"></i>
                    </div>
                    <div class="card-details">
                        <span class="card-title-sm">Top-Up Load</span>
                        <b >
                            <asp:Label ID="Label_TopUpIncome" runat="server" Text="₱0.00"></asp:Label>
                        </b>
                    </div>
                </div>
                <span class="bnet-badge-status green">Credits</span>
            </div>
        </div>

        <!-- Charts Row 1: Revenue trend + Customer types -->
        <div class="charts-grid">
            <div class="card-chart span-2">
                <div class="chart-header">
                    <span class="chart-title">Revenue Trend</span>
                </div>
                <div class="chart-container" id="ChartContainerLine">
                    <svg id="DashboardLineChart" viewBox="0 0 600 240"></svg>
                </div>
            </div>

            <div class="card-chart">
                <div class="chart-header">
                    <span class="chart-title">Customer Types</span>
                </div>
                <div class="chart-container" id="ChartContainerDonut">
                    <svg id="DashboardDonutChart" viewBox="0 0 200 200"></svg>
                </div>
                <div class="legend-container" id="DashboardDonutLegend">
                    <div class="legend-item"><div class="legend-color" style="background: var(--accent-green, #10b981);"></div>Members: <span id="LegendMembersValue">0</span></div>
                    <div class="legend-item"><div class="legend-color" style="background: var(--primary, #3b82f6);"></div>Walk-In: <span id="LegendWalkInValue">0</span></div>
                </div>
            </div>
        </div>

        <!-- Charts Row 2: Computer usage & Leaderboard -->
        <div class="charts-grid">
            <div class="card-chart span-2">
                <div class="chart-header">
                    <span class="chart-title">Computer Usage (Transactions per Terminal)</span>
                </div>
                <div class="chart-container" id="ChartContainerBar">
                    <svg id="DashboardBarChart" viewBox="0 0 800 240"></svg>
                </div>
            </div>

            <!-- Top 10 Member Spenders Leaderboard -->
            <div class="card-chart">
                <div class="chart-header">
                    <span class="chart-title">Top 10 Member Top-Up Spenders</span>
                </div>
                <div class="bnet-table-container">
                    <table class="bnet-table" id="TableTopUsers">
                        <thead>
                            <tr>
                                <th>#</th>
                                <th>Member</th>
                                <th>Count</th>
                                <th>Amount</th>
                            </tr>
                        </thead>
                        <tbody id="TableTopUsersBody">
                        </tbody>
                    </table>
                </div>
            </div>
        </div>
    </ContentTemplate>
</asp:UpdatePanel>



<script>
    (function () {
        function renderTopUsersTable(topUsers) {
            var tbody = document.getElementById("TableTopUsersBody");
            if (!tbody) return;
            tbody.innerHTML = "";

            if (!topUsers || topUsers.length === 0) {
                tbody.innerHTML = "<tr><td colspan='4' style='text-align:center;'>No top-ups</td></tr>";
                return;
            }

            topUsers.forEach(function (user, index) {
                var tr = document.createElement("tr");
                tr.innerHTML =
                    "<td><b>#" + (index + 1) + "</b></td>" +
                    "<td>" + user.name + "</td>" +
                    "<td>" + user.count + "x</td>" +
                    "<td><strong style='color: var(--accent-green, #10b981);'>₱" + user.amount.toFixed(2) + "</strong></td>";
                tbody.appendChild(tr);
            });
        }

        function initBNetDashboardCharts() {
            var hidden = document.querySelector("input[id*='HiddenField_ChartData']");
            if (!hidden || !hidden.value) return;

            var data;
            try {
                data = JSON.parse(hidden.value);
            } catch (ex) {
                console.error("Dashboard chart data JSON invalid:", ex, hidden.value);
                return;
            }

            // 1. Render Line Chart using BNetLineChart
            if (typeof BNetLineChart !== "undefined") {
                window.dashboardLineChart = new BNetLineChart({
                    container: "#ChartContainerLine",
                    viewBox: "0 0 600 240",
                    width: 600,
                    height: 240,
                    xKey: "date",
                    yKey: "amount",
                    currencySymbol: "₱",
                    data: data.revenueTrend || []
                });
            }

            // 2. Render Donut Chart using BNetDonutChart
            if (typeof BNetDonutChart !== "undefined") {
                window.dashboardDonutChart = new BNetDonutChart({
                    container: "#ChartContainerDonut",
                    viewBox: "0 0 200 200",
                    radius: 65,
                    cx: 100,
                    cy: 100,
                    labelKey: "label",
                    valueKey: "value",
                    colorKey: "color",
                    data: data.customerTypes || [],
                    onLegendUpdate: function (customerTypes) {
                        var membersEl = document.getElementById("LegendMembersValue");
                        var walkInEl = document.getElementById("LegendWalkInValue");
                        var members = customerTypes.find(function (c) { return c.label === "Members"; });
                        var walkIn = customerTypes.find(function (c) { return c.label === "Walk-In"; });
                        if (membersEl) membersEl.textContent = members ? members.value : 0;
                        if (walkInEl) walkInEl.textContent = walkIn ? walkIn.value : 0;
                    }
                });
            }

            // 3. Render Bar Chart using BNetBarChart
            if (typeof BNetBarChart !== "undefined") {
                window.dashboardBarChart = new BNetBarChart({
                    container: "#ChartContainerBar",
                    viewBox: "0 0 800 240",
                    width: 800,
                    height: 240,
                    labelKey: "pc",
                    valueKey: "count",
                    unitLabel: "rentals",
                    data: data.computerUsage || []
                });
            }

            // 4. Populate Leaderboard Table
            renderTopUsersTable(data.topUsers || []);
        }

        document.addEventListener("DOMContentLoaded", initBNetDashboardCharts);

        // Bind for ASP.NET AJAX UpdatePanel postbacks
        if (typeof Sys !== "undefined" && Sys.WebForms && Sys.WebForms.PageRequestManager) {
            Sys.WebForms.PageRequestManager.getInstance().add_endRequest(function () {
                initBNetDashboardCharts();
            });
        }

        window.initBNetDashboardCharts = initBNetDashboardCharts;
    })();
</script>