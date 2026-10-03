<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="RemoteRestart.ascx.cs" Inherits="BNet.Cafe.Server.Forms.Modals.RemoteRestart" %>
<asp:HiddenField ID="HiddenField_RestartId" runat="server" />
<asp:HiddenField ID="HiddenField_RestartClientId" runat="server" />

<!-- BNet Modal Template -->
<div id="restartModal" class="bnet-modal-overlay">
    <div class="bnet-modal-container">
        <div class="bnet-modal-header">
            <h3 class="bnet-modal-title">Confirm Restart</h3>
            <span class="bnet-modal-close" onclick="closeRestartModal()">&times;</span>
        </div>
        <div class="bnet-modal-body">
            <p>Are you sure you want to restart computer <strong id="restartTargetName"></strong>?</p>
            <p style="color: var(--text-light-secondary); font-size: 13px; margin-top: 6px;">This action will reboot the target machine immediately.</p>
        </div>
        <div class="bnet-modal-footer">
            <span class="btn btn-secondary" onclick="closeRestartModal()">Cancel</span>
            <asp:LinkButton ID="LinkButton_ConfirmRestart" OnClientClick="disableRestartButton()" OnClick="LinkButton_ConfirmRestart_Click" CssClass="btn btn-danger" runat="server">Restart</asp:LinkButton>
        </div>
    </div>
</div>

<script type="text/javascript">
    let restartModalInstance = null;

    function getRestartModal() {
        if (!restartModalInstance) {
            restartModalInstance = new BNetModal('#restartModal');
        }
        return restartModalInstance;
    }

    function openRestartModal(id, name) {
        document.getElementById('<%= HiddenField_RestartId.ClientID %>').value = id;
        document.getElementById('<%= HiddenField_RestartClientId.ClientID %>').value = name;
        document.getElementById('restartTargetName').innerText = name;

        getRestartModal().open();
    }

    function closeRestartModal() {
        getRestartModal().close();
    }

    if (typeof Sys !== 'undefined' && Sys.WebForms && Sys.WebForms.PageRequestManager) {
        Sys.WebForms.PageRequestManager.getInstance().add_endRequest(function () {
            restartModalInstance = new BNetModal('#restartModal');
        });
    }

    function disableRestartButton() {
        var btn = document.querySelector('[id$="LinkButton_ConfirmRestart"]');
        if (btn) {
            btn.classList.add('disabled');
            btn.style.pointerEvents = 'none';
            btn.innerHTML = '<i class="fa fa-spinner fa-spin"></i> Processing...';
        }
    }
</script>