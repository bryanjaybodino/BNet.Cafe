function ValidateTopUp() {
    var amountInput = document.querySelector('[id$="TextBox_Amount"]');
    var durationInput = document.querySelector('[id$="TextBox_Duration"]');

    var errors = [];

    if (!amountInput || parseFloat(amountInput.value) <= 0) {
        errors.push('Please enter a valid amount paid.');
    }

    if (!durationInput || parseInt(durationInput.value, 10) <= 0) {
        errors.push('The entered amount does not result in any time credit.');
    }

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
