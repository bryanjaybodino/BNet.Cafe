/**
 * Validates the Inventory Item form fields, applies error styles, and triggers alert.
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
        btn.innerHTML = '<i class="fa-solid fa-spinner fa-spin"></i> Saving...';
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
            btn.innerHTML = '<i class="fa-solid fa-floppy-disk"></i> Save Changes';
        }
    });
}




var selectedInventoryImage = window.selectedInventoryImage || null;

document.addEventListener('DOMContentLoaded', function () {
    syncInventoryImageState();

    if (typeof Sys !== 'undefined' && Sys.WebForms && Sys.WebForms.PageRequestManager) {
        Sys.WebForms.PageRequestManager.getInstance().add_endRequest(function () {
            syncInventoryImageState();
        });
    }
});

// Drag & Drop / Click Delegate
document.addEventListener('click', function (e) {
    const dropZone = e.target.closest('#inventoryDropZone');
    const fileInput = document.getElementById('inventoryFileInput');
    if (dropZone && fileInput && e.target !== fileInput && !e.target.closest('.remove-img-btn')) {
        fileInput.click();
    }
});

document.addEventListener('change', function (e) {
    if (e.target && e.target.id === 'inventoryFileInput') {
        if (e.target.files && e.target.files[0]) {
            handleSingleInventoryFile(e.target.files[0]);
        }
        e.target.value = '';
    }
});

document.addEventListener('dragover', function (e) {
    const dropZone = e.target.closest('#inventoryDropZone');
    if (dropZone) {
        e.preventDefault();
        dropZone.classList.add('dragover');
    }
});

document.addEventListener('dragleave', function (e) {
    const dropZone = e.target.closest('#inventoryDropZone');
    if (dropZone) {
        e.preventDefault();
        dropZone.classList.remove('dragover');
    }
});

document.addEventListener('drop', function (e) {
    const dropZone = e.target.closest('#inventoryDropZone');
    if (dropZone) {
        e.preventDefault();
        dropZone.classList.remove('dragover');
        if (e.dataTransfer && e.dataTransfer.files && e.dataTransfer.files[0]) {
            handleSingleInventoryFile(e.dataTransfer.files[0]);
        }
    }
});

function handleSingleInventoryFile(file) {
    const validTypes = ['image/jpeg', 'image/png', 'image/webp'];
    const maxSize = 5 * 1024 * 1024; // 5MB

    if (!validTypes.includes(file.type)) {
        const msg = `"${file.name}" is not a valid image format (JPG, PNG, WEBP).`;
        typeof ShowAlert === 'function' ? ShowAlert(msg) : alert(msg);
        return;
    }

    if (file.size > maxSize) {
        const msg = `"${file.name}" exceeds the 5MB limit.`;
        typeof ShowAlert === 'function' ? ShowAlert(msg) : alert(msg);
        return;
    }

    const reader = new FileReader();
    reader.onload = function (e) {
        selectedInventoryImage = e.target.result;
        renderSingleImagePreview();
    };
    reader.readAsDataURL(file);
}

function renderSingleImagePreview() {
    const placeholder = document.getElementById('uploadPlaceholder');
    const previewWrapper = document.getElementById('uploadPreviewWrapper');
    const previewImg = document.getElementById('inventoryPreviewImg');
    const hiddenField = document.querySelector('[id$="HiddenField_ImageData"]');

    if (!placeholder || !previewWrapper || !previewImg) return;

    if (selectedInventoryImage) {
        previewImg.src = selectedInventoryImage;
        if (hiddenField) hiddenField.value = selectedInventoryImage;
        placeholder.classList.add('d-none');
        previewWrapper.classList.remove('d-none');
    } else {
        previewImg.src = '';
        if (hiddenField) hiddenField.value = '';
        previewWrapper.classList.add('d-none');
        placeholder.classList.remove('d-none');
    }
}

function clearInventoryImage(e) {
    if (e) e.stopPropagation();
    selectedInventoryImage = null;
    renderSingleImagePreview();
}

function syncInventoryImageState() {
    const hiddenField = document.querySelector('[id$="HiddenField_ImageData"]');
    if (hiddenField && hiddenField.value) {
        selectedInventoryImage = hiddenField.value;
        renderSingleImagePreview();
    }
}