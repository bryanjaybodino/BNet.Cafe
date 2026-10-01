<%@ Page Language="C#" AutoEventWireup="true" Async="true" EnableEventValidation="false" AsyncTimeout="1000000000" CodeBehind="BNetPage.aspx.cs" Inherits="BNet.Cafe.Server.BNetPage" %>

<%@ Register Src="~/Forms/Modals/GitHubUpdateModal.ascx" TagPrefix="uc1" TagName="GitHubUpdateModal" %>


<!DOCTYPE html>
<html lang="en">
<head>
    <script>
        (function () {
            var savedTheme = localStorage.getItem('theme') || 'light';
            document.documentElement.setAttribute('data-theme', savedTheme);
            document.documentElement.classList.add('no-transitions');
        })();
    </script>
    <meta charset="UTF-8">
    <meta name="viewport" content="width=device-width, initial-scale=1.0">
    <title>BNet Cafe</title>
    <link href="Assets/fontawesome/font-awesome.min.css" rel="stylesheet" />
    <% Response.Write(BNet.Cafe.Server.Services.FileCssHelper.StyleSheetVersion("Assets/bundle.min.css")); %>
</head>
<body>
    <div id="pageLoadingOverlay" class="loading-overlay">
        <div class="loading-spinner"></div>
    </div>
    <div class="container">
        <!-- Include GitHub Update Modal -->
        <uc1:GitHubUpdateModal runat="server" ID="GitHubUpdateModal" />
        <!-- Sidebar -->
        <div class="sidebar" id="sidebar">
            <div class="logo">
                <i class="fas fa-cube"></i>
                <span>BNet Cafe</span>
            </div>

            <div class="sidebar-menu">
                <!-- GENERAL / DASHBOARD -->
                <div class="menu-item">
                    <asp:HyperLink ID="HyperLink_Dashboard" runat="server" ToolTip="Dashboard" onclick="navigateTo('?Form=Dashboard'); return false;">
                        <i class="fas fa-home"></i><span>Dashboard</span>
                    </asp:HyperLink>
                </div>

                <!-- SECTION: COMPUTER RENTAL -->
                <details class="menu-group" id="group_Computers" runat="server">
                    <summary class="menu-header">
                        <i class="fas fa-desktop"></i>
                        <span>Computer Rental</span>
                        <i class="fas fa-chevron-down arrow"></i>
                    </summary>
                    <div class="menu-sub-items">
                        <asp:HyperLink ID="HyperLink_Computers" runat="server" ToolTip="Computer" onclick="navigateTo('?Form=Computers'); return false;">
                            <i class="fas fa-computer"></i><span>Computers</span>
                        </asp:HyperLink>
                        <asp:HyperLink ID="HyperLink_SeatMap" runat="server" ToolTip="SeatMap" onclick="navigateTo('?Form=SeatMap'); return false;">
                            <i class="fas fa-map-location-dot"></i><span>Seat Map</span>
                        </asp:HyperLink>
                        <asp:HyperLink ID="HyperLink_Remote" runat="server" ToolTip="Remote" onclick="navigateTo('?Form=Remote'); return false;">
                            <i class="fas fa-display"></i><span>Remote</span>
                        </asp:HyperLink>
                        <asp:HyperLink ID="HyperLink_PricingSettings" runat="server" ToolTip="PricingSettings" onclick="navigateTo('?Form=PricingSettings'); return false;">
                            <i class="fas fa-cash-register"></i><span>Pricing Setting</span>
                        </asp:HyperLink>
                    </div>
                </details>

                <!-- SECTION: POS & INVENTORY -->
                <details class="menu-group" id="group_POS" runat="server">
                    <summary class="menu-header">
                        <i class="fas fa-boxes-packing"></i>
                        <span>POS & Inventory</span>
                        <i class="fas fa-chevron-down arrow"></i>
                    </summary>
                    <div class="menu-sub-items">
                        <asp:HyperLink ID="HyperLink_POS" runat="server" ToolTip="POS" onclick="navigateTo('?Form=POS'); return false;">
                            <i class="fas fa-cart-shopping"></i><span>Point of Sale</span>
                        </asp:HyperLink>
                        <asp:HyperLink ID="HyperLink_StockIn" runat="server" ToolTip="StockIn" onclick="navigateTo('?Form=StockIn'); return false;">
                            <i class="fas fa-boxes-packing"></i><span>Stock In</span>
                        </asp:HyperLink>
                        <asp:HyperLink ID="HyperLink_Inventory" runat="server" ToolTip="Inventory" onclick="navigateTo('?Form=Inventory'); return false;">
                            <i class="fas fa-boxes-stacked"></i><span>Inventory</span>
                        </asp:HyperLink>
                        <asp:HyperLink ID="HyperLink_Suppliers" runat="server" ToolTip="Suppliers" onclick="navigateTo('?Form=Suppliers'); return false;">
                            <i class="fas fa-truck-field"></i><span>Suppliers</span>
                        </asp:HyperLink>
                    </div>
                </details>

                <!-- SECTION: SPORT TIMER -->
                <details class="menu-group" id="group_SportTimer" runat="server">
                    <summary class="menu-header">
                        <i class="fas fa-stopwatch"></i>
                        <span>Sport Timer</span>
                        <i class="fas fa-chevron-down arrow"></i>
                    </summary>
                    <div class="menu-sub-items">
                        <asp:HyperLink ID="HyperLink_SportTimer" runat="server" ToolTip="SportTimer" onclick="navigateTo('?Form=SportTimer'); return false;">
                            <i class="fas fa-stopwatch-20"></i><span>Sport Timer</span>
                        </asp:HyperLink>
                    </div>
                </details>

                <!-- SECTION: MANAGEMENT -->
                <details class="menu-group" id="group_Management" runat="server">
                    <summary class="menu-header">
                        <i class="fas fa-sliders"></i>
                        <span>Management</span>
                        <i class="fas fa-chevron-down arrow"></i>
                    </summary>
                    <div class="menu-sub-items">
                        <asp:HyperLink ID="HyperLink_Billings" runat="server" ToolTip="Billing" onclick="navigateTo('?Form=Billings'); return false;">
                            <i class="fa-solid fa-file-invoice-dollar"></i><span>Billings</span>
                        </asp:HyperLink>
                        <asp:HyperLink ID="HyperLink_Users" runat="server" ToolTip="User" onclick="navigateTo('?Form=Users'); return false;">
                            <i class="fas fa-users"></i><span>Users</span>
                        </asp:HyperLink>
                        <asp:HyperLink ID="HyperLink_TopUp" runat="server" ToolTip="TopUp" onclick="navigateTo('?Form=TopUp'); return false;">
                            <i class="fas fa-trophy"></i><span>Top-Up</span>
                        </asp:HyperLink>
                        <asp:HyperLink ID="HyperLink_WallPaper" runat="server" ToolTip="Wallpaper" onclick="navigateTo('?Form=Wallpaper'); return false;">
                            <i class="fas fa-image"></i><span>Wallpaper</span>
                        </asp:HyperLink>
                        <asp:HyperLink ID="HyperLink_Commits" runat="server" ToolTip="GitHubCommitsFeed" onclick="navigateTo('?Form=GitHubCommitsFeed'); return false;">
                            <i class="fab fa-github"></i><span>Commits</span>
                        </asp:HyperLink>
                    </div>
                </details>
            </div>
        </div>

        <!-- Main Wrapper -->
        <div class="main-wrapper" id="mainWrapper">
            <!-- Topbar -->
            <div class="topbar">
                <div class="topbar-left">
                    <button class="toggle-btn" id="toggleBtn" title="Toggle sidebar">
                        <i class="fas fa-bars"></i>
                    </button>
                </div>

                <div class="topbar-right">
                    <button class="theme-toggle" id="themeToggle" title="Toggle dark mode">
                        <i class="fas fa-moon"></i>
                    </button>
                    <div class="user-menu">
                        <div class="bnet-dropdown" tabindex="0">
                            <div class="user-menu" style="cursor: pointer;">
                                <div class="user-avatar">
                                    <asp:Label ID="Label_InitialName" runat="server" Text=""></asp:Label>
                                </div>
                                <div style="font-size: 13px;">
                                    <div style="font-weight: 600; color: var(--text-light);">
                                        <asp:Label ID="label_FullName" runat="server" Text=""></asp:Label>
                                    </div>
                                    <div style="color: var(--text-light-secondary); font-size: 12px;">Admin</div>
                                </div>
                                <i class="fas fa-chevron-down" style="font-size: 10px; margin-left: 4px;"></i>
                            </div>

                            <div class="bnet-dropdown-menu" style="right: 0; left: auto;">
                                <a href="BNetPage.aspx?Logout=true" class="bnet-dropdown-item">
                                    <i class="fas fa-right-from-bracket"></i>
                                    <span>Logout</span>
                                </a>
                            </div>
                        </div>
                    </div>
                </div>
            </div>

            <!-- Main Content -->
            <div class="main-content">
                <form runat="server" id="MainForm">
                    <asp:ScriptManager ID="ScriptManager1" EnableCdn="false" EnablePageMethods="true" EnablePartialRendering="true" AsyncPostBackTimeout="99999999" ScriptMode="Release" ValidateRequestMode="Enabled" EnableScriptLocalization="true" EnableScriptGlobalization="true" LoadScriptsBeforeUI="false" CompositeScript-ScriptMode="Release" CompositeScript-ResourceUICultures="Release" runat="server"></asp:ScriptManager>
                    <asp:PlaceHolder ID="PlaceHolder_Container" runat="server"></asp:PlaceHolder>
                </form>
            </div>
        </div>
    </div>
</body>
</html>