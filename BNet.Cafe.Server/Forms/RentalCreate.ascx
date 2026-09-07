<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="RentalCreate.ascx.cs" Inherits="BNet.Cafe.Server.Forms.RentalCreate" %>
<!-- Register script via ScriptManager to ensure compatibility with UpdatePanel -->
<asp:ScriptManagerProxy ID="ScriptManagerProxy1" runat="server">
    <Scripts>
        <asp:ScriptReference Path="~/Assets/js/RentalCreate.js" />
    </Scripts>
</asp:ScriptManagerProxy>

<asp:UpdatePanel ID="UpdatePanel1" runat="server">
    <ContentTemplate>
        <div class="table-wrapper">
            <div class="content-header" style="margin-bottom: 20px;">
                <h1 style="font-size: 20px; display: flex; align-items: center; gap: 8px;">
                    <i class="fa-solid fa-clock" style="color: var(--primary);"></i>Add New Rental
                </h1>
                <p>Enter the rental details below to record a new session.</p>
            </div>

            <div class="form-grid">
                <!-- Computer -->
                <div class="form-group">
                    <label for="<%= TextBox_ComputerName.ClientID %>">Computer <span style="color: #ef4444;">*</span></label>
                    <asp:TextBox ID="TextBox_ComputerName" Enabled="false" runat="server" CssClass="form-control" placeholder="e.g., PC-01" MaxLength="50"></asp:TextBox>

                </div>

                <!-- Customer -->
                <div class="form-group">
                    <label for="<%= DropDownList_Customer.ClientID %>">Customer <span style="color: #ef4444;">*</span></label>
                    <asp:DropDownList ID="DropDownList_Customer" runat="server" CssClass="bnet-select"></asp:DropDownList>
                </div>

                <!-- Duration -->
                <div class="form-group">
                    <label for="<%= TextBox_Duration.ClientID %>">Duration (Minutes) <span style="color: #ef4444;">*</span></label>
                    <asp:TextBox ID="TextBox_Duration" runat="server" CssClass="form-control" placeholder="e.g., 60" TextMode="Number"></asp:TextBox>
                </div>

                <!-- Amount -->
                <div class="form-group">
                    <label for="<%= TextBox_Amount.ClientID %>">Amount <span style="color: #ef4444;">*</span></label>
                    <asp:TextBox ID="TextBox_Amount" runat="server" CssClass="form-control" placeholder="0.00" TextMode="Number" step="0.01"></asp:TextBox>
                </div>
            </div>

            <!-- Actions -->
            <div class="form-grid-action">
                <span onclick="navigateTo('BNetPage.aspx?Form=Rentals')" class="btn btn-secondary">Back</span>
                <asp:LinkButton ID="LinkButton_Submit" OnClick="LinkButton_Submit_Click" CssClass="btn btn-primary" OnClientClick="return Validate();" runat="server">
                    <i class="fa fa-save"></i>Add Rental
                </asp:LinkButton>
            </div>
        </div>
    </ContentTemplate>
</asp:UpdatePanel>
