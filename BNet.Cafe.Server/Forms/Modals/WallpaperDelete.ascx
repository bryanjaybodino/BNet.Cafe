<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="WallpaperDelete.ascx.cs" Inherits="BNet.Cafe.Server.Forms.Modals.WallpaperDelete" %>

<asp:HiddenField ID="HiddenField_DeleteTarget" runat="server" />

<!-- Wallpaper Delete Confirmation Modal -->
<div id="deleteWallpaperModal" class="bnet-modal-overlay">
    <div class="bnet-modal-container">
        <div class="bnet-modal-header">
            <h3 class="bnet-modal-title">Confirm Deletion</h3>
            <span class="bnet-modal-close" onclick="closeDeleteWallpaperModal()">&times;</span>
        </div>
        <div class="bnet-modal-body">
            <p>Are you sure you want to delete <strong id="deleteWallpaperTargetName"></strong> from the server?</p>
            <p style="color: var(--text-light-secondary); font-size: 13px; margin-top: 6px;">This action cannot be undone.</p>
        </div>
        <div class="bnet-modal-footer">
            <span class="btn btn-secondary" onclick="closeDeleteWallpaperModal()">Cancel</span>
            <asp:LinkButton ID="LinkButton_ConfirmDelete" OnClientClick="disableDeleteWallpaperButton()" OnClick="LinkButton_ConfirmDelete_Click" CssClass="btn btn-danger" runat="server">
                Delete
            </asp:LinkButton>
        </div>
    </div>
</div>

<script type="text/javascript">
    let deleteWallpaperModalInstance = null;

    function getDeleteWallpaperModal() {
        if (!deleteWallpaperModalInstance) {
            deleteWallpaperModalInstance = new BNetModal('#deleteWallpaperModal');
        }
        return deleteWallpaperModalInstance;
    }

    function openDeleteWallpaperModal(id, name) {
        document.getElementById('<%= HiddenField_DeleteTarget.ClientID %>').value = name || id;

        const targetNameElem = document.getElementById('deleteWallpaperTargetName');
        if (targetNameElem) {
            targetNameElem.innerText = name || id;
        }

        getDeleteWallpaperModal().open();
    }

    function closeDeleteWallpaperModal() {
        getDeleteWallpaperModal().close();
    }

    // Re-initialize modal reference and auto-close modal after Partial PostBacks
    if (typeof Sys !== 'undefined' && Sys.WebForms && Sys.WebForms.PageRequestManager) {
        Sys.WebForms.PageRequestManager.getInstance().add_endRequest(function () {
            closeDeleteWallpaperModal();
            deleteWallpaperModalInstance = new BNetModal('#deleteWallpaperModal');
        });
    }

    function disableDeleteWallpaperButton() {
        const wallpaperDataField = document.querySelector('[id$="HiddenField_WallpaperData"]');
        if (wallpaperDataField) {
            wallpaperDataField.value = '';
        }

        const btn = document.querySelector('[id$="LinkButton_ConfirmDelete"]');
        if (btn) {
            btn.classList.add('disabled');
            btn.style.pointerEvents = 'none';
            btn.innerHTML = '<i class="fa fa-spinner fa-spin"></i> Deleting...';
        }
    }
</script>