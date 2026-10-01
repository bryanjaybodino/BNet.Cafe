/**
 * Validates the Supplier form fields (Supplier Name, Contact Person, Phone, Email, Address),
 * applies error states, and triggers alerts.
 * @returns {boolean} True if valid, false to cancel postback.
 */
function ValidateSupplier() {
    var supplierNameInput = document.querySelector('[id$="TextBox_SupplierName"]');
    var contactPersonInput = document.querySelector('[id$="TextBox_ContactPerson"]');
    var phoneInput = document.querySelector('[id$="TextBox_Phone"]');
    var emailInput = document.querySelector('[id$="TextBox_Email"]');
    var addressInput = document.querySelector('[id$="TextBox_Address"]');

    var errors = [];
    var emailRegex = /^[\w-\.]+@([\w-]+\.)+[\w-]{2,4}$/;

    // Reset validation styles
    if (supplierNameInput) supplierNameInput.classList.remove('is-invalid', 'is-valid');
    if (contactPersonInput) contactPersonInput.classList.remove('is-invalid', 'is-valid');
    if (phoneInput) phoneInput.classList.remove('is-invalid', 'is-valid');
    if (emailInput) emailInput.classList.remove('is-invalid', 'is-valid');
    if (addressInput) addressInput.classList.remove('is-invalid', 'is-valid');

    // Supplier Name (Required)
    if (!supplierNameInput || !supplierNameInput.value.trim()) {
        if (supplierNameInput) supplierNameInput.classList.add('is-invalid');
        errors.push('Please enter a supplier name.');
    } else {
        supplierNameInput.classList.add('is-valid');
    }

    // Optional Email Format Check (If provided)
    if (emailInput && emailInput.value.trim() !== '') {
        if (!emailRegex.test(emailInput.value.trim())) {
            emailInput.classList.add('is-invalid');
            errors.push('Please enter a valid email address.');
        } else {
            emailInput.classList.add('is-valid');
        }
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