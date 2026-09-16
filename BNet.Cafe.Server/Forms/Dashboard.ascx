<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="Dashboard.ascx.cs" Inherits="BNet.Cafe.Server.Forms.Dashboard" %>
<asp:UpdatePanel ID="UpdatePanel1" runat="server">
    <ContentTemplate>
        <asp:LinkButton ID="LinkButton_Refresh" OnClick="LinkButton_Refresh_Click" runat="server"></asp:LinkButton>
        <asp:HiddenField ID="HiddenField_ChartData" runat="server" />

        <div class="bnet-table-toolbar dashboard-toolbar">
            <h2 class="dashboard-heading">Dashboard</h2>
            <asp:TextBox ID="TextBox_DateRage" AutoPostBack="true" CssClass="bnet-datepicker" data-mode="range"
                runat="server" OnTextChanged="TextBox_DateRage_TextChanged"></asp:TextBox>
        </div>

        <!-- KPI Cards (same markup/classes as Billings.ascx) -->
        <div class="cards-grid cards-grid-compact">
            <div class="card card-compact">
                <div class="card-inline-content">
                    <div class="card-icon-sm">
                        <i class="fa-solid fa-receipt"></i>
                    </div>
                    <div class="card-details">
                        <span class="card-title-sm">Total Transactions</span>
                        <span class="card-value-sm">
                            <asp:Label ID="Label_TotalTransactions" runat="server" Text="0"></asp:Label>
                        </span>
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
                        <span class="card-value-sm">
                            <asp:Label ID="Label_Users" runat="server" Text="0"></asp:Label>
                        </span>
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
                        <span class="card-value-sm">
                            <asp:Label ID="Label_WalkIn" runat="server" Text="0"></asp:Label>
                        </span>
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
                        <span class="card-value-sm">
                            <asp:Label ID="Label_Income" runat="server" Text="₱0.00"></asp:Label>
                        </span>
                    </div>
                </div>
                <span class="bnet-badge-status yellow">Revenue</span>
            </div>
        </div>

        <!-- Charts Row 1: Revenue trend + Customer types -->
        <div class="charts-grid">
            <div class="card-chart span-2">
                <div class="chart-header">
                    <span class="chart-title">Revenue Trend</span>
                </div>
                <div class="chart-container">
                    <svg id="DashboardLineChart" viewBox="0 0 600 240"></svg>
                </div>
            </div>

            <div class="card-chart">
                <div class="chart-header">
                    <span class="chart-title">Customer Types</span>
                </div>
                <div class="chart-container">
                    <svg id="DashboardDonutChart" viewBox="0 0 200 200"></svg>
                </div>
                <div class="legend-container">
                    <div class="legend-item"><div class="legend-color" style="background: var(--accent-green);"></div>Members: <span id="LegendMembersValue">0</span></div>
                    <div class="legend-item"><div class="legend-color" style="background: var(--primary);"></div>Walk-In: <span id="LegendWalkInValue">0</span></div>
                </div>
            </div>
        </div>

        <!-- Charts Row 2: Computer usage -->
        <div class="charts-grid">
            <div class="card-chart" style="grid-column: 1 / -1;">
                <div class="chart-header">
                    <span class="chart-title">Computer Usage (Transactions per Terminal)</span>
                </div>
                <div class="chart-container">
                    <svg id="DashboardBarChart" viewBox="0 0 800 240"></svg>
                </div>
            </div>
        </div>

        <div class="chart-tooltip" id="DashboardTooltip"></div>
    </ContentTemplate>
</asp:UpdatePanel>

<style>
    /* Scoped styles for the chart section. Card / badge / table classes are
       already defined in the site's shared Style.css (see Billings.ascx). */
    .dashboard-toolbar {
        display: flex;
        align-items: center;
        justify-content: space-between;
        margin-bottom: 16px;
    }

    .dashboard-heading {
        font-size: 20px;
        font-weight: 700;
        color: var(--text-light);
        margin: 0;
    }

    .charts-grid {
        display: grid;
        grid-template-columns: repeat(3, 1fr);
        gap: 20px;
        margin-top: 20px;
    }

    @media (max-width: 1024px) {
        .charts-grid {
            grid-template-columns: 1fr;
        }
    }

    .card-chart {
        background-color: var(--bg-light-secondary);
        border: 1px solid var(--border-light);
        border-radius: 12px;
        padding: 24px;
        display: flex;
        flex-direction: column;
    }

    .card-chart.span-2 {
        grid-column: span 2;
    }

    @media (max-width: 1024px) {
        .card-chart.span-2 {
            grid-column: span 1;
        }
    }

    .chart-header {
        display: flex;
        justify-content: space-between;
        align-items: center;
        margin-bottom: 20px;
    }

    .chart-title {
        font-size: 1.1rem;
        font-weight: 600;
        color: var(--text-light);
    }

    .chart-container {
        position: relative;
        flex-grow: 1;
        min-height: 250px;
        display: flex;
        align-items: center;
        justify-content: center;
    }

    .chart-container svg {
        width: 100%;
        height: 100%;
        overflow: visible;
    }

    .donut-segment {
        transition: stroke-width 0.2s ease;
        cursor: pointer;
    }

    .donut-segment:hover {
        stroke-width: 26;
    }

    .legend-container {
        display: flex;
        justify-content: center;
        gap: 16px;
        margin-top: 16px;
        flex-wrap: wrap;
    }

    .legend-item {
        display: flex;
        align-items: center;
        gap: 8px;
        font-size: 0.85rem;
        color: var(--text-light-secondary);
    }

    .legend-color {
        width: 10px;
        height: 10px;
        border-radius: 50%;
    }

    .chart-tooltip {
        position: fixed;
        background: var(--bg-light-tertiary);
        border: 1px solid var(--border-light);
        color: var(--text-light);
        padding: 6px 12px;
        border-radius: 6px;
        font-size: 0.8rem;
        pointer-events: none;
        opacity: 0;
        transition: opacity 0.15s ease;
        z-index: 999;
        box-shadow: 0 4px 6px rgba(0, 0, 0, 0.1);
    }
</style>

<script>
    // Render functions are defined ONCE, client-side, in plain markup that is
    // part of the initial page (not re-sent by the server on postback).
    // Data comes from HiddenField_ChartData, which code-behind refreshes on
    // every Page_PreRender. Since it's a plain form element inside the
    // UpdatePanel, its .value is correctly applied even when the surrounding
    // markup is swapped in via innerHTML during an async postback — unlike a
    // <script> tag, which the browser would silently refuse to re-execute.
    (function () {
        function showDashboardTooltip(evt, text) {
            var tooltip = document.getElementById("DashboardTooltip");
            if (!tooltip) return;
            tooltip.innerText = text;
            tooltip.style.opacity = "1";
            tooltip.style.left = evt.clientX + 12 + "px";
            tooltip.style.top = evt.clientY - 28 + "px";
        }

        function hideDashboardTooltip() {
            var tooltip = document.getElementById("DashboardTooltip");
            if (tooltip) tooltip.style.opacity = "0";
        }

        function renderDonut(customerTypes) {
            var svg = document.getElementById("DashboardDonutChart");
            if (!svg) return;
            svg.innerHTML = "";

            var total = customerTypes.reduce(function (acc, item) { return acc + item.value; }, 0);
            var radius = 65;
            var cx = 100, cy = 100;

            if (total <= 0) {
                var emptyText = document.createElementNS("http://www.w3.org/2000/svg", "text");
                emptyText.setAttribute("x", cx);
                emptyText.setAttribute("y", cy);
                emptyText.setAttribute("text-anchor", "middle");
                emptyText.setAttribute("fill", "var(--text-light-secondary, #888)");
                emptyText.setAttribute("font-size", "14");
                emptyText.textContent = "No data";
                svg.appendChild(emptyText);
                return;
            }

            var accumulatedAngle = 0;

            customerTypes.forEach(function (item) {
                if (item.value <= 0) return;

                var angle = (item.value / total) * 360;
                var startAngle = accumulatedAngle;
                var endAngle = accumulatedAngle + angle;
                accumulatedAngle += angle;

                var effectiveEndAngle = angle >= 360 ? startAngle + 359.999 : endAngle;

                var x1 = cx + radius * Math.cos(Math.PI * (startAngle - 90) / 180);
                var y1 = cy + radius * Math.sin(Math.PI * (startAngle - 90) / 180);
                var x2 = cx + radius * Math.cos(Math.PI * (effectiveEndAngle - 90) / 180);
                var y2 = cy + radius * Math.sin(Math.PI * (effectiveEndAngle - 90) / 180);

                var largeArcFlag = angle > 180 ? 1 : 0;
                var pathData = "M " + x1 + " " + y1 + " A " + radius + " " + radius + " 0 " + largeArcFlag + " 1 " + x2 + " " + y2;

                var path = document.createElementNS("http://www.w3.org/2000/svg", "path");
                path.setAttribute("d", pathData);
                path.setAttribute("fill", "none");

                // Ensure hex fallbacks are present if CSS variable fails to resolve
                var fallbackColor = item.label === "Members" ? "#10b981" : "#3b82f6";
                path.setAttribute("stroke", item.color || fallbackColor);
                path.setAttribute("stroke-width", "20");
                path.setAttribute("stroke-linecap", "butt"); // Prevents slice edges from hiding behind each other
                path.setAttribute("class", "donut-segment");

                path.addEventListener("mousemove", function (e) {
                    showDashboardTooltip(e, item.label + ": " + item.value + " (" + ((item.value / total) * 100).toFixed(1) + "%)");
                });
                path.addEventListener("mouseleave", hideDashboardTooltip);

                svg.appendChild(path);
            });

            var text = document.createElementNS("http://www.w3.org/2000/svg", "text");
            text.setAttribute("x", cx);
            text.setAttribute("y", cy + 5);
            text.setAttribute("text-anchor", "middle");
            text.setAttribute("fill", "var(--text-light, #333)");
            text.setAttribute("font-size", "18");
            text.setAttribute("font-weight", "bold");
            text.textContent = total;
            svg.appendChild(text);
        }

        function renderLine(revenueTrend) {
            var svg = document.getElementById("DashboardLineChart");
            if (!svg) return;
            svg.innerHTML = "";

            var width = 600, height = 240, padding = 40;

            // Check if empty or all amounts sum to <= 0
            var totalAmount = revenueTrend.reduce(function (acc, item) { return acc + (item.amount || 0); }, 0);
            if (!revenueTrend.length || totalAmount <= 0) {
                var emptyText = document.createElementNS("http://www.w3.org/2000/svg", "text");
                emptyText.setAttribute("x", width / 2);
                emptyText.setAttribute("y", height / 2);
                emptyText.setAttribute("text-anchor", "middle");
                emptyText.setAttribute("fill", "var(--text-light-secondary, #888)");
                emptyText.setAttribute("font-size", "14");
                emptyText.textContent = "No data";
                svg.appendChild(emptyText);
                return;
            }

            var maxVal = Math.max.apply(null, revenueTrend.map(function (d) { return d.amount; })) * 1.1 || 1;
            var stepX = revenueTrend.length > 1 ? (width - padding * 2) / (revenueTrend.length - 1) : 0;

            var points = revenueTrend.map(function (d, i) {
                var x = padding + i * stepX;
                var y = height - padding - (d.amount / maxVal) * (height - padding * 2);
                return { x: x, y: y, data: d };
            });

            // Draw gridlines and labels
            for (var i = 0; i <= 4; i++) {
                var y = height - padding - (i / 4) * (height - padding * 2);
                var val = (maxVal * (i / 4)).toFixed(0);

                var line = document.createElementNS("http://www.w3.org/2000/svg", "line");
                line.setAttribute("x1", padding); line.setAttribute("y1", y);
                line.setAttribute("x2", width - padding); line.setAttribute("y2", y);
                line.setAttribute("stroke", "var(--border-light)");
                line.setAttribute("stroke-dasharray", "4");
                svg.appendChild(line);

                var label = document.createElementNS("http://www.w3.org/2000/svg", "text");
                label.setAttribute("x", padding - 10); label.setAttribute("y", y + 4);
                label.setAttribute("text-anchor", "end");
                label.setAttribute("fill", "var(--text-light-secondary)");
                label.setAttribute("font-size", "10");
                label.textContent = "\u20B1" + val;
                svg.appendChild(label);
            }

            // Draw path
            var pathData = points.reduce(function (acc, pt, i) {
                return i === 0 ? ("M " + pt.x + " " + pt.y) : (acc + " L " + pt.x + " " + pt.y);
            }, "");

            var path = document.createElementNS("http://www.w3.org/2000/svg", "path");
            path.setAttribute("d", pathData);
            path.setAttribute("fill", "none");
            path.setAttribute("stroke", "var(--primary)");
            path.setAttribute("stroke-width", "3");
            svg.appendChild(path);

            // Draw data points & x-axis labels
            points.forEach(function (pt) {
                var circle = document.createElementNS("http://www.w3.org/2000/svg", "circle");
                circle.setAttribute("cx", pt.x); circle.setAttribute("cy", pt.y);
                circle.setAttribute("r", "5");
                circle.setAttribute("fill", "var(--primary)");
                circle.style.cursor = "pointer";
                circle.addEventListener("mousemove", function (e) {
                    showDashboardTooltip(e, pt.data.date + ": \u20B1" + pt.data.amount.toFixed(2));
                });
                circle.addEventListener("mouseleave", hideDashboardTooltip);
                svg.appendChild(circle);

                var dateLabel = document.createElementNS("http://www.w3.org/2000/svg", "text");
                dateLabel.setAttribute("x", pt.x); dateLabel.setAttribute("y", height - 10);
                dateLabel.setAttribute("text-anchor", "middle");
                dateLabel.setAttribute("fill", "var(--text-light-secondary)");
                dateLabel.setAttribute("font-size", "11");
                dateLabel.textContent = pt.data.date;
                svg.appendChild(dateLabel);
            });
        }
        function renderBar(computerUsage) {
            var svg = document.getElementById("DashboardBarChart");
            if (!svg) return;
            svg.innerHTML = "";

            var width = 800, height = 240, padding = 40;

            // Check if empty or all counts sum to <= 0
            var totalCount = computerUsage.reduce(function (acc, item) { return acc + (item.count || 0); }, 0);
            if (!computerUsage.length || totalCount <= 0) {
                var emptyText = document.createElementNS("http://www.w3.org/2000/svg", "text");
                emptyText.setAttribute("x", width / 2);
                emptyText.setAttribute("y", height / 2);
                emptyText.setAttribute("text-anchor", "middle");
                emptyText.setAttribute("fill", "var(--text-light-secondary, #888)");
                emptyText.setAttribute("font-size", "14");
                emptyText.textContent = "No data";
                svg.appendChild(emptyText);
                return;
            }

            var maxVal = Math.max.apply(null, computerUsage.map(function (d) { return d.count; })) * 1.15 || 1;
            var availableWidth = width - padding * 2;
            var gap = availableWidth / computerUsage.length;
            var barWidth = Math.min(60, gap * 0.6);

            // Gridlines and Y-axis labels
            for (var i = 0; i <= 4; i++) {
                var y = height - padding - (i / 4) * (height - padding * 2);
                var val = (maxVal * (i / 4)).toFixed(0);

                var line = document.createElementNS("http://www.w3.org/2000/svg", "line");
                line.setAttribute("x1", padding); line.setAttribute("y1", y);
                line.setAttribute("x2", width - padding); line.setAttribute("y2", y);
                line.setAttribute("stroke", "var(--border-light)");
                line.setAttribute("stroke-dasharray", "4");
                svg.appendChild(line);

                var label = document.createElementNS("http://www.w3.org/2000/svg", "text");
                label.setAttribute("x", padding - 10); label.setAttribute("y", y + 4);
                label.setAttribute("text-anchor", "end");
                label.setAttribute("fill", "var(--text-light-secondary)");
                label.setAttribute("font-size", "10");
                label.textContent = val;
                svg.appendChild(label);
            }

            // Render Bars
            computerUsage.forEach(function (item, i) {
                var x = padding + i * gap + (gap - barWidth) / 2;
                var barHeight = (item.count / maxVal) * (height - padding * 2);
                var y = height - padding - barHeight;

                var rect = document.createElementNS("http://www.w3.org/2000/svg", "rect");
                rect.setAttribute("x", x); rect.setAttribute("y", y);
                rect.setAttribute("width", barWidth); rect.setAttribute("height", barHeight);
                rect.setAttribute("fill", "var(--secondary)");
                rect.setAttribute("rx", "4");
                rect.style.cursor = "pointer";
                rect.addEventListener("mousemove", function (e) {
                    showDashboardTooltip(e, item.pc + ": " + item.count + " rentals");
                });
                rect.addEventListener("mouseleave", hideDashboardTooltip);
                svg.appendChild(rect);

                var pcLabel = document.createElementNS("http://www.w3.org/2000/svg", "text");
                pcLabel.setAttribute("x", x + barWidth / 2); pcLabel.setAttribute("y", height - 12);
                pcLabel.setAttribute("text-anchor", "middle");
                pcLabel.setAttribute("fill", "var(--text-light-secondary)");
                pcLabel.setAttribute("font-size", "12");
                pcLabel.textContent = item.pc;
                svg.appendChild(pcLabel);

                var countLabel = document.createElementNS("http://www.w3.org/2000/svg", "text");
                countLabel.setAttribute("x", x + barWidth / 2); countLabel.setAttribute("y", y - 8);
                countLabel.setAttribute("text-anchor", "middle");
                countLabel.setAttribute("fill", "var(--text-light)");
                countLabel.setAttribute("font-size", "11");
                countLabel.setAttribute("font-weight", "bold");
                countLabel.textContent = item.count;
                svg.appendChild(countLabel);
            });
        }

        function updateLegend(customerTypes) {
            var membersEl = document.getElementById("LegendMembersValue");
            var walkInEl = document.getElementById("LegendWalkInValue");
            var members = customerTypes.filter(function (c) { return c.label === "Members"; })[0];
            var walkIn = customerTypes.filter(function (c) { return c.label === "Walk-In"; })[0];
            if (membersEl) membersEl.textContent = members ? members.value : 0;
            if (walkInEl) walkInEl.textContent = walkIn ? walkIn.value : 0;
        }

        function initBNetDashboardCharts() {
            var hidden = document.getElementById("<%= HiddenField_ChartData.ClientID %>");
            if (!hidden || !hidden.value) return;

            var data;
            try {
                data = JSON.parse(hidden.value);
            } catch (ex) {
                console.error("Dashboard chart data was not valid JSON:", ex, hidden.value);
                return;
            }

            renderDonut(data.customerTypes || []);
            renderLine(data.revenueTrend || []);
            renderBar(data.computerUsage || []);
            updateLegend(data.customerTypes || []);
        }

        document.addEventListener("DOMContentLoaded", initBNetDashboardCharts);

        // Support ASP.NET AJAX UpdatePanel partial postbacks
        if (typeof Sys !== "undefined" && Sys.WebForms && Sys.WebForms.PageRequestManager) {
            Sys.WebForms.PageRequestManager.getInstance().add_endRequest(function () {
                initBNetDashboardCharts();
            });
        }

        window.initBNetDashboardCharts = initBNetDashboardCharts;
    })();

    console.log(JSON.parse(document.getElementById('<%= HiddenField_ChartData.ClientID %>').value).customerTypes);
</script>
