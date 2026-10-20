<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="POS.ascx.cs" Inherits="BNet.Cafe.Server.Forms.POS" %>
<%@ Register Src="~/Forms/Modals/POSReceipt.ascx" TagPrefix="uc1" TagName="POSReceipt" %>

<asp:UpdatePanel ID="UpdatePanel1" runat="server" UpdateMode="Conditional" ChildrenAsTriggers="true">
    <ContentTemplate>
        <div class="pos-container">
            <!-- Left Panel: Catalog -->
            <div class="pos-catalog-panel">
                <div class="pos-toolbar">
                    <div class="pos-filter-group">
                        <div class="pos-search-box">
                            <i class="fa-solid fa-magnifying-glass"></i>
                            <asp:TextBox ID="TextBox_Search" runat="server" CssClass="search-input"
                                Placeholder="Search items..." AutoPostBack="true"
                                OnTextChanged="Filter_Changed"
                                onkeydown="if(event.keyCode===13){this.blur(); return false;}" />
                        </div>

                        <div class="pos-select-box">
                            <i class="fa-solid fa-filter select-icon"></i>
                            <asp:DropDownList ID="DropDownList_Category" runat="server" CssClass="category-select"
                                AutoPostBack="true" OnSelectedIndexChanged="Filter_Changed">
                            </asp:DropDownList>
                        </div>
                    </div>
                </div>
                <div class="form-actions"></div>
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
                    <asp:Panel ID="Panel_NoResults" runat="server" Visible="false" Style="grid-column: 1 / -1; text-align: center; padding: 40px 10px; color: var(--text-light-secondary);">
                        <i class="fa-solid fa-box-open" style="font-size: 32px; margin-bottom: 8px; opacity: 0.5;"></i>
                        <div style="font-weight: 600;">No items found</div>
                        <div style="font-size: 12px; margin-top: 4px;">Try clearing your search or changing the category filter.</div>
                    </asp:Panel>
                </div>
            </div>

            <!-- Right Panel: Order Cart (Maximum space for item list) -->
            <div class="pos-checkout-panel" style="display: flex; flex-direction: column; height: 100%; overflow: hidden;">
<%--                <div class="pos-toolbar" style="flex-shrink: 0; padding: 12px 16px;">
                    <div class="pos-title" style="font-size: 15px;">
                        <i class="fa-solid fa-cart-shopping"></i>Current Order
                    </div>
                </div>--%>
                <!-- Flexible Scrollable Cart Table Container (Gets maximum height) -->
                <div class="cart-table-wrapper" style="flex: 1; overflow-y: auto; max-height: none;">
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
                            <div style="text-align: center; color: var(--text-light-secondary); padding: 30px 10px; font-size: 13px;">
                                <i class="fa-solid fa-basket-shopping" style="font-size: 28px; margin-bottom: 6px; color: var(--border-light);"></i>
                                <div>Cart is empty. Select products.</div>
                            </div>
                        </EmptyDataTemplate>
                    </asp:GridView>
                </div>

                <!-- Footer Summary & Customer/PC Selection Placed at the Bottom -->
                <div class="cart-summary" style="flex-shrink: 0; padding: 12px 16px; border-top: 1px solid var(--border-light); background: rgba(0,0,0,0.01);">

                    <!-- PC Terminal & Customer Selection Moved Below -->
                    <div style="display: grid; grid-template-columns: 1fr 1fr; gap: 8px; margin-bottom: 8px; padding-bottom: 8px; border-bottom: 1px dashed var(--border-light);">
                        <div>
                            <label style="font-size: 10px; font-weight: 600; display: block; margin-bottom: 2px;">PC Terminal</label>
                            <asp:DropDownList ID="DropDownList_POSComputer" runat="server" CssClass="form-control bnet-select" Style="font-size: 11px; padding: 2px 6px; height: 28px;" AutoPostBack="true" OnSelectedIndexChanged="DropDownList_POSComputer_SelectedIndexChanged">
                            </asp:DropDownList>
                        </div>
                        <div>
                            <label style="font-size: 10px; font-weight: 600; display: block; margin-bottom: 2px;">Customer</label>
                            <asp:DropDownList ID="DropDownList_POSCustomer" runat="server" CssClass="form-control bnet-select" Style="font-size: 11px; padding: 2px 6px; height: 28px;" AutoPostBack="true" OnSelectedIndexChanged="DropDownList_POSCustomer_SelectedIndexChanged">
                            </asp:DropDownList>
                        </div>
                    </div>

                    <!-- Modern BNet Toggle Switch for Points -->
                    <asp:Panel ID="Panel_Points" runat="server" Visible="false" Style="display: flex; align-items: center; justify-content: space-between; font-size: 12px; margin-bottom: 8px; padding: 4px 6px; background: rgba(99, 102, 241, 0.05); border-radius: 6px;">
                        <div>
                            <span>Points: </span>
                            <strong style="color: var(--primary);">
                                <asp:Literal ID="litAvailablePoints" runat="server">0</asp:Literal></strong>
                        </div>
                        <div>
                            <label class="bnet-toggle-wrapper">
                                <asp:CheckBox ID="CheckBox_UsePoints" runat="server" AutoPostBack="true" OnCheckedChanged="CheckBox_UsePoints_CheckedChanged" />
                                <span class="bnet-toggle-slider"></span>
                                <span>Use Points</span>
                            </label>
                        </div>
                    </asp:Panel>

                    <div class="total-row" style="margin-bottom: 8px;">
                        <span class="total-label" style="font-size: 13px;">Total Payable</span>
                        <span class="total-amount" style="font-size: 18px;">₱<asp:Label ID="Label_Total" runat="server" Text="0.00"></asp:Label></span>
                    </div>

                    <div class="cart-actions" style="display: flex; gap: 8px;">
                        <asp:LinkButton ID="LinkButton_Clear" OnClick="LinkButton_Clear_Click" CssClass="btn btn-secondary" runat="server" Style="padding: 8px 12px; font-size: 12px;">
                            Clear
                        </asp:LinkButton>
                        <asp:LinkButton ID="LinkButton_Submit" OnClick="LinkButton_Submit_Click" CssClass="btn btn-primary" Style="flex: 1; justify-content: center; padding: 8px 12px; font-size: 12px;" OnClientClick="return ValidatePOSCart();" runat="server">
                            <i class="fa-solid fa-cart-shopping"></i> Complete Sale
                        </asp:LinkButton>
                    </div>
                </div>
            </div>
        </div>
        <uc1:POSReceipt runat="server" ID="POSReceipt" />
    </ContentTemplate>
</asp:UpdatePanel>
