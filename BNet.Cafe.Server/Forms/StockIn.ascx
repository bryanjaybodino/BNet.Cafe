<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="StockIn.ascx.cs" Inherits="BNet.Cafe.Server.Forms.StockIn" %>
<%@ Register Src="~/Forms/Modals/StockInDelete.ascx" TagPrefix="uc" TagName="StockInDelete" %>

<asp:UpdatePanel ID="UpdatePanel1" runat="server" UpdateMode="Conditional">
    <ContentTemplate>
        <div class="pricing-settings-wrapper" style="width: 100%;">
            <div class="pricing-container" style="display: flex; flex-direction: column; width: 100%; gap: 20px;">
                <!-- Inline Top Restock Form Panel -->
                <div class="pricing-form-panel" style="width: 100%;">
                    <div class="form-card">
                        <h2 class="form-section-title" style="padding-bottom: 15px;">
                            <i class="fa-solid fa-boxes-packing"></i>Stock In / Restock
                        </h2>

                        <!-- Responsive Form Container -->
                        <div class="stockin-form-grid">
                            <!-- Row 1, Column 1: Select Item -->
                            <div class="form-group">
                                <label for="DropDownList_Item">
                                    Inventory Item <span class="required-badge">*</span>
                                </label>
                                <asp:DropDownList ID="DropDownList_Item" runat="server" CssClass="form-control bnet-select">
                                </asp:DropDownList>
                            </div>

                            <!-- Row 1, Column 2: Restock Quantity -->
                            <div class="form-group">
                                <label for="TextBox_Quantity">
                                    Restock Quantity <span class="required-badge">*</span>
                                </label>
                                <asp:TextBox ID="TextBox_Quantity" runat="server" TextMode="Number"
                                    CssClass="form-control pricing-input" placeholder="0" min="1"></asp:TextBox>
                            </div>

                            <!-- Row 2 (Spans 2 Columns): Action Buttons -->
                            <div class="form-actions">
                                <asp:LinkButton ID="LinkButton_Cancel" OnClick="LinkButton_Cancel_Click"
                                    CssClass="btn btn-secondary btn-responsive" runat="server">
                                    <i class="fa fa-times-circle"></i> Clear
                                </asp:LinkButton>
                                <asp:LinkButton ID="LinkButton_Submit" OnClick="LinkButton_Submit_Click"
                                     CssClass="btn btn-primary btn-responsive" OnClientClick="return ValidateStockIn();" runat="server">
                                    <i class="fa fa-arrow-down"></i> Process Stock In
                                </asp:LinkButton>
                            </div>
                        </div>
                    </div>
                </div>

                <!-- 100% Width Grid Panel with Pagination Template -->
                <div class="pricing-grid-panel" style="width: 100%;">
                    <div class="grid-card">
                        <div class="grid-header">
                            <h2 class="grid-title"><i class="fa-solid fa-list"></i>Recent Stock In Logs</h2>
                        </div>

                        <div class="bnet-table-wrapper" style="width: 100%;">
                            <div class="bnet-table-container pricing-grid-container" style="width: 100%;">
                                <asp:GridView ID="GridView_StockIn" runat="server" AutoGenerateColumns="False"
                                    GridLines="None" CssClass="bnet-table pricing-table" Style="width: 100%;"
                                    AllowPaging="True" OnPageIndexChanging="GridView_StockIn_PageIndexChanging">

                                    <PagerSettings Mode="NumericFirstLast" FirstPageText="&laquo;" LastPageText="&raquo;" PreviousPageText="&lsaquo;" NextPageText="&rsaquo;" PageButtonCount="5" />
                                    <PagerStyle CssClass="bnet-pagination" HorizontalAlign="Center" />

                                    <Columns>
                                        <asp:BoundField DataField="DBItemId" HeaderText="Item ID" />
                                        <asp:BoundField DataField="DBItemName" HeaderText="Item Name" />
                                        <asp:BoundField DataField="DBUserName" HeaderText="Processed By" />
                                        <asp:BoundField DataField="DBQuantity" HeaderText="Quantity Added" />
                                        <asp:BoundField DataField="DBDateCreated" HeaderText="Date" />
                                        <asp:BoundField DataField="DBTimeCreated" HeaderText="Time" />
                                        <asp:TemplateField HeaderText="Actions">
                                            <ItemTemplate>
                                                <button type="button" class="btn btn-sm btn-danger" 
                                                    onclick='openStockInDeleteModal("<%# Eval("DBId") %>", "<%# Eval("DBItemName") %>")'>
                                                    <i class="fa-solid fa-trash-can"></i>
                                                </button>
                                            </ItemTemplate>
                                        </asp:TemplateField>
                                    </Columns>
                                </asp:GridView>
                            </div>
                            <asp:Panel ID="Panel_Pagination" runat="server" />
                        </div>
                    </div>
                </div>
            </div>
        </div>

        <!-- Stock In Delete Modal -->
        <uc:StockInDelete ID="StockInDeleteModal" runat="server" />
    </ContentTemplate>
</asp:UpdatePanel>