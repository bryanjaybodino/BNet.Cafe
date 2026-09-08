<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="RemoteMessage.ascx.cs" Inherits="BNet.Cafe.Server.Forms.Modals.RemoteMessage" %>
<asp:HiddenField ID="HiddenField_ClientName" runat="server" />

<div id="remoteMessageModal" class="bnet-modal-overlay">
    <div class="bnet-modal-container">
        <div class="bnet-modal-header">
            <h3 class="bnet-modal-title">Send Message to Client</h3>
            <span class="bnet-modal-close" onclick="closeRemoteMessageModal()">&times;</span>
        </div>
        <div class="bnet-modal-body">
            <p style="margin-bottom: 12px;">Sending message to: <strong id="messageTargetClient">—</strong></p>
            <div style="display: flex; flex-direction: column; gap: 6px;">
                <label style="font-size: 11px; font-family: monospace; font-weight: 700; color: var(--text-light-secondary); text-transform: uppercase;">Message Text</label>
                <asp:TextBox ID="TextBox_MessageContent" TextMode="MultiLine" Rows="4" CssClass="form-control" Style="width: 100%; border-radius: 8px; padding: 10px; border: 1px solid var(--border-light); background: var(--bg-light-tertiary); resize: vertical;" runat="server" Placeholder="Type message here..."></asp:TextBox>
            </div>
        </div>
        <div class="bnet-modal-footer">
            <span class="btn btn-secondary" onclick="closeRemoteMessageModal()">Cancel</span>
            <asp:LinkButton ID="LinkButton_SendMessage" OnClientClick="sendRemoteMessageModal()" CssClass="btn btn-primary" runat="server">Send Message</asp:LinkButton>
        </div>
    </div>
</div>

<script type="text/javascript">
    let remoteMessageModalInstance = null;

    function getRemoteMessageModal() {
        if (!remoteMessageModalInstance) {
            remoteMessageModalInstance = new BNetModal('#remoteMessageModal');
        }
        return remoteMessageModalInstance;
    }

    function openRemoteMessageModal(clientName) {
        // Hide/deselect sidebar when modal opens
        if (typeof deselectDevice === 'function') {
            deselectDevice();
        }

        document.getElementById('<%= HiddenField_ClientName.ClientID %>').value = clientName || '';
        document.getElementById('messageTargetClient').innerText = clientName || '—';
        document.getElementById('<%= TextBox_MessageContent.ClientID %>').value = '';

        getRemoteMessageModal().open();
    }

    function closeRemoteMessageModal() {
        getRemoteMessageModal().close();
    }

    if (typeof Sys !== 'undefined' && Sys.WebForms && Sys.WebForms.PageRequestManager) {
        Sys.WebForms.PageRequestManager.getInstance().add_endRequest(function () {
            remoteMessageModalInstance = new BNetModal('#remoteMessageModal');
        });
    }

    function sendRemoteMessageModal() {
        var message = document.getElementById('<%= TextBox_MessageContent.ClientID %>').value;
        var clientName = document.getElementById('<%= HiddenField_ClientName.ClientID %>').value;
        sendTextMessageToPC(clientName, message);
    }
</script>
