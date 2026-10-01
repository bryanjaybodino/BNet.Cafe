<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="Shop.aspx.cs" Inherits="BNet.Cafe.Server.Shop" %>

<!DOCTYPE html>
<html lang="en" xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <script>
        // Apply saved theme immediately before render to prevent flickering
        (function () {
            var savedTheme = localStorage.getItem('theme') || 'dark';
            document.documentElement.setAttribute('data-theme', savedTheme);
            document.documentElement.classList.add('no-transitions');
        })();
    </script>
    <meta charset="UTF-8" />
    <meta name="viewport" content="width=device-width, initial-scale=1.0, maximum-scale=1.0, user-scalable=no" />
    <title>BNet Cafe - Shop</title>
    <% Response.Write(BNet.Cafe.Server.Services.FileCssHelper.StyleSheetVersion("Assets/Shop/Style.css")); %>
    <% Response.Write(BNet.Cafe.Server.Services.FileCssHelper.StyleSheetVersion("Assets/Pages/Style.css")); %>
    <% Response.Write(BNet.Cafe.Server.Services.FileCssHelper.StyleSheetVersion("Assets/fontawesome/font-awesome.min.css")); %>
</head>
<body>
    <form id="form1" runat="server">
        <asp:ScriptManager ID="ScriptManager1" EnableCdn="false" EnablePageMethods="true" EnablePartialRendering="true" AsyncPostBackTimeout="99999999" ScriptMode="Release" ValidateRequestMode="Enabled" EnableScriptLocalization="true" EnableScriptGlobalization="true" LoadScriptsBeforeUI="false" CompositeScript-ScriptMode="Release" CompositeScript-ResourceUICultures="Release" runat="server"></asp:ScriptManager>
        <div class="portal-layout">

            <!-- Navbar Header -->
            <header class="portal-nav">
                <div class="brand-logo">
                    <i class="fa fa-shopping-bag"></i>
                    <span>BNet Shop</span>
                </div>

                <div class="nav-actions">
                    <button type="button" class="btn-theme-toggle" id="themeToggle" title="Toggle theme">
                        <i class="fa fa-moon" id="themeIcon"></i>
                    </button>
                    <asp:Button ID="btnLogout" runat="server" Text="Logout" CssClass="btn-logout-modern" OnClick="btnLogout_Click" />
                </div>
            </header>

            <!-- Main Content Area -->
            <main>
                <asp:UpdatePanel ID="UpdatePanel1" runat="server">
                    <ContentTemplate>
                        <!-- Filter & Search Controls (2 Columns) -->
                        <div class="shop-toolbar-grid">
                            <div class="shop-search-box">
                                <i class="fa fa-search"></i>
                                <asp:TextBox ID="TextBox_Search" runat="server" CssClass="shop-input"
                                    Placeholder="Search products..." AutoPostBack="true"
                                    OnTextChanged="Filter_Changed" />
                            </div>

                            <div>
                                <asp:DropDownList ID="DropDownList_Category" runat="server" CssClass="shop-select"
                                    AutoPostBack="true" OnSelectedIndexChanged="Filter_Changed">
                                </asp:DropDownList>
                            </div>
                        </div>

                        <!-- Product Display Grid (View Only) -->
                        <div class="shop-product-grid">
                            <asp:Repeater ID="Repeater_Products" runat="server">
                                <ItemTemplate>
                                    <div class="shop-product-card">
                                        <!-- Full-bleed top image -->
                                        <div class="product-img-wrapper">
                                            <img src='<%# GetProductImage(Eval("DBId")) %>' alt='<%# Eval("DBItemName") %>' onerror="this.src='Uploads/Inventory/default.png';" />
                                        </div>

                                        <!-- Card details with padding -->
                                        <div class="product-card-body">
                                            <div class="product-info">
                                                <div class="product-name" title='<%# Eval("DBItemName") %>'><%# Eval("DBItemName") %></div>
                                                <div class="product-category"><%# Eval("DBCategory") %></div>
                                                <div class="product-price">₱<%# Convert.ToDouble(Eval("DBUnitPrice")).ToString("N2") %></div>
                                            </div>

                                            <div class='<%# Convert.ToInt32(Eval("DBQuantityInStock")) <= 0 ? "stock-pill out-of-stock" : "stock-pill in-stock" %>'>
                                                <i class='<%# Convert.ToInt32(Eval("DBQuantityInStock")) <= 0 ? "fa fa-times-circle" : "fa fa-check-circle" %>'></i>
                                                <span><%# Convert.ToInt32(Eval("DBQuantityInStock")) <= 0 ? "Out of Stock" : Eval("DBQuantityInStock") + " in stock" %></span>
                                            </div>
                                        </div>
                                    </div>
                                </ItemTemplate>
                            </asp:Repeater>

                            <asp:Panel ID="Panel_NoResults" runat="server" Visible="false" CssClass="shop-empty">
                                <i class="fa fa-box-open" style="font-size: 36px; margin-bottom: 10px; opacity: 0.5;"></i>
                                <div>No products found matching your filters.</div>
                            </asp:Panel>
                        </div>
                    </ContentTemplate>
                </asp:UpdatePanel>
            </main>
        </div>
    </form>
</body>
</html>
