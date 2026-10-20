<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="Points.ascx.cs" Inherits="BNet.Cafe.Server.Forms.Points" %>
<asp:UpdatePanel ID="UpdatePanel1" runat="server">
    <ContentTemplate>
        <asp:LinkButton ID="LinkButton_Refresh" runat="server"></asp:LinkButton>
        <div class="cards-grid cards-grid-compact">
            <!-- Total Transactions Card -->
            <div class="card card-compact">
                <div class="card-inline-content">
                    <div class="card-icon-sm">
                        <i class="fa-solid fa-star"></i>
                    </div>
                    <div class="card-details">
                        <span class="card-title-sm">Total Logs</span>
                        <span class="card-value-sm">
                            <asp:Label ID="Label_TotalPointsLogs" runat="server" Text="0"></asp:Label>
                        </span>
                    </div>
                </div>
                <span class="positive-sm">Overall</span>
            </div>

            <!-- Users Points Card -->
            <div class="card card-compact">
                <div class="card-inline-content">
                    <div class="card-icon-sm">
                        <i class="fa-solid fa-users"></i>
                    </div>
                    <div class="card-details">
                        <span class="card-title-sm">Points Users</span>
                        <span class="card-value-sm">
                            <asp:Label ID="Label_Users" runat="server" Text="0"></asp:Label>
                        </span>
                    </div>
                </div>
                <span class="bnet-badge-status green">Members</span>
            </div>

            <!-- Total Points Earned Card -->
            <div class="card card-compact">
                <div class="card-inline-content">
                    <div class="card-icon-sm">
                        <i class="fa-solid fa-award"></i>
                    </div>
                    <div class="card-details">
                        <span class="card-title-sm">Total Points</span>
                        <span class="card-value-sm">
                            <asp:Label ID="Label_TotalPoints" runat="server" Text="0.00"></asp:Label>
                        </span>
                    </div>
                </div>
                <span class="bnet-badge-status yellow">Accumulated</span>
            </div>
        </div>

        <div class="bnet-table-wrapper">
            <div class="bnet-table-toolbar">
                <div class="search-box">
                    <asp:TextBox ID="TextBox_Search" runat="server" CssClass="search-input" AutoPostBack="True" OnTextChanged="TextBox_Search_TextChanged" placeholder="Search customer or email..." />
                </div>
                <asp:TextBox ID="TextBox_DateRage" AutoPostBack="true" CssClass="bnet-datepicker" data-mode="range" runat="server"></asp:TextBox>
            </div>

            <div class="bnet-table-container">
                <asp:GridView ID="GridViewTable" runat="server" ShowHeaderWhenEmpty="true"
                    AutoGenerateColumns="False" CssClass="bnet-table" GridLines="None"
                    AllowPaging="True" OnPageIndexChanging="GridViewTable_PageIndexChanging">

                    <PagerSettings Mode="NumericFirstLast" FirstPageText="&laquo;" LastPageText="&raquo;" PreviousPageText="&lsaquo;" NextPageText="&rsaquo;" PageButtonCount="5" />
                    <PagerStyle CssClass="bnet-pagination" HorizontalAlign="Center" />

                    <Columns>
                        <asp:TemplateField HeaderText="Id" ItemStyle-Width="180px">
                            <ItemTemplate>
                                <asp:Label ID="Label_DBId" runat="server" Text='<%# Eval("DBId") %>'></asp:Label>
                            </ItemTemplate>
                        </asp:TemplateField>

                        <asp:TemplateField HeaderText="Customer" ItemStyle-Width="200px">
                            <ItemTemplate>
                                <asp:Label ID="Label_DBName" runat="server" Text='<%# Eval("DBName") %>'></asp:Label>
                            </ItemTemplate>
                        </asp:TemplateField>

                        <asp:TemplateField HeaderText="Email" ItemStyle-Width="200px">
                            <ItemTemplate>
                                <asp:Label ID="Label_DBEmail" runat="server" Text='<%# Eval("DBEmail") %>'></asp:Label>
                            </ItemTemplate>
                        </asp:TemplateField>

                        <asp:TemplateField HeaderText="Description" ItemStyle-Width="180px">
                            <ItemTemplate>
                                <asp:Label ID="Label_DBDescription" runat="server" Text='<%# Eval("DBDescription") %>'></asp:Label>
                            </ItemTemplate>
                        </asp:TemplateField>

                        <asp:TemplateField HeaderText="Points" ItemStyle-Width="150px">
                            <ItemTemplate>
                                <asp:Label ID="Label_DBPoints" runat="server" Text='<%# Eval("DBPoints") %>'></asp:Label>
                            </ItemTemplate>
                        </asp:TemplateField>

                        <asp:TemplateField HeaderText="Date & Time" ItemStyle-Width="220px">
                            <ItemTemplate>
                                <asp:Label ID="Label_DBDateCreated_DBTimeCreated" runat="server" Text='<%# string.Format("{0} - {1}", Eval("DBDateCreated"), Eval("DBTimeCreated")) %>'></asp:Label>
                            </ItemTemplate>
                        </asp:TemplateField>
                    </Columns>
                </asp:GridView>
            </div>
            <asp:Panel ID="Panel_Pagination" runat="server" />
        </div>
    </ContentTemplate>
</asp:UpdatePanel>