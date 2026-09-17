<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="RentalManage.ascx.cs" Inherits="BNet.Cafe.Server.Forms.RentalManage" %>
<%@ Register Src="~/Forms/Modals/RemoteOpenTime.ascx" TagPrefix="uc1" TagName="RemoteOpenTime" %>

<link href="Assets/Pages/RentalManage.css" rel="stylesheet" />
<asp:ScriptManagerProxy ID="ScriptManagerProxy1" runat="server">
    <Scripts>
        <asp:ScriptReference Path="~/Assets/Pages/RentalManage.js" />
        <asp:ScriptReference Path="~/Assets/Pages/RemoteMessaging.js" />
    </Scripts>
</asp:ScriptManagerProxy>

<asp:UpdatePanel ID="UpdatePanel1" runat="server">
    <ContentTemplate>
        <!-- Hidden fields to store loaded DB state -->
        <asp:HiddenField ID="HiddenField_InitialDuration" runat="server" Value="0" />
        <asp:HiddenField ID="HiddenField_InitialAmount" runat="server" Value="0.00" />

        <asp:Panel ID="Panel_Form" runat="server">
            <div class="bnet-table-wrapper">
                <div class="content-header" style="margin-bottom: 20px;">
                    <h1 style="font-size: 20px; display: flex; align-items: center; gap: 8px;">
                        <i class="fa-solid fa-clock" style="color: var(--primary);"></i>
                        <asp:Label ID="Label_HeaderText" runat="server" Text=""></asp:Label>
                        <asp:Label ID="Label_RentalId" runat="server" Text=""></asp:Label>
                        <asp:Label ID="Label_Status" runat="server" Text=""></asp:Label>
                    </h1>
                    <p>Select customer details, manage rental duration, and calculate charges.</p>
                </div>

                <div class="form-grid">
                    <!-- Computer Selection -->
                    <div class="form-group">
                        <label for="<%= TextBox_ComputerName.ClientID %>">Computer <span style="color: #ef4444;">*</span></label>
                        <asp:TextBox ID="TextBox_ComputerName" Enabled="false" runat="server" CssClass="form-control" placeholder="e.g., PC-01" MaxLength="50"></asp:TextBox>
                    </div>

                    <!-- Customer Selection -->
                    <div class="form-group">
                        <label for="<%= DropDownList_Customer.ClientID %>">Customer <span style="color: #ef4444;">*</span></label>
                        <asp:DropDownList ID="DropDownList_Customer" Enabled="false" runat="server" CssClass="form-control bnet-select"></asp:DropDownList>
                    </div>
                </div>

                <!-- Interactive Duration Controls -->
                <div class="duration-section">
                    <label>Duration Quick Actions</label>
                    <div class="quick-btn-group">
                        <button type="button" class="btn btn-secondary" onclick="adjustDuration(15)">+15 Mins</button>
                        <button type="button" class="btn btn-secondary" onclick="adjustDuration(30)">+30 Mins</button>
                        <button type="button" class="btn btn-secondary" onclick="adjustDuration(60)">+1 Hour</button>
                        <button type="button" class="btn btn-danger-outline" onclick="adjustDuration(-15)">-15 Mins</button>
                        <button type="button" class="btn btn-danger-outline" onclick="adjustDuration(-30)">-30 Mins</button>
                        <button type="button" class="btn btn-danger-outline" onclick="adjustDuration(-60)">-1 Hour</button>
                        <button type="button" class="btn btn-danger-outline" onclick="resetDuration()">Reset</button>
                    </div>

                    <div class="form-grid" style="margin-top: 15px;">
                        <div class="form-group">
                            <label for="<%= TextBox_Duration.ClientID %>">Total Input (Minutes)</label>
                            <asp:TextBox ID="TextBox_Duration" runat="server" CssClass="form-control" Text="0" TextMode="Number" min="0" oninput="NumberOnly(this); calculateAmount();"></asp:TextBox>
                        </div>
                        <div class="form-group">
                            <label for="<%= TextBox_Amount.ClientID %>">Total Input Amount (₱)</label>
                            <asp:TextBox ID="TextBox_Amount" runat="server" CssClass="form-control" Text="0.00" TextMode="Number" oninput="NumberOnly(this); calculateTimeFromAmount();"></asp:TextBox>
                        </div>
                    </div>
                </div>

                <!-- Highlighted Summary Display Box -->
                <div class="summary-display-card">
                    <div class="summary-item">
                        <span class="summary-label"><i class="fa-solid fa-hourglass-half"></i>Total Time</span>
                        <span id="display_FormattedTime" class="summary-value highlight-time">0 hrs 0 mins</span>
                        <small id="display_TimeBreakdown" style="display:none; color: #6b7280; font-size: 12px; margin-top: 4px;"></small>
                    </div>
                    <div class="summary-divider"></div>
                    <div class="summary-item">
                        <span class="summary-label"><i class="fa-solid fa-peso-sign"></i>Total Amount</span>
                        <span id="display_TotalAmount" class="summary-value highlight-amount">₱ 0.00</span>
                        <small id="display_AmountBreakdown" style="display:none; color: #10b981; font-size: 12px; font-weight: 600; margin-top: 4px;"></small>
                    </div>
                </div>
                
                <asp:Panel ID="Panel_Buttons" runat="server">
                    <!-- Form Actions -->
                    <div class="form-grid-action">
                        <span onclick="navigateTo('BNetPage.aspx?Form=Computers')" class="btn btn-secondary">Back</span>
                        <span class="btn btn-danger" onclick="openOpenTimeModal('<%= Label_RentalId.Text %>','<%= TextBox_ComputerName.Text %>','<%= DropDownList_Customer.SelectedValue %>','<%= Label_Status.Text %>')"><i class="fa fa-hourglass"></i>Open Time</span>
                        <asp:LinkButton ID="LinkButton_Submit" OnClick="LinkButton_Submit_Click" CssClass="btn btn-primary" OnClientClick="return Validate();" runat="server">
                            <i class="fa fa-save"></i>Add Rental
                        </asp:LinkButton>
                    </div>
                </asp:Panel>
            </div>
        </asp:Panel>
        <uc1:RemoteOpenTime runat="server" ID="RemoteOpenTime" />
    </ContentTemplate>
</asp:UpdatePanel>