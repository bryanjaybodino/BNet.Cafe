<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="Suppliers.ascx.cs" Inherits="BNet.Cafe.Server.Forms.Suppliers" %>
<%@ Register Src="~/Forms/Modals/SupplierDelete.ascx" TagPrefix="uc" TagName="SupplierDelete" %>

<asp:UpdatePanel ID="UpdatePanel1" runat="server">
    <ContentTemplate>
        <asp:LinkButton ID="LinkButton_Refresh" runat="server"></asp:LinkButton>

        <div class="bnet-table-wrapper">
            <div class="bnet-table-toolbar">
                <div class="search-box">
                    <asp:TextBox ID="TextBox_Search" runat="server" CssClass="search-input" AutoPostBack="True" OnTextChanged="TextBox_Search_TextChanged" placeholder="Search supplier name or contact..." />
                </div>
                <div style="display: flex; gap: 8px;">
                    <asp:HyperLink ID="HyperLink_Add" NavigateUrl="~/BNetPage.aspx?Form=SupplierCreate" CssClass="btn btn-primary" runat="server">
                        + Add Supplier
                    </asp:HyperLink>
                </div>
            </div>

            <!-- Table Container -->
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
                                        <a onclick="navigateTo('BNetPage.aspx?Form=SupplierEdit&id=<%# Eval("DBId") %>')" class="bnet-dropdown-item">
                                            <i class="fa-solid fa-pen-to-square"></i>Edit Supplier
                                        </a>
                                        <a onclick="openDeleteModal('<%# Eval("DBId") %>', '<%# Eval("DBSupplierName") %>')" class="bnet-dropdown-item text-danger">
                                            <i class="fa-solid fa-trash"></i>Delete
                                        </a>
                                    </div>
                                </div>
                            </ItemTemplate>
                        </asp:TemplateField>

                        <asp:TemplateField HeaderText="Supplier Name" ItemStyle-Width="200px">
                            <ItemTemplate>
                                <asp:Label ID="Label_DBSupplierName" runat="server" Text='<%# Eval("DBSupplierName") %>'></asp:Label>
                            </ItemTemplate>
                        </asp:TemplateField>

                        <asp:TemplateField HeaderText="Contact Person" ItemStyle-Width="180px">
                            <ItemTemplate>
                                <asp:Label ID="Label_DBContactPerson" runat="server" Text='<%# Eval("DBContactPerson") %>'></asp:Label>
                            </ItemTemplate>
                        </asp:TemplateField>

                        <asp:TemplateField HeaderText="Phone" ItemStyle-Width="140px">
                            <ItemTemplate>
                                <asp:Label ID="Label_DBPhone" runat="server" Text='<%# Eval("DBPhone") %>'></asp:Label>
                            </ItemTemplate>
                        </asp:TemplateField>

                        <asp:TemplateField HeaderText="Email" ItemStyle-Width="200px">
                            <ItemTemplate>
                                <asp:Label ID="Label_DBEmail" runat="server" Text='<%# Eval("DBEmail") %>'></asp:Label>
                            </ItemTemplate>
                        </asp:TemplateField>

                        <asp:TemplateField HeaderText="Address" ItemStyle-Width="220px">
                            <ItemTemplate>
                                <asp:Label ID="Label_DBAddress" runat="server" Text='<%# Eval("DBAddress") %>'></asp:Label>
                            </ItemTemplate>
                        </asp:TemplateField>

                        <asp:TemplateField HeaderText="Date & Time" ItemStyle-Width="200px">
                            <ItemTemplate>
                                <asp:Label ID="Label_DBDateCreated" runat="server" Text='<%# string.Format("{0} - {1}", Eval("DBDateCreated"), Eval("DBTimeCreated")) %>'></asp:Label>
                            </ItemTemplate>
                        </asp:TemplateField>
                    </Columns>
                </asp:GridView>
            </div>
            <asp:Panel ID="Panel_Pagination" runat="server" />
        </div>

        <uc:SupplierDelete runat="server" ID="SupplierDeleteModal" />
    </ContentTemplate>
</asp:UpdatePanel>