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
    <title>Kiosk Store - Browse & Order</title>
    <% Response.Write(BNet.Cafe.Server.Services.FileCssHelper.StyleSheetVersion("Assets/Shop/Style.css")); %>
    <% Response.Write(BNet.Cafe.Server.Services.FileCssHelper.StyleSheetVersion("Assets/Pages/Style.css")); %>
    <% Response.Write(BNet.Cafe.Server.Services.FileCssHelper.StyleSheetVersion("Assets/fontawesome/font-awesome.min.css")); %>
    <script src="Assets/Shop/Script.js" defer></script>
</head>
<body>
    <form id="form1" runat="server">
        <asp:ScriptManager ID="ScriptManager1" EnableCdn="false" EnablePageMethods="true" EnablePartialRendering="true" AsyncPostBackTimeout="99999999" ScriptMode="Release" ValidateRequestMode="Enabled" EnableScriptLocalization="true" EnableScriptGlobalization="true" LoadScriptsBeforeUI="false" CompositeScript-ScriptMode="Release" CompositeScript-ResourceUICultures="Release" runat="server"></asp:ScriptManager>
        
        <!-- Hidden ASP.NET inputs & trigger for WebSockets / Server Sync -->
        <asp:TextBox ID="TextBox_ChatMessage" runat="server" Style="display: none;"></asp:TextBox>
        <asp:TextBox ID="TextBox_ComputerName" runat="server" Style="display: none;"></asp:TextBox>
        <asp:TextBox ID="TextBox_UserId" runat="server" Style="display: none;"></asp:TextBox>
        <asp:LinkButton ID="LinkButton_SaveMessage" runat="server" OnClick="LinkButton_SaveMessage_Click" Style="display: none;"></asp:LinkButton>

        <div class="ecom-store-container">
            <!-- Header Bar -->
            <header class="ecom-header">
                <div class="brand-logo">
                    <i class="fa fa-shopping-bag brand-icon"></i>
                    <h2>Kiosk Store</h2>
                </div>
                <div class="header-actions">
                    <button type="button" id="cartToggle" class="cart-toggle-btn" title="View Cart">
                        <i class="fa fa-shopping-cart"></i>
                        <span id="cartBadgeCount" class="cart-badge">0</span>
                    </button>
                    <button type="button" id="themeToggle" class="theme-toggle-btn" title="Toggle Light/Dark Theme">
                        <i class="fa fa-sun"></i>
                    </button>
                </div>
            </header>

            <!-- Filter & Search Controls -->
            <asp:UpdatePanel ID="UpdatePanel1" runat="server">
                <ContentTemplate>
                    <div class="ecom-toolbar">
                        <div class="ecom-search-box">
                            <i class="fa fa-search"></i>
                            <asp:TextBox ID="TextBox_Search" runat="server" CssClass="ecom-input"
                                Placeholder="Search products..." AutoPostBack="true"
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

                    <!-- E-Commerce Grid -->
                    <div class="ecom-product-grid">
                        <asp:Repeater ID="Repeater_Products" runat="server">
                            <ItemTemplate>
                                <div class="ecom-product-card" 
                                     data-id='<%# Eval("DBId") %>' 
                                     data-name='<%# Eval("DBItemName") %>' 
                                     data-price='<%# Eval("DBUnitPrice") %>'
                                     data-stock='<%# Eval("DBQuantityInStock") %>'>
                                    <div class="product-media">
                                        <img src='<%# GetProductImage(Eval("DBId")) %>' alt='<%# Eval("DBItemName") %>' />
                                        <span class="category-badge"><%# Eval("DBCategory") %></span>
                                    </div>

                                    <div class="product-details">
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

                                        <button type="button" class="btn-add-cart" 
                                            <%# Convert.ToInt32(Eval("DBQuantityInStock")) <= 0 ? "disabled" : "" %>>
                                            <i class="fa fa-cart-plus"></i> Add to Cart
                                        </button>
                                    </div>
                                </div>
                            </ItemTemplate>
                        </asp:Repeater>

                        <asp:Panel ID="Panel_NoResults" runat="server" Visible="false" CssClass="ecom-empty">
                            <div class="empty-icon-circle">
                                <i class="fa fa-box-open"></i>
                            </div>
                            <h3>No Products Found</h3>
                            <p>We couldn't find anything matching your filters.</p>
                        </asp:Panel>
                    </div>
                </ContentTemplate>
            </asp:UpdatePanel>
        </div>

        <!-- Sliding Kiosk Cart Drawer -->
        <div id="cartOverlay" class="cart-overlay"></div>
        <aside id="cartDrawer" class="cart-drawer">
            <div class="cart-header">
                <h3><i class="fa fa-shopping-cart"></i> Your Kiosk Cart</h3>
                <button type="button" id="closeCartBtn" class="close-cart-btn">&times;</button>
            </div>
            
            <div id="cartItemsContainer" class="cart-items-body">
                <!-- Cart items dynamically rendered via Script.js -->
            </div>

            <div class="cart-footer">
                <div class="cart-total-row">
                    <span>Total Amount:</span>
                    <span id="cartGrandTotal">₱0.00</span>
                </div>
                <button type="button" id="checkoutBtn" class="btn-checkout">
                    <span>Send Order to Client Chat</span>
                    <i class="fa fa-paper-plane"></i>
                </button>
            </div>
        </aside>
    </form>
</body>
</html>