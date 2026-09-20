<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="PricingSettingDelete.ascx.cs" Inherits="BNet.Cafe.Server.Forms.Modals.PricingSettingDelete" %>
<asp:HiddenField ID="HiddenField_DeletePriceId" runat="server" />

<div id="deletePricingModal" class="bnet-modal-overlay">
    <div class="bnet-modal-container">
        <div class="bnet-modal-header">
            <h3 class="bnet-modal-title">Confirm Deletion</h3>
            <span class="bnet-modal-close" onclick="closePricingDeleteModal()">&times;</span>
        </div>
        <div class="bnet-modal-body">
            <p>Are you sure you want to delete this pricing rule</p>
            <p>(<strong id="deletePricingTargetInfo"></strong>)?</p>
            <p style="color: var(--text-light-secondary); font-size: 13px; margin-top: 6px;">This action cannot be undone.</p>
        </div>
        <div class="bnet-modal-footer">
            <span class="btn btn-secondary" onclick="closePricingDeleteModal()">Cancel</span>
            <asp:LinkButton ID="LinkButton_ConfirmDelete" OnClientClick="disablePricingDeleteButton()" OnClick="LinkButton_ConfirmDelete_Click" CssClass="btn btn-danger" runat="server">Delete</asp:LinkButton>
        </div>
    </div>
</div>

<script type="text/javascript">
    let deletePricingModalInstance = null;

    function getDeletePricingModal() {
        if (!deletePricingModalInstance) {
            deletePricingModalInstance = new BNetModal('#deletePricingModal');
        }
        return deletePricingModalInstance;
    }

    function openPricingDeleteModal(id, info) {
        document.getElementById('<%= HiddenField_DeletePriceId.ClientID %>').value = id;
        document.getElementById('deletePricingTargetInfo').innerText = info;

        getDeletePricingModal().open();
    }

    function closePricingDeleteModal() {
        getDeletePricingModal().close();
    }

    if (typeof Sys !== 'undefined' && Sys.WebForms && Sys.WebForms.PageRequestManager) {
        Sys.WebForms.PageRequestManager.getInstance().add_endRequest(function () {
            deletePricingModalInstance = new BNetModal('#deletePricingModal');
        });
    }

    function disablePricingDeleteButton() {
        var btn = document.querySelector('[id$="LinkButton_ConfirmDelete"]');
        if (btn) {
            btn.classList.add('disabled');
            btn.style.pointerEvents = 'none';
            btn.innerHTML = '<i class="fa fa-spinner fa-spin"></i> Deleting...';
        }
    }
</script>
