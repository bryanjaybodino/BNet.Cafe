<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="Computers.ascx.cs" Inherits="BNet.Cafe.Server.Forms.Computers" %>
<%@ Register Src="~/Forms/Modals/ComputerDelete.ascx" TagPrefix="uc1" TagName="ComputerDelete" %>
<%@ Register Src="~/Forms/Modals/RemoteLogout.ascx" TagPrefix="uc1" TagName="RemoteLogout" %>
<%@ Register Src="~/Forms/Modals/RemoteTimeTransfer.ascx" TagPrefix="uc1" TagName="RemoteTimeTransfer" %>
<%@ Register Src="~/Forms/Modals/RemoteTimePause.ascx" TagPrefix="uc1" TagName="RemoteTimePause" %>

<asp:UpdatePanel ID="UpdatePanel1" runat="server">
    <ContentTemplate>
        <asp:LinkButton ID="LinkButton_Refresh" runat="server"></asp:LinkButton>

        <div class="cards-grid cards-grid-compact">
            <!-- Total Computers Card -->
            <div class="card card-compact">
                <div class="card-inline-content">
                    <div class="card-icon-sm">
                        <i class="fa-solid fa-desktop"></i>
                    </div>
                    <div class="card-details">
                        <span class="card-title-sm">Total Computers</span>
                        <span class="card-value-sm">
                            <asp:Label ID="Label_Total" runat="server" Text="0"></asp:Label>
                        </span>
                    </div>
                </div>
                <span class="positive-sm">100% Total</span>
            </div>

            <!-- Offline Computers Card -->
            <div class="card card-compact">
                <div class="card-inline-content">
                    <div class="card-icon-sm">
                        <i class="fa-solid fa-power-off"></i>
                    </div>
                    <div class="card-details">
                        <span class="card-title-sm">Offline</span>
                        <span class="card-value-sm">
                            <asp:Label ID="Label_Offline" runat="server" Text="0"></asp:Label>
                        </span>
                    </div>
                </div>
                <span class="bnet-badge-status gray">Offline</span>
            </div>

            <!-- Occupied Computers Card -->
            <div class="card card-compact">
                <div class="card-inline-content">
                    <div class="card-icon-sm">
                        <i class="fa-solid fa-user-check"></i>
                    </div>
                    <div class="card-details">
                        <span class="card-title-sm">Occupied</span>
                        <span class="card-value-sm">
                            <asp:Label ID="Label_Occupied" runat="server" Text="0"></asp:Label>
                        </span>
                    </div>
                </div>
                <span class="bnet-badge-status red">In Use</span>
            </div>

            <!-- Available Computers Card -->
            <div class="card card-compact">
                <div class="card-inline-content">
                    <div class="card-icon-sm">
                        <i class="fa-solid fa-circle-check"></i>
                    </div>
                    <div class="card-details">
                        <span class="card-title-sm">Available</span>
                        <span class="card-value-sm">
                            <asp:Label ID="Label_Available" runat="server" Text="0"></asp:Label>
                        </span>
                    </div>
                </div>
                <span class="bnet-badge-status green">Ready</span>
            </div>
        </div>
        <div class="bnet-table-wrapper">
            <div class="bnet-table-toolbar">
                <!-- Group filters together inline -->
                <div class="toolbar-filters">
                    <div class="search-box">
                        <asp:TextBox ID="TextBox_Search" runat="server" CssClass="search-input" placeholder="Search computers..." />
                    </div>
                    <div class="search-box" style="min-width:200px!important">
                        <asp:DropDownList ID="DropDownList_Status" AutoPostBack="true" runat="server"  CssClass="form-control bnet-select">
                            <asp:ListItem Text="All Status" Value="All"></asp:ListItem>
                            <asp:ListItem Text="Occupied" Value="Occupied"></asp:ListItem>
                            <asp:ListItem Text="Available" Value="Available"></asp:ListItem>
                            <asp:ListItem Text="Paused" Value="Paused"></asp:ListItem>
                            <asp:ListItem Text="Offline" Value="Offline"></asp:ListItem>
                        </asp:DropDownList>
                    </div>
                </div>

                <div style="display: flex; gap: 8px;">
                    <a class="btn btn-secondary" onclick="openPauseModal('')">
                        <i class="fa-solid fa-circle-pause"></i>Pause / Resume Time
            </a>
                    <asp:HyperLink ID="HyperLink_Add" NavigateUrl="~/BNetPage.aspx?Form=ComputerCreate" CssClass="btn btn-primary" runat="server">
                + Add Computer 
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
                                <div class="bnet-dropdown" tabindex="0">
                                    <button type="button" class="bnet-dropdown-toggle">
                                        Command <i class="fa-solid fa-chevron-down"></i>
                                    </button>
                                    <div class="bnet-dropdown-menu">
                                        <asp:Panel ID="Panel_ManageRental" Visible='<%# Eval("IsManageRentalVisible") %>' runat="server">
                                            <a onclick="navigateTo('BNetPage.aspx?Form=RentalManage&id=<%# Eval("DBId") %>')" class="bnet-dropdown-item">
                                                <i class="fa-regular fa-clock"></i>Manage Rental Session
                                            </a>
                                        </asp:Panel>
                                        <asp:Panel ID="Panel_Logout" Visible='<%# Eval("IsLogoutVisible") %>' runat="server">
                                            <a class="bnet-dropdown-item"
                                                onclick="openLogoutModal(this, '<%# Eval("DBId") %>', '<%# Eval("DBComputerName") %>')">
                                                <i class="fa-solid fa-right-from-bracket"></i>Log Out
                                            </a>
                                        </asp:Panel>
                                        <asp:Panel ID="Panel_Transfer" Visible='<%# Eval("IsTransferVisible") %>' runat="server">
                                            <a class="bnet-dropdown-item"
                                                onclick="openTransferModal('<%# Eval("DBId") %>', '<%# Eval("DBComputerName") %>')">
                                                <i class="fa-solid fa-right-left"></i>Transfer Session
                                            </a>
                                        </asp:Panel>
                                        <asp:Panel ID="Panel_PauseResume" Visible='<%# Eval("IsPauseResumeVisible") %>' runat="server">
                                            <a class="bnet-dropdown-item" onclick="openPauseModal('<%# Eval("DBComputerName") %>')">
                                                <i class="fa-solid fa-circle-pause"></i>Pause / Resume Time
                                            </a>
                                        </asp:Panel>
                                        <a onclick="navigateTo('BNetPage.aspx?Form=ComputerEarnings&id=<%# Eval("DBId") %>')" class="bnet-dropdown-item">
                                            <i class="fa-solid fa-clock-rotate-left"></i>Session History
                                        </a>
                                        <a onclick="navigateTo('BNetPage.aspx?Form=ComputerEdit&id=<%# Eval("DBId") %>')" class="bnet-dropdown-item">
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
                                <asp:Label ID="Label_DBId" Visible="false" runat="server" Text='<%# Eval("DBId") %>'></asp:Label>
                                <asp:Label ID="Label_DBComputerName" runat="server" Text='<%# Eval("DBComputerName") %>'></asp:Label>
                            </ItemTemplate>
                        </asp:TemplateField>

                        <asp:TemplateField HeaderText="IP Address" ItemStyle-Width="200px">
                            <ItemTemplate>
                                <asp:Label ID="Label_IPAddress" runat="server" Text='<%# Eval("IPAddress") %>'></asp:Label>
                            </ItemTemplate>
                        </asp:TemplateField>

                        <asp:TemplateField HeaderText="Time Start" ItemStyle-Width="200px">
                            <ItemTemplate>
                                <asp:Label ID="Label_TimeStart" runat="server" Text='<%# Eval("TimeStart") %>'></asp:Label>
                            </ItemTemplate>
                        </asp:TemplateField>

                        <asp:TemplateField HeaderText="Time End" ItemStyle-Width="200px">
                            <ItemTemplate>
                                <asp:Label ID="Label_TimeEnd" runat="server" Text='<%# Eval("TimeEnd") %>'></asp:Label>
                            </ItemTemplate>
                        </asp:TemplateField>

                        <asp:TemplateField HeaderText="Total Hours" ItemStyle-Width="200px">
                            <ItemTemplate>
                                <asp:Label ID="Label_TotalHours" runat="server" Text='<%# Eval("TotalHours") %>'></asp:Label>
                            </ItemTemplate>
                        </asp:TemplateField>

                        <asp:TemplateField HeaderText="Status" ItemStyle-Width="200px">
                            <ItemTemplate>
                                <asp:Label ID="Label_Status" runat="server" Text='<%# Eval("Status") %>' CssClass='<%# Eval("StatusCssClass") %>'></asp:Label>
                            </ItemTemplate>
                        </asp:TemplateField>

                        <asp:TemplateField HeaderText="Billing" ItemStyle-Width="200px">
                            <ItemTemplate>
                                <asp:Label ID="Label_Billing" runat="server" Text='<%# Eval("Billing") %>'></asp:Label>
                            </ItemTemplate>
                        </asp:TemplateField>
                    </Columns>
                </asp:GridView>
            </div>
            <asp:Panel ID="Panel_Pagination" runat="server" />
        </div>
        <uc1:ComputerDelete runat="server" ID="ComputerDelete" />
        <uc1:RemoteLogout runat="server" ID="RemoteLogout" />
        <uc1:RemoteTimeTransfer runat="server" ID="RemoteTimeTransfer" />
        <uc1:RemoteTimePause runat="server" ID="RemoteTimePause" />
    </ContentTemplate>
</asp:UpdatePanel>
