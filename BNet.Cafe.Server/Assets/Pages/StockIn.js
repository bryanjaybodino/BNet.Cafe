function ValidateStockIn() {
    var itemSelect = document.querySelector('[id$="DropDownList_Item"]');
    var qtyInput = document.querySelector('[id$="TextBox_Quantity"]');

    if (!itemSelect || !itemSelect.value) {
        alert('Please select an item to restock.');
        return false;
    }

    var qty = parseInt(qtyInput.value) || 0;
    if (qty <= 0) {
        alert('Please enter a valid stock quantity greater than 0.');
        return false;
    }

    disableStockInSubmit();
    return true;
}

function disableStockInSubmit() {
    var btn = document.querySelector('[id$="LinkButton_Submit"]');
    if (btn) {
        btn.classList.add('disabled');
        btn.style.pointerEvents = 'none';
        btn.innerHTML = '<i class="fa fa-spinner fa-spin"></i> Processing...';
    }
}

// Re-enable submit button after ASP.NET AJAX Partial PostBack completes
if (typeof Sys !== 'undefined' && Sys.WebForms && Sys.WebForms.PageRequestManager) {
    var prm = Sys.WebForms.PageRequestManager.getInstance();

    prm.add_endRequest(function (sender, args) {
        var btn = document.querySelector('[id$="LinkButton_Submit"]');
        if (btn) {
            btn.classList.remove('disabled');
            btn.style.pointerEvents = 'auto';
        }
    });
}