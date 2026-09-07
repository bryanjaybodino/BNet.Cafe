function Validate() {
    var computerSelect = document.querySelector('[id$="DropDownList_Computer"]');
    var durationInput = document.querySelector('[id$="TextBox_Duration"]');
    var amountInput = document.querySelector('[id$="TextBox_Amount"]');

    var errors = [];

    // 1. Validate Computer Selection using requiredValidation
    if (!computerSelect || !requiredValidation(computerSelect.id) || computerSelect.value === '0') {
        errors.push('Please select a computer.');
    }

    // 2. Validate Duration
    if (!durationInput || !requiredValidation(durationInput.id) || parseFloat(durationInput.value) <= 0) {
        errors.push('Please enter a valid duration in minutes.');
    }

    // 3. Validate Amount
    if (!amountInput || !requiredValidation(amountInput.id) || parseFloat(amountInput.value) < 0) {
        errors.push('Please enter a valid amount.');
    }

    // If any validation errors exist, alert all of them and prevent postback
    if (errors.length > 0) {
        ShowAlert(errors);
        return false;
    }

    // Disable button and add loading spinner upon successful client-side validation
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

// Keep the button disabled after UpdatePanel updates
if (typeof Sys !== 'undefined' && Sys.WebForms && Sys.WebForms.PageRequestManager) {
    var prm = Sys.WebForms.PageRequestManager.getInstance();
    // After UpdatePanel content updates on page
    prm.add_pageLoaded(function (sender, args) {
        var panelsUpdated = args.get_panelsUpdated();
        if (panelsUpdated.length > 0) {
            // Re-disable button during the 1.5s AlertService redirect window
            disableSubmitButton();
        }
    });
}