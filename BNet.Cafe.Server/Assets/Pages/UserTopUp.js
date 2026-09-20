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
        btn.innerHTML = '<i class="fa fa-spinner fa-spin"></i> Saving...';
    }
}


// Automatically compute duration when Admin manually inputs/changes the Amount
function calculateTimeFromAmount() {
    var amountInput = document.querySelector('[id$="TextBox_Amount"]');
    if (!amountInput) return;

    var amount = parseFloat(amountInput.value) || 0;
    calculateDurationFromAmountBackend(amount);
}



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

                // Render formatted visual time (e.g., "1 hr 30 mins")
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