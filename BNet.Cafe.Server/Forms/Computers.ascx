<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="Computers.ascx.cs" Inherits="BNet.Cafe.Server.Forms.Computers" %>

<asp:UpdatePanel ID="UpdatePanel1" runat="server">
    <ContentTemplate>

        <div class="table-wrapper">
            <div class="table-toolbar">
                <div class="search-box">
                    <asp:TextBox ID="TextBox_Search" runat="server" CssClass="search-input" placeholder="Search computers..." AutoPostBack="true" />
                </div>
                <button type="button" class="btn btn-primary">
                    + Add Computer 
                </button>
            </div>

            <!-- Scrollable container wrapper -->
            <div class="table-container">
                <asp:GridView ID="GridViewTable" runat="server" ShowHeaderWhenEmpty="true" AutoGenerateColumns="False" CssClass="data-table" GridLines="None">
                    <Columns>
                        <asp:TemplateField HeaderText="Id" ItemStyle-Width="100px">
                            <ItemTemplate>
                                <asp:Label ID="Label_DBId" runat="server" Text='<%#Eval("DBId") %>'></asp:Label>
                            </ItemTemplate>
                        </asp:TemplateField>

                        <asp:TemplateField HeaderText="Computer Name" ItemStyle-Width="200px">
                            <ItemTemplate>
                                <asp:Label ID="Label_DBComputerName1" runat="server" Text='<%#Eval("DBComputerName") %>'></asp:Label>
                            </ItemTemplate>
                        </asp:TemplateField>

                        <asp:TemplateField HeaderText="Column 2" ItemStyle-Width="200px">
                            <ItemTemplate>
                                <asp:Label ID="Label_DBComputerName2" runat="server" Text='<%#Eval("DBComputerName") %>'></asp:Label>
                            </ItemTemplate>
                        </asp:TemplateField>

                        <asp:TemplateField HeaderText="Column 3" ItemStyle-Width="200px">
                            <ItemTemplate>
                                <asp:Label ID="Label_DBComputerName3" runat="server" Text='<%#Eval("DBComputerName") %>'></asp:Label>
                            </ItemTemplate>
                        </asp:TemplateField>

                        <asp:TemplateField HeaderText="Column 4" ItemStyle-Width="200px">
                            <ItemTemplate>
                                <asp:Label ID="Label_DBComputerName4" runat="server" Text='<%#Eval("DBComputerName") %>'></asp:Label>
                            </ItemTemplate>
                        </asp:TemplateField>

                        <asp:TemplateField HeaderText="Column 5" ItemStyle-Width="200px">
                            <ItemTemplate>
                                <asp:Label ID="Label_DBComputerName5" runat="server" Text='<%#Eval("DBComputerName") %>'></asp:Label>
                            </ItemTemplate>
                        </asp:TemplateField>

                        <asp:TemplateField HeaderText="Column 6" ItemStyle-Width="200px">
                            <ItemTemplate>
                                <asp:Label ID="Label_DBComputerName6" runat="server" Text='<%#Eval("DBComputerName") %>'></asp:Label>
                            </ItemTemplate>
                        </asp:TemplateField>

                        <asp:TemplateField HeaderText="Column 7" ItemStyle-Width="200px">
                            <ItemTemplate>
                                <asp:Label ID="Label_DBComputerName7" runat="server" Text='<%#Eval("DBComputerName") %>'></asp:Label>
                            </ItemTemplate>
                        </asp:TemplateField>
                    </Columns>
                </asp:GridView>
            </div>
        </div>

    </ContentTemplate>
</asp:UpdatePanel>