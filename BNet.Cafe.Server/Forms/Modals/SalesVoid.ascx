<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="SalesVoid.ascx.cs" Inherits="BNet.Cafe.Server.Forms.Modals.SalesVoid" %>
<asp:HiddenField ID="HiddenField_VoidId" runat="server" />

<div id="voidSalesModal" class="bnet-modal-overlay">
    <div class="bnet-modal-container">
        <div class="bnet-modal-header">
            <h3 class="bnet-modal-title">Confirm Void Sale</h3>
            <span class="bnet-modal-close" onclick="closeVoidModal()">&times;</span>
        </div>
        <div class="bnet-modal-body">
            <p>Are you sure you want to void sale transaction <strong id="voidTargetId"></strong> for item <strong id="voidTargetItem"></strong>?</p>
            <p style="color: var(--text-light-secondary); font-size: 13px; margin-top: 6px;">This action cannot be undone and will restore item stock.</p>
        </div>
        <div class="bnet-modal-footer">
            <span class="btn btn-secondary" onclick="closeVoidModal()">Cancel</span>
            <asp:LinkButton ID="LinkButton_ConfirmVoid" OnClientClick="disableVoidButton()" OnClick="LinkButton_ConfirmVoid_Click" CssClass="btn btn-danger" runat="server">Void Sale</asp:LinkButton>
        </div>
    </div>
</div>

<script type="text/javascript">
    let voidSalesModalInstance = null;

    function getVoidSalesModal() {
        if (!voidSalesModalInstance) {
            voidSalesModalInstance = new BNetModal('#voidSalesModal');
        }
        return voidSalesModalInstance;
    }

    function openVoidModal(id, itemName) {
        document.getElementById('<%= HiddenField_VoidId.ClientID %>').value = id;
        document.getElementById('voidTargetId').innerText = "#" + id;
        document.getElementById('voidTargetItem').innerText = itemName;
        getVoidSalesModal().open();
    }

    function closeVoidModal() {
        getVoidSalesModal().close();
    }

    if (typeof Sys !== 'undefined' && Sys.WebForms && Sys.WebForms.PageRequestManager) {
        Sys.WebForms.PageRequestManager.getInstance().add_endRequest(function () {
            voidSalesModalInstance = new BNetModal('#voidSalesModal');
        });
    }

    function disableVoidButton() {
        var btn = document.querySelector('[id$="LinkButton_ConfirmVoid"]');
        if (btn) {
            btn.classList.add('disabled');
            btn.style.pointerEvents = 'none';
            btn.innerHTML = '<i class="fa fa-spinner fa-spin"></i> Voiding...';
        }
    }
</script>