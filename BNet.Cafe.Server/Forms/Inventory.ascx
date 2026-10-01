<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="Inventory.ascx.cs" Inherits="BNet.Cafe.Server.Forms.Inventory" %>
<%@ Register Src="~/Forms/Modals/InventoryDelete.ascx" TagPrefix="uc" TagName="InventoryDelete" %>

<asp:UpdatePanel ID="UpdatePanel1" runat="server">
    <ContentTemplate>
        <asp:LinkButton ID="LinkButton_Refresh" runat="server"></asp:LinkButton>

        <div class="bnet-table-wrapper">
            <div class="bnet-table-toolbar">
                <div class="search-box">
                    <asp:TextBox ID="TextBox_Search" runat="server" CssClass="search-input" AutoPostBack="True" OnTextChanged="TextBox_Search_TextChanged" placeholder="Search item name or category..." />
                </div>
                <div style="display: flex; gap: 8px;">
                    <asp:HyperLink ID="HyperLink_Add" NavigateUrl="~/BNetPage.aspx?Form=InventoryCreate" CssClass="btn btn-primary" runat="server">
                        + Add Inventory Item
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
                                        <a onclick="navigateTo('BNetPage.aspx?Form=InventoryEdit&id=<%# Eval("DBId") %>')" class="bnet-dropdown-item">
                                            <i class="fa-solid fa-pen-to-square"></i>Edit Item
                                        </a>
                                        <a onclick="openDeleteModal('<%# Eval("DBId") %>', '<%# Eval("DBItemName") %>')" class="bnet-dropdown-item text-danger">
                                            <i class="fa-solid fa-trash"></i>Delete
                                        </a>
                                    </div>
                                </div>
                            </ItemTemplate>
                        </asp:TemplateField>

                        <asp:TemplateField HeaderText="Item Name" ItemStyle-Width="200px">
                            <ItemTemplate>
                                <asp:Label ID="Label_DBItemName" runat="server" Text='<%# Eval("DBItemName") %>'></asp:Label>
                            </ItemTemplate>
                        </asp:TemplateField>

                        <asp:TemplateField HeaderText="Category" ItemStyle-Width="150px">
                            <ItemTemplate>
                                <asp:Label ID="Label_DBCategory" runat="server" Text='<%# Eval("DBCategory") %>'></asp:Label>
                            </ItemTemplate>
                        </asp:TemplateField>

                        <asp:TemplateField HeaderText="Unit Price" ItemStyle-Width="120px">
                            <ItemTemplate>
                                <asp:Label ID="Label_DBUnitPrice" runat="server" Text='<%# Eval("DBUnitPrice") %>'></asp:Label>
                            </ItemTemplate>
                        </asp:TemplateField>

                        <asp:TemplateField HeaderText="Stock Quantity" ItemStyle-Width="140px">
                            <ItemTemplate>
                                <asp:Label ID="Label_DBQuantityInStock" runat="server" Text='<%# Eval("DBQuantityInStock") %>'></asp:Label>
                            </ItemTemplate>
                        </asp:TemplateField>

                        <asp:TemplateField HeaderText="Reorder Level" ItemStyle-Width="130px">
                            <ItemTemplate>
                                <asp:Label ID="Label_DBReorderLevel" runat="server" Text='<%# Eval("DBReorderLevel") %>'></asp:Label>
                            </ItemTemplate>
                        </asp:TemplateField>

                        <asp:TemplateField HeaderText="Date & Time" ItemStyle-Width="200px">
                            <ItemTemplate>
                                <asp:Label ID="Label_DBDateCreated_DBTimeCreated" runat="server" Text='<%# string.Format("{0} - {1}", Eval("DBDateCreated"), Eval("DBTimeCreated")) %>'></asp:Label>
                            </ItemTemplate>
                        </asp:TemplateField>
                    </Columns>
                </asp:GridView>
            </div>
            <asp:Panel ID="Panel_Pagination" runat="server" />
        </div>

        <uc:InventoryDelete runat="server" ID="InventoryDeleteModal" />
    </ContentTemplate>
</asp:UpdatePanel>