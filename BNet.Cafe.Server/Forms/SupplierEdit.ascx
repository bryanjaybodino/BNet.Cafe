<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="SupplierEdit.ascx.cs" Inherits="BNet.Cafe.Server.Forms.SupplierEdit" %>
<asp:UpdatePanel ID="UpdatePanel1" runat="server">
    <ContentTemplate>
        <div class="bnet-table-wrapper">
            <div class="content-header" style="margin-bottom: 20px;">
                <h1 style="font-size: 20px; display: flex; align-items: center; gap: 8px;">
                    <i class="fa-solid fa-pen-to-square" style="color: var(--primary);"></i>Edit Supplier
                </h1>
                <p>Update existing details for this supplier.</p>
            </div>

            <div class="form-grid">
                <!-- Supplier Name -->
                <div class="form-group">
                    <label for="<%= TextBox_SupplierName.ClientID %>">Supplier Name <span style="color: #ef4444;">*</span></label>
                    <asp:TextBox ID="TextBox_SupplierName" runat="server" CssClass="form-control" placeholder="e.g., Acme Supplies" MaxLength="150"></asp:TextBox>
                </div>

                <!-- Contact Person -->
                <div class="form-group">
                    <label for="<%= TextBox_ContactPerson.ClientID %>">Contact Person</label>
                    <asp:TextBox ID="TextBox_ContactPerson" runat="server" CssClass="form-control" placeholder="e.g., John Smith" MaxLength="100"></asp:TextBox>
                </div>

                <!-- Phone -->
                <div class="form-group">
                    <label for="<%= TextBox_Phone.ClientID %>">Phone Number</label>
                    <asp:TextBox ID="TextBox_Phone" runat="server" CssClass="form-control" placeholder="e.g., +1234567890" MaxLength="30"></asp:TextBox>
                </div>

                <!-- Email -->
                <div class="form-group">
                    <label for="<%= TextBox_Email.ClientID %>">Email Address</label>
                    <asp:TextBox ID="TextBox_Email" runat="server" CssClass="form-control" placeholder="e.g., supplier@domain.com" MaxLength="100"></asp:TextBox>
                </div>

                <!-- Address -->
                <div class="form-group" style="grid-column: span 2;">
                    <label for="<%= TextBox_Address.ClientID %>">Address</label>
                    <asp:TextBox ID="TextBox_Address" runat="server" TextMode="MultiLine" Rows="2" CssClass="form-control" placeholder="Enter physical address" MaxLength="250"></asp:TextBox>
                </div>
            </div>

            <!-- Actions -->
            <div class="form-grid-action">
                <span onclick="navigateTo('BNetPage.aspx?Form=Suppliers')" class="btn btn-secondary">Back</span>
                <asp:LinkButton ID="LinkButton_Submit" OnClick="LinkButton_Submit_Click" CssClass="btn btn-primary" OnClientClick="return ValidateSupplier();" runat="server">
                    <i class="fa fa-save"></i> Save Changes
                </asp:LinkButton>
            </div>
        </div>
    </ContentTemplate>
</asp:UpdatePanel>