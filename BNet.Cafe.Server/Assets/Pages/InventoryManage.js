/**
 * Validates the Inventory form fields, applies error styles, and triggers alerts.
 * @returns {boolean} True if valid, false to cancel ASP.NET postback
 */
function ValidateInventory() {
    var itemNameInput = document.querySelector('[id$="TextBox_ItemName"]');
    var unitPriceInput = document.querySelector('[id$="TextBox_UnitPrice"]');
    var quantityInput = document.querySelector('[id$="TextBox_QuantityInStock"]');
    var reorderLevelInput = document.querySelector('[id$="TextBox_ReorderLevel"]');

    var errors = [];

    // Reset validation states
    if (itemNameInput) itemNameInput.classList.remove('is-invalid', 'is-valid');
    if (unitPriceInput) unitPriceInput.classList.remove('is-invalid', 'is-valid');
    if (quantityInput) quantityInput.classList.remove('is-invalid', 'is-valid');
    if (reorderLevelInput) reorderLevelInput.classList.remove('is-invalid', 'is-valid');

    // Item Name Validation
    if (!itemNameInput || !itemNameInput.value.trim()) {
        if (itemNameInput) itemNameInput.classList.add('is-invalid');
        errors.push('Please enter an item name.');
    } else {
        itemNameInput.classList.add('is-valid');
    }

    // Unit Price Validation
    if (!unitPriceInput || !unitPriceInput.value.trim() || isNaN(unitPriceInput.value.trim()) || parseFloat(unitPriceInput.value.trim()) < 0) {
        if (unitPriceInput) unitPriceInput.classList.add('is-invalid');
        errors.push('Please enter a valid unit price.');
    } else {
        unitPriceInput.classList.add('is-valid');
    }

    // Quantity In Stock Validation
    if (!quantityInput || !quantityInput.value.trim() || isNaN(quantityInput.value.trim()) || parseInt(quantityInput.value.trim(), 10) < 0) {
        if (quantityInput) quantityInput.classList.add('is-invalid');
        errors.push('Please enter a valid stock quantity.');
    } else {
        quantityInput.classList.add('is-valid');
    }

    // Reorder Level Validation
    if (!reorderLevelInput || !reorderLevelInput.value.trim() || isNaN(reorderLevelInput.value.trim()) || parseInt(reorderLevelInput.value.trim(), 10) < 0) {
        if (reorderLevelInput) reorderLevelInput.classList.add('is-invalid');
        errors.push('Please enter a valid reorder level.');
    } else {
        reorderLevelInput.classList.add('is-valid');
    }

    // Handle Errors
    if (errors.length > 0) {
        if (typeof ShowAlert === 'function') {
            ShowAlert(errors);
        } else {
            alert(errors.join('\n'));
        }
        return false;
    }

    disableSubmitButton();
    return true;
}

function disableSubmitButton() {
    var btn = document.querySelector('[id$="LinkButton_Submit"]');
    if (btn) {
        btn.classList.add('disabled');
        btn.style.pointerEvents = 'none';
        btn.innerHTML = '<i class="fa fa-spinner fa-spin"></i> Saving...';
    }
}

if (typeof Sys !== 'undefined' && Sys.WebForms && Sys.WebForms.PageRequestManager) {
    var prm = Sys.WebForms.PageRequestManager.getInstance();

    prm.add_pageLoaded(function (sender, args) {
        var panelsUpdated = args.get_panelsUpdated();
        if (panelsUpdated.length > 0) {
            disableSubmitButton();
        }
    });

    prm.add_endRequest(function (sender, args) {
        var btn = document.querySelector('[id$="LinkButton_Submit"]');
        if (btn) {
            btn.classList.remove('disabled');
            btn.style.pointerEvents = 'auto';
            btn.innerHTML = '<i class="fa fa-save"></i> Save Changes';
        }
    });
}