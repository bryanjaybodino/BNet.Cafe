<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="POS.ascx.cs" Inherits="BNet.Cafe.Server.Forms.POS" %>

<asp:UpdatePanel ID="UpdatePanel1" runat="server" UpdateMode="Conditional">
    <ContentTemplate>
        <div class="pricing-settings-wrapper">
            <div class="pricing-container">
                <!-- Left: Catalog -->
                <div class="pricing-grid-panel" style="flex: 1.5;">
                    <div class="grid-card">
                        <div class="grid-header">
                            <h2 class="grid-title"><i class="fa-solid fa-store"></i> Items Catalog</h2>
                        </div>
                        <div class="bnet-table-container pricing-grid-container">
                            <asp:GridView ID="GridView_POSItems" runat="server" AutoGenerateColumns="False" 
                                GridLines="None" CssClass="bnet-table pricing-table" OnRowCommand="GridView_POSItems_RowCommand">
                                <Columns>
                                    <asp:BoundField DataField="DBItemName" HeaderText="Item Name" />
                                    <asp:BoundField DataField="DBCategory" HeaderText="Category" />
                                    <asp:TemplateField HeaderText="Price">
                                        <ItemTemplate>₱ <%# Convert.ToDouble(Eval("DBUnitPrice")).ToString("F2") %></ItemTemplate>
                                    </asp:TemplateField>
                                    <asp:BoundField DataField="DBQuantityInStock" HeaderText="Stock" />
                                    <asp:TemplateField HeaderText="Action">
                                        <ItemTemplate>
                                            <asp:LinkButton ID="btnAddToCart" runat="server" CommandName="AddToCart" 
                                                CommandArgument='<%# Eval("DBId") %>' CssClass="btn-action btn-edit">
                                                <i class="fa fa-cart-plus"></i> Add
                                            </asp:LinkButton>
                                        </ItemTemplate>
                                    </asp:TemplateField>
                                </Columns>
                            </asp:GridView>
                        </div>
                    </div>
                </div>

                <!-- Right: Checkout -->
                <div class="pricing-form-panel">
                    <div class="form-card">
                        <h2 class="form-section-title"><i class="fa-solid fa-receipt"></i> Checkout</h2>

                        <div class="form-group">
                            <label>Selected Item:</label>
                            <asp:HiddenField ID="HiddenField_SelectedItemId" runat="server" />
                            <asp:TextBox ID="TextBox_SelectedItemName" runat="server" CssClass="form-control" ReadOnly="true"></asp:TextBox>
                        </div>

                        <div class="form-group">
                            <label>Unit Price (₱):</label>
                            <asp:TextBox ID="TextBox_UnitPrice" runat="server" CssClass="form-control" ReadOnly="true">0.00</asp:TextBox>
                        </div>

                        <div class="form-group">
                            <label>Quantity to Sell:</label>
                            <asp:TextBox ID="TextBox_Quantity" runat="server" TextMode="Number" 
                                CssClass="form-control pricing-input" min="1" Text="1" OnTextChanged="CalculateTotal" AutoPostBack="true"></asp:TextBox>
                        </div>

                        <div class="helper-text" style="margin: 15px 0;">
                            Total Payable: <span id="POSTotalDisplay" class="value-highlight">₱ <asp:Label ID="Label_Total" runat="server" Text="0.00"></asp:Label></span>
                        </div>

                        <div class="form-actions">
                            <asp:LinkButton ID="LinkButton_Submit" OnClick="LinkButton_Submit_Click"
                                CssClass="btn btn-primary btn-lg" OnClientClick="return ValidatePOS();" runat="server">
                                <i class="fa fa-shopping-cart"></i> Complete Sale
                            </asp:LinkButton>
                        </div>
                    </div>
                </div>
            </div>
        </div>
    </ContentTemplate>
</asp:UpdatePanel>