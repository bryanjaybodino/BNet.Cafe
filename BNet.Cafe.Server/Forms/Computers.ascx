<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="Computers.ascx.cs" Inherits="BNet.Cafe.Server.Forms.Computers" %>
<%@ Register Src="~/Forms/Modals/ComputerDelete.ascx" TagPrefix="uc1" TagName="ComputerDelete" %>
<%@ Register Src="~/Forms/Modals/RemoteLogout.ascx" TagPrefix="uc1" TagName="RemoteLogout" %>

<asp:ScriptManagerProxy ID="ScriptManagerProxy1" runat="server">
    <Scripts>
        <asp:ScriptReference Path="~/Assets/Pages/RemoteMessaging.js" />
        <asp:ScriptReference Path="~/Assets/Pages/Computers.js" />
    </Scripts>
</asp:ScriptManagerProxy>

<asp:UpdatePanel ID="UpdatePanel1" runat="server">
    <ContentTemplate>
        <asp:LinkButton ID="LinkButton_Refresh" runat="server"></asp:LinkButton>
        <div class="bnet-table-wrapper">
            <div class="bnet-table-toolbar">
                <div class="search-box">
                    <asp:TextBox ID="TextBox_Search" runat="server" CssClass="search-input" placeholder="Search computers..." />
                </div>
                <asp:HyperLink ID="HyperLink_Add" NavigateUrl="~/BNetPage.aspx?Form=ComputerCreate" CssClass="btn btn-primary" runat="server">
                     + Add Computer 
                </asp:HyperLink>
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
                                        <asp:Panel ID="Panel_ManageRental" Visible="false" runat="server">
                                            <a onclick="navigateTo('BNetPage.aspx?Form=RentalManage&id=<%# Eval("DBId") %>')" class="bnet-dropdown-item">
                                                <i class="fa-regular fa-clock"></i>Manage Rental Session
                                            </a>
                                        </asp:Panel>
                                        <asp:Panel ID="Panel_Logout" Visible="false" runat="server">
                                            <a class="bnet-dropdown-item"
                                                onclick="openLogoutModal(this, '<%# Eval("DBId") %>', '<%# Eval("DBComputerName") %>')">
                                                <i class="fa-solid fa-right-from-bracket"></i>Log Out
                                            </a>
                                        </asp:Panel>
                                        <a class="bnet-dropdown-item">
                                            <i class="fa-solid fa-clock-rotate-left"></i>Session History
                                        </a>
                                        <a href='<%# "BNetPage.aspx?Form=ComputerEdit&id=" + Eval("DBId") %>' class="bnet-dropdown-item">
                                            <i class="fa-solid fa-pen-to-square"></i>Edit Computer
                                        </a>
                                        <div class="bnet-dropdown-divider"></div>
                                        <span class="bnet-dropdown-item text-danger" onclick="openDeleteModal('<%# Eval("DBId") %>', '<%# Eval("DBComputerName") %>')">
                                            <i class="fa-solid fa-trash-can"></i>Delete Computer
                                        </span>
                                    </div>
                                </div>
                            </ItemTemplate>
                        </asp:TemplateField>
                        <asp:TemplateField HeaderText="Computer Name" ItemStyle-Width="200px">
                            <ItemTemplate>
                                <asp:Label ID="Label_DBComputerName" runat="server" Text='<%#Eval("DBComputerName") %>'></asp:Label>
                            </ItemTemplate>
                        </asp:TemplateField>
                        <asp:TemplateField HeaderText="Time Start" ItemStyle-Width="200px">
                            <ItemTemplate>
                                <asp:Label ID="Label_TimeStart" runat="server" Text=""></asp:Label>
                            </ItemTemplate>
                        </asp:TemplateField>
                        <asp:TemplateField HeaderText="Time End" ItemStyle-Width="200px">
                            <ItemTemplate>
                                <asp:Label ID="Label_TimeEnd" runat="server" Text=""></asp:Label>
                            </ItemTemplate>
                        </asp:TemplateField>
                        <asp:TemplateField HeaderText="Total Hours" ItemStyle-Width="200px">
                            <ItemTemplate>
                                <asp:Label ID="Label_TotalHours" runat="server" Text=""></asp:Label>
                            </ItemTemplate>
                        </asp:TemplateField>
                        <asp:TemplateField HeaderText="Status" ItemStyle-Width="200px">
                            <ItemTemplate>
                                <asp:Label ID="Label_Status" runat="server" Text=""></asp:Label>
                            </ItemTemplate>
                        </asp:TemplateField>
                        <asp:TemplateField HeaderText="Billing" ItemStyle-Width="200px">
                            <ItemTemplate>
                                <asp:Label ID="Label_Billing" runat="server" Text=""></asp:Label>
                            </ItemTemplate>
                        </asp:TemplateField>
                    </Columns>
                </asp:GridView>
            </div>
            <asp:Panel ID="Panel_Pagination" runat="server" />
        </div>
        <uc1:ComputerDelete runat="server" ID="ComputerDelete" />
        <uc1:RemoteLogout runat="server" ID="RemoteLogout" />
    </ContentTemplate>
</asp:UpdatePanel>
