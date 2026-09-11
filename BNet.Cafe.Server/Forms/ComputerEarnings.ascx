<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="ComputerEarnings.ascx.cs" Inherits="BNet.Cafe.Server.Forms.ComputerEarnings" %>
<asp:UpdatePanel ID="UpdatePanel1" runat="server">
    <ContentTemplate>
        <asp:LinkButton ID="LinkButton_Refresh" OnClick="LinkButton_Refresh_Click" runat="server"></asp:LinkButton>
        <div class="cards-grid cards-grid-compact">
            <!-- Total Transactions Card -->
            <div class="card card-compact">
                <div class="card-inline-content">
                    <div class="card-icon-sm">
                        <i class="fa-solid fa-receipt"></i>
                    </div>
                    <div class="card-details">
                        <span class="card-title-sm">Total Transactions</span>
                        <span class="card-value-sm">
                            <asp:Label ID="Label_TotalTransactions" runat="server" Text="0"></asp:Label>
                        </span>
                    </div>
                </div>
                <span class="positive-sm">Overall</span>
            </div>

            <!-- Registered Users Card -->
            <div class="card card-compact">
                <div class="card-inline-content">
                    <div class="card-icon-sm">
                        <i class="fa-solid fa-users"></i>
                    </div>
                    <div class="card-details">
                        <span class="card-title-sm">Users</span>
                        <span class="card-value-sm">
                            <asp:Label ID="Label_Users" runat="server" Text="0"></asp:Label>
                        </span>
                    </div>
                </div>
                <span class="bnet-badge-status green">Members</span>
            </div>

            <!-- Walk-In Guests Card -->
            <div class="card card-compact">
                <div class="card-inline-content">
                    <div class="card-icon-sm">
                        <i class="fa-solid fa-user-clock"></i>
                    </div>
                    <div class="card-details">
                        <span class="card-title-sm">Walk In</span>
                        <span class="card-value-sm">
                            <asp:Label ID="Label_WalkIn" runat="server" Text="0"></asp:Label>
                        </span>
                    </div>
                </div>
                <span class="bnet-badge-status gray">Guests</span>
            </div>

            <!-- Total Income Card -->
            <div class="card card-compact">
                <div class="card-inline-content">
                    <div class="card-icon-sm">
                        <i class="fa-solid fa-peso-sign"></i>
                    </div>
                    <div class="card-details">
                        <span class="card-title-sm">Income</span>
                        <span class="card-value-sm">
                            <asp:Label ID="Label_Income" runat="server" Text="₱0.00"></asp:Label>
                        </span>
                    </div>
                </div>
                <span class="bnet-badge-status yellow">Revenue</span>
            </div>
        </div>
        <div class="bnet-table-wrapper">
            <div class="bnet-table-toolbar">

            </div>

            <!-- Scrollable container wrapper -->
            <div class="bnet-table-container">
                <asp:GridView ID="GridViewTable" runat="server" ShowHeaderWhenEmpty="true"
                    AutoGenerateColumns="False" CssClass="bnet-table" GridLines="None"
                    AllowPaging="True" OnPageIndexChanging="GridViewTable_PageIndexChanging">

                    <PagerSettings Mode="NumericFirstLast" FirstPageText="&laquo;" LastPageText="&raquo;" PreviousPageText="&lsaquo;" NextPageText="&rsaquo;" PageButtonCount="5" />
                    <PagerStyle CssClass="bnet-pagination" HorizontalAlign="Center" />

                    <Columns>
                        <asp:TemplateField HeaderText="Computer" ItemStyle-Width="180px">
                            <ItemTemplate>
                                <asp:Label ID="Label_ComputerName" runat="server" Text='<%# Eval("DBComputerName") %>'></asp:Label>
                            </ItemTemplate>
                        </asp:TemplateField>

                        <asp:TemplateField HeaderText="Customer" ItemStyle-Width="200px">
                            <ItemTemplate>
                                <asp:Label ID="Label_CustomerName" runat="server" Text='<%# Eval("DBEmail") %>'></asp:Label>
                            </ItemTemplate>
                        </asp:TemplateField>

                        <asp:TemplateField HeaderText="Duration" ItemStyle-Width="150px">
                            <ItemTemplate>
                                <asp:Label ID="Label_Duration" runat="server" Text='<%# Eval("DBFormattedDuration") %>'></asp:Label>
                            </ItemTemplate>
                        </asp:TemplateField>

                        <asp:TemplateField HeaderText="Amount Paid" ItemStyle-Width="150px">
                            <ItemTemplate>
                                <asp:Label ID="Label_Amount" runat="server" Text='<%# string.Format("₱{0:N2}", Eval("DBAmount")) %>'></asp:Label>
                            </ItemTemplate>
                        </asp:TemplateField>

                        <asp:TemplateField HeaderText="Date & Time" ItemStyle-Width="220px">
                            <ItemTemplate>
                                <asp:Label ID="Label_DateTime" runat="server" Text='<%# string.Format("{0} - {1}", Eval("DBDateCreated"), Eval("DBTimeCreated")) %>'></asp:Label>
                            </ItemTemplate>
                        </asp:TemplateField>
                    </Columns>
                </asp:GridView>
            </div>
            <asp:Panel ID="Panel_Pagination" runat="server" />
        </div>
    </ContentTemplate>
</asp:UpdatePanel>
