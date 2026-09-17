<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="PricingSettings.ascx.cs" Inherits="BNet.Cafe.Server.Forms.PricingSettings" %>

<asp:UpdatePanel ID="UpdatePanel1" runat="server" UpdateMode="Conditional">
    <ContentTemplate>
        <asp:HiddenField ID="HiddenField_PriceId" runat="server" Value="0" />
        <asp:HiddenField ID="HiddenField_TotalMinutes" runat="server" Value="0" />

        <div class="pricing-settings-wrapper">
            <!-- Header Section -->
            <div class="pricing-header">
                <div>
                    <h1 class="pricing-title">
                        <i class="fa-solid fa-tags"></i>Pricing Intervals
                    </h1>
                    <p class="pricing-subtitle">
                        Define pricing tiers based on duration. The system will interpolate prices between intervals.
                   
                    </p>
                </div>
            </div>

            <!-- Main Content Grid -->
            <div class="pricing-container">
                <!-- Left: Form Panel -->
                <div class="pricing-form-panel">
                    <div class="form-card">
                        <h2 class="form-section-title">
                            <i class="fa-solid fa-plus-circle"></i>Add/Edit Pricing Rule
                        </h2>

                        <!-- Customer Type -->
                        <div class="form-group">
                            <label for="DropDownList_CustomerType">
                                Customer Tier <span class="required-badge">*</span>
                            </label>
                            <asp:DropDownList ID="DropDownList_CustomerType" AutoPostBack="true" runat="server" CssClass="form-control pricing-select">
                                <asp:ListItem Text="Guest / Walk-In" Value="GUEST"></asp:ListItem>
                                <asp:ListItem Text="Member" Value="MEMBER"></asp:ListItem>
                            </asp:DropDownList>
                        </div>

                        <!-- Duration: Hours and Minutes Input -->
                        <div class="form-group">
                            <label>
                                Duration <span class="required-badge">*</span>
                            </label>

                            <!-- Hours and Minutes Inputs -->
                            <div class="duration-input-group">
                                <div class="duration-input-item">
                                    <label for="TextBox_Hours" class="duration-label">Hours</label>
                                    <asp:TextBox ID="TextBox_Hours" runat="server" TextMode="Number"
                                        CssClass="form-control pricing-input duration-input"
                                        placeholder="0" min="0" max="24"></asp:TextBox>
                                </div>

                                <div class="duration-input-separator">
                                    <span class="separator-text">+</span>
                                </div>

                                <div class="duration-input-item">
                                    <label for="TextBox_Minutes_Only" class="duration-label">Minutes</label>
                                    <asp:TextBox ID="TextBox_Minutes_Only" runat="server" TextMode="Number"
                                        CssClass="form-control pricing-input duration-input"
                                        placeholder="0" min="0" max="59"></asp:TextBox>
                                </div>
                            </div>

                            <!-- Total Minutes Display -->
                            <div class="helper-text" style="margin-top: 12px;">
                                <i class="fa fa-clock"></i>Total Duration: 
                               
                                <span id="TotalDurationDisplay" class="value-highlight">0 mins</span>
                                <span id="TotalMinutesValue" class="value-highlight" style="display: none;">0</span>
                            </div>
                        </div>

                        <!-- Price -->
                        <div class="form-group">
                            <label for="TextBox_Price">
                                Price (₱) <span class="required-badge">*</span>
                            </label>
                            <div class="input-group-with-helper">
                                <div class="price-input-wrapper">
                                    <span class="currency-symbol">₱</span>
                                    <asp:TextBox ID="TextBox_Price" runat="server" TextMode="Number" CssClass="form-control pricing-input"
                                        placeholder="0.00" step="0.01" min="0"></asp:TextBox>
                                </div>
                                <div class="helper-text">
                                    Amount: <span id="PriceDisplay" class="value-highlight">₱ 0.00</span>
                                </div>
                            </div>
                        </div>

                        <!-- Action Buttons -->
                        <div class="form-actions">
                            <asp:LinkButton ID="LinkButton_Cancel" OnClick="LinkButton_Cancel_Click"
                                CssClass="btn btn-secondary" runat="server">
                                <i class="fa fa-times-circle"></i> Clear
                            </asp:LinkButton>
                            <asp:LinkButton ID="LinkButton_Submit" OnClick="LinkButton_Submit_Click"
                                CssClass="btn btn-primary btn-lg" OnClientClick="return ValidatePricing();" runat="server">
                                <i class="fa fa-save"></i> Save Rule
                            </asp:LinkButton>
                        </div>
                    </div>

                    <!-- Quick Reference -->
                    <div class="quick-reference">
                        <h3>💡 Quick Reference</h3>
                        <p>
                            Set price points at key intervals (30 min, 1 hr, 1.5 hrs, etc). 
                           The system automatically calculates intermediate prices.
                        </p>
                        <div class="reference-example">
                            <strong>Example Input:</strong>
                            <ul>
                                <li>0 hours + 30 minutes → ₱10.00</li>
                                <li>1 hour + 0 minutes → ₱15.00</li>
                                <li>1 hour + 30 minutes → ₱20.00</li>
                                <li>2 hours + 0 minutes → ₱25.00</li>
                                <li>4 hours + 0 minutes → ₱50.00</li>
                            </ul>
                            <em>45 mins will auto-calculate to ₱12.50</em>
                        </div>
                    </div>
                </div>

                <!-- Right: Data Grid Panel -->
                <div class="pricing-grid-panel">
                    <div class="grid-card">
                        <div class="grid-header">
                            <h2 class="grid-title">
                                <i class="fa-solid fa-list"></i>Existing Rules
                            </h2>
                            <span class="grid-count">Total: <strong id="RuleCount">
                                <asp:Label ID="Label_TotalRules" runat="server" Text="0"></asp:Label>
                            </strong>rules
                            </span>
                        </div>

                        <!-- Grid Container with scroll -->
                        <div class="bnet-table-container pricing-grid-container">
                            <asp:GridView ID="GridView_Pricing" runat="server" AutoGenerateColumns="False" GridLines="None"
                                CssClass="bnet-table pricing-table" OnRowCommand="GridView_Pricing_RowCommand"
                                OnDataBound="GridView_Pricing_DataBound">
                                <Columns>

                                    <asp:TemplateField HeaderText="Tier">
                                        <HeaderStyle Width="20%" CssClass="header-cell" />
                                        <ItemStyle Width="20%" CssClass="cell-tier" />
                                        <ItemTemplate>
                                            <span class="tier-badge">
                                                <%# FormatCustomerType(Eval("DBCustomerType").ToString()) %>
                                            </span>
                                        </ItemTemplate>
                                    </asp:TemplateField>


                                    <asp:TemplateField HeaderText="Duration">
                                        <HeaderStyle Width="30%" CssClass="header-cell" />
                                        <ItemStyle Width="30%" CssClass="cell-duration" />
                                        <ItemTemplate>
                                            <span class="duration-value">
                                                <%# FormatDuration(int.Parse(Eval("DBMinutes").ToString())) %>
                                            </span>
                                            <span class="duration-minutes" style="display: block; font-size: 11px; color: var(--text-light-secondary); margin-top: 4px;">(<%# Eval("DBMinutes") %> mins)
                                            </span>
                                        </ItemTemplate>
                                    </asp:TemplateField>

                                    <asp:TemplateField HeaderText="Price">
                                        <HeaderStyle Width="25%" CssClass="header-cell" />
                                        <ItemStyle Width="25%" CssClass="cell-price" />
                                        <ItemTemplate>
                                            <span class="price-value">₱ <%# double.Parse(Eval("DBPrice").ToString()).ToString("F2") %>
                                            </span>
                                        </ItemTemplate>
                                    </asp:TemplateField>

                                    <asp:TemplateField HeaderText="Actions" HeaderStyle-Width="25%">
                                        <HeaderStyle Width="25%" CssClass="header-cell" />
                                        <ItemStyle Width="25%" CssClass="cell-actions" />
                                        <ItemTemplate>
                                            <div class="action-buttons">
                                                <asp:LinkButton ID="LinkButton_Edit" runat="server" CommandName="EditRate"
                                                    CommandArgument='<%# Eval("DBId") %>' CssClass="btn-action btn-edit"
                                                    ToolTip="Edit this rule">
                                                    <i class="fa fa-edit"></i>
                                                </asp:LinkButton>
                                                <asp:LinkButton ID="LinkButton_Delete" runat="server" CommandName="DeleteRate"
                                                    CommandArgument='<%# Eval("DBId") %>' CssClass="btn-action btn-delete"
                                                    OnClientClick="return confirm('Are you sure you want to delete this rule?');"
                                                    ToolTip="Delete this rule">
                                                    <i class="fa fa-trash"></i>
                                                </asp:LinkButton>
                                            </div>
                                        </ItemTemplate>
                                    </asp:TemplateField>
                                </Columns>
                                <EmptyDataTemplate>
                                    <div class="empty-state">
                                        <i class="fa-solid fa-inbox"></i>
                                        <p>No pricing rules defined yet.</p>
                                        <small>Create your first pricing rule to get started.</small>
                                    </div>
                                </EmptyDataTemplate>
                            </asp:GridView>
                        </div>
                    </div>
                </div>
            </div>
        </div>
    </ContentTemplate>
</asp:UpdatePanel>

<!-- Styles -->
<style>
    /* Pricing Settings Specific Styles */
    .pricing-settings-wrapper {
        display: flex;
        flex-direction: column;
        gap: 24px;
        padding: 20px;
        background-color: var(--bg-light);
        border-radius: 12px;
    }

    .pricing-header {
        display: flex;
        justify-content: space-between;
        align-items: flex-start;
        padding-bottom: 20px;
        border-bottom: 1px solid var(--border-light);
    }

    .pricing-title {
        font-size: 28px;
        font-weight: 700;
        color: var(--text-light);
        display: flex;
        align-items: center;
        gap: 12px;
        margin-bottom: 8px;
    }

    .pricing-subtitle {
        color: var(--text-light-secondary);
        font-size: 14px;
        margin: 0;
    }

    .pricing-container {
        display: grid;
        grid-template-columns: 1fr 1.5fr;
        gap: 24px;
        min-height: 600px;
    }

    /* Form Panel */
    .pricing-form-panel {
        display: flex;
        flex-direction: column;
        gap: 20px;
    }

    .form-card {
        background-color: var(--bg-light-secondary);
        border: 1px solid var(--border-light);
        border-radius: 12px;
        padding: 24px;
        box-shadow: 0 2px 4px rgba(0, 0, 0, 0.05);
    }

    .form-section-title {
        font-size: 16px;
        font-weight: 700;
        color: var(--text-light);
        display: flex;
        align-items: center;
        gap: 10px;
        margin-bottom: 20px;
        padding-bottom: 15px;
        border-bottom: 2px solid var(--primary);
    }

    .form-group {
        display: flex;
        flex-direction: column;
        gap: 8px;
        margin-bottom: 18px;
    }

        .form-group label {
            font-size: 14px;
            font-weight: 600;
            color: var(--text-light);
        }

    .required-badge {
        color: #ef4444;
        font-weight: 700;
    }

    /* Duration Input Group */
    .duration-input-group {
        display: grid;
        grid-template-columns: 1fr auto 1fr;
        gap: 12px;
        align-items: flex-end;
    }

    .duration-input-item {
        display: flex;
        flex-direction: column;
        gap: 6px;
    }

    .duration-label {
        font-size: 12px;
        font-weight: 600;
        color: var(--text-light-secondary);
        text-transform: uppercase;
        letter-spacing: 0.5px;
    }

    .duration-input {
        padding: 12px 14px;
        border: 1px solid var(--border-light);
        border-radius: 8px;
        background-color: var(--bg-light-secondary);
        color: var(--text-light);
        font-size: 14px;
        font-weight: 600;
        transition: border-color 0.2s ease, box-shadow 0.2s ease;
    }

        .duration-input:focus {
            outline: none;
            border-color: var(--primary);
            box-shadow: 0 0 0 3px rgba(59, 130, 246, 0.1);
        }

    .duration-input-separator {
        display: flex;
        align-items: center;
        justify-content: center;
        height: 40px;
    }

    .separator-text {
        font-size: 18px;
        font-weight: 700;
        color: var(--primary);
    }

    .pricing-select {
        padding: 12px 14px;
        border: 1px solid var(--border-light);
        border-radius: 8px;
        background-color: var(--bg-light-secondary);
        color: var(--text-light);
        font-size: 14px;
        transition: border-color 0.2s ease, box-shadow 0.2s ease;
    }

        .pricing-select:focus {
            outline: none;
            border-color: var(--primary);
            box-shadow: 0 0 0 3px rgba(59, 130, 246, 0.1);
        }

    .input-group-with-helper {
        display: flex;
        flex-direction: column;
        gap: 6px;
    }

    .helper-text {
        font-size: 12px;
        color: var(--text-light-secondary);
    }

    .value-highlight {
        font-weight: 600;
        color: var(--primary);
    }

    .price-input-wrapper {
        display: flex;
        align-items: center;
        position: relative;
    }

    .currency-symbol {
        position: absolute;
        left: 14px;
        font-size: 16px;
        font-weight: 600;
        color: var(--text-light-secondary);
    }

    .price-input-wrapper .pricing-input {
        padding-left: 36px;
    }

    .pricing-input {
        padding: 12px 14px;
        border: 1px solid var(--border-light);
        border-radius: 8px;
        background-color: var(--bg-light-secondary);
        color: var(--text-light);
        font-size: 14px;
        transition: border-color 0.2s ease, box-shadow 0.2s ease;
    }

        .pricing-input:focus {
            outline: none;
            border-color: var(--primary);
            box-shadow: 0 0 0 3px rgba(59, 130, 246, 0.1);
        }

    /* Form Actions */
    .form-actions {
        display: flex;
        gap: 12px;
        margin-top: 24px;
        padding-top: 20px;
        border-top: 1px solid var(--border-light);
    }

        .form-actions .btn {
            flex: 1;
            padding: 12px 16px;
            border-radius: 8px;
            font-weight: 600;
            cursor: pointer;
            display: flex;
            align-items: center;
            justify-content: center;
            gap: 8px;
            transition: all 0.2s ease;
        }

        .form-actions .btn-primary {
            background-color: var(--primary);
            color: white;
            border: none;
        }

            .form-actions .btn-primary:hover {
                background-color: var(--primary-dark);
                box-shadow: 0 4px 12px rgba(59, 130, 246, 0.3);
            }

        .form-actions .btn-secondary {
            background-color: var(--bg-light-tertiary);
            color: var(--text-light);
            border: 1px solid var(--border-light);
        }

            .form-actions .btn-secondary:hover {
                background-color: var(--border-light);
            }

    .btn-lg {
        padding: 14px 24px !important;
        font-size: 15px !important;
    }

    /* Quick Reference */
    .quick-reference {
        background: linear-gradient(135deg, var(--primary), var(--secondary));
        color: white;
        border-radius: 12px;
        padding: 20px;
        font-size: 14px;
    }

        .quick-reference h3 {
            margin: 0 0 12px 0;
            font-size: 15px;
            font-weight: 700;
        }

        .quick-reference p {
            margin: 0 0 12px 0;
            line-height: 1.5;
        }

    .reference-example {
        background-color: rgba(255, 255, 255, 0.15);
        border-radius: 8px;
        padding: 12px;
        margin-top: 12px;
    }

        .reference-example strong {
            display: block;
            margin-bottom: 8px;
        }

        .reference-example ul {
            list-style: none;
            padding: 0;
            margin: 0 0 8px 0;
        }

        .reference-example li {
            padding: 4px 0;
            font-size: 13px;
        }

        .reference-example em {
            font-style: italic;
            opacity: 0.9;
            display: block;
            margin-top: 8px;
            font-size: 12px;
        }

    /* Grid Panel */
    .pricing-grid-panel {
        display: flex;
        flex-direction: column;
    }

    .grid-card {
        background-color: var(--bg-light-secondary);
        border: 1px solid var(--border-light);
        border-radius: 12px;
        padding: 24px;
        display: flex;
        flex-direction: column;
        gap: 16px;
        box-shadow: 0 2px 4px rgba(0, 0, 0, 0.05);
    }

    .grid-header {
        display: flex;
        justify-content: space-between;
        align-items: center;
        padding-bottom: 16px;
        border-bottom: 2px solid var(--primary);
    }

    .grid-title {
        font-size: 16px;
        font-weight: 700;
        color: var(--text-light);
        display: flex;
        align-items: center;
        gap: 10px;
        margin: 0;
    }

    .grid-count {
        font-size: 13px;
        color: var(--text-light-secondary);
        background-color: var(--bg-light-tertiary);
        padding: 6px 12px;
        border-radius: 20px;
    }

    /* Pricing Table Customizations */
    .pricing-grid-container {
        min-height: auto;
        max-height: 550px;
    }

    .pricing-table th {
        background-color: var(--bg-light-tertiary);
        color: var(--text-light-secondary);
        font-weight: 700;
        text-transform: uppercase;
        font-size: 12px;
        letter-spacing: 0.5px;
    }

    .pricing-table td {
        padding: 16px;
        vertical-align: middle;
    }

    .tier-badge {
        display: inline-block;
        background-color: var(--primary);
        color: white;
        padding: 6px 12px;
        border-radius: 20px;
        font-size: 12px;
        font-weight: 600;
    }

    .duration-value {
        font-weight: 600;
        color: var(--text-light);
        display: block;
        font-size: 15px;
    }

    .duration-minutes {
        font-size: 11px;
        color: var(--text-light-secondary);
    }

    .price-value {
        font-size: 16px;
        font-weight: 700;
        color: var(--primary);
    }

    .cell-actions {
        text-align: center;
    }

    .action-buttons {
        display: flex;
        gap: 8px;
        justify-content: center;
    }

    .btn-action {
        display: inline-flex;
        align-items: center;
        justify-content: center;
        width: 36px;
        height: 36px;
        border-radius: 8px;
        border: none;
        cursor: pointer;
        font-size: 14px;
        transition: all 0.2s ease;
    }

    .btn-edit {
        background-color: rgba(59, 130, 246, 0.1);
        color: var(--primary);
    }

        .btn-edit:hover {
            background-color: var(--primary);
            color: white;
        }

    .btn-delete {
        background-color: rgba(239, 68, 68, 0.1);
        color: #ef4444;
    }

        .btn-delete:hover {
            background-color: #ef4444;
            color: white;
        }

    /* Empty State */
    .empty-state {
        display: flex;
        flex-direction: column;
        align-items: center;
        justify-content: center;
        padding: 60px 20px;
        color: var(--text-light-secondary);
        text-align: center;
    }

        .empty-state i {
            font-size: 48px;
            color: var(--border-light);
            margin-bottom: 16px;
        }

        .empty-state p {
            margin: 0;
            font-size: 16px;
            font-weight: 600;
        }

        .empty-state small {
            display: block;
            margin-top: 8px;
        }

    /* Responsive Design */
    @media (max-width: 1200px) {
        .pricing-container {
            grid-template-columns: 1fr;
        }

        .pricing-grid-container {
            max-height: none;
        }
    }

    @media (max-width: 768px) {
        .pricing-settings-wrapper {
            padding: 12px;
            gap: 16px;
        }

        .pricing-title {
            font-size: 22px;
        }

        .pricing-container {
            gap: 16px;
        }

        .form-card,
        .grid-card {
            padding: 16px;
        }

        .form-actions {
            flex-direction: column;
        }

        .grid-header {
            flex-direction: column;
            align-items: flex-start;
            gap: 12px;
        }

        .pricing-table {
            font-size: 12px;
        }

            .pricing-table td {
                padding: 12px 8px;
            }

        .duration-input-group {
            grid-template-columns: 1fr;
        }

        .duration-input-separator {
            display: none;
        }
    }
</style>

<!-- JavaScript for Duration Conversion -->
<script>
    document.addEventListener('DOMContentLoaded', function () {
        const hoursInput = document.querySelector('[id$="TextBox_Hours"]');
        const minutesInput = document.querySelector('[id$="TextBox_Minutes_Only"]');
        const priceInput = document.querySelector('[id$="TextBox_Price"]');
        const totalDurationDisplay = document.getElementById('TotalDurationDisplay');
        const totalMinutesValue = document.getElementById('TotalMinutesValue');
        const priceDisplay = document.getElementById('PriceDisplay');

        // Function to calculate and display total duration
        function updateDurationDisplay() {
            const hours = parseInt(hoursInput.value) || 0;
            const minutes = parseInt(minutesInput.value) || 0;

            // Validate minutes
            if (minutes > 59) {
                minutesInput.value = 59;
                return;
            }

            // Calculate total minutes
            const totalMinutes = (hours * 60) + minutes;

            // Update displays
            const displayText = formatDurationDisplay(totalMinutes);
            totalDurationDisplay.textContent = displayText;
            totalMinutesValue.textContent = totalMinutes;

            // Store in hidden field for server-side use
            const hiddenField = document.querySelector('[id$="HiddenField_TotalMinutes"]');
            if (hiddenField) {
                hiddenField.value = totalMinutes;
            }
        }

        // Function to format duration for display
        function formatDurationDisplay(totalMinutes) {
            if (totalMinutes <= 0) return "0 mins";

            const hrs = Math.floor(totalMinutes / 60);
            const mins = totalMinutes % 60;

            if (hrs > 0 && mins > 0) {
                return `${hrs} hr${hrs > 1 ? 's' : ''} ${mins} min${mins > 1 ? 's' : ''}`;
            } else if (hrs > 0) {
                return `${hrs} hr${hrs > 1 ? 's' : ''}`;
            } else {
                return `${mins} min${mins > 1 ? 's' : ''}`;
            }
        }

        // Update price display
        if (priceInput) {
            priceInput.addEventListener('input', function () {
                const value = parseFloat(this.value) || 0;
                priceDisplay.textContent = '₱ ' + value.toFixed(2);
            });
        }

        // Add event listeners for duration inputs
        if (hoursInput && minutesInput) {
            hoursInput.addEventListener('input', updateDurationDisplay);
            minutesInput.addEventListener('input', updateDurationDisplay);
            hoursInput.addEventListener('change', updateDurationDisplay);
            minutesInput.addEventListener('change', updateDurationDisplay);

            // Initialize display on page load
            updateDurationDisplay();
        }
    });

    function ValidatePricing() {
        var hoursInput = document.querySelector('[id$="TextBox_Hours"]');
        var minutesInput = document.querySelector('[id$="TextBox_Minutes_Only"]');
        var price = document.querySelector('[id$="TextBox_Price"]');
        var customerType = document.querySelector('[id$="DropDownList_CustomerType"]');

        if (!customerType || !customerType.value) {
            alert('Please select a customer tier.');
            return false;
        }

        var hours = parseInt(hoursInput.value) || 0;
        var minutes = parseInt(minutesInput.value) || 0;
        var totalMinutes = (hours * 60) + minutes;

        if (totalMinutes <= 0) {
            alert('Please enter a valid duration (minimum 1 minute).');
            return false;
        }

        if (minutes > 59) {
            alert('Minutes must be between 0 and 59.');
            return false;
        }

        if (!price || !price.value || parseFloat(price.value) < 0) {
            alert('Please enter a valid price.');
            return false;
        }

        return true;
    }
</script>
