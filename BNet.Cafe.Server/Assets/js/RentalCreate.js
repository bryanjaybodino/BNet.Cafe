/**
 * Validates the Rental Creation form fields, disables submit button, and shows loading indicator
 * @returns {boolean} True if valid, false to cancel ASP.NET postback
 */
function Validate() {
    var computerSelect = document.querySelector('[id$="DropDownList_Computer"]');
    var durationInput = document.querySelector('[id$="TextBox_Duration"]');
    var amountInput = document.querySelector('[id$="TextBox_Amount"]');

    var errors = [];

    // 1. Validate Computer Selection
    if (!computerSelect || !computerSelect.value || computerSelect.value === '0' || computerSelect.value === '') {
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
        ShowAlert(errors.join('\n'));
        return false;
    }

    // Disable button and add loading spinner upon successful client-side validation
    var submitBtn = document.querySelector('[id$="LinkButton_Submit"]');
    if (submitBtn) {
        submitBtn.classList.add('disabled');
        submitBtn.style.pointerEvents = 'none';
        submitBtn.innerHTML = '<i class="fa-solid fa-spinner fa-spin"></i> Processing...';
    }

    return true;
}