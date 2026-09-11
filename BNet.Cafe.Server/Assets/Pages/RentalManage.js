// Dynamic Time Formatter (Fallback UI string helper)
function formatMinutesToHours(totalMinutes) {
    if (!totalMinutes || totalMinutes <= 0) return "0 hrs 0 mins";

    var hours = Math.floor(totalMinutes / 60);
    var minutes = totalMinutes % 60;

    if (hours > 0 && minutes > 0) {
        return hours + (hours === 1 ? " hr " : " hrs ") + minutes + " mins";
    } else if (hours > 0) {
        return hours + (hours === 1 ? " hr" : " hrs");
    } else {
        return minutes + " mins";
    }
}

// Map custom entered amount back into estimated minutes based on rate matrix
function convertAmountToMinutes(amount) {
    if (amount <= 0) return 0;

    // Rate Table Tiers: 
    // <= 5  -> 15 mins
    // <= 10 -> 30 mins
    // <= 15 -> 60 mins
    // <= 25 -> 120 mins
    // <= 40 -> 180 mins
    // <= 50 -> 240 mins
    // > 50  -> 240 mins + 60 mins per 10 PHP extra

    if (amount <= 5) {
        return Math.round((amount / 5) * 15);
    } else if (amount <= 10) {
        return 15 + Math.round(((amount - 5) / 5) * 15);
    } else if (amount <= 15) {
        return 30 + Math.round(((amount - 10) / 5) * 30);
    } else if (amount <= 25) {
        return 60 + Math.round(((amount - 15) / 10) * 60);
    } else if (amount <= 40) {
        return 120 + Math.round(((amount - 25) / 15) * 60);
    } else if (amount <= 50) {
        return 180 + Math.round(((amount - 40) / 10) * 60);
    } else {
        var extraAmount = amount - 50;
        var extraHoursInMins = Math.round((extraAmount / 10) * 60);
        return 240 + extraHoursInMins;
    }
}

// Automatically compute duration when Admin manually inputs/changes the Amount
function calculateTimeFromAmount() {
    var amountInput = document.querySelector('[id$="TextBox_Amount"]');
    var durationInput = document.querySelector('[id$="TextBox_Duration"]');
    var displayAmount = document.getElementById('display_TotalAmount');

    if (!amountInput || !durationInput) return;

    var amount = parseFloat(amountInput.value) || 0;
    var computedMinutes = convertAmountToMinutes(amount);

    durationInput.value = computedMinutes;

    if (displayAmount) {
        displayAmount.innerText = "₱ " + amount.toFixed(2);
    }

    var displayTime = document.getElementById('display_FormattedTime');
    if (displayTime) {
        displayTime.innerText = formatMinutesToHours(computedMinutes);
    }
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
        ? window.location.pathname.replace(/[^\/]+\.aspx$/i, 'Ashx/CalculateRentalPrice.ashx')
        : window.location.pathname.replace(/[^\/]+$/i, 'Ashx/CalculateRentalPrice.ashx');

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
    if (!durationInput) return;

    var minutes = parseInt(durationInput.value, 10) || 0;

    calculateRentalPriceBackend(minutes);
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