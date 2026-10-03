<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="Dashboard.ascx.cs" Inherits="BNet.Cafe.Server.Forms.Dashboard" %>

<asp:UpdatePanel ID="UpdatePanel1" runat="server">
    <ContentTemplate>
        <asp:LinkButton ID="LinkButton_Refresh" runat="server"></asp:LinkButton>
        <asp:HiddenField ID="HiddenField_ChartData" runat="server" />

        <div class="bnet-table-toolbar dashboard-toolbar">
            <h2 class="dashboard-heading">Dashboard</h2>
            <asp:TextBox ID="TextBox_DateRage" AutoPostBack="true" CssClass="bnet-datepicker" data-mode="range"
                runat="server" OnTextChanged="TextBox_DateRage_TextChanged"></asp:TextBox>
        </div>

        <!-- KPI Cards Grid -->
        <div class="cards-grid cards-grid-compact">
            <!-- Transactions -->
            <div class="card card-compact">
                <div class="card-inline-content">
                    <div class="card-icon-sm">
                        <i class="fa-solid fa-receipt"></i>
                    </div>
                    <div class="card-details">
                        <span class="card-title-sm">Total Transactions</span>
                        <b>
                            <asp:Label ID="Label_TotalTransactions" runat="server" Text="0"></asp:Label></b>
                    </div>
                </div>
                <span class="bnet-badge-status blue">Overall</span>
            </div>

            <!-- Users -->
            <div class="card card-compact">
                <div class="card-inline-content">
                    <div class="card-icon-sm">
                        <i class="fa-solid fa-users"></i>
                    </div>
                    <div class="card-details">
                        <span class="card-title-sm">Users</span>
                        <b>
                            <asp:Label ID="Label_Users" runat="server" Text="0"></asp:Label></b>
                    </div>
                </div>
                <span class="bnet-badge-status green">Members</span>
            </div>

            <!-- Walk-In -->
            <div class="card card-compact">
                <div class="card-inline-content">
                    <div class="card-icon-sm">
                        <i class="fa-solid fa-user-clock"></i>
                    </div>
                    <div class="card-details">
                        <span class="card-title-sm">Walk In</span>
                        <b>
                            <asp:Label ID="Label_WalkIn" runat="server" Text="0"></asp:Label></b>
                    </div>
                </div>
                <span class="bnet-badge-status gray">Guests</span>
            </div>

            <!-- Rental Income -->
            <div class="card card-compact">
                <div class="card-inline-content">
                    <div class="card-icon-sm">
                        <i class="fa-solid fa-desktop"></i>
                    </div>
                    <div class="card-details">
                        <span class="card-title-sm">Rental Income</span>
                        <b>
                            <asp:Label ID="Label_Income" runat="server" Text="₱0.00"></asp:Label></b>
                    </div>
                </div>
                <span class="bnet-badge-status yellow">Rentals</span>
            </div>
            <!-- Top-Up Load -->
            <div class="card card-compact">
                <div class="card-inline-content">
                    <div class="card-icon-sm">
                        <i class="fa-solid fa-wallet"></i>
                    </div>
                    <div class="card-details">
                        <span class="card-title-sm">Top-Up Load</span>
                        <b>
                            <asp:Label ID="Label_TopUpIncome" runat="server" Text="₱0.00"></asp:Label></b>
                    </div>
                </div>
                <span class="bnet-badge-status green">Credits</span>
            </div>
        </div>
        <div class="cards-grid cards-grid-compact">
            <!-- Product Sales Revenue -->
            <div class="card card-compact">
                <div class="card-inline-content">
                    <div class="card-icon-sm">
                        <i class="fa-solid fa-cart-shopping"></i>
                    </div>
                    <div class="card-details">
                        <span class="card-title-sm">Sales Revenue</span>
                        <b>
                            <asp:Label ID="Label_SalesRevenue" runat="server" Text="₱0.00"></asp:Label></b>
                    </div>
                </div>
                <span class="bnet-badge-status green">Sales</span>
            </div>

            <!-- Total Restock Costs -->
            <div class="card card-compact">
                <div class="card-inline-content">
                    <div class="card-icon-sm">
                        <i class="fa-solid fa-coins"></i>
                    </div>
                    <div class="card-details">
                        <span class="card-title-sm">Restock Costs</span>
                        <b>
                            <asp:Label ID="Label_TotalCosts" runat="server" Text="₱0.00"></asp:Label></b>
                    </div>
                </div>
                <span class="bnet-badge-status red">Stock In Cost</span>
            </div>

            <!-- Inventory Summary -->
            <div class="card card-compact">
                <div class="card-inline-content">
                    <div class="card-icon-sm">
                        <i class="fa-solid fa-boxes-stacked"></i>
                    </div>
                    <div class="card-details">
                        <span class="card-title-sm">Inventory Items</span>
                        <b>
                            <asp:Label ID="Label_TotalInventory" runat="server" Text="0"></asp:Label></b>
                    </div>
                </div>
                <span class="bnet-badge-status gray">Low:
                    <asp:Label ID="Label_LowStockCount" runat="server" Text="0"></asp:Label>
                </span>
            </div>

            <!-- Active Suppliers -->
            <div class="card card-compact">
                <div class="card-inline-content">
                    <div class="card-icon-sm">
                        <i class="fa-solid fa-truck-field"></i>
                    </div>
                    <div class="card-details">
                        <span class="card-title-sm">Suppliers</span>
                        <b>
                            <asp:Label ID="Label_TotalSuppliers" runat="server" Text="0"></asp:Label></b>
                    </div>
                </div>
                <span class="bnet-badge-status blue">Active</span>
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
                    <div class="legend-item">
                        <div class="legend-color" style="background: var(--accent-green, #10b981);"></div>
                        Members: <span id="LegendMembersValue">0</span>
                    </div>
                    <div class="legend-item">
                        <div class="legend-color" style="background: var(--primary, #3b82f6);"></div>
                        Walk-In: <span id="LegendWalkInValue">0</span>
                    </div>
                </div>
            </div>
        </div>

        <!-- Charts Row 2: Sales vs Restock Cost Trend -->
        <div class="charts-grid">
            <div class="card-chart span-3">
                <div class="chart-header">
                    <span class="chart-title">Sales vs. Restock Cost Overview</span>
                    <div class="legend-container" style="margin-top: 0;">
                        <div class="legend-item">
                            <div class="legend-color" style="background: #10b981;"></div>
                            Sales Revenue
                        </div>
                        <div class="legend-item">
                            <div class="legend-color" style="background: #ef4444;"></div>
                            Restock Cost
                        </div>
                    </div>
                </div>
                <div class="chart-container" id="ChartContainerSalesVsCost">
                    <svg id="DashboardSalesVsCostChart" viewBox="0 0 900 240"></svg>
                </div>
            </div>
        </div>

        <!-- Charts Row 3: Computer usage & Leaderboard -->
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

        function renderSalesVsCostChart(salesVsCost) {
            var svg = document.getElementById("DashboardSalesVsCostChart");
            if (!svg) return;
            svg.innerHTML = "";

            if (!salesVsCost || salesVsCost.length === 0) {
                var emptyText = document.createElementNS("http://www.w3.org/2000/svg", "text");
                emptyText.setAttribute("x", "450");
                emptyText.setAttribute("y", "120");
                emptyText.setAttribute("text-anchor", "middle");
                emptyText.setAttribute("fill", "#888");
                emptyText.textContent = "No Sales or Cost Data Available";
                svg.appendChild(emptyText);
                return;
            }

            var width = 900;
            var height = 240;
            var padding = { top: 25, right: 30, bottom: 40, left: 50 };
            var chartW = width - padding.left - padding.right;
            var chartH = height - padding.top - padding.bottom;

            var maxVal = 10;
            salesVsCost.forEach(function (d) {
                if (d.sales > maxVal) maxVal = d.sales;
                if (d.cost > maxVal) maxVal = d.cost;
            });
            maxVal *= 1.15; // Padding above highest bar

            // Horizontal Grid Lines & Y-Axis Scale
            for (var k = 0; k <= 4; k++) {
                var yPos = (height - padding.bottom) - (k / 4) * chartH;
                var gridValue = (maxVal * (k / 4)).toFixed(0);

                var line = document.createElementNS("http://www.w3.org/2000/svg", "line");
                line.setAttribute("x1", padding.left);
                line.setAttribute("y1", yPos);
                line.setAttribute("x2", width - padding.right);
                line.setAttribute("y2", yPos);
                line.setAttribute("stroke", "var(--border-light, #334155)");
                line.setAttribute("stroke-dasharray", "4");
                svg.appendChild(line);

                var yLabel = document.createElementNS("http://www.w3.org/2000/svg", "text");
                yLabel.setAttribute("x", padding.left - 10);
                yLabel.setAttribute("y", yPos + 4);
                yLabel.setAttribute("text-anchor", "end");
                yLabel.setAttribute("fill", "var(--text-light-secondary, #94a3b8)");
                yLabel.setAttribute("font-size", "10");
                yLabel.textContent = "₱" + gridValue;
                svg.appendChild(yLabel);
            }

            var tooltip = document.getElementById("BNetChartTooltip");
            function showChartTooltip(evt, text) {
                if (!tooltip) return;
                tooltip.innerText = text;
                tooltip.style.opacity = "1";
                tooltip.style.left = (evt.clientX + 12) + "px";
                tooltip.style.top = (evt.clientY - 28) + "px";
            }
            function hideChartTooltip() {
                if (tooltip) tooltip.style.opacity = "0";
            }

            var groupWidth = chartW / salesVsCost.length;
            var barWidth = Math.min(28, (groupWidth - 16) / 2);

            salesVsCost.forEach(function (d, i) {
                var groupX = padding.left + i * groupWidth + (groupWidth / 2);

                var salesH = (d.sales / maxVal) * chartH;
                var salesY = (height - padding.bottom) - salesH;
                var salesX = groupX - barWidth - 3;

                var costH = (d.cost / maxVal) * chartH;
                var costY = (height - padding.bottom) - costH;
                var costX = groupX + 3;

                // Sales Bar
                var salesBar = document.createElementNS("http://www.w3.org/2000/svg", "rect");
                salesBar.setAttribute("x", salesX);
                salesBar.setAttribute("y", salesY);
                salesBar.setAttribute("width", barWidth);
                salesBar.setAttribute("height", salesH);
                salesBar.setAttribute("fill", "#10b981");
                salesBar.setAttribute("rx", "4");
                salesBar.addEventListener("mousemove", function (e) {
                    showChartTooltip(e, d.date + " Sales: ₱" + d.sales.toFixed(2));
                });
                salesBar.addEventListener("mouseleave", hideChartTooltip);
                svg.appendChild(salesBar);

                // Cost Bar
                var costBar = document.createElementNS("http://www.w3.org/2000/svg", "rect");
                costBar.setAttribute("x", costX);
                costBar.setAttribute("y", costY);
                costBar.setAttribute("width", barWidth);
                costBar.setAttribute("height", costH);
                costBar.setAttribute("fill", "#ef4444");
                costBar.setAttribute("rx", "4");
                costBar.addEventListener("mousemove", function (e) {
                    showChartTooltip(e, d.date + " Restock Cost: ₱" + d.cost.toFixed(2));
                });
                costBar.addEventListener("mouseleave", hideChartTooltip);
                svg.appendChild(costBar);

                // Date Label
                var dateLabel = document.createElementNS("http://www.w3.org/2000/svg", "text");
                dateLabel.setAttribute("x", groupX);
                dateLabel.setAttribute("y", height - 12);
                dateLabel.setAttribute("text-anchor", "middle");
                dateLabel.setAttribute("fill", "var(--text-light-secondary, #94a3b8)");
                dateLabel.setAttribute("font-size", "11");
                dateLabel.textContent = d.date;
                svg.appendChild(dateLabel);
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

            renderSalesVsCostChart(data.salesVsCost || []);
            renderTopUsersTable(data.topUsers || []);
        }

        document.addEventListener("DOMContentLoaded", initBNetDashboardCharts);

        if (typeof Sys !== "undefined" && Sys.WebForms && Sys.WebForms.PageRequestManager) {
            Sys.WebForms.PageRequestManager.getInstance().add_endRequest(function () {
                initBNetDashboardCharts();
            });
        }

        window.initBNetDashboardCharts = initBNetDashboardCharts;
    })();
</script>
