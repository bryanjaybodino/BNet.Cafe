<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="Computers.ascx.cs" Inherits="BNet.Cafe.Server.Forms.Computers" %>
<%@ Register Src="~/Forms/Modals/ComputerDelete.ascx" TagPrefix="uc1" TagName="ComputerDelete" %>


<asp:UpdatePanel ID="UpdatePanel1" runat="server">
    <ContentTemplate>

        <div class="table-wrapper">
            <div class="table-toolbar">
                <div class="search-box">
                    <asp:TextBox ID="TextBox_Search" runat="server" CssClass="search-input" placeholder="Search computers..." />
                </div>
                <asp:HyperLink ID="HyperLink_Add" NavigateUrl="~/BNetPage.aspx?Form=ComputerCreate" CssClass="btn btn-primary" runat="server">
                     + Add Computer 
                </asp:HyperLink>
            </div>

            <!-- Scrollable container wrapper -->
            <div class="table-container">
                <asp:GridView ID="GridViewTable" runat="server" ShowHeaderWhenEmpty="true"
                    AutoGenerateColumns="False" CssClass="data-table" GridLines="None"
                    AllowPaging="True" OnPageIndexChanging="GridViewTable_PageIndexChanging">

                    <PagerSettings Mode="NumericFirstLast" FirstPageText="&laquo;" LastPageText="&raquo;" PreviousPageText="&lsaquo;" NextPageText="&rsaquo;" PageButtonCount="5" />
                    <PagerStyle CssClass="custom-pagination" HorizontalAlign="Center" />

                    <Columns>
                        <asp:TemplateField HeaderText="Action" ItemStyle-Width="120px">
                            <ItemTemplate>
                                <!-- Pure CSS Click-Triggered Dropdown -->
                                <div class="action-dropdown" tabindex="0">
                                    <button type="button" class="action-dropdown-toggle">
                                        Command <i class="fa-solid fa-chevron-down"></i>
                                    </button>
                                    <div class="action-dropdown-menu">
                                        <a href='<%# "BNetPage.aspx?Form=RentTime&id=" + Eval("DBId") %>' class="dropdown-item">
                                            <i class="fa-regular fa-clock"></i>Start Rental Session
                                        </a>
                                        <a href='<%# "BNetPage.aspx?Form=Billing&id=" + Eval("DBId") %>' class="dropdown-item">
                                            <i class="fa-solid fa-file-invoice-dollar"></i>Billing & Invoices
                                        </a>
                                        <a href='<%# "BNetPage.aspx?Form=History&id=" + Eval("DBId") %>' class="dropdown-item">
                                            <i class="fa-solid fa-clock-rotate-left"></i>Session History
                                        </a>
                                        <a href='<%# "BNetPage.aspx?Form=ComputerEdit&id=" + Eval("DBId") %>' class="dropdown-item">
                                            <i class="fa-solid fa-pen-to-square"></i>Edit Computer
                                        </a>
                                        <div class="dropdown-divider"></div>
                                        <span class="dropdown-item text-danger" onclick="openDeleteModal('<%# Eval("DBId") %>', '<%# Eval("DBComputerName") %>')">
                                            <i class="fa-solid fa-trash-can"></i>Delete Computer
                                        </span>
                                    </div>
                                </div>
                            </ItemTemplate>
                        </asp:TemplateField>

                        <asp:TemplateField HeaderText="Id" ItemStyle-Width="100px">
                            <ItemTemplate>
                                <asp:Label ID="Label_DBId" runat="server" Text='<%#Eval("DBId") %>'></asp:Label>
                            </ItemTemplate>
                        </asp:TemplateField>

                        <asp:TemplateField HeaderText="Computer Name" ItemStyle-Width="200px">
                            <ItemTemplate>
                                <asp:Label ID="Label_DBComputerName" runat="server" Text='<%#Eval("DBComputerName") %>'></asp:Label>
                            </ItemTemplate>
                        </asp:TemplateField>
                        <asp:TemplateField HeaderText="Time Start" ItemStyle-Width="200px">
                            <ItemTemplate></ItemTemplate>
                        </asp:TemplateField>
                        <asp:TemplateField HeaderText="Time End" ItemStyle-Width="200px">
                            <ItemTemplate></ItemTemplate>
                        </asp:TemplateField>
                        <asp:TemplateField HeaderText="Total Hours" ItemStyle-Width="200px">
                            <ItemTemplate></ItemTemplate>
                        </asp:TemplateField>
                        <asp:TemplateField HeaderText="Status" ItemStyle-Width="200px">
                            <ItemTemplate></ItemTemplate>
                        </asp:TemplateField>
                        <asp:TemplateField HeaderText="Billing" ItemStyle-Width="200px">
                            <ItemTemplate></ItemTemplate>
                        </asp:TemplateField>
                    </Columns>
                </asp:GridView>
            </div>
            <asp:Panel ID="Panel_Pagination" runat="server"/>
        </div>
        <uc1:ComputerDelete runat="server" id="ComputerDelete" />
    </ContentTemplate>
</asp:UpdatePanel>
