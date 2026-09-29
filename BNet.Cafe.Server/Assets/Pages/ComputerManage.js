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

if (typeof Sys !== 'undefined' && Sys.WebForms && Sys.WebForms.PageRequestManager) {
    var prm = Sys.WebForms.PageRequestManager.getInstance();

    // Triggered after UpdatePanel content updates
    prm.add_pageLoaded(function (sender, args) {
        var panelsUpdated = args.get_panelsUpdated();
        if (panelsUpdated.length > 0) {
            // Re-disable button during redirect window if needed
            disableSubmitButton();
        }
    });

    // Triggered when an async postback finishes (success or error)
    prm.add_endRequest(function (sender, args) {
        var btn = document.querySelector('[id$="LinkButton_Submit"]');
        if (btn) {
            btn.classList.remove('disabled');
            btn.style.pointerEvents = 'auto';
            btn.innerHTML = 'Submit'; // Restore original text/HTML
        }
    });
}