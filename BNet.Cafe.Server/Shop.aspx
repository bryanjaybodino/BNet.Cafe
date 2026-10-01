<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="Shop.aspx.cs" Inherits="BNet.Cafe.Server.Shop" %>

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
    <title>Little Store - Browse Products</title>
    <% Response.Write(BNet.Cafe.Server.Services.FileCssHelper.StyleSheetVersion("Assets/Shop/Style.css")); %>
    <% Response.Write(BNet.Cafe.Server.Services.FileCssHelper.StyleSheetVersion("Assets/Pages/Style.css")); %>
    <% Response.Write(BNet.Cafe.Server.Services.FileCssHelper.StyleSheetVersion("Assets/fontawesome/font-awesome.min.css")); %>
</head>
<body>
    <form id="form1" runat="server">
        <asp:ScriptManager ID="ScriptManager1" EnableCdn="false" EnablePageMethods="true" EnablePartialRendering="true" AsyncPostBackTimeout="99999999" ScriptMode="Release" ValidateRequestMode="Enabled" EnableScriptLocalization="true" EnableScriptGlobalization="true" LoadScriptsBeforeUI="false" CompositeScript-ScriptMode="Release" CompositeScript-ResourceUICultures="Release" runat="server"></asp:ScriptManager>
        
        <div class="ecom-store-container">
            <!-- Header Bar with Store Branding & Theme Switcher -->
            <header class="ecom-header">
                <div class="brand-logo">
                    <i class="fa fa-shopping-bag brand-icon"></i>
                    <h2>Little Store</h2>
                </div>
                <button type="button" id="themeToggle" class="theme-toggle-btn" title="Toggle Light/Dark Theme">
                    <i class="fa fa-sun"></i>
                </button>
            </header>

            <!-- Filter & Search Controls -->
            <asp:UpdatePanel ID="UpdatePanel1" runat="server">
                <ContentTemplate>
                    <div class="ecom-toolbar">
                        <div class="ecom-search-box">
                            <i class="fa fa-search"></i>
                            <asp:TextBox ID="TextBox_Search" runat="server" CssClass="ecom-input"
                                Placeholder="Search by product name..." AutoPostBack="true"
                                OnTextChanged="Filter_Changed" />
                        </div>

                        <div class="ecom-filter-group">
                            <div class="ecom-select-box">
                                <i class="fa fa-filter select-icon"></i>
                                <asp:DropDownList ID="DropDownList_Category" runat="server" CssClass="ecom-select"
                                    AutoPostBack="true" OnSelectedIndexChanged="Filter_Changed">
                                </asp:DropDownList>
                            </div>
                        </div>
                    </div>

                    <!-- E-Commerce Showcase Grid -->
                    <div class="ecom-product-grid">
                        <asp:Repeater ID="Repeater_Products" runat="server">
                            <ItemTemplate>
                                <div class="ecom-product-card">
                                    <div class="product-media">
                                        <img src='<%# GetProductImage(Eval("DBId")) %>' alt='<%# Eval("DBItemName") %>' onerror="this.src='Uploads/Inventory/default.png';" />
                                        <span class="category-badge"><%# Eval("DBCategory") %></span>
                                    </div>

                                    <div class="product-details">
                                        <div class="product-rating">
                                            <i class="fa fa-star active"></i>
                                            <i class="fa fa-star active"></i>
                                            <i class="fa fa-star active"></i>
                                            <i class="fa fa-star active"></i>
                                            <i class="fa fa-star-half-o active"></i>
                                            <span class="rating-count">(4.8)</span>
                                        </div>

                                        <h3 class="product-title" title='<%# Eval("DBItemName") %>'><%# Eval("DBItemName") %></h3>

                                        <div class="product-bottom-row">
                                            <div class="price-container">
                                                <span class="currency">₱</span>
                                                <span class="amount"><%# Convert.ToDouble(Eval("DBUnitPrice")).ToString("N2") %></span>
                                            </div>

                                            <div class='<%# Convert.ToInt32(Eval("DBQuantityInStock")) <= 0 ? "stock-status out-of-stock" : "stock-status in-stock" %>'>
                                                <span class="dot"></span>
                                                <%# Convert.ToInt32(Eval("DBQuantityInStock")) <= 0 ? "Out of Stock" : Eval("DBQuantityInStock") + " left" %>
                                            </div>
                                        </div>
                                    </div>
                                </div>
                            </ItemTemplate>
                        </asp:Repeater>

                        <asp:Panel ID="Panel_NoResults" runat="server" Visible="false" CssClass="ecom-empty">
                            <div class="empty-icon-circle">
                                <i class="fa fa-box-open"></i>
                            </div>
                            <h3>No Products Found</h3>
                            <p>We couldn't find anything matching your filters. Try clearing your search keyword or selecting a different category.</p>
                        </asp:Panel>
                    </div>
                </ContentTemplate>
            </asp:UpdatePanel>
        </div>
    </form>
</body>
</html>