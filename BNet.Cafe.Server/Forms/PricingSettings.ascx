<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="PricingSettings.ascx.cs" Inherits="BNet.Cafe.Server.Forms.PricingSettings" %>

<asp:UpdatePanel ID="UpdatePanel1" runat="server">
    <ContentTemplate>
        <asp:HiddenField ID="HiddenField_PriceId" runat="server" Value="0" />

        <div class="bnet-table-wrapper">
            <div class="content-header" style="margin-bottom: 20px;">
                <h1 style="font-size: 20px; display: flex; align-items: center; gap: 8px;">
                    <i class="fa-solid fa-tags" style="color: var(--primary);"></i>Time Pricing Settings
                </h1>
                <p>Configure hourly and fixed rates for Guest/Walk-in and Member tiers.</p>
            </div>

            <!-- Form inputs -->
            <div class="form-grid" style="display: grid; grid-template-columns: repeat(3, 1fr); gap: 15px; margin-bottom: 20px;">
                <div class="form-group">
                    <label>Customer Tier <span style="color: #ef4444;">*</span></label>
                    <asp:DropDownList ID="DropDownList_CustomerType" runat="server" CssClass="form-control">
                        <asp:ListItem Text="Guest / Walk-In" Value="GUEST"></asp:ListItem>
                        <asp:ListItem Text="Member" Value="MEMBER"></asp:ListItem>
                    </asp:DropDownList>
                </div>

                <div class="form-group">
                    <label>Duration (Minutes) <span style="color: #ef4444;">*</span></label>
                    <asp:TextBox ID="TextBox_Minutes" runat="server" TextMode="Number" CssClass="form-control" placeholder="e.g., 60"></asp:TextBox>
                </div>

                <div class="form-group">
                    <label>Price <span style="color: #ef4444;">*</span></label>
                    <asp:TextBox ID="TextBox_Price" runat="server" CssClass="form-control" placeholder="e.g., 50.00"></asp:TextBox>
                </div>
            </div>

            <!-- Actions -->
            <div class="form-grid-action" style="margin-bottom: 25px;">
                <asp:LinkButton ID="LinkButton_Cancel" OnClick="LinkButton_Cancel_Click" CssClass="btn btn-secondary" runat="server">Cancel / Clear</asp:LinkButton>
                <asp:LinkButton ID="LinkButton_Submit" OnClick="LinkButton_Submit_Click" CssClass="btn btn-primary" OnClientClick="return ValidatePricing();" runat="server">
                    <i class="fa fa-save"></i> Save Pricing Rule
                </asp:LinkButton>
            </div>

            <!-- Existing Rates Data Grid Wrapper -->
            <div class="bnet-table-container">
                <asp:GridView ID="GridView_Pricing" runat="server" AutoGenerateColumns="False" GridLines="None" CssClass="bnet-table" OnRowCommand="GridView_Pricing_RowCommand" Style="table-layout: fixed; width: 100%;">
                    <Columns>

                        <asp:TemplateField HeaderText="Type">
                            <HeaderStyle Width="20%" />
                            <ItemStyle Width="20%" />
                            <ItemTemplate>
                                <%# Eval("DBCustomerType") %>
                            </ItemTemplate>
                        </asp:TemplateField>

                        <asp:TemplateField HeaderText="Minutes">
                            <HeaderStyle Width="20%" />
                            <ItemStyle Width="20%" />
                            <ItemTemplate>
                                <%# Eval("DBMinutes") %>
                            </ItemTemplate>
                        </asp:TemplateField>

                        <asp:TemplateField HeaderText="Price">
                            <HeaderStyle Width="20%" />
                            <ItemStyle Width="20%" />
                            <ItemTemplate>
                                <%# Eval("DBPrice") %>
                            </ItemTemplate>
                        </asp:TemplateField>

                        <asp:TemplateField HeaderText="Actions">
                            <HeaderStyle Width="20%" />
                            <ItemStyle Width="20%" />
                            <ItemTemplate>
                                <asp:LinkButton ID="LinkButton_Edit" runat="server" CommandName="EditRate" CommandArgument='<%# Eval("DBId") %>' CssClass="btn btn-sm btn-primary">
                    <i class="fa fa-edit"></i> Edit
                                </asp:LinkButton>
                                <asp:LinkButton ID="LinkButton_Delete" runat="server" CommandName="DeleteRate" CommandArgument='<%# Eval("DBId") %>' CssClass="btn btn-sm btn-danger" OnClientClick="return confirm('Are you sure you want to delete this rate?');">
                    <i class="fa fa-trash"></i> Delete
                                </asp:LinkButton>
                            </ItemTemplate>
                        </asp:TemplateField>
                    </Columns>
                </asp:GridView>
            </div>
        </div>
    </ContentTemplate>
</asp:UpdatePanel>

<script>
    function ValidatePricing() {
        var minutes = document.querySelector('[id$="TextBox_Minutes"]');
        var price = document.querySelector('[id$="TextBox_Price"]');

        if (!minutes || !minutes.value || parseInt(minutes.value) <= 0) {
            ShowAlert('Please enter a valid duration in minutes.');
            return false;
        }
        if (!price || !price.value || parseFloat(price.value) < 0) {
            ShowAlert('Please enter a valid price.');
            return false;
        }

        disablePricingSubmitButton();
        return true;
    }

</script>
