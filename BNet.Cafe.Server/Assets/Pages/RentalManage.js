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


// Calculate amount and handle breakdown display
function calculateAmount() {
    var durationInput = document.querySelector('[id$="TextBox_Duration"]');
    var initialDurationInput = document.querySelector('[id$="HiddenField_InitialDuration"]');
    var initialAmountInput = document.querySelector('[id$="HiddenField_InitialAmount"]');
    var amountInput = document.querySelector('[id$="TextBox_Amount"]');

    var displayTime = document.getElementById('display_FormattedTime');
    var displayAmount = document.getElementById('display_TotalAmount');
    var timeBreakdown = document.getElementById('display_TimeBreakdown');
    var amountBreakdown = document.getElementById('display_AmountBreakdown');

    if (!durationInput) return;

    var currentMinutes = parseInt(durationInput.value, 10) || 0;
    var initialMinutes = initialDurationInput ? (parseInt(initialDurationInput.value, 10) || 0) : 0;
    var initialAmount = initialAmountInput ? (parseFloat(initialAmountInput.value) || 0) : 0;

    // PREVENT DEDUCTING BELOW BASE DURATION:
    // If user tries to go lower than initial duration, snap back to initial duration
    if (currentMinutes < initialMinutes) {
        currentMinutes = initialMinutes;
        durationInput.value = initialMinutes;
    }

    var extendedMinutes = currentMinutes - initialMinutes;

    // Helper function to format display time
    function formatTime(mins) {
        var h = Math.floor(mins / 60);
        var m = mins % 60;
        if (h > 0 && m > 0) return h + ' hrs ' + m + ' mins';
        if (h > 0) return h + ' hrs';
        return m + ' mins';
    }

    // Always keep Total Time UI locked to at least full current time
    if (displayTime) displayTime.innerText = formatTime(currentMinutes);

    // If extended time exists (greater than base)
    if (extendedMinutes > 0) {
        var endpoint = /\.aspx$/i.test(window.location.pathname)
            ? window.location.pathname.replace(/[^\/]+\.aspx$/i, 'Ashx/CalculateAmountFromDuration.ashx')
            : window.location.pathname.replace(/[^\/]+$/i, 'Ashx/CalculateAmountFromDuration.ashx');

        fetch(endpoint + '?minutes=' + extendedMinutes)
            .then(function (response) { return response.json(); })
            .then(function (data) {
                if (data) {
                    var extensionCharge = data.totalAmount;
                    var totalCharge = initialAmount + extensionCharge;

                    if (amountInput) amountInput.value = totalCharge.toFixed(2);
                    if (displayAmount) displayAmount.innerText = '₱ ' + totalCharge.toFixed(2);

                    // Show breakdown separating base time from added extra time
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
        // Returned to exact Base Duration (Extended minutes = 0)
        if (amountInput) amountInput.value = initialAmount.toFixed(2);
        if (displayAmount) displayAmount.innerText = '₱ ' + initialAmount.toFixed(2);

        if (timeBreakdown) timeBreakdown.style.display = 'none';

        if (amountBreakdown) {
            if (initialAmount === 0 && initialMinutes > 0) {
                amountBreakdown.innerText = '(Paid via User Top-up Balance)';
                amountBreakdown.style.display = 'block';
            } else {
                amountBreakdown.style.display = 'none';
            }
        }
    }
}

// Quick action duration adjustment (+ / - buttons)
function adjustDuration(amount) {
    var durationInput = document.querySelector('[id$="TextBox_Duration"]');
    var initialDurationInput = document.querySelector('[id$="HiddenField_InitialDuration"]');
    if (!durationInput) return;

    var current = parseInt(durationInput.value, 10) || 0;
    var initialMinutes = initialDurationInput ? (parseInt(initialDurationInput.value, 10) || 0) : 0;

    var updated = current + amount;

    // Do not allow reducing below initial base duration
    if (updated < initialMinutes) {
        updated = initialMinutes;
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