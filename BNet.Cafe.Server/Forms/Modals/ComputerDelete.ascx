<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="ComputerDelete.ascx.cs" Inherits="BNet.Cafe.Server.Forms.Modals.ComputerDelete" %>
<asp:HiddenField ID="HiddenField_DeleteId" runat="server" />

<!-- BNet Modal Template -->
<div id="deleteModal" class="bnet-modal-overlay">
    <div class="bnet-modal-container">
        <div class="bnet-modal-header">
            <h3 class="bnet-modal-title">Confirm Deletion</h3>
            <span class="bnet-modal-close" onclick="closeDeleteModal()">&times;</span>
        </div>
        <div class="bnet-modal-body">
            <p>Are you sure you want to delete computer <strong id="deleteTargetName"></strong>?</p>
            <p style="color: var(--text-light-secondary); font-size: 13px; margin-top: 6px;">This action cannot be undone.</p>
        </div>
        <div class="bnet-modal-footer">
            <span class="btn btn-secondary" onclick="closeDeleteModal()">Cancel</span>
            <asp:LinkButton ID="LinkButton_ConfirmDelete" OnClick="LinkButton_ConfirmDelete_Click" CssClass="btn btn-danger" runat="server">Delete</asp:LinkButton>
        </div>
    </div>
</div>

<script type="text/javascript">
    // Global instance tracker
    let deleteModalInstance = null;

    function getDeleteModal() {
        if (!deleteModalInstance) {
            deleteModalInstance = new BNetModal('#deleteModal');
        }
        return deleteModalInstance;
    }

    function openDeleteModal(id, name) {
        document.getElementById('<%= HiddenField_DeleteId.ClientID %>').value = id;
        document.getElementById('deleteTargetName').innerText = name;

        getDeleteModal().open();
    }

    function closeDeleteModal() {
        getDeleteModal().close();
    }

    // Re-initialize modal reference on ASP.NET AJAX Partial PostBacks
    if (typeof Sys !== 'undefined' && Sys.WebForms && Sys.WebForms.PageRequestManager) {
        Sys.WebForms.PageRequestManager.getInstance().add_endRequest(function () {
            deleteModalInstance = new BNetModal('#deleteModal');
        });
    }
</script>