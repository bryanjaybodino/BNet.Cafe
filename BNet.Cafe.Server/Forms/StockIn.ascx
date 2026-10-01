<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="StockIn.ascx.cs" Inherits="BNet.Cafe.Server.Forms.StockIn" %>

<asp:UpdatePanel ID="UpdatePanel1" runat="server" UpdateMode="Conditional">
    <ContentTemplate>
        <div class="pricing-settings-wrapper">
            <div class="pricing-container">
                <!-- Form Panel -->
                <div class="pricing-form-panel">
                    <div class="form-card">
                        <h2 class="form-section-title" style="padding-bottom:25px">
                            <i class="fa-solid fa-boxes-packing"></i> Stock In / Restock
                        </h2>

                        <!-- Select Item -->
                        <div class="form-group">
                            <label for="DropDownList_Item">
                                Inventory Item <span class="required-badge">*</span>
                            </label>
                            <asp:DropDownList ID="DropDownList_Item" runat="server" CssClass="form-control bnet-select">
                            </asp:DropDownList>
                        </div>

                        <!-- Quantity -->
                        <div class="form-group">
                            <label for="TextBox_Quantity">
                                Restock Quantity <span class="required-badge">*</span>
                            </label>
                            <asp:TextBox ID="TextBox_Quantity" runat="server" TextMode="Number" 
                                CssClass="form-control pricing-input" placeholder="0" min="1"></asp:TextBox>
                        </div>

                        <!-- Action Buttons -->
                        <div class="form-actions">
                            <asp:LinkButton ID="LinkButton_Cancel" OnClick="LinkButton_Cancel_Click"
                                CssClass="btn btn-secondary" runat="server">
                                <i class="fa fa-times-circle"></i> Clear
                            </asp:LinkButton>
                            <asp:LinkButton ID="LinkButton_Submit" OnClick="LinkButton_Submit_Click"
                                CssClass="btn btn-primary btn-lg" OnClientClick="return ValidateStockIn();" runat="server">
                                <i class="fa fa-arrow-down"></i> Process Stock In
                            </asp:LinkButton>
                        </div>
                    </div>
                </div>

                <!-- Recent Restocks Table -->
                <div class="pricing-grid-panel">
                    <div class="grid-card">
                        <div class="grid-header">
                            <h2 class="grid-title"><i class="fa-solid fa-list"></i> Recent Stock In Logs</h2>
                        </div>
                        <div class="bnet-table-container pricing-grid-container">
                            <asp:GridView ID="GridView_StockIn" runat="server" AutoGenerateColumns="False" GridLines="None"
                                CssClass="bnet-table pricing-table">
                                <Columns>
                                    <asp:BoundField DataField="DBItemId" HeaderText="Item ID" />
                                    <asp:BoundField DataField="DBQuantity" HeaderText="Quantity Added" />
                                    <asp:BoundField DataField="DBDateCreated" HeaderText="Date" />
                                    <asp:BoundField DataField="DBTimeCreated" HeaderText="Time" />
                                </Columns>
                            </asp:GridView>
                        </div>
                    </div>
                </div>
            </div>
        </div>
    </ContentTemplate>
</asp:UpdatePanel>