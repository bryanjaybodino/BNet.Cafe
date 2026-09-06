<%@ Page Language="C#" AutoEventWireup="true" Async="true" EnableEventValidation="false" AsyncTimeout="1000000000" CodeBehind="BNetPage.aspx.cs" Inherits="BNet.Cafe.Server.BNetPage" %>

<!DOCTYPE html>
<html lang="en">
<head>
    <meta charset="UTF-8">
    <meta name="viewport" content="width=device-width, initial-scale=1.0">
    <title>BNet Cafe</title>
    <link rel="stylesheet" href="https://cdnjs.cloudflare.com/ajax/libs/font-awesome/6.4.0/css/all.min.css">
    <link href="Assets/css/Style.css" rel="stylesheet" />
</head>
<body>
    <div id="pageLoadingOverlay" class="loading-overlay">
        <div class="loading-spinner"></div>
    </div>
    <div class="container">
        <!-- Sidebar -->
        <div class="sidebar" id="sidebar">
            <div class="logo">
                <i class="fas fa-cube"></i>
                <span>BNet Cafe</span>
            </div>
            <ul class="sidebar-menu">
                <li>
                    <asp:HyperLink ID="HyperLink_Dashboard" runat="server" ToolTip="Dashboard" onclick="navigateTo('?Form=Dashboard'); return false;">
            <i class="fas fa-home"></i><span>Dashboard</span>
                    </asp:HyperLink>
                </li>
                <li>
                    <asp:HyperLink ID="HyperLink_Computers" runat="server" ToolTip="Computer" onclick="navigateTo('?Form=Computers'); return false;">
            <i class="fas fa-computer"></i><span>Computers</span>
                    </asp:HyperLink>
                </li>
                <li>
                    <asp:HyperLink ID="HyperLink_Remote" runat="server" ToolTip="Remote" onclick="navigateTo('?Form=Remote'); return false;">
            <i class="fas fa-display"></i><span>Remote</span>
                    </asp:HyperLink>
                </li>
                <li>
                    <asp:HyperLink ID="HyperLink_BillingHistory" runat="server" ToolTip="BillingHistory" onclick="navigateTo('?Form=BillingHistory'); return false;">
            <i class="fa-solid fa-file-invoice-dollar"></i><span>Billing History</span>
                    </asp:HyperLink>
                </li>
                <li>
                    <asp:HyperLink ID="HyperLink_Users" runat="server" ToolTip="User" onclick="navigateTo('?Form=Users'); return false;">
            <i class="fas fa-users"></i><span>Users</span>
                    </asp:HyperLink>
                </li>
            </ul>
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
                        <div class="user-avatar">JD</div>
                        <div style="font-size: 13px;">
                            <div style="font-weight: 600; color: var(--text-light);">John Doe</div>
                            <div style="color: var(--text-light-secondary); font-size: 12px;">Admin</div>
                        </div>
                    </div>
                </div>
            </div>

            <!-- Main Content -->
            <div class="main-content">
                <form runat="server" id="MainForm">
                    <asp:ScriptManager ID="ScriptManager1" runat="server"></asp:ScriptManager>
                    <asp:PlaceHolder ID="PlaceHolder_Container" runat="server"></asp:PlaceHolder>
                </form>
            </div>
        </div>
    </div>
    <script src="Assets/js/Script.js"></script>

</body>
</html>
