// Dynamic Time Formatter
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
// Exact Linear/Decimal Rate Calculation Engine
function calculateRentalPrice(totalMinutes) {
    if (!totalMinutes || totalMinutes <= 0) return 0;

    var totalAmount = 0;

    // 0 to 30 mins: Linear scale based on ₱10.00 / 30 mins (₱0.3333/min)
    if (totalMinutes <= 30) {
        totalAmount = totalMinutes * (10 / 30);
    }
    // 30 to 60 mins (1 hr): Interpolate from ₱10.00 up to ₱15.00
    else if (totalMinutes <= 60) {
        var base = 10;
        var extraMins = totalMinutes - 30;
        var ratePerMin = (15 - 10) / 30; // ₱5 over 30 mins = ₱0.1667/min
        totalAmount = base + (extraMins * ratePerMin);
    }
    // 60 to 120 mins (2 hrs): Interpolate from ₱15.00 up to ₱25.00
    else if (totalMinutes <= 120) {
        var base = 15;
        var extraMins = totalMinutes - 60;
        var ratePerMin = (25 - 15) / 60; // ₱10 over 60 mins = ₱0.1667/min
        totalAmount = base + (extraMins * ratePerMin);
    }
    // 120 to 180 mins (3 hrs): Interpolate from ₱25.00 up to ₱40.00
    else if (totalMinutes <= 180) {
        var base = 25;
        var extraMins = totalMinutes - 120;
        var ratePerMin = (40 - 25) / 60; // ₱15 over 60 mins = ₱0.25/min
        totalAmount = base + (extraMins * ratePerMin);
    }
    // 180 to 240 mins (4 hrs): Interpolate from ₱40.00 up to ₱50.00
    else if (totalMinutes <= 240) {
        var base = 40;
        var extraMins = totalMinutes - 180;
        var ratePerMin = (50 - 40) / 60; // ₱10 over 60 mins = ₱0.1667/min
        totalAmount = base + (extraMins * ratePerMin);
    }
    // Beyond 4 Hours (240+ mins): ₱50.00 + ₱10.00/hr (₱0.1667/min)
    else {
        var base = 50;
        var extraMins = totalMinutes - 240;
        var ratePerMin = 10 / 60;
        totalAmount = base + (extraMins * ratePerMin);
    }

    return totalAmount;
}
// Update calculated amount & formatted hours display in real-time
function calculateAmount() {
    var durationInput = document.querySelector('[id$="TextBox_Duration"]');
    var amountInput = document.querySelector('[id$="TextBox_Amount"]');
    var displayTime = document.getElementById('display_FormattedTime');
    var displayAmount = document.getElementById('display_TotalAmount');

    if (!durationInput) return;

    var minutes = parseInt(durationInput.value, 10) || 0;
    var totalAmount = calculateRentalPrice(minutes);

    // Sync values to UI displays
    if (displayTime) {
        displayTime.innerText = formatMinutesToHours(minutes);
    }

    if (displayAmount) {
        displayAmount.innerText = "₱ " + totalAmount.toFixed(2);
    }

    // Sync hidden server control for postback submit
    if (amountInput) {
        amountInput.value = totalAmount.toFixed(2);
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

    //if (!customerSelect || customerSelect.value === '0' || customerSelect.value === '') {
    //    errors.push('Please select a customer.');
    //}

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
    //disableSubmitButton();
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