<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="InventoryEdit.ascx.cs" Inherits="BNet.Cafe.Server.Forms.InventoryEdit" %>

<asp:UpdatePanel ID="UpdatePanel1" runat="server">
    <ContentTemplate>
        <div class="bnet-table-wrapper">
            <div class="content-header" style="margin-bottom: 24px;">
                <h1 style="font-size: 20px; display: flex; align-items: center; gap: 8px;">
                    <i class="fa-solid fa-boxes-stacked" style="color: var(--primary);"></i>Edit Inventory Item
                </h1>
                <p>Update inventory item details and stock parameters.</p>
            </div>

            <!-- Modern Form Layout: Image Box on Left, Inputs on Right -->
            <div class="inventory-form-layout">
                
                <!-- Left Column: Square Upload Box -->
                <div class="image-upload-col">
                    <label class="form-label">Item Image</label>
                    <div class="inventory-upload-box" id="inventoryDropZone">
                        <!-- Placeholder State (Shown when no image is selected or available) -->
                        <div id="uploadPlaceholder" class="upload-placeholder">
                            <i class="fa-solid fa-cloud-arrow-up upload-icon"></i>
                            <span class="upload-title">Upload Image</span>
                            <span class="upload-sub">Drag & drop or <span class="browse-link">browse</span></span>
                        </div>

                        <!-- Image Filled State (Shown when image exists or selected) -->
                        <div id="uploadPreviewWrapper" class="upload-preview-wrapper d-none">
                            <img id="inventoryPreviewImg" src="" alt="Item Preview" />
                            <button type="button" class="remove-img-btn" title="Remove Image" onclick="clearInventoryImage(event)">
                                <i class="fa-solid fa-xmark"></i>
                            </button>
                        </div>

                        <input type="file" id="inventoryFileInput" accept="image/jpeg,image/png,image/webp" class="d-none" />
                    </div>
                    <asp:HiddenField ID="HiddenField_ImageData" runat="server" />
                </div>

                <!-- Right Column: Form Fields Grid -->
                <div class="fields-col">
                    <div class="form-group">
                        <label for="<%= TextBox_ItemName.ClientID %>">Item Name <span class="required">*</span></label>
                        <asp:TextBox ID="TextBox_ItemName" runat="server" CssClass="form-control" placeholder="e.g., Energy Drink 250ml" MaxLength="150"></asp:TextBox>
                    </div>

                    <div class="form-group">
                        <label for="<%= TextBox_Category.ClientID %>">Category</label>
                        <asp:TextBox ID="TextBox_Category" runat="server" CssClass="form-control" placeholder="e.g., Beverages" MaxLength="100"></asp:TextBox>
                    </div>

                    <div class="form-group">
                        <label for="<%= TextBox_UnitPrice.ClientID %>">Unit Price <span class="required">*</span></label>
                        <asp:TextBox ID="TextBox_UnitPrice" oninput="NumberOnly(this);" TextMode="Number"  runat="server" CssClass="form-control" placeholder="0.00" MaxLength="20"></asp:TextBox>
                    </div>

                    <div class="form-row-2col">
                        <div class="form-group">
                            <label for="<%= TextBox_QuantityInStock.ClientID %>">Quantity In Stock <span class="required">*</span></label>
                            <asp:TextBox ID="TextBox_QuantityInStock" oninput="NumberOnly(this);" TextMode="Number"  runat="server" CssClass="form-control" placeholder="0" MaxLength="10"></asp:TextBox>
                        </div>

                        <div class="form-group">
                            <label for="<%= TextBox_ReorderLevel.ClientID %>">Reorder Level <span class="required">*</span></label>
                            <asp:TextBox ID="TextBox_ReorderLevel" oninput="NumberOnly(this);" TextMode="Number"  runat="server" CssClass="form-control" placeholder="0" MaxLength="10"></asp:TextBox>
                        </div>
                    </div>
                </div>

            </div>

            <!-- Actions -->
            <div class="form-grid-action" style="margin-top: 24px;">
                <span onclick="navigateTo('BNetPage.aspx?Form=Inventory')" class="btn btn-secondary">Back</span>
                <asp:LinkButton ID="LinkButton_Submit" OnClick="LinkButton_Submit_Click" CssClass="btn btn-primary" OnClientClick="return ValidateInventory();" runat="server">
                    <i class="fa fa-save"></i> Save Changes
                </asp:LinkButton>
            </div>
        </div>
    </ContentTemplate>
</asp:UpdatePanel>