<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="PointsSettings.ascx.cs" Inherits="BNet.Cafe.Server.Forms.PointsSettings" %>
<%@ Register Src="~/Forms/Modals/PointsSettingDelete.ascx" TagPrefix="uc1" TagName="PointsSettingDelete" %>

<asp:UpdatePanel ID="UpdatePanel1" runat="server" UpdateMode="Conditional">
    <ContentTemplate>
        <asp:HiddenField ID="HiddenField_PointId" runat="server" Value="0" />
        <asp:HiddenField ID="HiddenField_TotalMinutes" runat="server" Value="0" />

        <div class="pricing-settings-wrapper">
            <!-- Quick Reference -->
            <div class="quick-reference">
                <h3>💡 Points System Quick Reference</h3>
                <p>
                    Set point earnings at key duration intervals. Intermediate durations will be automatically calculated proportionally by the system.
                </p>
                <div class="reference-example">
                    <strong>Example Manual Inputs:</strong>
                    <ul>
                        <li>0 hours + 30 minutes → 0.50 Points</li>
                        <li>1 hour + 0 minutes → 1.00 Point</li>
                        <li>2 hours + 0 minutes → 2.00 Points</li>
                        <li>4 hours + 0 minutes → 4.00 Points</li>
                    </ul>
                    <div class="auto-calc-badge">
                        <strong>⚡ Automatic Calculation:</strong>
                        <p>30 minutes (half of 1 hour) automatically calculates to <b>0.50 Points</b></p>
                    </div>
                </div>
            </div>

            <!-- Main Content Grid -->
            <div class="pricing-container">
                <!-- Left: Form Panel -->
                <div class="pricing-form-panel">
                    <div class="form-card">
                        <h2 class="form-section-title" style="padding-bottom:25px">
                            <i class="fa-solid fa-plus-circle"></i>Add/Edit Points Rule
                        </h2>

                        <!-- Customer Type -->
                        <div class="form-group">
                            <label for="DropDownList_CustomerType">
                                Customer Tier <span class="required-badge">*</span>
                            </label>
                            <asp:DropDownList ID="DropDownList_CustomerType" AutoPostBack="true" runat="server" CssClass="form-control bnet-select">
                                <asp:ListItem Text="Member" Value="MEMBER"></asp:ListItem>
                                <asp:ListItem Text="VIP" Value="VIP"></asp:ListItem>
                            </asp:DropDownList>
                        </div>

                        <!-- Duration: Hours and Minutes Input -->
                        <div class="form-group">
                            <label>
                                Duration <span class="required-badge">*</span>
                            </label>

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

                        <!-- Points -->
                        <div class="form-group">
                            <label for="TextBox_Points">
                                Equivalent Points <span class="required-badge">*</span>
                            </label>
                            <div class="input-group-with-helper">
                                <div class="price-input-wrapper">
                                    <span class="currency-symbol"><i class="fa fa-star" style="font-size:13px; color:var(--primary);"></i></span>
                                    <asp:TextBox ID="TextBox_Points" runat="server" TextMode="Number" CssClass="form-control pricing-input"
                                        placeholder="0.00" step="0.01" min="0" style="padding-left: 36px;"></asp:TextBox>
                                </div>
                                <div class="helper-text">
                                    Points Awarded: <span id="PointsDisplay" class="value-highlight">0.00 pts</span>
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
                                CssClass="btn btn-primary btn-lg" OnClientClick="return ValidatePointsRule();" runat="server">
                                <i class="fa fa-save"></i> Save Rule
                            </asp:LinkButton>
                        </div>
                        <br /><br /><br />
                    </div>
                </div>

                <!-- Right: Data Grid Panel -->
                <div class="pricing-grid-panel">
                    <div class="grid-card">
                        <div class="grid-header">
                            <h2 class="grid-title">
                                <i class="fa-solid fa-list"></i>Existing Points Rules
                            </h2>
                            <span class="grid-count">Total: <strong id="RuleCount">
                                <asp:Label ID="Label_TotalRules" runat="server" Text="0"></asp:Label>
                            </strong>rules
                            </span>
                        </div>

                        <!-- Grid Container with scroll -->
                        <div class="bnet-table-container pricing-grid-container">
                            <asp:GridView ID="GridView_Points" runat="server" AutoGenerateColumns="False" GridLines="None"
                                CssClass="bnet-table pricing-table">
                                <Columns>
                                    <asp:TemplateField HeaderText="Tier">
                                        <HeaderStyle Width="20%" CssClass="header-cell" />
                                        <ItemStyle Width="20%" CssClass="cell-tier" />
                                        <ItemTemplate>
                                            <span class="tier-badge">
                                                <%# Eval("DBCustomerType") %>
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

                                    <asp:TemplateField HeaderText="Points">
                                        <HeaderStyle Width="25%" CssClass="header-cell" />
                                        <ItemStyle Width="25%" CssClass="cell-price" />
                                        <ItemTemplate>
                                            <span class="price-value">
                                                <%# double.Parse(Eval("DBPoints").ToString()).ToString("F2") %> pts
                                            </span>
                                        </ItemTemplate>
                                    </asp:TemplateField>

                                    <asp:TemplateField HeaderText="Actions">
                                        <HeaderStyle Width="25%" CssClass="header-cell" />
                                        <ItemStyle Width="25%" CssClass="cell-actions" />
                                        <ItemTemplate>
                                            <div class="action-buttons">
                                                <button type="button"
                                                    class="btn-action btn-edit"
                                                    title="Edit this rule"
                                                    data-id='<%# Eval("DBId") %>'
                                                    data-tier='<%# Eval("DBCustomerType") %>'
                                                    data-minutes='<%# Eval("DBMinutes") %>'
                                                    data-points='<%# Eval("DBPoints") %>'
                                                    onclick="editPointsRule(this)">
                                                    <i class="fa fa-edit"></i>
                                                </button>

                                                <button type="button"
                                                    class="btn-action btn-delete"
                                                    title="Delete this rule"
                                                    onclick="openPointsDeleteModal('<%# Eval("DBId") %>', '<%# Eval("DBCustomerType") %> - <%# FormatDuration(int.Parse(Eval("DBMinutes").ToString())) %>')">
                                                    <i class="fa fa-trash"></i>
                                                </button>
                                            </div>
                                        </ItemTemplate>
                                    </asp:TemplateField>
                                </Columns>
                                <EmptyDataTemplate>
                                    <div class="empty-state">
                                        <i class="fa-solid fa-inbox"></i>
                                        <p>No points rules defined yet.</p>
                                        <small>Create your first points earning rule to get started.</small>
                                    </div>
                                </EmptyDataTemplate>
                            </asp:GridView>
                        </div>
                    </div>
                </div>
            </div>
        </div>
        <uc1:PointsSettingDelete runat="server" ID="PointsSettingDelete" />
    </ContentTemplate>
</asp:UpdatePanel>