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
                <label style="font-size: 12px; opacity: 0.7; display: block; margin-bottom: 4px;">Message / Order:</label>
                <!-- Container for message or parsed order table -->
                <div id="chatModalMessageContainer" style="width: 100%; max-height: 250px; overflow-y: auto; background: rgba(0,0,0,0.2); color: inherit; padding: 10px; border-radius: 6px; border: 1px solid rgba(255,255,255,0.1); word-break: break-word; box-sizing: border-box; line-height: 1.4;"></div>
            </div>
            <p style="color: var(--text-light-secondary); font-size: 13px; margin-top: 10px;">Are you sure you want to delete this message?</p>
        </div>
        <div class="bnet-modal-footer" style="display: flex; gap: 8px; align-items: center; justify-content: flex-end;">
            <button id="btnProcessOrderPOS" type="button" class="btn btn-primary" style="display: none;" onclick="openPOSWithOrder()"><i class="fa fa-cart-shopping"></i> Process in POS</button>
            <span class="btn btn-secondary" onclick="closeChatMessageModal()">Cancel</span>
            <button id="btnConfirmDeleteChat" type="button" class="btn btn-danger" onclick="executeChatMessageDelete()">Delete</button>
        </div>
    </div>
</div>

<script type="text/javascript">
    let chatMessageModalInstance = null;
    let currentParsedOrderItems = [];

    function getChatMessageModal() {
        if (!chatMessageModalInstance) {
            chatMessageModalInstance = new BNetModal('#chatMessageModal');
        }
        return chatMessageModalInstance;
    }

    function renderFormattedMessage(str) {
        currentParsedOrderItems = [];
        if (!str) {
            document.getElementById('btnProcessOrderPOS').style.display = 'none';
            return '';
        }

        let cleaned = str.trim();
        // Regex matches "ITEM_NAME x QUANTITY" or "ITEM_NAME X QUANTITY" pattern
        const orderRegex = /(.+?)\s+[xX]\s+(\d+)/gi;
        let matches = [...cleaned.matchAll(orderRegex)];

        if (matches.length > 0) {
            let tableHtml = `
                <table style="width: 100%; border-collapse: collapse; text-align: left; font-size: 13px;">
                    <thead>
                        <tr style="border-bottom: 1px solid rgba(255,255,255,0.2);">
                            <th style="padding: 6px 8px;">Item Description</th>
                            <th style="padding: 6px 8px; text-align: center; width: 80px;">Qty</th>
                        </tr>
                    </thead>
                    <tbody>`;

            matches.forEach(match => {
                const item = match[1].trim();
                const qty = parseInt(match[2].trim(), 10);

                // Save parsed items to array for passing into POS URL
                currentParsedOrderItems.push({ name: item, qty: qty });

                tableHtml += `
                    <tr style="border-bottom: 1px solid rgba(255,255,255,0.05);">
                        <td style="padding: 6px 8px;">${escapeHtml(item)}</td>
                        <td style="padding: 6px 8px; text-align: center; font-weight: bold;">${escapeHtml(qty.toString())}</td>
                    </tr>`;
            });

            tableHtml += `</tbody></table>`;

            // Show the "Process in POS" button since an order was detected
            document.getElementById('btnProcessOrderPOS').style.display = 'inline-flex';
            return tableHtml;
        }

        // Hide button if it's just a regular chat message
        document.getElementById('btnProcessOrderPOS').style.display = 'none';
        return `<div style="white-space: pre-wrap; font-family: inherit;">${escapeHtml(cleaned)}</div>`;
    }

    function escapeHtml(text) {
        const div = document.createElement('div');
        div.innerText = text;
        return div.innerHTML;
    }

    function openChatMessageModal(id, computerName, message, formattedTime) {
        document.getElementById('chatMessageModal_DeleteId').value = id || '';
        document.getElementById('chatModalComputerName').innerText = computerName || 'Terminal';

        document.getElementById('chatModalMessageContainer').innerHTML = renderFormattedMessage(message || '');
        document.getElementById('chatModalTime').innerText = formattedTime || '';

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

    function openPOSWithOrder() {
        const pcName = document.getElementById('chatModalComputerName').innerText;
        if (!pcName) return;

        // Encode parsed order items into query parameter string
        let orderPayload = encodeURIComponent(JSON.stringify(currentParsedOrderItems));
        window.location.href = `BNetPage.aspx?Form=POS&ComputerName=${encodeURIComponent(pcName)}&OrderItems=${orderPayload}`;
    }

    function executeChatMessageDelete() {
        const msgId = document.getElementById('chatMessageModal_DeleteId').value;
        if (!msgId) return;

        const btn = document.getElementById('btnConfirmDeleteChat');
        if (btn) {
            btn.disabled = true;
            btn.style.pointerEvents = 'none';
            btn.innerHTML = '<i class="fa fa-spinner fa-spin"></i> Deleting...';
        }

        if (typeof deleteSingleMessageJS === 'function') {
            deleteSingleMessageJS(msgId);
        } else {
            console.error("deleteSingleMessageJS function is missing from RemoteMessaging.js");
        }
    }

    if (typeof Sys !== 'undefined' && Sys.WebForms && Sys.WebForms.PageRequestManager) {
        Sys.WebForms.PageRequestManager.getInstance().add_endRequest(function () {
            chatMessageModalInstance = new BNetModal('#chatMessageModal');
        });
    }
</script>