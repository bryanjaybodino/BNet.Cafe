<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="Users.ascx.cs" Inherits="BNet.Cafe.Server.Forms.Users" %>
<asp:UpdatePanel ID="UpdatePanel1" runat="server">
    <ContentTemplate>
        <asp:LinkButton ID="LinkButton_Refresh" OnClick="LinkButton_Refresh_Click" runat="server"></asp:LinkButton>

        <div class="bnet-table-wrapper">
            <div class="bnet-table-toolbar">
                <div class="search-box">
                    <asp:TextBox ID="TextBox_Search" runat="server" CssClass="search-input" AutoPostBack="True" OnTextChanged="TextBox_Search_TextChanged" placeholder="Search user name or email..." />
                </div>
                <div style="display: flex; gap: 8px;">
                    <asp:HyperLink ID="HyperLink_Add" NavigateUrl="~/BNetPage.aspx?Form=UserCreate" CssClass="btn btn-primary" runat="server">
                        + Add User 
                    </asp:HyperLink>
                </div>
            </div>

            <!-- Scrollable container wrapper -->
            <div class="bnet-table-container">
                <asp:GridView ID="GridViewTable" runat="server" ShowHeaderWhenEmpty="true"
                    AutoGenerateColumns="False" CssClass="bnet-table" GridLines="None"
                    AllowPaging="True" OnPageIndexChanging="GridViewTable_PageIndexChanging">

                    <PagerSettings Mode="NumericFirstLast" FirstPageText="&laquo;" LastPageText="&raquo;" PreviousPageText="&lsaquo;" NextPageText="&rsaquo;" PageButtonCount="5" />
                    <PagerStyle CssClass="bnet-pagination" HorizontalAlign="Center" />

                    <Columns>
                        <asp:TemplateField HeaderText="Action" ItemStyle-Width="120px">
                            <ItemTemplate>
                                <!-- Pure CSS Click-Triggered Dropdown -->
                                <div class="bnet-dropdown" tabindex="0">
                                    <button type="button" class="bnet-dropdown-toggle">
                                        Command <i class="fa-solid fa-chevron-down"></i>
                                    </button>
                                    <div class="bnet-dropdown-menu">
                                        <asp:Panel ID="Panel_TopUp" Visible="false" runat="server">
                                            <a onclick="navigateTo('BNetPage.aspx?Form=UserTopUp&id=<%# Eval("DBId") %>')" class="bnet-dropdown-item">
                                                <i class="fa-regular fa-credit-card"></i>Top-up
                                            </a>
                                            <a onclick="navigateTo('BNetPage.aspx?Form=UserBonus&id=<%# Eval("DBId") %>')" class="bnet-dropdown-item">
                                                <i class="fa-regular fa-plus-square"></i>Bonus
                                            </a>
                                        </asp:Panel>
                                        <a onclick="navigateTo('BNetPage.aspx?Form=UserEdit&id=<%# Eval("DBId") %>')" class="bnet-dropdown-item">
                                            <i class="fa-solid fa-pen-to-square"></i>Edit User
                                        </a>
                                    </div>
                                </div>
                            </ItemTemplate>
                        </asp:TemplateField>
                        <asp:TemplateField HeaderText="Id" ItemStyle-Width="180px">
                            <ItemTemplate>
                                <asp:Label ID="Label_DBId" runat="server" Text='<%# Eval("DBId") %>'></asp:Label>
                            </ItemTemplate>
                        </asp:TemplateField>

                        <asp:TemplateField HeaderText="Name" ItemStyle-Width="200px">
                            <ItemTemplate>
                                <asp:Label ID="Label_DBName" runat="server" Text='<%# Eval("DBName") %>'></asp:Label>
                            </ItemTemplate>
                        </asp:TemplateField>

                        <asp:TemplateField HeaderText="Email" ItemStyle-Width="220px">
                            <ItemTemplate>
                                <asp:Label ID="Label_DBEmail" runat="server" Text='<%# Eval("DBEmail") %>'></asp:Label>
                            </ItemTemplate>
                        </asp:TemplateField>

                        <asp:TemplateField HeaderText="Role" ItemStyle-Width="150px">
                            <ItemTemplate>
                                <asp:Label ID="Label_DBRole" runat="server" Text='<%# Eval("DBRole") %>'></asp:Label>
                            </ItemTemplate>
                        </asp:TemplateField>

                        <asp:TemplateField HeaderText="Balance" ItemStyle-Width="150px">
                            <ItemTemplate>
                                <asp:Label ID="Label_DBFormattedTotalDuration" runat="server" Text='<%# Eval("DBFormattedTotalDuration") %>'></asp:Label>
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
