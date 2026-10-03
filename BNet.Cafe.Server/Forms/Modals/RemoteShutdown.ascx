<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="RemoteShutdown.ascx.cs" Inherits="BNet.Cafe.Server.Forms.Modals.RemoteShutdown" %>
<asp:HiddenField ID="HiddenField_ShutdownId" runat="server" />
<asp:HiddenField ID="HiddenField_ShutdownClientId" runat="server" />

<!-- BNet Modal Template -->
<div id="shutdownModal" class="bnet-modal-overlay">
    <div class="bnet-modal-container">
        <div class="bnet-modal-header">
            <h3 class="bnet-modal-title">Confirm Shutdown</h3>
            <span class="bnet-modal-close" onclick="closeShutdownModal()">&times;</span>
        </div>
        <div class="bnet-modal-body">
            <p>Are you sure you want to shut down computer <strong id="shutdownTargetName"></strong>?</p>
            <p style="color: var(--text-light-secondary); font-size: 13px; margin-top: 6px;">This action will immediately power off the target machine.</p>
        </div>
        <div class="bnet-modal-footer">
            <span class="btn btn-secondary" onclick="closeShutdownModal()">Cancel</span>
            <asp:LinkButton ID="LinkButton_ConfirmShutdown" OnClientClick="disableShutdownButton()" OnClick="LinkButton_ConfirmShutdown_Click" CssClass="btn btn-danger" runat="server">Shutdown</asp:LinkButton>
        </div>
    </div>
</div>

<script type="text/javascript">
    let shutdownModalInstance = null;

    function getShutdownModal() {
        if (!shutdownModalInstance) {
            shutdownModalInstance = new BNetModal('#shutdownModal');
        }
        return shutdownModalInstance;
    }

    function openShutdownModal(id, name) {
        document.getElementById('<%= HiddenField_ShutdownId.ClientID %>').value = id;
        document.getElementById('<%= HiddenField_ShutdownClientId.ClientID %>').value = name;
        document.getElementById('shutdownTargetName').innerText = name;

        getShutdownModal().open();
    }

    function closeShutdownModal() {
        getShutdownModal().close();
    }

    if (typeof Sys !== 'undefined' && Sys.WebForms && Sys.WebForms.PageRequestManager) {
        Sys.WebForms.PageRequestManager.getInstance().add_endRequest(function () {
            shutdownModalInstance = new BNetModal('#shutdownModal');
        });
    }

    function disableShutdownButton() {
        var btn = document.querySelector('[id$="LinkButton_ConfirmShutdown"]');
        if (btn) {
            btn.classList.add('disabled');
            btn.style.pointerEvents = 'none';
            btn.innerHTML = '<i class="fa fa-spinner fa-spin"></i> Processing...';
        }
    }
</script>