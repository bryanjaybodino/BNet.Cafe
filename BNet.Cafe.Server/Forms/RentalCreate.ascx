<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="RentalCreate.ascx.cs" Inherits="BNet.Cafe.Server.Forms.RentalCreate" %>

<style>
 .duration-section {background-color: var(--bg-light-tertiary);border: 1px solid var(--border-light);border-radius: 10px;padding: 16px;margin: 16px 0;}.quick-btn-group {display: flex;flex-wrap: wrap;gap: 8px;margin-top: 8px;}.btn-danger-outline {background-color: transparent;color: #ef4444;border: 1px solid #ef4444;}.btn-danger-outline:hover {background-color: #ef4444;color: #ffffff;}.rate-legend-box {background-color: var(--bg-light-secondary);border: 1px dashed var(--border-light);border-radius: 8px;padding: 12px 16px;margin-top: 12px;}.rate-legend-title {font-size: 12px;font-weight: 700;color: var(--text-light-secondary);text-transform: uppercase;margin-bottom: 8px;}.rate-grid {display: grid;grid-template-columns: repeat(auto-fit, minmax(110px, 1fr));gap: 8px;font-size: 13px;color: var(--text-light);}.rate-note {margin-top: 10px;padding-top: 8px;border-top: 1px dashed var(--border-light);font-size: 12px;color: var(--text-light-secondary);display: flex;align-items: center;gap: 6px;}.rate-note i {color: var(--primary);}.amount-display {font-weight: 700;font-size: 16px;color: var(--primary) !important;}.summary-display-card {display: flex;align-items: center;justify-content: space-around;background-color: var(--bg-light-secondary);border: 2px solid var(--primary);border-radius: 12px;padding: 20px;margin: 20px 0;box-shadow: 0 4px 12px rgba(0, 0, 0, 0.05);}.summary-item {display: flex;flex-direction: column;align-items: center;gap: 6px;}.summary-label {font-size: 13px;font-weight: 600;color: var(--text-light-secondary);text-transform: uppercase;letter-spacing: 0.5px;}.summary-value {font-size: 28px;font-weight: 800;line-height: 1.2;}.highlight-time {color: var(--text-light);}.highlight-amount {color: #10b981;}.summary-divider {width: 1px;height: 50px;background-color: var(--border-light);}@media (max-width: 576px) {.summary-display-card {flex-direction: column;gap: 16px;}.summary-divider {width: 100%;height: 1px;}}
</style>



<asp:ScriptManagerProxy ID="ScriptManagerProxy1" runat="server">
    <Scripts>
        <asp:ScriptReference Path="~/Assets/js/RentalCreate.js" />
    </Scripts>
</asp:ScriptManagerProxy>

<asp:UpdatePanel ID="UpdatePanel1" runat="server">
    <ContentTemplate>
        <div class="bnet-table-wrapper">
            <div class="content-header" style="margin-bottom: 20px;">
                <h1 style="font-size: 20px; display: flex; align-items: center; gap: 8px;">
                    <i class="fa-solid fa-clock" style="color: var(--primary);"></i>Add New Rental
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
                    <asp:DropDownList ID="DropDownList_Customer" runat="server" CssClass="form-control bnet-select"></asp:DropDownList>
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
                    <button type="button" class="btn btn-danger-outline" onclick="resetDuration()">Reset</button>
                </div>

                <div class="form-group" style="margin-top: 15px;">
                    <label for="<%= TextBox_Duration.ClientID %>">Total Input (Minutes)</label>
                    <asp:TextBox ID="TextBox_Duration" runat="server" CssClass="form-control" Text="0" TextMode="Number" min="0" oninput="calculateAmount()"></asp:TextBox>
                </div>
            </div>

            <!-- Highlighted Summary Display Box -->
            <div class="summary-display-card">
                <div class="summary-item">
                    <span class="summary-label"><i class="fa-solid fa-hourglass-half"></i> Total Time</span>
                    <span id="display_FormattedTime" class="summary-value highlight-time">0 hrs 0 mins</span>
                </div>
                <div class="summary-divider"></div>
                <div class="summary-item">
                    <span class="summary-label"><i class="fa-solid fa-peso-sign"></i>Total Amount</span>
                    <span id="display_TotalAmount" class="summary-value highlight-amount">₱ 0.00</span>
                </div>
            </div>

            <!-- Hidden field to keep server-side sync for Amount -->
            <asp:TextBox ID="TextBox_Amount" runat="server" CssClass="d-none" Text="0.00"></asp:TextBox>

            <!-- Rate Tier Reference Box -->
            <div class="rate-legend-box">
                <div class="rate-legend-title"><i class="fa-solid fa-tags"></i> Current Rate Reference</div>
                <div class="rate-grid">
                    <span>15 Mins: <strong>₱5</strong></span>
                    <span>30 Mins: <strong>₱10</strong></span>
                    <span>1 Hour: <strong>₱15</strong></span>
                    <span>2 Hours: <strong>₱25</strong></span>
                    <span>3 Hours: <strong>₱40</strong></span>
                    <span>4 Hours: <strong>₱50</strong></span>
                </div>
                <div class="rate-note">
                    <i class="fa-solid fa-circle-info"></i><strong>Note:</strong> After reaching 4 hours (₱50), each additional hour extension is charged at a flat rate of <strong>₱10/hr</strong>.
                </div>
            </div>

            <!-- Form Actions -->
            <div class="form-grid-action">
                <span onclick="navigateTo('BNetPage.aspx?Form=Rentals')" class="btn btn-secondary">Back</span>
                <asp:LinkButton ID="LinkButton_Submit" OnClick="LinkButton_Submit_Click" CssClass="btn btn-primary" OnClientClick="return Validate();" runat="server">
                    <i class="fa fa-save"></i>Add Rental
                </asp:LinkButton>
            </div>
        </div>
    </ContentTemplate>
</asp:UpdatePanel>
