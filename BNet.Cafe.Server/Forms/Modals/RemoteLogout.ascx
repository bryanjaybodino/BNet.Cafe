<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="RemoteLogout.ascx.cs" Inherits="BNet.Cafe.Server.Forms.Modals.RemoteLogout" %>
<asp:HiddenField ID="HiddenField_LogoutId" runat="server" />
<asp:HiddenField ID="HiddenField_LogoutClientId" runat="server" />

<!-- BNet Modal Template -->
<div id="logoutModal" class="bnet-modal-overlay">
    <div class="bnet-modal-container">
        <div class="bnet-modal-header">
            <h3 class="bnet-modal-title">Confirm Logout</h3>
            <span class="bnet-modal-close" onclick="closeLogoutModal()">&times;</span>
        </div>
        <div class="bnet-modal-body">
            <p>Are you sure you want to logout computer <strong id="logoutTargetName"></strong>?</p>
            <p style="color: var(--text-light-secondary); font-size: 13px; margin-top: 6px;">This action cannot be undone.</p>
        </div>
        <div class="bnet-modal-footer">
            <span class="btn btn-secondary" onclick="closeLogoutModal()">Cancel</span>
            <asp:LinkButton ID="LinkButton_ConfirmLogout" OnClick="LinkButton_ConfirmLogout_Click" CssClass="btn btn-danger" runat="server">Logout</asp:LinkButton>
        </div>
    </div>
</div>

<script type="text/javascript">
    // Global instance tracker
    let logoutModalInstance = null;

    function getLogoutModal() {
        if (!logoutModalInstance) {
            logoutModalInstance = new BNetModal('#logoutModal');
        }
        return logoutModalInstance;
    }

    function openLogoutModal(id, name) {
        document.getElementById('<%= HiddenField_LogoutId.ClientID %>').value = id;
        document.getElementById('<%= HiddenField_LogoutClientId.ClientID %>').value = name;
        document.getElementById('logoutTargetName').innerText = name;
        getLogoutModal().open();
    }

    function closeLogoutModal() {
        getLogoutModal().close();
    }

    // Re-initialize modal reference on ASP.NET AJAX Partial PostBacks
    if (typeof Sys !== 'undefined' && Sys.WebForms && Sys.WebForms.PageRequestManager) {
        Sys.WebForms.PageRequestManager.getInstance().add_endRequest(function () {
            logoutModalInstance = new BNetModal('#logoutModal');
        });
    }
</script>
