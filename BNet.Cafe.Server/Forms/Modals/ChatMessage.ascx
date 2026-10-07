<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="ChatMessage.ascx.cs" Inherits="BNet.Cafe.Server.Forms.Modals.ChatMessage" %>

<input type="hidden" id="chatMessageModal_DeleteId" />

<!-- BNet Message View & Delete Modal -->
<div id="chatMessageModal" class="bnet-modal-overlay">
    <div class="bnet-modal-container">
        <div class="bnet-modal-header">
            <h3 class="bnet-modal-title"><i class="fas fa-comment-dots"></i> Message Details</h3>
            <span class="bnet-modal-close" onclick="closeChatMessageModal()">&times;</span>
        </div>
        <div class="bnet-modal-body">
            <div style="margin-bottom: 12px; display: flex; justify-content: space-between; align-items: center; border-bottom: 1px solid rgba(255,255,255,0.1); padding-bottom: 8px;">
                <div><strong>Client:</strong> <span id="chatModalComputerName"></span></div>
                <div style="font-size: 12px; opacity: 0.8;"><span id="chatModalTime"></span></div>
            </div>
            <div style="margin-bottom: 12px;">
                <label style="font-size: 12px; opacity: 0.7; display: block; margin-bottom: 4px;">Message:</label>
                <textarea id="chatModalMessageText" readonly rows="8" style="width: 100%; background: rgba(0,0,0,0.2); color: inherit; padding: 10px; border-radius: 6px; border: 1px solid rgba(255,255,255,0.1); word-break: break-word; resize: vertical; box-sizing: border-box; font-family: monospace; line-height: 1.4;"></textarea>
            </div>
            <p style="color: var(--text-light-secondary); font-size: 13px; margin-top: 10px;">Are you sure you want to delete this message?</p>
        </div>
        <div class="bnet-modal-footer">
            <span class="btn btn-secondary" onclick="closeChatMessageModal()">Cancel</span>
            <button id="btnConfirmDeleteChat" type="button" class="btn btn-danger" onclick="executeChatMessageDelete()">Delete</button>
        </div>
    </div>
</div>

<script type="text/javascript">
    let chatMessageModalInstance = null;

    function getChatMessageModal() {
        if (!chatMessageModalInstance) {
            chatMessageModalInstance = new BNetModal('#chatMessageModal');
        }
        return chatMessageModalInstance;
    }

    function formatTextForTextArea(str) {
        if (!str) return '';

        // Sanitize or clean the input string before regex matching
        let cleaned = str.trim();

        let items = [];
        let matches = cleaned.matchAll(/([0-9A-Z\s\-\(\)]+?\s+X\s+\d+)/gi);

        for (const match of matches) {
            let itemStr = match[0].trim();
            if (itemStr) items.push(itemStr);
        }

        // Construct content list
        const content = items.length > 0 ? items.join("\n") : cleaned;

        return content;
    }

    function openChatMessageModal(id, computerName, message, formattedTime) {
        document.getElementById('chatMessageModal_DeleteId').value = id || '';
        document.getElementById('chatModalComputerName').innerText = computerName || 'Terminal';

        // Populate textarea using .value property
        document.getElementById('chatModalMessageText').value = formatTextForTextArea(message || '');
        document.getElementById('chatModalTime').innerText = formattedTime || '';

        // Reset delete button state
        const btn = document.getElementById('btnConfirmDeleteChat');
        if (btn) {
            btn.disabled = false;
            btn.style.pointerEvents = 'auto';
            btn.innerHTML = 'Delete';
        }

        getChatMessageModal().open();
    }

    function closeChatMessageModal() {
        getChatMessageModal().close();
    }

    // Pure Client-Side Delete Function
    function executeChatMessageDelete() {
        const msgId = document.getElementById('chatMessageModal_DeleteId').value;
        if (!msgId) return;

        const btn = document.getElementById('btnConfirmDeleteChat');
        if (btn) {
            btn.disabled = true;
            btn.style.pointerEvents = 'none';
            btn.innerHTML = '<i class="fa fa-spinner fa-spin"></i> Deleting...';
        }

        // Call the ASHX Handler via JS fetch
        if (typeof deleteSingleMessageJS === 'function') {
            deleteSingleMessageJS(msgId);
        } else {
            console.error("deleteSingleMessageJS function is missing from RemoteMessaging.js");
        }
    }

    // Re-initialize modal reference on ASP.NET AJAX Partial PostBacks
    if (typeof Sys !== 'undefined' && Sys.WebForms && Sys.WebForms.PageRequestManager) {
        Sys.WebForms.PageRequestManager.getInstance().add_endRequest(function () {
            chatMessageModalInstance = new BNetModal('#chatMessageModal');
        });
    }
</script>