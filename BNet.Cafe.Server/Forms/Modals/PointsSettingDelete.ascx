<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="PointsSettingDelete.ascx.cs" Inherits="BNet.Cafe.Server.Forms.Modals.PointsSettingDelete" %>
<asp:HiddenField ID="HiddenField_DeletePointId" runat="server" />

<div id="deletePointsModal" class="bnet-modal-overlay">
    <div class="bnet-modal-container">
        <div class="bnet-modal-header">
            <h3 class="bnet-modal-title">Confirm Deletion</h3>
            <span class="bnet-modal-close" onclick="closePointsDeleteModal()">&times;</span>
        </div>
        <div class="bnet-modal-body">
            <p>Are you sure you want to delete this points rule</p>
            <p>(<strong id="deletePointsTargetInfo"></strong>)?</p>
            <p style="color: var(--text-light-secondary); font-size: 13px; margin-top: 6px;">This action cannot be undone.</p>
        </div>
        <div class="bnet-modal-footer">
            <span class="btn btn-secondary" onclick="closePointsDeleteModal()">Cancel</span>
            <asp:LinkButton ID="LinkButton_ConfirmDelete" OnClientClick="disablePointsDeleteButton()" OnClick="LinkButton_ConfirmDelete_Click" CssClass="btn btn-danger" runat="server">Delete</asp:LinkButton>
        </div>
    </div>
</div>

<script type="text/javascript">
    let deletePointsModalInstance = null;

    function getDeletePointsModal() {
        if (!deletePointsModalInstance) {
            deletePointsModalInstance = new BNetModal('#deletePointsModal');
        }
        return deletePointsModalInstance;
    }

    function openPointsDeleteModal(id, info) {
        document.getElementById('<%= HiddenField_DeletePointId.ClientID %>').value = id;
        document.getElementById('deletePointsTargetInfo').innerText = info;

        getDeletePointsModal().open();
    }

    function closePointsDeleteModal() {
        getDeletePointsModal().close();
    }

    if (typeof Sys !== 'undefined' && Sys.WebForms && Sys.WebForms.PageRequestManager) {
        Sys.WebForms.PageRequestManager.getInstance().add_endRequest(function () {
            deletePointsModalInstance = new BNetModal('#deletePointsModal');
        });
    }

    function disablePointsDeleteButton() {
        var btn = document.querySelector('[id$="LinkButton_ConfirmDelete"]');
        if (btn) {
            btn.classList.add('disabled');
            btn.style.pointerEvents = 'none';
            btn.innerHTML = '<i class="fa fa-spinner fa-spin"></i> Deleting...';
        }
    }
</script>