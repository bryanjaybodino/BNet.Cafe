<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="InventoryDelete.ascx.cs" Inherits="BNet.Cafe.Server.Forms.Modals.InventoryDelete" %>
<asp:HiddenField ID="HiddenField_DeleteId" runat="server" />

<div id="deleteInventoryModal" class="bnet-modal-overlay">
    <div class="bnet-modal-container">
        <div class="bnet-modal-header">
            <h3 class="bnet-modal-title">Confirm Deletion</h3>
            <span class="bnet-modal-close" onclick="closeDeleteModal()">&times;</span>
        </div>
        <div class="bnet-modal-body">
            <p>Are you sure you want to delete item <strong id="deleteTargetName"></strong>?</p>
            <p style="color: var(--text-light-secondary); font-size: 13px; margin-top: 6px;">This action cannot be undone.</p>
        </div>
        <div class="bnet-modal-footer">
            <span class="btn btn-secondary" onclick="closeDeleteModal()">Cancel</span>
            <asp:LinkButton ID="LinkButton_ConfirmDelete" OnClientClick="disableDeleteButton()" OnClick="LinkButton_ConfirmDelete_Click" CssClass="btn btn-danger" runat="server">Delete</asp:LinkButton>
        </div>
    </div>
</div>

<script type="text/javascript">
    let deleteInventoryModalInstance = null;

    function getDeleteInventoryModal() {
        if (!deleteInventoryModalInstance) {
            deleteInventoryModalInstance = new BNetModal('#deleteInventoryModal');
        }
        return deleteInventoryModalInstance;
    }

    function openDeleteModal(id, name) {
        document.getElementById('<%= HiddenField_DeleteId.ClientID %>').value = id;
        document.getElementById('deleteTargetName').innerText = name;
        getDeleteInventoryModal().open();
    }

    function closeDeleteModal() {
        getDeleteInventoryModal().close();
    }

    if (typeof Sys !== 'undefined' && Sys.WebForms && Sys.WebForms.PageRequestManager) {
        Sys.WebForms.PageRequestManager.getInstance().add_endRequest(function () {
            deleteInventoryModalInstance = new BNetModal('#deleteInventoryModal');
        });
    }

    function disableDeleteButton() {
        var btn = document.querySelector('[id$="LinkButton_ConfirmDelete"]');
        if (btn) {
            btn.classList.add('disabled');
            btn.style.pointerEvents = 'none';
            btn.innerHTML = '<i class="fa fa-spinner fa-spin"></i> Deleting...';
        }
    }
</script>