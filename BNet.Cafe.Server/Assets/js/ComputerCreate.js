/**
 * Validates the Computer Creation form fields, disables submit button, and shows loading indicator
 * @returns {boolean} True if valid, false to cancel ASP.NET postback
 */
function Validate() {
    var computerInput = document.querySelector('[id$="TextBox_ComputerName"]');
    var inputId = computerInput ? computerInput.id : 'TextBox_ComputerName';

    var isValidComputer = requiredValidation(inputId);

    if (!isValidComputer) {
        ShowAlert('Please enter a computer name.');
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