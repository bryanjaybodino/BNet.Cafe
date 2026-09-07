<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="ComputerCreate.ascx.cs" Inherits="BNet.Cafe.Server.Forms.ComputerCreate" %>
<!-- Register script via ScriptManager to ensure compatibility with UpdatePanel -->
<asp:ScriptManagerProxy ID="ScriptManagerProxy1" runat="server">
    <Scripts>
        <asp:ScriptReference Path="~/Assets/Pages/ComputerCreate.js" />
    </Scripts>
</asp:ScriptManagerProxy>

<asp:UpdatePanel ID="UpdatePanel1" runat="server">
    <ContentTemplate>
        <div class="bnet-table-wrapper">
            <div class="content-header" style="margin-bottom: 20px;">
                <h1 style="font-size: 20px; display: flex; align-items: center; gap: 8px;">
                    <i class="fa-solid fa-desktop" style="color: var(--primary);"></i>Add New Computer
                </h1>
                <p>Enter the machine details below to add a new station to the system. Make sure it is similar to client name of your app.config</p>
            </div>

            <div style="form-grid">
                <!-- Computer Name -->
                <div class="form-group">
                    <label for="<%= TextBox_ComputerName.ClientID %>">Computer Name <span style="color: #ef4444;">*</span></label>
                    <asp:TextBox ID="TextBox_ComputerName" runat="server" CssClass="form-control" placeholder="e.g., PC-01" onkeydown="return event.key !== ' ';" MaxLength="50"></asp:TextBox>
                </div>
            </div>
            <!-- Actions -->
            <div class="form-grid-action">
                <span onclick="navigateTo('BNetPage.aspx?Form=Computers')" class="btn btn-secondary">Back</span>
                <asp:LinkButton ID="LinkButton_Submit" OnClick="LinkButton_Submit_Click" CssClass="btn btn-primary" OnClientClick="return Validate();" runat="server">
            <i class="fa fa-save"></i>Add Computer</asp:LinkButton>
            </div>
        </div>
    </ContentTemplate>
</asp:UpdatePanel>