<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="POSReceipt.ascx.cs" Inherits="BNet.Cafe.Server.Forms.Modals.POSReceipt" %>

<div id="posReceiptModal" class="bnet-modal-overlay">
    <div class="bnet-modal-container" style="max-width: 420px;">
        <div class="bnet-modal-header">
            <h3 class="bnet-modal-title">
                <i class="fa-solid fa-circle-check" style="color: #22c55e;"></i> Transaction Successful
            </h3>
        </div>
        
        <div class="bnet-modal-body" style="display: flex; flex-direction: column; gap: 14px;">
            <!-- Amount Display -->
            <div style="background-color: var(--bg-light-tertiary); padding: 12px; border-radius: 8px; text-align: center;">
                <span style="font-size: 12px; color: var(--text-light-secondary); display: block; margin-bottom: 2px;">Total Amount Due</span>
                <span style="font-size: 26px; font-weight: bold; color: #22c55e;">₱<span id="receiptTotalAmount">0.00</span></span>
            </div>

            <!-- Customer Cash Input -->
            <div>
                <label style="font-size: 13px; font-weight: 600; display: block; margin-bottom: 6px;">Customer Cash (₱)</label>
                <input type="number" id="inputCustomerCash" class="search-input" placeholder="0.00" min="0" step="any" oninput="handleQuantityInput(this);" style="width: 100%; font-size: 16px; font-weight: 600; text-align: right;" />
            </div>

            <!-- Calculated Change Display -->
            <div style="display: flex; justify-content: space-between; align-items: center; background: var(--bg-light-tertiary); padding: 10px 14px; border-radius: 6px;">
                <span style="font-weight: 600; font-size: 14px;">Change:</span>
                <span style="font-size: 18px; font-weight: 700; color: var(--primary);" id="receiptChangeDisplay">₱0.00</span>
            </div>

            <!-- Warning for Insufficient Cash -->
            <div id="cashWarning" style="color: #ef4444; font-size: 12px; display: none; text-align: center;">
                Customer cash is less than the total amount due.
            </div>
        </div>

        <div class="bnet-modal-footer">
            <button type="button" class="btn btn-primary" id="btnDoneReceipt" onclick="closePOSReceiptModal()" style="width: 100%; justify-content: center;" disabled>
                Done
            </button>
        </div>
    </div>
</div>

<script type="text/javascript">
    let posReceiptModalInstance = null;
    let currentReceiptTotal = 0;

    function getPOSReceiptModal() {
        if (!posReceiptModalInstance) {
            posReceiptModalInstance = new BNetModal('#posReceiptModal');
        }
        return posReceiptModalInstance;
    }

    function openPOSReceiptModal(total) {
        currentReceiptTotal = parseFloat(total) || 0;

        // Reset inputs and displays
        document.getElementById('receiptTotalAmount').innerText = currentReceiptTotal.toLocaleString('en-US', { minimumFractionDigits: 2, maximumFractionDigits: 2 });
        const cashInput = document.getElementById('inputCustomerCash');
        cashInput.value = '';
        document.getElementById('receiptChangeDisplay').innerText = '₱0.00';
        document.getElementById('cashWarning').style.display = 'none';
        document.getElementById('btnDoneReceipt').disabled = true;

        // Open Modal using BNetModal instance
        getPOSReceiptModal().open();

        // Prevent closing when user clicks outside the modal on the backdrop overlay
        const overlay = document.getElementById('posReceiptModal');
        if (overlay) {
            // Remove previous event listener to prevent duplication
            overlay.removeEventListener('click', preventBackdropClose, true);
            // Add event listener on CAPTURE phase to intercept click before BNetModal handles it
            overlay.addEventListener('click', preventBackdropClose, true);
        }

        setTimeout(function () {
            if (cashInput) cashInput.focus();
        }, 100);
    }

    function preventBackdropClose(e) {
        const overlay = document.getElementById('posReceiptModal');
        // If the user clicked directly on the outer backdrop/overlay (outside container)
        if (e.target === overlay) {
            e.stopPropagation();
            e.stopImmediatePropagation();
            e.preventDefault();
        }
    }

    function calculatePOSReceiptChange() {
        const cashInput = document.getElementById('inputCustomerCash');
        const cashPaid = parseFloat(cashInput.value) || 0;
        const change = cashPaid - currentReceiptTotal;

        const changeDisplay = document.getElementById('receiptChangeDisplay');
        const warning = document.getElementById('cashWarning');
        const doneBtn = document.getElementById('btnDoneReceipt');

        if (cashPaid >= currentReceiptTotal && currentReceiptTotal > 0) {
            changeDisplay.innerText = '₱' + change.toLocaleString('en-US', { minimumFractionDigits: 2, maximumFractionDigits: 2 });
            changeDisplay.style.color = '#16a34a';
            warning.style.display = 'none';
            doneBtn.disabled = false;
        } else {
            changeDisplay.innerText = '₱0.00';
            changeDisplay.style.color = '#ef4444';
            warning.style.display = (cashPaid > 0 && cashPaid < currentReceiptTotal) ? 'block' : 'none';
            doneBtn.disabled = true;
        }
    }

    function closePOSReceiptModal() {
        getPOSReceiptModal().close();
    }

    // Re-initialize modal reference on ASP.NET AJAX Partial PostBacks
    if (typeof Sys !== 'undefined' && Sys.WebForms && Sys.WebForms.PageRequestManager) {
        Sys.WebForms.PageRequestManager.getInstance().add_endRequest(function () {
            posReceiptModalInstance = new BNetModal('#posReceiptModal');
        });
    }
</script>