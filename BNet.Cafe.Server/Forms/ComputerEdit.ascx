<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="ComputerEdit.ascx.cs" Inherits="BNet.Cafe.Server.Forms.ComputerEdit" %>
<!-- Register script via ScriptManager to ensure compatibility with UpdatePanel -->
<asp:ScriptManagerProxy ID="ScriptManagerProxy1" runat="server">
    <Scripts>
        <asp:ScriptReference Path="~/Assets/Pages/ComputerEdit.js" />
    </Scripts>
</asp:ScriptManagerProxy>

<asp:UpdatePanel ID="UpdatePanel1" runat="server">
    <ContentTemplate>
        <div class="bnet-table-wrapper">
            <div class="content-header" style="margin-bottom: 20px;">
                <h1 style="font-size: 20px; display: flex; align-items: center; gap: 8px;">
                    <i class="fa-solid fa-desktop" style="color: var(--primary);"></i>Edit Computer
                </h1>
                <p>Update the machine details below for this station.</p>
            </div>

            <div class="form-grid">
                <!-- Computer Name -->
                <div class="form-group">
                    <label for="<%= TextBox_ComputerName.ClientID %>">Computer Name <span style="color: #ef4444;">*</span></label>
                    <asp:TextBox ID="TextBox_ComputerName" runat="server" CssClass="form-control" placeholder="e.g., PC-01" MaxLength="50"></asp:TextBox>
                </div>
            </div>

            <!-- Actions -->
            <div class="form-grid-action">
                <span onclick="navigateTo('BNetPage.aspx?Form=Computers')" class="btn btn-secondary">Back</span>
                <asp:LinkButton ID="LinkButton_Submit" OnClick="LinkButton_Submit_Click" CssClass="btn btn-primary" OnClientClick="return Validate();" runat="server">
                    <i class="fa fa-save"></i> Save Changes
                </asp:LinkButton>
            </div>
        </div>
    </ContentTemplate>
</asp:UpdatePanel>