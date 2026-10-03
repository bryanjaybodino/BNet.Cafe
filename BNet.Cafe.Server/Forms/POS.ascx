<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="POS.ascx.cs" Inherits="BNet.Cafe.Server.Forms.POS" %>
<%@ Register Src="~/Forms/Modals/POSReceipt.ascx" TagPrefix="uc1" TagName="POSReceipt" %>

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
                                        <img src='<%# GetProductImage(Eval("DBId")) %>' alt='<%# Eval("DBItemName") %>' />
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
        <uc1:POSReceipt runat="server" id="POSReceipt" />
    </ContentTemplate>
</asp:UpdatePanel>