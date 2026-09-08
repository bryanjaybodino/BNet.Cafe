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





    // Call the backend handler
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
                // Sync values returned from the backend ASHX handler
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

    // Invoke backend ASHX API call
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
    if (!durationInput) return;

    durationInput.value = 0;
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

    var data = {
        customerName: customerSelect ? customerSelect.value : null,
        duration: durationInput ? parseInt(durationInput.value, 10) : 0,
        amount: amountInput ? parseFloat(amountInput.value) : 0,
        command: 'CREATE'
    };

    sendTextMessageToPC(computerInput.value, JSON.stringify(data));
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