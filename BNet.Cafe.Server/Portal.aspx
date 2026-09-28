<%@ Page Language="C#" AutoEventWireup="true" Async="true" EnableEventValidation="false" AsyncTimeout="1000000000" CodeBehind="Portal.aspx.cs" Inherits="BNet.Cafe.Server.Portal" %>

<!DOCTYPE html>
<html lang="en" xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <script>
        (function () {
            var savedTheme = localStorage.getItem('theme') || 'dark';
            document.documentElement.setAttribute('data-theme', savedTheme);
            document.documentElement.classList.add('no-transitions');
        })();
    </script>
    <meta charset="UTF-8" />
    <meta name="viewport" content="width=device-width, initial-scale=1.0, maximum-scale=1.0, user-scalable=no" />
    <title>BNet Cafe - Portal</title>
    <% Response.Write(BNet.Cafe.Server.Services.FileCssHelper.StyleSheetVersion("Assets/Portal/Style.css")); %>
    <% Response.Write(BNet.Cafe.Server.Services.FileCssHelper.StyleSheetVersion("Assets/Pages/Style.css")); %>
    <% Response.Write(BNet.Cafe.Server.Services.FileCssHelper.StyleSheetVersion("Assets/BNetTable/Style.css")); %>
    <% Response.Write(BNet.Cafe.Server.Services.FileCssHelper.StyleSheetVersion("Assets/fontawesome/font-awesome.min.css")); %>
</head>
<body>
    <form id="form1" runat="server">
        <asp:ScriptManager ID="ScriptManager1" EnableCdn="false" EnablePageMethods="true" EnablePartialRendering="true" AsyncPostBackTimeout="99999999" ScriptMode="Release" ValidateRequestMode="Enabled" EnableScriptLocalization="true" EnableScriptGlobalization="true" LoadScriptsBeforeUI="false" CompositeScript-ScriptMode="Release" CompositeScript-ResourceUICultures="Release" runat="server"></asp:ScriptManager>
        <div class="portal-layout">

            <!-- Navbar Header -->
            <header class="portal-nav">
                <div class="brand-logo">
                    <i class="fa fa-cube"></i>
                    <span>BNet Portal</span>
                </div>

                <div class="nav-actions">
                    <button type="button" class="btn-theme-toggle" id="themeToggle" title="Toggle theme">
                        <i class="fa fa-moon"></i>
                    </button>

                    <asp:Button ID="btnLogout" runat="server" Text="Logout" CssClass="btn-logout-modern" OnClick="btnLogout_Click" />
                </div>
            </header>

            <!-- Main Content Area -->
            <main>
                <!-- User Information Section (Main Body) -->
                <div class="user-hero-card">
                    <div class="hero-avatar">
                        <asp:Literal ID="litAvatar" runat="server">U</asp:Literal>
                    </div>
                    <div class="hero-details">
                        <div class="hero-name">
                            <asp:Literal ID="litUserName" runat="server">User</asp:Literal>
                        </div>
                        <div class="hero-email">
                            <asp:Literal ID="litUserEmail" runat="server"></asp:Literal>
                        </div>
                        <div class="hero-badge">
                            <i class="fa fa-circle"></i>Active Account
                        </div>
                    </div>
                </div>

                <!-- Grid Container for Dual Balance Cards -->
                <div class="balance-cards-grid">
                    <!-- Card 1: Active Session Balance (Currently in Use) -->
                    <div class="balance-card session-card">
                        <div class="card-label">
                            <i class="fa fa-play-circle"></i>Active Session Time
                        </div>
                        <div class="balance-value">
                            <asp:Literal ID="litActiveSessionTime" runat="server">0 mins</asp:Literal>
                        </div>
                        <div class="balance-footer">
                            Deducted for current session: <span>
                                <asp:Literal ID="litActiveSessionMins" runat="server">0</asp:Literal>
                                mins</span>
                        </div>
                    </div>

                    <!-- Card 2: Account / Top-Up Balance (Unused / Mid-game Top-ups) -->
                    <div class="balance-card account-card">
                        <div class="card-label">
                            <i class="fa fa-wallet"></i>Account Balance (Top-Ups)
                        </div>
                        <div class="balance-value">
                            <asp:Literal ID="litAccountBalanceTime" runat="server">0 mins</asp:Literal>
                        </div>
                        <div class="balance-footer">
                            Available to claim: <span>
                                <asp:Literal ID="litAccountBalanceMins" runat="server">0</asp:Literal>
                                mins</span>
                        </div>
                    </div>
                </div>

                <asp:UpdatePanel ID="UpdatePanel1" runat="server">
                    <ContentTemplate>
                        <asp:HiddenField ID="HiddenField_UserId" runat="server" />
                        <!-- Transaction Log Table -->
                        <div class="bnet-table-wrapper">
                            <div class="bnet-table-toolbar">
                                <div style="font-size: 16px; font-weight: 700; color: var(--portal-text-main); display: flex; align-items: center; gap: 8px;">
                                    <i class="fa fa-history"></i>Transaction History
                                </div>
                            </div>
                            <div class="bnet-table-container">
                                <asp:GridView ID="GridViewTable" runat="server" ShowHeaderWhenEmpty="true"
                                    AutoGenerateColumns="False" CssClass="bnet-table" GridLines="None"
                                    AllowPaging="True" OnPageIndexChanging="GridViewTable_PageIndexChanging">
                                    <Columns>
                                        <asp:TemplateField HeaderText="Date">
                                            <ItemTemplate>
                                                <asp:Label ID="Label_DBDateCreated" runat="server" Text='<%# Eval("DBDateCreated") %>'></asp:Label>
                                            </ItemTemplate>
                                        </asp:TemplateField>
                                        <asp:TemplateField HeaderText="Time">
                                            <ItemTemplate>
                                                <asp:Label ID="Label_DBTimeCreated" runat="server" Text='<%# Eval("DBTimeCreated") %>'></asp:Label>
                                            </ItemTemplate>
                                        </asp:TemplateField>
                                        <asp:TemplateField HeaderText="Minutes">
                                            <ItemTemplate>
                                                <asp:Label ID="Label_DBDuration" runat="server" Text='<%# Eval("DBDuration") %>'></asp:Label>
                                            </ItemTemplate>
                                        </asp:TemplateField>
                                        <asp:TemplateField HeaderText="Duration">
                                            <ItemTemplate>
                                                <asp:Label ID="Label_DBFormattedDuration" runat="server" Text='<%# Eval("DBFormattedDuration") %>'></asp:Label>
                                            </ItemTemplate>
                                        </asp:TemplateField>
                                        <asp:TemplateField HeaderText="Amount">
                                            <ItemTemplate>
                                                <asp:Label ID="Label_DBAmount" runat="server" Text='<%# Eval("DBAmount") %>'></asp:Label>
                                            </ItemTemplate>
                                        </asp:TemplateField>
                                        <asp:TemplateField HeaderText="Description">
                                            <ItemTemplate>
                                                <asp:Label ID="Label_DBDescription" runat="server" Text='<%# Eval("DBDescription") %>'></asp:Label>
                                            </ItemTemplate>
                                        </asp:TemplateField>
                                    </Columns>
                                    <EmptyDataTemplate>
                                        <div class="bnet-table-empty-container">
                                            No balance transactions found.
                                        </div>
                                    </EmptyDataTemplate>
                                </asp:GridView>
                                <asp:Panel ID="Panel_Pagination" runat="server" CssClass="bnet-table-pagination-container" />
                            </div>
                        </div>
                    </ContentTemplate>
                </asp:UpdatePanel>
            </main>
        </div>
    </form>
</body>
</html>
