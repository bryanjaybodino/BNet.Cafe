<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="Computers.ascx.cs" Inherits="BNet.Cafe.Server.Forms.Computers" %>

<asp:UpdatePanel ID="UpdatePanel1" runat="server">
    <ContentTemplate>

        <div class="table-wrapper">
            <div class="table-toolbar">
                <div class="search-box">
                    <asp:TextBox ID="TextBox_Search" runat="server" CssClass="search-input" placeholder="Search computers..." AutoPostBack="true" />
                </div>
                <asp:HyperLink ID="HyperLink_Add" NavigateUrl="~/BNetPage.aspx?Form=ComputerCreate" CssClass="btn btn-primary" runat="server">
                     + Add Computer 
                </asp:HyperLink>
            </div>

            <!-- Scrollable container wrapper -->
            <div class="table-container">
                <asp:GridView ID="GridViewTable" runat="server" ShowHeaderWhenEmpty="true" AutoGenerateColumns="False" CssClass="data-table" GridLines="None">
                    <Columns>
                        <asp:TemplateField HeaderText="Action" ItemStyle-Width="120px">
                            <ItemTemplate>
                                <!-- Pure CSS Click-Triggered Dropdown -->
                                <div class="action-dropdown" tabindex="0">
                                    <button type="button" class="action-dropdown-toggle">
                                        Command <i class="fa-solid fa-chevron-down"></i>
                                    </button>
                                    <div class="action-dropdown-menu">
                                        <!-- 1. Rent Time -->
                                        <a href='<%# "BNetPage.aspx?Form=RentTime&id=" + Eval("DBId") %>' class="dropdown-item">
                                            <i class="fa-regular fa-clock"></i>Start Rental Session
                                        </a>

                                        <!-- 2. Billing -->
                                        <a href='<%# "BNetPage.aspx?Form=Billing&id=" + Eval("DBId") %>' class="dropdown-item">
                                            <i class="fa-solid fa-file-invoice-dollar"></i>Billing & Invoices
                                        </a>

                                        <!-- 3. History -->
                                        <a href='<%# "BNetPage.aspx?Form=History&id=" + Eval("DBId") %>' class="dropdown-item">
                                            <i class="fa-solid fa-clock-rotate-left"></i>Session History
                                        </a>

                                        <div class="dropdown-divider"></div>

                                        <!-- 4. Delete Trigger (Opens Modal) -->
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
                            <ItemTemplate>
                            </ItemTemplate>
                        </asp:TemplateField>
                        <asp:TemplateField HeaderText="Time End" ItemStyle-Width="200px">
                            <ItemTemplate>
                            </ItemTemplate>
                        </asp:TemplateField>
                        <asp:TemplateField HeaderText="Total Hours" ItemStyle-Width="200px">
                            <ItemTemplate>
                            </ItemTemplate>
                        </asp:TemplateField>
                        <asp:TemplateField HeaderText="Status" ItemStyle-Width="200px">
                            <ItemTemplate>
                            </ItemTemplate>
                        </asp:TemplateField>
                        <asp:TemplateField HeaderText="Billing" ItemStyle-Width="200px">
                            <ItemTemplate>
                            </ItemTemplate>
                        </asp:TemplateField>
                    </Columns>
                </asp:GridView>
            </div>
        </div>

        <!-- Hidden Field to store target DBId for deletion -->
        <asp:HiddenField ID="HiddenField_DeleteId" runat="server" />

        <!-- Delete Modal Confirmation Overlay -->
        <div id="deleteModal" class="modal-overlay">
            <div class="modal-container">
                <div class="modal-header">
                    <h3 class="modal-title">Confirm Deletion</h3>
                    <button type="button" class="modal-close" onclick="closeDeleteModal()">&times;</button>
                </div>
                <div class="modal-body">
                    <p>Are you sure you want to delete computer <strong id="deleteTargetName"></strong>?</p>
                    <p style="color: var(--text-light-secondary); font-size: 13px; margin-top: 6px;">This action cannot be undone.</p>
                </div>
                <div class="modal-footer">
                    <button type="button" class="btn btn-secondary" onclick="closeDeleteModal()">Cancel</button>
                    <asp:Button ID="Button_ConfirmDelete" runat="server" CssClass="btn btn-danger" Text="Delete" />
                </div>
            </div>
        </div>

        <script type="text/javascript">
            function openDeleteModal(id, name) {
                document.getElementById('<%= HiddenField_DeleteId.ClientID %>').value = id;
                document.getElementById('deleteTargetName').innerText = name;
                document.getElementById('deleteModal').classList.add('active');
            }

            function closeDeleteModal() {
                document.getElementById('deleteModal').classList.remove('active');
            }
        </script>

    </ContentTemplate>
</asp:UpdatePanel>
