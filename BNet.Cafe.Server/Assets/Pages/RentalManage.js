// Asynchronously fetch exact duration calculation from the backend ASHX Handler based on amount
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

    var customerType = getCustomerType();

    var endpoint = /\.aspx$/i.test(window.location.pathname)
        ? window.location.pathname.replace(/[^\/]+\.aspx$/i, 'Ashx/CalculateDurationFromAmountHandler.ashx')
        : window.location.pathname.replace(/[^\/]+$/i, 'Ashx/CalculateDurationFromAmountHandler.ashx');

    fetch(endpoint + '?amount=' + amount + '&customerType=' + encodeURIComponent(customerType))
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
    CalculateDurationFromAmountHandlerBackend(amount);
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

    var customerType = getCustomerType();

    var endpoint = /\.aspx$/i.test(window.location.pathname)
        ? window.location.pathname.replace(/[^\/]+\.aspx$/i, 'Ashx/CalculateAmountFromDurationHandler.ashx')
        : window.location.pathname.replace(/[^\/]+$/i, 'Ashx/CalculateAmountFromDurationHandler.ashx');

    fetch(endpoint + '?minutes=' + totalMinutes + '&customerType=' + encodeURIComponent(customerType))
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


// Calculate amount and handle breakdown display
function calculateAmount() {
    var durationInput = document.querySelector('[id$="TextBox_Duration"]');
    var initialDurationInput = document.querySelector('[id$="HiddenField_InitialDuration"]');
    var initialAmountInput = document.querySelector('[id$="HiddenField_InitialAmount"]');
    var customerSelect = document.querySelector('[id$="DropDownList_Customer"]');
    var amountInput = document.querySelector('[id$="TextBox_Amount"]');

    var displayTime = document.getElementById('display_FormattedTime');
    var displayAmount = document.getElementById('display_TotalAmount');
    var timeBreakdown = document.getElementById('display_TimeBreakdown');
    var amountBreakdown = document.getElementById('display_AmountBreakdown');

    if (!durationInput) return;

    var currentMinutes = parseInt(durationInput.value, 10) || 0;

    // Check if dropdown is empty / unselected / walk-in
    var isWalkIn = !customerSelect || !customerSelect.value || customerSelect.value === "";

    var initialMinutes = (!isWalkIn && initialDurationInput) ? (parseInt(initialDurationInput.value, 10) || 0) : 0;
    var initialAmount = (!isWalkIn && initialAmountInput) ? (parseFloat(initialAmountInput.value) || 0) : 0;

    // PREVENT DEDUCTING BELOW BASE DURATION ONLY FOR EXISTING ACTIVE RENTALS
    if (currentMinutes < initialMinutes) {
        currentMinutes = initialMinutes;
        durationInput.value = initialMinutes;
    }

    // Do not allow total minutes to fall below 0
    if (currentMinutes < 0) {
        currentMinutes = 0;
        durationInput.value = 0;
    }

    var extendedMinutes = currentMinutes - initialMinutes;

    function formatTime(mins) {
        var h = Math.floor(mins / 60);
        var m = mins % 60;
        if (h > 0 && m > 0) return h + ' hrs ' + m + ' mins';
        if (h > 0) return h + ' hrs';
        return m + ' mins';
    }

    if (displayTime) displayTime.innerText = formatTime(currentMinutes);

    // If walk-in or new rental with no initial base, calculate rate directly
    if (initialMinutes === 0) {
        calculateRentalPriceBackend(currentMinutes);
        if (timeBreakdown) timeBreakdown.style.display = 'none';
        if (amountBreakdown) amountBreakdown.style.display = 'none';
        return;
    }

    if (extendedMinutes > 0) {
        var customerType = getCustomerType();

        var endpoint = /\.aspx$/i.test(window.location.pathname)
            ? window.location.pathname.replace(/[^\/]+\.aspx$/i, 'Ashx/CalculateAmountFromDurationHandler.ashx')
            : window.location.pathname.replace(/[^\/]+$/i, 'Ashx/CalculateAmountFromDurationHandler.ashx');

        fetch(endpoint + '?minutes=' + extendedMinutes + '&customerType=' + encodeURIComponent(customerType))
            .then(function (response) { return response.json(); })
            .then(function (data) {
                if (data) {
                    var extensionCharge = data.totalAmount;
                    var totalCharge = initialAmount + extensionCharge;

                    if (amountInput) amountInput.value = totalCharge.toFixed(2);
                    if (displayAmount) displayAmount.innerText = '₱ ' + totalCharge.toFixed(2);

                    if (timeBreakdown) {
                        timeBreakdown.innerText = 'Base: ' + formatTime(initialMinutes) + ' | Extra: +' + formatTime(extendedMinutes);
                        timeBreakdown.style.display = 'block';
                    }
                    if (amountBreakdown) {
                        amountBreakdown.innerText = '(Prepaid: ₱' + initialAmount.toFixed(2) + ' + Extra: ₱' + extensionCharge.toFixed(2) + ')';
                        amountBreakdown.style.display = 'block';
                    }
                }
            })
            .catch(function (err) { console.error(err); });
    } else {
        if (amountInput) amountInput.value = initialAmount.toFixed(2);
        if (displayAmount) displayAmount.innerText = '₱ ' + initialAmount.toFixed(2);

        if (timeBreakdown) timeBreakdown.style.display = 'none';
        if (amountBreakdown) amountBreakdown.style.display = 'none';
    }
}

function adjustDuration(amount) {
    var durationInput = document.querySelector('[id$="TextBox_Duration"]');
    var initialDurationInput = document.querySelector('[id$="HiddenField_InitialDuration"]');
    var customerSelect = document.querySelector('[id$="DropDownList_Customer"]');
    if (!durationInput) return;

    var current = parseInt(durationInput.value, 10) || 0;
    var isWalkIn = !customerSelect || !customerSelect.value || customerSelect.value === "";

    var initialMinutes = (!isWalkIn && initialDurationInput) ? (parseInt(initialDurationInput.value, 10) || 0) : 0;

    var updated = current + amount;

    if (updated < initialMinutes) {
        updated = initialMinutes;
    }

    if (updated < 0) {
        updated = 0;
    }

    durationInput.value = updated;
    calculateAmount();
}

// Reset button returns back to the base duration instead of 0
function resetDuration() {
    var durationInput = document.querySelector('[id$="TextBox_Duration"]');
    var initialDurationInput = document.querySelector('[id$="HiddenField_InitialDuration"]');

    var initialMinutes = initialDurationInput ? (parseInt(initialDurationInput.value, 10) || 0) : 0;

    if (durationInput) durationInput.value = initialMinutes;

    calculateAmount();
}



// Client validation before submission
function Validate() {
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

function getCustomerType() {
    var customerType = document.querySelector('[id$="HiddenField_Role"]');
    return customerType.value.toUpperCase();
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
            calculateAmount();
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