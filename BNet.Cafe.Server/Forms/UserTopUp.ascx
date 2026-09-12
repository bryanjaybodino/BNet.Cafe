<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="UserTopUp.ascx.cs" Inherits="BNet.Cafe.Server.Forms.UserTopUp" %>
<asp:ScriptManagerProxy ID="ScriptManagerProxy1" runat="server">
    <Scripts>
        <asp:ScriptReference Path="~/Assets/Pages/RentalManage.js" />
        <asp:ScriptReference Path="~/Assets/Pages/UserTopUp.js" />
    </Scripts>
</asp:ScriptManagerProxy>

<asp:UpdatePanel ID="UpdatePanel1" runat="server">
    <ContentTemplate>
        <div class="bnet-table-wrapper">
            <!-- Header -->
            <div class="content-header">
                <h1>
                    <i class="fa-solid fa-wallet"></i>
                    User Balance Top-Up
                    </h1>
                <p>Enter payment amount to credit account balance.</p>
            </div>

            <!-- User Details Banner -->
            <div class="card card-compact" style="margin-bottom: 20px;">
                <div class="card-inline-content">
                    <div class="card-icon-sm">
                        <i class="fa-solid fa-user"></i>
                    </div>
                    <div class="card-details">
                        <span class="card-title-sm">Account User</span>
                        <span class="card-value-sm">
                            <asp:Label ID="Label_UserName" runat="server" Text="Loading User..."></asp:Label>
                        </span>
                        <span class="card-footer">
                            <asp:Label ID="Label_UserEmail" runat="server" Text="--"></asp:Label>
                        </span>
                    </div>
                </div>
                <div class="card-details" style="text-align: right;">
                    <span class="card-title-sm">Current Balance</span>
                    <span class="card-value-sm">
                        <asp:Label ID="Label_CurrentBalance" runat="server" Text="0 Mins"></asp:Label>
                    </span>
                </div>
            </div>

            <!-- Amount Top-Up Form -->
            <div class="form-grid">
                <div class="form-group">
                    <label for="<%= TextBox_Amount.ClientID %>">Amount Paid (₱) <span style="color: #ef4444;">*</span></label>
                    <asp:TextBox ID="TextBox_Amount" runat="server" CssClass="form-control" Text="0.00" TextMode="Number" step="0.01" min="0" oninput="NumberOnly(this); calculateTimeFromAmount();"></asp:TextBox>
                </div>

                <div class="form-group">
                    <label for="<%= TextBox_Duration.ClientID %>">Equivalent Duration (Minutes)</label>
                    <asp:TextBox ID="TextBox_Duration" runat="server" CssClass="form-control" Text="0" ReadOnly="true"></asp:TextBox>
                </div>
            </div>

            <div class="form-group">
                <label for="<%= TextBox_Description.ClientID %>">Description / Remarks</label>
                <asp:TextBox ID="TextBox_Description" runat="server" CssClass="form-control" Text="Top-Up Load Credit"></asp:TextBox>
            </div>

            <!-- Action Buttons -->
            <div class="form-grid-action">
                <span onclick="navigateTo('BNetPage.aspx?Form=Users')" class="btn btn-secondary">
                    <i class="fa-solid fa-arrow-left"></i>Back
                    </span>
                <asp:LinkButton ID="LinkButton_Submit" OnClick="LinkButton_Submit_Click" CssClass="btn btn-primary" OnClientClick="return ValidateTopUp();" runat="server">
                        <i class="fa-solid fa-circle-check"></i> Process Top-Up
                    </asp:LinkButton>
            </div>
        </div>

    </ContentTemplate>
</asp:UpdatePanel>
