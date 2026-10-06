<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="UserTopUp.ascx.cs" Inherits="BNet.Cafe.Server.Forms.UserTopUp" %>
<asp:UpdatePanel ID="UpdatePanel1" runat="server">
    <ContentTemplate>
        <asp:HiddenField ID="HiddenField_Role" runat="server" />
        <asp:Panel ID="Panel_Form" runat="server">
            <div class="bnet-table-wrapper">
                <!-- Header -->
                <div class="content-header">
                    <h1>
                        <i class="fa-solid fa-wallet"></i>
                        Top-Up & Deduction
                    </h1>
                    <p>Manage user balance by adding top-up credits or deducting balance.</p>
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

                <!-- Form Inputs -->
                <div class="form-grid">
                    <div class="form-group" style="margin-bottom: 20px;">
                        <label for="<%= DropDownList_Type.ClientID %>">Transaction Type</label>
                        <asp:DropDownList ID="DropDownList_Type" runat="server" CssClass="form-control" AutoPostBack="true" OnSelectedIndexChanged="DropDownList_Type_SelectedIndexChanged">
                            <asp:ListItem Text="Top-Up (Add Credit)" Value="TopUp" Selected="True" />
                            <asp:ListItem Text="Deduction (Subtract Balance)" Value="Deduction" />
                        </asp:DropDownList>
                    </div>
                    <div class="form-group">
                        <label for="<%= TextBox_Amount.ClientID %>">Amount (₱) <span style="color: #ef4444; font-weight: 700;">*</span></label>
                        <asp:TextBox ID="TextBox_Amount" runat="server" CssClass="form-control" Text="0.00" TextMode="Number" step="0.01" min="0" oninput="NumberOnly(this); calculateTimeFromAmount();"></asp:TextBox>
                    </div>

                    <div class="form-group">
                        <label for="<%= TextBox_Duration.ClientID %>">Equivalent Duration (Minutes)</label>
                        <asp:TextBox ID="TextBox_Duration" runat="server" CssClass="form-control" Text="0" TextMode="Number" min="0" oninput="NumberOnly(this); calculateAmountFromTime();"></asp:TextBox>
                    </div>
                </div>

                <!-- Conversion Preview Card -->
                <div class="card" style="margin-bottom: 20px; padding: 16px;">
                    <div style="display: flex; justify-content: space-between; align-items: center;">
                        <div>
                            <span class="card-title-sm" style="display: block; margin-bottom: 4px;">Formatted Credit/Deduct Time</span>
                            <strong id="display_FormattedTime" style="font-size: 2rem; color: var(--primary);">0 hrs 0 mins</strong>
                        </div>
                        <div style="text-align: right;">
                            <span class="card-title-sm" style="display: block; margin-bottom: 4px;">Total Charge/Deduction</span>
                            <strong id="display_TotalAmount" style="font-size: 2rem; color: #10b981;">₱ 0.00</strong>
                        </div>
                    </div>
                </div>

                <div class="form-group">
                    <label for="<%= TextBox_Description.ClientID %>">Description / Remarks</label>
                    <asp:TextBox ID="TextBox_Description" runat="server" Enabled="false" CssClass="form-control" Text="Top-Up Load Credit"></asp:TextBox>
                </div>

                <!-- Action Buttons -->
                <div class="form-grid-action">
                    <span onclick="navigateTo('BNetPage.aspx?Form=Users')" class="btn btn-secondary">
                        <i class="fa-solid fa-arrow-left"></i>Back
                    </span>
                    <asp:LinkButton ID="LinkButton_Submit" OnClick="LinkButton_Submit_Click" CssClass="btn btn-primary" OnClientClick="return ValidateTopUp();" runat="server">
                        <i class="fa-solid fa-circle-check"></i> Process Transaction
                    </asp:LinkButton>
                </div>
            </div>
        </asp:Panel>
    </ContentTemplate>
</asp:UpdatePanel>
