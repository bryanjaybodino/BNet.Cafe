/**
 * Validates the User Edit form fields, applies error styles, and triggers alert
 * @returns {boolean} True if valid, false to cancel ASP.NET postback
 */
function Validate() {
    var nameInput = document.querySelector('[id$="TextBox_Name"]');
    var emailInput = document.querySelector('[id$="TextBox_Email"]');
    var passwordInput = document.querySelector('[id$="TextBox_Password"]');

    var errors = [];
    var emailRegex = /^[\w-\.]+@([\w-]+\.)+[\w-]{2,4}$/;

    // Reset validation states
    if (nameInput) nameInput.classList.remove('is-invalid', 'is-valid');
    if (emailInput) emailInput.classList.remove('is-invalid', 'is-valid');
    if (passwordInput) passwordInput.classList.remove('is-invalid', 'is-valid');

    // Name Validation
    if (!nameInput || !nameInput.value.trim()) {
        if (nameInput) nameInput.classList.add('is-invalid');
        errors.push('Please enter a full name.');
    } else {
        nameInput.classList.add('is-valid');
    }

    // Email Validation
    if (!emailInput || !emailInput.value.trim()) {
        if (emailInput) emailInput.classList.add('is-invalid');
        errors.push('Please enter an email address.');
    } else if (!emailRegex.test(emailInput.value.trim())) {
        emailInput.classList.add('is-invalid');
        errors.push('Please enter a valid email address.');
    } else {
        emailInput.classList.add('is-valid');
    }

    // Password Validation
    if (!passwordInput || !passwordInput.value.trim()) {
        if (passwordInput) passwordInput.classList.add('is-invalid');
        errors.push('Please enter a password.');
    } else {
        passwordInput.classList.add('is-valid');
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
}