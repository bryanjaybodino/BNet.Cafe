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