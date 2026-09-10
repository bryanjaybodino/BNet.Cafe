<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="RemoteOpenTime.ascx.cs" Inherits="BNet.Cafe.Server.Forms.Modals.RemoteOpenTime" %>
<asp:HiddenField ID="HiddenField_OpenTimeCustomerName" runat="server" />
<asp:HiddenField ID="HiddenField_OpenTimeClientId" runat="server" />
<asp:HiddenField ID="HiddenField_OpenTimeRentalId" runat="server" />
<asp:HiddenField ID="HiddenField_OpenTimeStatus" runat="server" />

<!-- BNet Modal Template -->
<div id="openTimeModal" class="bnet-modal-overlay">
    <div class="bnet-modal-container">
        <div class="bnet-modal-header">
            <h3 class="bnet-modal-title">Confirm OpenTime</h3>
            <span class="bnet-modal-close" onclick="closeOpenTimeModal()">&times;</span>
        </div>
        <div class="bnet-modal-body">
            <p>Are you sure you want to Open Time computer <strong id="openTimeTargetName"></strong>?</p>
            <p style="color: var(--text-light-secondary); font-size: 13px; margin-top: 6px;">This action cannot be undone.</p>
        </div>
        <div class="bnet-modal-footer">
            <span class="btn btn-secondary" onclick="closeOpenTimeModal()">Cancel</span>
            <asp:LinkButton ID="LinkButton_ConfirmOpenTime" OnClientClick="disableOpenTimeButton()" OnClick="LinkButton_ConfirmOpenTime_Click" CssClass="btn btn-danger" runat="server">OpenTime</asp:LinkButton>
        </div>
    </div>
</div>

<script type="text/javascript">
    // Global instance tracker
    let openTimeModalInstance = null;

    function getOpenTimeModal() {
        if (!openTimeModalInstance) {
            openTimeModalInstance = new BNetModal('#openTimeModal');
        }
        return openTimeModalInstance;
    }

    function openOpenTimeModal(rentalId,clientId, name,status) {
        document.getElementById('<%= HiddenField_OpenTimeClientId.ClientID %>').value = clientId;
        document.getElementById('<%= HiddenField_OpenTimeCustomerName.ClientID %>').value = name;
        document.getElementById('<%= HiddenField_OpenTimeRentalId.ClientID %>').value = rentalId;
        document.getElementById('<%= HiddenField_OpenTimeStatus.ClientID %>').value = status;
        document.getElementById('openTimeTargetName').innerText = clientId;
        getOpenTimeModal().open();
    }

    function closeOpenTimeModal() {
        getOpenTimeModal().close();
    }

    // Re-initialize modal reference on ASP.NET AJAX Partial PostBacks
    if (typeof Sys !== 'undefined' && Sys.WebForms && Sys.WebForms.PageRequestManager) {
        Sys.WebForms.PageRequestManager.getInstance().add_endRequest(function () {
            openTimeModalInstance = new BNetModal('#openTimeModal');
        });
    }

    function disableOpenTimeButton() {
        var btn = document.querySelector('[id$="LinkButton_ConfirmOpenTime"]');
        if (btn) {
            btn.classList.add('disabled');
            btn.style.pointerEvents = 'none';
            btn.innerHTML = '<i class="fa fa-spinner fa-spin"></i> Saving...';
        }
    }
</script>
