function ValidatePOS() {
    var itemId = document.querySelector('[id$="HiddenField_SelectedItemId"]');
    var qtyInput = document.querySelector('[id$="TextBox_Quantity"]');

    if (!itemId || !itemId.value) {
        alert('Please select an item from the catalog first.');
        return false;
    }

    var qty = parseInt(qtyInput.value) || 0;
    if (qty <= 0) {
        alert('Quantity must be greater than 0.');
        return false;
    }

    disablePOSSubmit();
    return true;
}

function disablePOSSubmit() {
    var btn = document.querySelector('[id$="LinkButton_Submit"]');
    if (btn) {
        btn.classList.add('disabled');
        btn.style.pointerEvents = 'none';
        btn.innerHTML = '<i class="fa fa-spinner fa-spin"></i> Processing Sale...';
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