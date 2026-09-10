<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="RemoteTimeTransfer.ascx.cs" Inherits="BNet.Cafe.Server.Forms.Modals.RemoteTimeTransfer" %>
<asp:HiddenField ID="HiddenField_SourceComputerId" runat="server" />
<asp:HiddenField ID="HiddenField_SourceComputerName" runat="server" />

<!-- BNet Transfer Modal -->
<div id="transferModal" class="bnet-modal-overlay">
    <div class="bnet-modal-container">
        <div class="bnet-modal-header">
            <h3 class="bnet-modal-title"><i class="fa-solid fa-right-left"></i> Transfer Session</h3>
            <span class="bnet-modal-close" onclick="closeTransferModal()">&times;</span>
        </div>
        <div class="bnet-modal-body">
            <p>Transfer active session from <strong id="transferSourceName"></strong> to:</p>
            <div style="margin-top: 15px;">
                <label style="display: block; font-size: 13px; font-weight: 600; margin-bottom: 5px;">Target Computer</label>
                <asp:DropDownList ID="DropDownList_TargetComputer" runat="server" CssClass="form-control" Style="width: 100%; padding: 8px; border-radius: 6px; border: 1px solid #ccc;">
                </asp:DropDownList>
                <asp:Label ID="Label_NoAvailablePc" runat="server" Text="No available computers online to receive transfer." ForeColor="Red" Visible="false" Style="display: block; margin-top: 5px; font-size: 12px;"></asp:Label>
            </div>
        </div>
        <div class="bnet-modal-footer">
            <span class="btn btn-secondary" onclick="closeTransferModal()">Cancel</span>
            <asp:LinkButton ID="LinkButton_ConfirmTransfer" OnClick="LinkButton_ConfirmTransfer_Click" CssClass="btn btn-primary" runat="server">
                <i class="fa-solid fa-arrow-right-arrow-left"></i> Transfer
            </asp:LinkButton>
        </div>
    </div>
</div>

<script type="text/javascript">
    let transferModalInstance = null;

    function getTransferModal() {
        if (!transferModalInstance) {
            transferModalInstance = new BNetModal('#transferModal');
        }
        return transferModalInstance;
    }

    function openTransferModal(id, name) {
        document.getElementById('<%= HiddenField_SourceComputerId.ClientID %>').value = id;
        document.getElementById('<%= HiddenField_SourceComputerName.ClientID %>').value = name;
        document.getElementById('transferSourceName').innerText = name;

        getTransferModal().open();
    }

    function closeTransferModal() {
        getTransferModal().close();
    }

    if (typeof Sys !== 'undefined' && Sys.WebForms && Sys.WebForms.PageRequestManager) {
        Sys.WebForms.PageRequestManager.getInstance().add_endRequest(function () {
            transferModalInstance = new BNetModal('#transferModal');
        });
    }
</script>