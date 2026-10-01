<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="InventoryCreate.ascx.cs" Inherits="BNet.Cafe.Server.Forms.InventoryCreate" %>
<asp:UpdatePanel ID="UpdatePanel1" runat="server">
    <ContentTemplate>
        <div class="bnet-table-wrapper">
            <div class="content-header" style="margin-bottom: 20px;">
                <h1 style="font-size: 20px; display: flex; align-items: center; gap: 8px;">
                    <i class="fa-solid fa-boxes-stacked" style="color: var(--primary);"></i>Add New Inventory Item
                </h1>
                <p>Enter details below to add a new item to the inventory system.</p>
            </div>

            <div class="form-grid">
                <!-- Item Name -->
                <div class="form-group">
                    <label for="<%= TextBox_ItemName.ClientID %>">Item Name <span style="color: #ef4444;">*</span></label>
                    <asp:TextBox ID="TextBox_ItemName" runat="server" CssClass="form-control" placeholder="e.g., Energy Drink 250ml" MaxLength="150"></asp:TextBox>
                </div>

                <!-- Category -->
                <div class="form-group">
                    <label for="<%= TextBox_Category.ClientID %>">Category</label>
                    <asp:TextBox ID="TextBox_Category" runat="server" CssClass="form-control" placeholder="e.g., Beverages" MaxLength="100"></asp:TextBox>
                </div>

                <!-- Unit Price -->
                <div class="form-group">
                    <label for="<%= TextBox_UnitPrice.ClientID %>">Unit Price <span style="color: #ef4444;">*</span></label>
                    <asp:TextBox ID="TextBox_UnitPrice" runat="server" Text="0" CssClass="form-control" placeholder="0.00" MaxLength="20"></asp:TextBox>
                </div>

                <!-- Quantity In Stock -->
                <div class="form-group">
                    <label for="<%= TextBox_QuantityInStock.ClientID %>">Quantity In Stock <span style="color: #ef4444;">*</span></label>
                    <asp:TextBox ID="TextBox_QuantityInStock" runat="server" Text="0" CssClass="form-control" placeholder="0" MaxLength="10"></asp:TextBox>
                </div>

                <!-- Reorder Level -->
                <div class="form-group">
                    <label for="<%= TextBox_ReorderLevel.ClientID %>">Reorder Level <span style="color: #ef4444;">*</span></label>
                    <asp:TextBox ID="TextBox_ReorderLevel" runat="server" Text="0" CssClass="form-control" placeholder="0" MaxLength="10"></asp:TextBox>
                </div>
            </div>

            <!-- Actions -->
            <div class="form-grid-action">
                <span onclick="navigateTo('BNetPage.aspx?Form=Inventory')" class="btn btn-secondary">Back</span>
                <asp:LinkButton ID="LinkButton_Submit" OnClick="LinkButton_Submit_Click" CssClass="btn btn-primary" OnClientClick="return ValidateInventory();" runat="server">
                    <i class="fa fa-save"></i> Add Item
                </asp:LinkButton>
            </div>
        </div>
    </ContentTemplate>
</asp:UpdatePanel>