<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="SampleModal.ascx.cs" Inherits="BNet.Cafe.Server.Forms.SampleModal" %>
<div class="modal-overlay active" id="computerCreateModal">
    <div class="modal-container">
        
        <div class="modal-header">
            <h2 class="modal-title">
                <i class="fa-solid fa-desktop" style="margin-right: 8px; color: var(--primary);"></i>Add New Computer
            </h2>
            <asp:LinkButton ID="btnCloseHeader" runat="server" CssClass="modal-close" CausesValidation="false">
                <i class="fa-solid fa-xmark"></i>
            </asp:LinkButton>
        </div>

        <div class="modal-body">
            <!-- Server-side alert container -->
            <asp:Panel ID="pnlAlert" runat="server" Visible="false" CssClass="form-group">
                <div style="padding: 10px 14px; border-radius: 8px; background-color: rgba(239, 68, 68, 0.1); border: 1px solid #ef4444; color: #ef4444; font-size: 14px;">
                    <asp:Literal ID="litErrorMessage" runat="server"></asp:Literal>
                </div>
            </asp:Panel>

            <!-- Computer Name -->
            <div class="form-group">
                <label for="<%= txtComputerName.ClientID %>">Computer Name <span style="color: #ef4444;">*</span></label>
                <asp:TextBox ID="txtComputerName" runat="server" CssClass="form-control" placeholder="e.g., PC-01" MaxLength="50"></asp:TextBox>

            </div>

            <!-- IP Address -->
            <div class="form-group">
                <label for="<%= txtIPAddress.ClientID %>">IP Address <span style="color: #ef4444;">*</span></label>
                <asp:TextBox ID="txtIPAddress" runat="server" CssClass="form-control" placeholder="192.168.1.100" MaxLength="15"></asp:TextBox>
          
            </div>

            <!-- MAC Address -->
            <div class="form-group">
                <label for="<%= txtMACAddress.ClientID %>">MAC Address</label>
                <asp:TextBox ID="txtMACAddress" runat="server" CssClass="form-control" placeholder="00:1A:2B:3C:4D:5E" MaxLength="17"></asp:TextBox>
            </div>

            <!-- Zone / Section Selection -->
            <div class="form-group">
                <label for="<%= ddlZone.ClientID %>">Zone / Section</label>
                <asp:DropDownList ID="ddlZone" runat="server" CssClass="form-control">
                    <asp:ListItem Text="-- Select Zone --" Value="" />
                    <asp:ListItem Text="VIP Area" Value="VIP" />
                    <asp:ListItem Text="Standard Gaming" Value="Standard" />
                    <asp:ListItem Text="Streaming Booth" Value="Streaming" />
                </asp:DropDownList>
            </div>

            <!-- Status Selection -->
            <div class="form-group">
                <label for="<%= ddlStatus.ClientID %>">Initial Status</label>
                <asp:DropDownList ID="ddlStatus" runat="server" CssClass="form-control">
                    <asp:ListItem Text="Available" Value="Available" Selected="True" />
                    <asp:ListItem Text="Maintenance" Value="Maintenance" />
                    <asp:ListItem Text="Offline" Value="Offline" />
                </asp:DropDownList>
            </div>
        </div>

        <div class="modal-footer">
            <asp:Button ID="btnCancel" runat="server" Text="Cancel" CssClass="btn btn-secondary" CausesValidation="false" />
            <asp:Button ID="btnSave" runat="server" Text="Add Computer" CssClass="btn btn-primary" />
        </div>

    </div>
</div>