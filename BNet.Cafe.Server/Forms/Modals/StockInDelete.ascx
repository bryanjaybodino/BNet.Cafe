<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="StockInDelete.ascx.cs" Inherits="BNet.Cafe.Server.Forms.Modals.StockInDelete" %>
<asp:HiddenField ID="HiddenField_DeleteId" runat="server" />

<!-- BNet Modal Template -->
<div id="stockInDeleteModal" class="bnet-modal-overlay">
    <div class="bnet-modal-container">
        <div class="bnet-modal-header">
            <h3 class="bnet-modal-title">
                <i class="fa-solid fa-trash-can" style="color: var(--danger, #dc3545);"></i> Confirm Delete Stock In
            </h3>
            <span class="bnet-modal-close" onclick="closeStockInDeleteModal()">&times;</span>
        </div>
        <div class="bnet-modal-body">
            <p>Are you sure you want to delete this stock entry for <strong id="stockInDeleteTargetName"></strong>?</p>
            <p style="color: var(--text-light-secondary); font-size: 13px; margin-top: 6px;">
                This action will remove the recorded stock entry from the log.
            </p>
        </div>
        <div class="bnet-modal-footer">
            <span class="btn btn-secondary" onclick="closeStockInDeleteModal()">Cancel</span>
            <asp:LinkButton ID="LinkButton_ConfirmDelete" OnClientClick="disableStockInDeleteButton()" OnClick="LinkButton_ConfirmDelete_Click" CssClass="btn btn-danger" runat="server">
                <i class="fa fa-trash"></i> Delete
            </asp:LinkButton>
        </div>
    </div>
</div>

<script type="text/javascript">
    // Global instance tracker
    let stockInDeleteModalInstance = null;

    function getStockInDeleteModal() {
        if (!stockInDeleteModalInstance) {
            stockInDeleteModalInstance = new BNetModal('#stockInDeleteModal');
        }
        return stockInDeleteModalInstance;
    }

    function openStockInDeleteModal(id, itemName) {
        document.getElementById('<%= HiddenField_DeleteId.ClientID %>').value = id;
        document.getElementById('stockInDeleteTargetName').innerText = itemName;

        getStockInDeleteModal().open();
    }

    function closeStockInDeleteModal() {
        getStockInDeleteModal().close();
    }

    // Re-initialize modal reference on ASP.NET AJAX Partial PostBacks
    if (typeof Sys !== 'undefined' && Sys.WebForms && Sys.WebForms.PageRequestManager) {
        Sys.WebForms.PageRequestManager.getInstance().add_endRequest(function () {
            stockInDeleteModalInstance = new BNetModal('#stockInDeleteModal');
        });
    }

    function disableStockInDeleteButton() {
        var btn = document.querySelector('[id$="LinkButton_ConfirmDelete"]');
        if (btn) {
            btn.classList.add('disabled');
            btn.style.pointerEvents = 'none';
            btn.innerHTML = '<i class="fa fa-spinner fa-spin"></i> Deleting...';
        }
    }
</script>