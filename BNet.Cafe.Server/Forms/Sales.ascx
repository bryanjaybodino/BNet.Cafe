<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="Sales.ascx.cs" Inherits="BNet.Cafe.Server.Forms.Sales" %>
<%@ Register Src="~/Forms/Modals/SalesVoid.ascx" TagPrefix="uc1" TagName="SalesVoid" %>

<asp:UpdatePanel ID="UpdatePanel1" runat="server">
    <ContentTemplate>
        <asp:LinkButton ID="LinkButton_Refresh" runat="server"></asp:LinkButton>
        <div class="cards-grid cards-grid-compact">
            <!-- Total Orders -->
            <div class="card card-compact">
                <div class="card-inline-content">
                    <div class="card-icon-sm">
                        <i class="fa-solid fa-receipt"></i>
                    </div>
                    <div class="card-details">
                        <span class="card-title-sm">Total Sales</span>
                        <span class="card-value-sm">
                            <asp:Label ID="Label_TotalOrders" runat="server" Text="0"></asp:Label>
                        </span>
                    </div>
                </div>
                <span class="positive-sm">Overall</span>
            </div>

            <!-- Total Quantity Sold -->
            <div class="card card-compact">
                <div class="card-inline-content">
                    <div class="card-icon-sm">
                        <i class="fa-solid fa-boxes-stacked"></i>
                    </div>
                    <div class="card-details">
                        <span class="card-title-sm">Items Sold</span>
                        <span class="card-value-sm">
                            <asp:Label ID="Label_ItemsSold" runat="server" Text="0"></asp:Label>
                        </span>
                    </div>
                </div>
                <span class="bnet-badge-status green">Quantity</span>
            </div>

            <!-- Average Order Value -->
            <div class="card card-compact">
                <div class="card-inline-content">
                    <div class="card-icon-sm">
                        <i class="fa-solid fa-chart-line"></i>
                    </div>
                    <div class="card-details">
                        <span class="card-title-sm">Avg. Sale</span>
                        <span class="card-value-sm">
                            <asp:Label ID="Label_AvgOrderValue" runat="server" Text="₱0.00"></asp:Label>
                        </span>
                    </div>
                </div>
                <span class="bnet-badge-status gray">Average</span>
            </div>

            <!-- Revenue -->
            <div class="card card-compact">
                <div class="card-inline-content">
                    <div class="card-icon-sm">
                        <i class="fa-solid fa-peso-sign"></i>
                    </div>
                    <div class="card-details">
                        <span class="card-title-sm">Total Revenue</span>
                        <span class="card-value-sm">
                            <asp:Label ID="Label_Revenue" runat="server" Text="₱0.00"></asp:Label>
                        </span>
                    </div>
                </div>
                <span class="bnet-badge-status yellow">Revenue</span>
            </div>
        </div>

        <div class="bnet-table-wrapper">
            <div class="bnet-table-toolbar">
                <div class="search-box">
                    <asp:TextBox ID="TextBox_Search" runat="server" CssClass="search-input" AutoPostBack="True" OnTextChanged="TextBox_Search_TextChanged" placeholder="Search product" />
                </div>
                <asp:TextBox ID="TextBox_DateRange" AutoPostBack="true" CssClass="bnet-datepicker" data-mode="range" runat="server"></asp:TextBox>
            </div>

            <div class="bnet-table-container">
                <asp:GridView ID="GridViewTable" runat="server" ShowHeaderWhenEmpty="true"
                    AutoGenerateColumns="False" CssClass="bnet-table" GridLines="None"
                    AllowPaging="True" OnPageIndexChanging="GridViewTable_PageIndexChanging">

                    <PagerSettings Mode="NumericFirstLast" FirstPageText="&laquo;" LastPageText="&raquo;" PreviousPageText="&lsaquo;" NextPageText="&rsaquo;" PageButtonCount="5" />
                    <PagerStyle CssClass="bnet-pagination" HorizontalAlign="Center" />

                    <Columns>
                        <asp:TemplateField HeaderText="Action" ItemStyle-Width="100px">
                            <ItemTemplate>
                                <button type="button" class="btn btn-sm btn-danger"
                                    onclick='openVoidModal("<%# Eval("DBId") %>", "<%# Eval("DBItemName") %>")'>
                                    <i class="fa-solid fa-ban"></i>Void 
                                </button>
                            </ItemTemplate>
                        </asp:TemplateField>
                        <asp:TemplateField HeaderText="Trans ID" ItemStyle-Width="120px">
                            <ItemTemplate>
                                <asp:Label ID="Label_DBId" runat="server" Text='<%# Eval("DBId") %>'></asp:Label>
                            </ItemTemplate>
                        </asp:TemplateField>

                        <asp:TemplateField HeaderText="Item Name" ItemStyle-Width="200px">
                            <ItemTemplate>
                                <asp:Label ID="Label_DBItemName" runat="server" Text='<%# Eval("DBItemName") %>'></asp:Label>
                            </ItemTemplate>
                        </asp:TemplateField>

                        <asp:TemplateField HeaderText="Processed By" ItemStyle-Width="180px">
                            <ItemTemplate>
                                <asp:Label ID="Label_DBUserName" runat="server" Text='<%# Eval("DBUserName") %>'></asp:Label>
                            </ItemTemplate>
                        </asp:TemplateField>

                        <asp:TemplateField HeaderText="Qty" ItemStyle-Width="100px">
                            <ItemTemplate>
                                <asp:Label ID="Label_DBQuantity" runat="server" Text='<%# Eval("DBQuantity") %>'></asp:Label>
                            </ItemTemplate>
                        </asp:TemplateField>

                        <asp:TemplateField HeaderText="Unit Price" ItemStyle-Width="120px">
                            <ItemTemplate>
                                <asp:Label ID="Label_DBCost" runat="server" Text='<%# string.Format("₱{0:N2}", Eval("DBCost")) %>'></asp:Label>
                            </ItemTemplate>
                        </asp:TemplateField>

                        <asp:TemplateField HeaderText="Total Amount" ItemStyle-Width="140px">
                            <ItemTemplate>
                                <asp:Label ID="Label_DBTotalCost" runat="server" Text='<%# string.Format("₱{0:N2}", Eval("DBTotalCost")) %>'></asp:Label>
                            </ItemTemplate>
                        </asp:TemplateField>

                        <asp:TemplateField HeaderText="Date & Time" ItemStyle-Width="200px">
                            <ItemTemplate>
                                <asp:Label ID="Label_DBDateCreated_DBTimeCreated" runat="server" Text='<%# string.Format("{0} - {1}", Eval("DBDateCreated"), Eval("DBTimeCreated")) %>'></asp:Label>
                            </ItemTemplate>
                        </asp:TemplateField>
                    </Columns>

                    <EmptyDataTemplate>
                        <div style="text-align: center; color: var(--text-light-secondary); padding: 30px 10px;">
                            <i class="fa-solid fa-inbox" style="font-size: 32px; margin-bottom: 8px; color: var(--border-light);"></i>
                            <div>No Sales Record Found</div>
                        </div>
                    </EmptyDataTemplate>
                </asp:GridView>
            </div>
            <asp:Panel ID="Panel_Pagination" runat="server" />
        </div>
        <uc1:SalesVoid runat="server" id="SalesVoid" />
    </ContentTemplate>
</asp:UpdatePanel>
