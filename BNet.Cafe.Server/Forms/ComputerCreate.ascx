<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="ComputerCreate.ascx.cs" Inherits="BNet.Cafe.Server.Forms.ComputerCreate" %>
<asp:UpdatePanel ID="UpdatePanel1" runat="server">
    <ContentTemplate>
        <div class="table-wrapper">
            <div class="content-header" style="margin-bottom: 20px;">
                <h1 style="font-size: 20px; display: flex; align-items: center; gap: 8px;">
                    <i class="fa-solid fa-desktop" style="color: var(--primary);"></i>Add New Computer
                </h1>
                <p>Enter the machine details below to add a new station to the system.</p>
            </div>

            <div style="form-grid">
                <!-- Computer Name -->
                <div class="form-group">
                    <label for="<%= txtComputerName.ClientID %>">Computer Name <span style="color: #ef4444;">*</span></label>
                    <asp:TextBox ID="txtComputerName" runat="server" CssClass="form-control" placeholder="e.g., PC-01" MaxLength="50"></asp:TextBox>
                </div>
            </div>
            <!-- Actions -->
            <div class="form-grid-action">
                <asp:HyperLink ID="HyperLink_Back" NavigateUrl="~/BNetPage.aspx?Form=Computers" runat="server" CssClass="btn btn-secondary">Back</asp:HyperLink>
                <asp:LinkButton ID="LinkButton_Submit" OnClick="LinkButton_Submit_Click" CssClass="btn btn-primary" runat="server">
            <i class="fa fa-save"></i>Add Computer</asp:LinkButton>
            </div>
        </div>
    </ContentTemplate>
</asp:UpdatePanel>
