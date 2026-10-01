<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="POS.ascx.cs" Inherits="BNet.Cafe.Server.Forms.POS" %>

<style>
    /* POS Components */
    .pos-container {
        display: flex;
        gap: 16px;
        height: calc(100vh - 110px);
        min-height: 500px;
        width: 100%;
        box-sizing: border-box;
        background-color: var(--bg-light);
        color: var(--text-light);
    }

    .pos-catalog-panel {
        flex: 1.6;
        display: flex;
        flex-direction: column;
        background-color: var(--bg-light-secondary);
        border: 1px solid var(--border-light);
        border-radius: 12px;
        padding: 16px;
        min-width: 0;
        height: 100%;
        overflow: hidden;
    }

    .pos-checkout-panel {
        flex: 1.1;
        display: flex;
        flex-direction: column;
        background-color: var(--bg-light-secondary);
        border: 1px solid var(--border-light);
        border-radius: 12px;
        padding: 16px;
        min-width: 0;
        height: 100%;
    }

    .pos-toolbar {
        display: flex;
        align-items: center;
        justify-content: space-between;
        gap: 12px;
        margin-bottom: 16px;
        flex-shrink: 0;
    }

    .pos-title {
        font-size: 18px;
        font-weight: 700;
        color: var(--text-light);
        display: flex;
        align-items: center;
        gap: 8px;
        white-space: nowrap;
    }

    .pos-search-box {
        position: relative;
        flex: 1;
        max-width: 280px;
    }

    .pos-search-box .search-input {
        width: 100%;
        padding-left: 34px;
        height: 38px;
        border: 1px solid var(--border-light);
        background-color: var(--bg-light-tertiary);
        color: var(--text-light);
        border-radius: 6px;
        padding-right: 10px;
        box-sizing: border-box;
        font-size: 14px;
    }

    .pos-search-box .search-input::placeholder {
        color: var(--text-light-secondary);
    }

    .pos-search-box i {
        position: absolute;
        left: 12px;
        top: 50%;
        transform: translateY(-50%);
        color: var(--text-light-secondary);
    }

    /* Product Cards Grid */
    .product-grid {
        display: grid;
        grid-template-columns: repeat(auto-fill, minmax(140px, 1fr));
        gap: 12px;
        align-items: start;
        align-content: start;
        overflow-y: auto;
        padding-right: 4px;
        flex: 1;
        -webkit-overflow-scrolling: touch;
    }

    .pos-card-link {
        text-decoration: none;
        color: inherit;
        display: block;
    }

    .pos-product-card {
        background-color: var(--bg-light-secondary);
        border: 1px solid var(--border-light);
        border-radius: 10px;
        overflow: hidden;
        transition: transform 0.15s ease, border-color 0.15s ease, box-shadow 0.15s ease;
        display: flex;
        flex-direction: column;
        position: relative;
        height: auto;
        cursor: pointer;
        pointer-events: none; /* Crucial: ensures ASP.NET LinkButton click event fires properly inside UpdatePanel */
    }

    .pos-card-link:hover .pos-product-card {
        border-color: var(--primary);
        transform: translateY(-2px);
        box-shadow: 0 4px 12px rgba(0, 0, 0, 0.15);
    }

    .product-img-wrapper {
        width: 100%;
        height: 95px;
        background-color: var(--bg-light-tertiary);
        position: relative;
        overflow: hidden;
    }

    .product-img-wrapper img {
        width: 100%;
        height: 100%;
        object-fit: cover;
    }

    .stock-badge {
        position: absolute;
        top: 6px;
        right: 6px;
        font-size: 10px;
        font-weight: 700;
        padding: 2px 6px;
        border-radius: 6px;
        color: #ffffff;
        z-index: 2;
    }

    .stock-badge.in-stock { background-color: #22c55e; }
    .stock-badge.out-of-stock { background-color: #ef4444; }

    .product-info {
        padding: 10px;
        display: flex;
        flex-direction: column;
    }

    .product-name {
        font-weight: 600;
        font-size: 13px;
        color: var(--text-light);
        margin-bottom: 2px;
        white-space: nowrap;
        overflow: hidden;
        text-overflow: ellipsis;
    }

    .product-category {
        font-size: 11px;
        color: var(--text-light-secondary);
        margin-bottom: 8px;
    }

    .product-price-row {
        display: flex;
        align-items: center;
        justify-content: space-between;
    }

    .product-price {
        color: var(--primary);
        font-weight: 700;
        font-size: 14px;
    }

    .add-btn-icon {
        background-color: var(--bg-light-tertiary);
        color: var(--primary);
        width: 24px;
        height: 24px;
        border-radius: 50%;
        display: flex;
        align-items: center;
        justify-content: center;
        font-size: 11px;
    }

    .cart-table-wrapper {
        flex: 1;
        overflow-y: auto;
        border: 1px solid var(--border-light);
        border-radius: 8px;
        margin-bottom: 16px;
        -webkit-overflow-scrolling: touch;
        background-color: var(--bg-light-secondary);
    }

    .cart-table {
        width: 100%;
        border-collapse: collapse;
        font-size: 13px;
        color: var(--text-light);
    }

    .cart-table th {
        background-color: var(--bg-light-tertiary);
        color: var(--text-light-secondary);
        padding: 10px;
        text-align: left;
        font-weight: 600;
        font-size: 12px;
        position: sticky;
        top: 0;
        z-index: 1;
    }

    .cart-table td {
        padding: 8px 10px;
        border-bottom: 1px solid var(--border-light);
        vertical-align: middle;
    }

    .qty-controls {
        display: inline-flex;
        align-items: center;
        gap: 4px;
    }

    .qty-btn {
        background: var(--bg-light-tertiary);
        border: 1px solid var(--border-light);
        color: var(--text-light);
        width: 28px;
        height: 28px;
        display: flex;
        align-items: center;
        justify-content: center;
        cursor: pointer;
        border-radius: 4px;
        font-weight: bold;
        text-decoration: none;
    }

    .qty-btn:hover {
        background: var(--border-light);
    }

    .cart-summary {
        border-top: 1px solid var(--border-light);
        padding-top: 12px;
        margin-top: auto;
        flex-shrink: 0;
    }

    .total-row {
        display: flex;
        justify-content: space-between;
        align-items: center;
        margin-bottom: 16px;
    }

    .total-label {
        font-size: 15px;
        font-weight: 600;
        color: var(--text-light);
    }

    .total-amount {
        font-size: 22px;
        font-weight: 700;
        color: #16a34a;
    }

    .cart-actions {
        display: flex;
        gap: 10px;
    }

    /* MOBILE RESPONSIBILITY BREAKPOINTS */
    @media (max-width: 768px) {
        .pos-container {
            flex-direction: column;
            height: auto;
            min-height: auto;
            gap: 12px;
        }

        .pos-catalog-panel, .pos-checkout-panel {
            height: auto;
            padding: 12px;
        }

        .pos-toolbar {
            flex-wrap: wrap;
            gap: 8px;
        }

        .pos-search-box {
            max-width: 100%;
            width: 100%;
        }

        .product-grid {
            grid-template-columns: repeat(auto-fill, minmax(130px, 1fr));
            max-height: 45vh;
            min-height: 220px;
        }

        .cart-table-wrapper {
            max-height: 300px;
            min-height: 150px;
        }

        .cart-actions {
            flex-direction: row;
        }

        .total-amount {
            font-size: 20px;
        }
    }

    @media (max-width: 480px) {
        .product-grid {
            grid-template-columns: repeat(2, 1fr);
            gap: 8px;
        }

        .product-img-wrapper {
            height: 80px;
        }

        .product-name {
            font-size: 12px;
        }

        .product-price {
            font-size: 13px;
        }
    }
</style>

<asp:UpdatePanel ID="UpdatePanel1" runat="server" UpdateMode="Conditional" ChildrenAsTriggers="true">
    <ContentTemplate>
        <div class="pos-container">
            <!-- Left Panel: Catalog -->
            <div class="pos-catalog-panel">
                <div class="pos-toolbar">
                    <div class="pos-title">
                        <i class="fa-solid fa-store"></i> Catalog
                    </div>
                    <div class="pos-search-box">
                        <i class="fa-solid fa-magnifying-glass"></i>
                        <asp:TextBox ID="TextBox_Search" runat="server" CssClass="search-input" 
                            Placeholder="Search items..." AutoPostBack="true" 
                            OnTextChanged="TextBox_Search_TextChanged" 
                            onkeydown="if(event.keyCode===13){this.blur(); return false;}" />
                    </div>
                </div>

                <div class="product-grid">
                    <asp:Repeater ID="Repeater_Products" runat="server" OnItemCommand="Repeater_Products_ItemCommand">
                        <ItemTemplate>
                            <asp:LinkButton ID="btnSelectProduct" runat="server" CommandName="AddToCart" CommandArgument='<%# Eval("DBId") %>' CssClass="pos-card-link">
                                <div class="pos-product-card">
                                    <div class='<%# Convert.ToInt32(Eval("DBQuantityInStock")) <= 0 ? "stock-badge out-of-stock" : "stock-badge in-stock" %>'>
                                        <%# Convert.ToInt32(Eval("DBQuantityInStock")) <= 0 ? "Out of Stock" : Eval("DBQuantityInStock") + " left" %>
                                    </div>
                                    <div class="product-img-wrapper">
                                        <img src='<%# GetProductImage(Eval("DBId")) %>' alt='<%# Eval("DBItemName") %>' onerror="this.src='Uploads/Inventory/default.png';" />
                                    </div>
                                    <div class="product-info">
                                        <div class="product-name" title='<%# Eval("DBItemName") %>'><%# Eval("DBItemName") %></div>
                                        <div class="product-category"><%# Eval("DBCategory") %></div>
                                        <div class="product-price-row">
                                            <span class="product-price">₱<%# Convert.ToDouble(Eval("DBUnitPrice")).ToString("N2") %></span>
                                            <span class="add-btn-icon"><i class="fa-solid fa-plus"></i></span>
                                        </div>
                                    </div>
                                </div>
                            </asp:LinkButton>
                        </ItemTemplate>
                    </asp:Repeater>
                </div>
            </div>

            <!-- Right Panel: Order Cart -->
            <div class="pos-checkout-panel">
                <div class="pos-toolbar">
                    <div class="pos-title">
                        <i class="fa-solid fa-cart-shopping"></i> Current Order
                    </div>
                </div>

                <div class="cart-table-wrapper">
                    <asp:GridView ID="GridView_Cart" runat="server" AutoGenerateColumns="False" 
                        CssClass="cart-table" OnRowCommand="GridView_Cart_RowCommand" GridLines="None">
                        <Columns>
                            <asp:BoundField DataField="ItemName" HeaderText="Item" />
                            <asp:TemplateField HeaderText="Qty">
                                <ItemTemplate>
                                    <div class="qty-controls">
                                        <asp:LinkButton ID="btnDecrease" runat="server" CommandName="DecreaseQty" CommandArgument='<%# Eval("ItemId") %>' CssClass="qty-btn">-</asp:LinkButton>
                                        <span style="font-weight: 600; min-width: 18px; text-align: center;"><%# Eval("Quantity") %></span>
                                        <asp:LinkButton ID="btnIncrease" runat="server" CommandName="IncreaseQty" CommandArgument='<%# Eval("ItemId") %>' CssClass="qty-btn">+</asp:LinkButton>
                                    </div>
                                </ItemTemplate>
                            </asp:TemplateField>
                            <asp:TemplateField HeaderText="Subtotal">
                                <ItemTemplate>
                                    ₱<%# Convert.ToDouble(Eval("Subtotal")).ToString("N2") %>
                                </ItemTemplate>
                            </asp:TemplateField>
                            <asp:TemplateField HeaderText="">
                                <ItemTemplate>
                                    <asp:LinkButton ID="btnRemove" runat="server" CommandName="RemoveItem" CommandArgument='<%# Eval("ItemId") %>' Style="color: #ef4444; padding: 6px;">
                                        <i class="fa-solid fa-trash"></i>
                                    </asp:LinkButton>
                                </ItemTemplate>
                            </asp:TemplateField>
                        </Columns>
                        <EmptyDataTemplate>
                            <div style="text-align: center; color: var(--text-light-secondary); padding: 30px 10px;">
                                <i class="fa-solid fa-basket-shopping" style="font-size: 32px; margin-bottom: 8px; color: var(--border-light);"></i>
                                <div>Cart is empty. Select products to add.</div>
                            </div>
                        </EmptyDataTemplate>
                    </asp:GridView>
                </div>

                <div class="cart-summary">
                    <div class="total-row">
                        <span class="total-label">Total Payable</span>
                        <span class="total-amount">₱<asp:Label ID="Label_Total" runat="server" Text="0.00"></asp:Label></span>
                    </div>

                    <div class="cart-actions">
                        <asp:LinkButton ID="LinkButton_Clear" OnClick="LinkButton_Clear_Click" CssClass="btn btn-secondary" runat="server" style="padding: 10px 16px;">
                            Clear
                        </asp:LinkButton>
                        <asp:LinkButton ID="LinkButton_Submit" OnClick="LinkButton_Submit_Click" CssClass="btn btn-primary" style="flex: 1; justify-content: center;" OnClientClick="return ValidatePOSCart();" runat="server">
                            <i class="fa-solid fa-cart-shopping"></i> Complete Sale
                        </asp:LinkButton>
                    </div>
                </div>
            </div>
        </div>
    </ContentTemplate>
</asp:UpdatePanel>