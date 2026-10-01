<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="POS.ascx.cs" Inherits="BNet.Cafe.Server.Forms.POS" %>

<asp:UpdatePanel ID="UpdatePanel1" runat="server" UpdateMode="Conditional" ChildrenAsTriggers="true">
    <ContentTemplate>
        <div class="pos-wrapper" style="display: flex; gap: 20px; padding: 15px;">
            <!-- Left: Catalog with Modern Image Cards -->
            <div class="pos-catalog" style="flex: 1.8;">
                <div class="grid-card">
                    <div class="grid-header" style="margin-bottom: 15px;">
                        <h2><i class="fa-solid fa-store"></i> Items Catalog</h2>
                    </div>
                    
                    <!-- Product Cards Grid -->
                    <div class="product-grid" style="display: grid; grid-template-columns: repeat(auto-fill, minmax(160px, 1fr)); gap: 16px; max-height: 680px; overflow-y: auto; padding: 5px;">
                        <asp:Repeater ID="Repeater_Products" runat="server" OnItemCommand="Repeater_Products_ItemCommand">
                            <ItemTemplate>
                                <asp:LinkButton ID="btnSelectProduct" runat="server" CommandName="AddToCart" CommandArgument='<%# Eval("DBId") %>' 
                                    CssClass="pos-card-link" style="text-decoration: none; color: inherit; display: block;">
                                    <div class="pos-product-card" style="position: relative; background: #ffffff; border: 1px solid #e2e8f0; border-radius: 12px; overflow: hidden; box-shadow: 0 2px 4px rgba(0,0,0,0.04); transition: all 0.2s ease-in-out; cursor: pointer; pointer-events: none;">
                                        
                                        <!-- Stock Badge -->
                                        <div style='<%# Convert.ToInt32(Eval("DBQuantityInStock")) <= 0 ? "background: #ef4444;" : "background: #22c55e;" %>' 
                                             style="position: absolute; top: 8px; right: 8px; color: #fff; font-size: 11px; font-weight: 600; padding: 2px 8px; border-radius: 12px; z-index: 2;">
                                            <%# Convert.ToInt32(Eval("DBQuantityInStock")) <= 0 ? "Out of Stock" : Eval("DBQuantityInStock") + " in stock" %>
                                        </div>

                                        <!-- Product Image Container -->
                                        <div style="width: 100%; height: 120px; background: #f8fafc; overflow: hidden; display: flex; align-items: center; justify-content: center;">
                                            <img src='<%# GetProductImage(Eval("DBId")) %>' 
                                                 alt='<%# Eval("DBItemName") %>' 
                                                 style="width: 100%; height: 100%; object-fit: cover;" 
                                                 onerror="this.src='Uploads/Inventory/default.png';" />
                                        </div>

                                        <!-- Card Details -->
                                        <div style="padding: 12px; text-align: left;">
                                            <div style="font-weight: 600; font-size: 14px; color: #1e293b; margin-bottom: 4px; display: -webkit-box; -webkit-line-clamp: 1; -webkit-box-orient: vertical; overflow: hidden; text-overflow: ellipsis;">
                                                <%# Eval("DBItemName") %>
                                            </div>
                                            <div style="font-size: 11px; color: #64748b; margin-bottom: 8px;">
                                                <%# Eval("DBCategory") %>
                                            </div>
                                            <div style="display: flex; justify-content: space-between; align-items: center; margin-top: 6px;">
                                                <span style="color: #2563eb; font-weight: 700; font-size: 16px;">
                                                    ₱<%# Convert.ToDouble(Eval("DBUnitPrice")).ToString("N2") %>
                                                </span>
                                                <span style="background: #eff6ff; color: #2563eb; width: 28px; height: 28px; border-radius: 50%; display: flex; align-items: center; justify-content: center; font-size: 12px;">
                                                    <i class="fa-solid fa-plus"></i>
                                                </span>
                                            </div>
                                        </div>

                                    </div>
                                </asp:LinkButton>
                            </ItemTemplate>
                        </asp:Repeater>
                    </div>
                </div>
            </div>

            <!-- Right: Dynamic Multi-Item Cart/Checkout -->
            <div class="pos-checkout" style="flex: 1.2;">
                <div class="form-card">
                    <h2 class="form-section-title"><i class="fa-solid fa-cart-shopping"></i> Current Order</h2>

                    <div class="cart-table-wrapper" style="max-height: 400px; overflow-y: auto; margin-bottom: 15px; border: 1px solid #e2e8f0; border-radius: 8px;">
                        <asp:GridView ID="GridView_Cart" runat="server" AutoGenerateColumns="False" 
                            CssClass="bnet-table" OnRowCommand="GridView_Cart_RowCommand" GridLines="None">
                            <Columns>
                                <asp:BoundField DataField="ItemName" HeaderText="Item" />
                                <asp:TemplateField HeaderText="Qty">
                                    <ItemTemplate>
                                        <div style="display: flex; align-items: center; gap: 6px;">
                                            <asp:LinkButton ID="btnDecrease" runat="server" CommandName="DecreaseQty" CommandArgument='<%# Eval("ItemId") %>' CssClass="btn btn-sm btn-light" style="padding: 2px 8px; border: 1px solid #cbd5e1;">-</asp:LinkButton>
                                            <span style="font-weight: 600; min-width: 18px; text-align: center;"><%# Eval("Quantity") %></span>
                                            <asp:LinkButton ID="btnIncrease" runat="server" CommandName="IncreaseQty" CommandArgument='<%# Eval("ItemId") %>' CssClass="btn btn-sm btn-light" style="padding: 2px 8px; border: 1px solid #cbd5e1;">+</asp:LinkButton>
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
                                        <asp:LinkButton ID="btnRemove" runat="server" CommandName="RemoveItem" CommandArgument='<%# Eval("ItemId") %>' Style="color: #ef4444; padding: 4px;">
                                            <i class="fa fa-trash"></i>
                                        </asp:LinkButton>
                                    </ItemTemplate>
                                </asp:TemplateField>
                            </Columns>
                            <EmptyDataTemplate>
                                <div style="text-align: center; color: #94a3b8; padding: 30px 10px;">
                                    <i class="fa-solid fa-basket-shopping" style="font-size: 32px; margin-bottom: 8px; color: #cbd5e1;"></i>
                                    <div>Cart is empty. Select products to add.</div>
                                </div>
                            </EmptyDataTemplate>
                        </asp:GridView>
                    </div>

                    <div style="border-top: 2px dashed #e2e8f0; padding-top: 15px; font-size: 18px; font-weight: bold; display: flex; justify-content: space-between; margin-bottom: 20px;">
                        <span>Total Payable:</span>
                        <span style="color: #16a34a; font-size: 22px;">₱<asp:Label ID="Label_Total" runat="server" Text="0.00"></asp:Label></span>
                    </div>

                    <div class="form-actions" style="display: flex; gap: 10px;">
                        <asp:LinkButton ID="LinkButton_Clear" OnClick="LinkButton_Clear_Click" CssClass="btn btn-secondary" runat="server" style="padding: 10px 16px;">
                            Clear
                        </asp:LinkButton>
                        <asp:LinkButton ID="LinkButton_Submit" OnClick="LinkButton_Submit_Click" CssClass="btn btn-primary btn-lg" style="width: 100%; justify-content: center;" OnClientClick="return ValidatePOSCart();" runat="server">
                            <i class="fa fa-shopping-cart"></i> Complete Sale
                        </asp:LinkButton>
                    </div>
                </div>
            </div>
        </div>
    </ContentTemplate>
</asp:UpdatePanel>

<style>
    .pos-card-link:hover .pos-product-card {
        border-color: #2563eb !important;
        transform: translateY(-2px);
        box-shadow: 0 10px 15px -3px rgba(37, 99, 235, 0.1) !important;
    }
</style>