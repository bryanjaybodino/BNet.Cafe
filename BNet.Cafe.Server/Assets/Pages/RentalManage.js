// Asynchronously fetch exact duration calculation from the backend ASHX Handler based on amount
function calculateDurationFromAmountBackend(amount) {
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
        ? window.location.pathname.replace(/[^\/]+\.aspx$/i, 'Ashx/CalculateDurationFromAmount.ashx')
        : window.location.pathname.replace(/[^\/]+$/i, 'Ashx/CalculateDurationFromAmount.ashx');

    fetch(endpoint + '?amount=' + amount)
        .then(function (response) {
            if (!response.ok) {
                throw new Error('Network response was not ok');
            }
            return response.json();
        })
        .then(function (data) {
            if (data) {
                if (durationInput) {
                    durationInput.value = data.totalMinutes;
                }

                if (displayTime) {
                    displayTime.innerText = data.formattedTime;
                }

                if (displayAmount) {
                    displayAmount.innerText = data.formattedAmount;
                }
            }
        })
        .catch(function (error) {
            console.error('Error fetching calculated duration from handler:', error);
        });
}

// Automatically compute duration when Admin manually inputs/changes the Amount
function calculateTimeFromAmount() {
    var amountInput = document.querySelector('[id$="TextBox_Amount"]');
    if (!amountInput) return;

    var amount = parseFloat(amountInput.value) || 0;
    calculateDurationFromAmountBackend(amount);
}

// Asynchronously fetch exact rate calculation from the backend ASHX Handler
function calculateRentalPriceBackend(totalMinutes) {
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
        ? window.location.pathname.replace(/[^\/]+\.aspx$/i, 'Ashx/CalculateAmountFromDuration.ashx')
        : window.location.pathname.replace(/[^\/]+$/i, 'Ashx/CalculateAmountFromDuration.ashx');

    fetch(endpoint + '?minutes=' + totalMinutes)
        .then(function (response) {
            if (!response.ok) {
                throw new Error('Network response was not ok');
            }
            return response.json();
        })
        .then(function (data) {
            if (data) {
                if (displayTime) {
                    displayTime.innerText = data.formattedTime;
                }

                if (displayAmount) {
                    displayAmount.innerText = data.formattedAmount;
                }

                if (amountInput) {
                    amountInput.value = data.totalAmount.toFixed(2);
                }
            }
        })
        .catch(function (error) {
            console.error('Error fetching calculated price from handler:', error);
        });
}

// Update calculated amount & formatted hours display in real-time
function calculateAmount() {
    var durationInput = document.querySelector('[id$="TextBox_Duration"]');
    var initialDurationInput = document.querySelector('[id$="HiddenField_InitialDuration"]');
    var initialAmountInput = document.querySelector('[id$="HiddenField_InitialAmount"]');
    var amountInput = document.querySelector('[id$="TextBox_Amount"]');
    var displayTime = document.getElementById('display_FormattedTime');
    var displayAmount = document.getElementById('display_TotalAmount');

    if (!durationInput) return;

    var currentMinutes = parseInt(durationInput.value, 10) || 0;
    var initialMinutes = initialDurationInput ? (parseInt(initialDurationInput.value, 10) || 0) : 0;
    var initialAmount = initialAmountInput ? (parseFloat(initialAmountInput.value) || 0) : 0;

    // Check if extra minutes were added
    var extendedMinutes = currentMinutes - initialMinutes;

    if (extendedMinutes > 0) {
        // Fetch price calculation ONLY for the added extended minutes
        var endpoint = /\.aspx$/i.test(window.location.pathname)
            ? window.location.pathname.replace(/[^\/]+\.aspx$/i, 'Ashx/CalculateAmountFromDuration.ashx')
            : window.location.pathname.replace(/[^\/]+$/i, 'Ashx/CalculateAmountFromDuration.ashx');

        fetch(endpoint + '?minutes=' + extendedMinutes)
            .then(function (response) { return response.json(); })
            .then(function (data) {
                if (data) {
                    var totalCharge = initialAmount + data.totalAmount;

                    if (amountInput) amountInput.value = totalCharge.toFixed(2);
                    if (displayAmount) displayAmount.innerText = '₱ ' + totalCharge.toFixed(2);

                    // Format current total time
                    var hrs = Math.floor(currentMinutes / 60);
                    var mins = currentMinutes % 60;
                    if (displayTime) displayTime.innerText = hrs + ' hrs ' + mins + ' mins';
                }
            })
            .catch(function (error) {
                console.error('Error fetching calculated extension price:', error);
            });
    } else {
        // Session has not been extended beyond initial duration
        if (amountInput) amountInput.value = initialAmount.toFixed(2);
        if (displayAmount) displayAmount.innerText = '₱ ' + initialAmount.toFixed(2);

        var hrs = Math.floor(currentMinutes / 60);
        var mins = currentMinutes % 60;
        if (displayTime) displayTime.innerText = hrs + ' hrs ' + mins + ' mins';
    }
}

// Quick action buttons logic
function adjustDuration(amount) {
    var durationInput = document.querySelector('[id$="TextBox_Duration"]');
    if (!durationInput) return;

    var current = parseInt(durationInput.value, 10) || 0;
    var updated = current + amount;

    if (updated < 0) updated = 0;

    durationInput.value = updated;
    calculateAmount();
}

function resetDuration() {
    var durationInput = document.querySelector('[id$="TextBox_Duration"]');
    var amountInput = document.querySelector('[id$="TextBox_Amount"]');

    if (durationInput) durationInput.value = 0;
    if (amountInput) amountInput.value = "0.00";

    calculateAmount();
}

// Client validation before submission
function Validate() {
    var computerInput = document.querySelector('[id$="TextBox_ComputerName"]');
    var customerSelect = document.querySelector('[id$="DropDownList_Customer"]');
    var durationInput = document.querySelector('[id$="TextBox_Duration"]');
    var amountInput = document.querySelector('[id$="TextBox_Amount"]');

    var errors = [];

    if (!durationInput || parseInt(durationInput.value, 10) <= 0) {
        errors.push('Please select or input a valid duration.');
    }

    if (!amountInput || parseFloat(amountInput.value) <= 0) {
        errors.push('Calculated amount must be greater than 0.');
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

// Maintain UI state on ASP.NET WebForms UpdatePanel partial postbacks
if (typeof Sys !== 'undefined' && Sys.WebForms && Sys.WebForms.PageRequestManager) {
    var prm = Sys.WebForms.PageRequestManager.getInstance();
    prm.add_pageLoaded(function () {
        calculateAmount();
    });
}