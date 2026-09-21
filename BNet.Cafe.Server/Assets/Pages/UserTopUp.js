function formatMinutes(totalMinutes) {
    var mins = parseInt(totalMinutes, 10) || 0;
    if (mins <= 0) return "0 hrs 0 mins";

    var hours = Math.floor(mins / 60);
    var remainingMins = mins % 60;

    var hrsText = hours === 1 ? "1 hr" : hours + " hrs";
    var minsText = remainingMins === 1 ? "1 min" : remainingMins + " mins";

    if (hours === 0) return minsText;
    if (remainingMins === 0) return hrsText;
    return hrsText + " " + minsText;
}

// Automatically compute duration when Admin inputs/changes the Amount
function calculateTimeFromAmount() {
    var amountInput = document.querySelector('[id$="TextBox_Amount"]');
    if (!amountInput) return;

    var amount = parseFloat(amountInput.value) || 0;
    CalculateDurationFromAmountHandlerBackend(amount);
}

// Automatically compute amount when Admin inputs/changes the Duration (Minutes)
function calculateAmountFromTime() {
    var durationInput = document.querySelector('[id$="TextBox_Duration"]');
    if (!durationInput) return;

    var totalMinutes = parseInt(durationInput.value, 10) || 0;
    CalculateAmountFromDurationHandlerBackend(totalMinutes);
}

// Call backend handler for Amount -> Duration conversion
function CalculateDurationFromAmountHandlerBackend(amount) {
    var durationInput = document.querySelector('[id$="TextBox_Duration"]');
    var displayTime = document.getElementById('display_FormattedTime');
    var displayAmount = document.getElementById('display_TotalAmount');

    if (!amount || amount <= 0) {
        if (displayTime) displayTime.innerText = "0 hrs 0 mins";
        if (displayAmount) displayAmount.innerText = "₱ 0.00";
        if (durationInput) durationInput.value = "0";
        return;
    }

    var endpoint = /\.aspx$/i.test(window.location.pathname)
        ? window.location.pathname.replace(/[^\/]+\.aspx$/i, 'Ashx/CalculateDurationFromAmountHandler.ashx')
        : window.location.pathname.replace(/[^\/]+$/i, 'Ashx/CalculateDurationFromAmountHandler.ashx');

    fetch(endpoint + '?amount=' + amount + '&customerType=USER')
        .then(function (response) {
            if (!response.ok) {
                throw new Error('Network response was not ok');
            }
            return response.json();
        })
        .then(function (data) {
            if (data) {
                var minutes = data.totalMinutes || 0;

                if (durationInput) {
                    durationInput.value = minutes;
                }

                if (displayTime) {
                    displayTime.innerText = data.formattedTime || formatMinutes(minutes);
                }

                if (displayAmount) {
                    displayAmount.innerText = data.formattedAmount || ("₱ " + parseFloat(amount).toFixed(2));
                }
            }
        })
        .catch(function (error) {
            console.error('Error fetching calculated duration from handler:', error);
        });
}

// Call backend handler for Duration -> Amount conversion
function CalculateAmountFromDurationHandlerBackend(totalMinutes) {
    var amountInput = document.querySelector('[id$="TextBox_Amount"]');
    var displayTime = document.getElementById('display_FormattedTime');
    var displayAmount = document.getElementById('display_TotalAmount');

    if (!totalMinutes || totalMinutes <= 0) {
        if (displayTime) displayTime.innerText = "0 hrs 0 mins";
        if (displayAmount) displayAmount.innerText = "₱ 0.00";
        if (amountInput) amountInput.value = "0.00";
        return;
    }

    var endpoint = /\.aspx$/i.test(window.location.pathname)
        ? window.location.pathname.replace(/[^\/]+\.aspx$/i, 'Ashx/CalculateAmountFromDurationHandler.ashx')
        : window.location.pathname.replace(/[^\/]+$/i, 'Ashx/CalculateAmountFromDurationHandler.ashx');

    fetch(endpoint + '?minutes=' + totalMinutes + '&customerType=USER')
        .then(function (response) {
            if (!response.ok) {
                throw new Error('Network response was not ok');
            }
            return response.json();
        })
        .then(function (data) {
            if (data) {
                if (displayTime) {
                    displayTime.innerText = data.formattedTime || formatMinutes(totalMinutes);
                }

                if (displayAmount) {
                    displayAmount.innerText = data.formattedAmount || ("₱ " + parseFloat(data.totalAmount).toFixed(2));
                }

                if (amountInput && data.totalAmount !== undefined) {
                    amountInput.value = parseFloat(data.totalAmount).toFixed(2);
                }
            }
        })
        .catch(function (error) {
            console.error('Error fetching calculated price from handler:', error);
        });
}

// Client validation
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
        btn.innerHTML = '<i class="fa fa-spinner fa-spin"></i> Processing...';
    }
}

// Re-run initial calculation on ASP.NET WebForms UpdatePanel partial postbacks
if (typeof Sys !== 'undefined' && Sys.WebForms && Sys.WebForms.PageRequestManager) {
    var prm = Sys.WebForms.PageRequestManager.getInstance();
    prm.add_pageLoaded(function () {
        calculateTimeFromAmount();
    });
}