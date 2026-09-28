<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="UserCreate.ascx.cs" Inherits="BNet.Cafe.Server.Forms.UserCreate" %>
<asp:UpdatePanel ID="UpdatePanel1" runat="server">
    <ContentTemplate>
        <div class="bnet-table-wrapper">
            <div class="content-header" style="margin-bottom: 20px;">
                <h1 style="font-size: 20px; display: flex; align-items: center; gap: 8px;">
                    <i class="fa-solid fa-user-plus" style="color: var(--primary);"></i>Add New User
                </h1>
                <p>Enter the account details below to add a new user to the system.</p>
            </div>

            <div class="form-grid">
                <!-- Name -->
                <div class="form-group">
                    <label for="<%= TextBox_Name.ClientID %>">Full Name <span style="color: #ef4444;">*</span></label>
                    <asp:TextBox ID="TextBox_Name" runat="server" CssClass="form-control" placeholder="e.g., John Doe" MaxLength="100"></asp:TextBox>
                </div>

                <!-- Email -->
                <div class="form-group">
                    <label for="<%= TextBox_Email.ClientID %>">Email Address <span style="color: #ef4444;">*</span></label>
                    <asp:TextBox ID="TextBox_Email" runat="server" CssClass="form-control" placeholder="e.g., user@domain.com" MaxLength="100"></asp:TextBox>
                </div>

                <!-- Password -->
                <div class="form-group">
                    <label for="<%= TextBox_Password.ClientID %>">Password <span style="color: #ef4444;">*</span></label>
                    <asp:TextBox ID="TextBox_Password" runat="server"  Text="USER@123" CssClass="form-control" placeholder="Enter password" MaxLength="100"></asp:TextBox>
                </div>

                <!-- Role -->
                <div class="form-group">
                    <label for="<%= DropDownList_Role.ClientID %>">Role <span style="color: #ef4444;">*</span></label>
                    <asp:DropDownList ID="DropDownList_Role" runat="server" CssClass="form-control">
                        <asp:ListItem Text="Member" Value="MEMBER" Selected="True"></asp:ListItem>
                        <asp:ListItem Text="VIP" Value="VIP"></asp:ListItem>
                        <asp:ListItem Text="Admin" Value="ADMIN"></asp:ListItem>
                    </asp:DropDownList>
                </div>
            </div>

            <!-- Actions -->
            <div class="form-grid-action">
                <span onclick="navigateTo('BNetPage.aspx?Form=Users')" class="btn btn-secondary">Back</span>
                <asp:LinkButton ID="LinkButton_Submit" OnClick="LinkButton_Submit_Click" CssClass="btn btn-primary" OnClientClick="return Validate();" runat="server">
                    <i class="fa fa-save"></i> Add User
                </asp:LinkButton>
            </div>
        </div>
    </ContentTemplate>
</asp:UpdatePanel>