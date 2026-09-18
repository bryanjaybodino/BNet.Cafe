document.addEventListener('DOMContentLoaded', function () {
    const hoursInput = document.querySelector('[id$="TextBox_Hours"]');
    const minutesInput = document.querySelector('[id$="TextBox_Minutes_Only"]');
    const priceInput = document.querySelector('[id$="TextBox_Price"]');
    const totalDurationDisplay = document.getElementById('TotalDurationDisplay');
    const totalMinutesValue = document.getElementById('TotalMinutesValue');
    const priceDisplay = document.getElementById('PriceDisplay');

    // Function to calculate and display total duration
    function updateDurationDisplay() {
        const hoursInput = document.querySelector('[id$="TextBox_Hours"]');
        const minutesInput = document.querySelector('[id$="TextBox_Minutes_Only"]');
        const totalDurationDisplay = document.getElementById('TotalDurationDisplay');
        const totalMinutesValue = document.getElementById('TotalMinutesValue');
        const hiddenField = document.querySelector('[id$="HiddenField_TotalMinutes"]');

        if (!hoursInput || !minutesInput || !totalDurationDisplay) return;

        let hours = parseInt(hoursInput.value) || 0;
        let minutes = parseInt(minutesInput.value) || 0;

        // Cap hours at 24 max
        if (hours > 24) {
            hours = 24;
            hoursInput.value = 24;
        }

        // Cap minutes at 59 max
        if (minutes > 59) {
            minutes = 59;
            minutesInput.value = 59;
        }

        // If 24 hours is reached, reset minutes to 0 so total does not exceed 24 hours
        if (hours === 24 && minutes > 0) {
            minutes = 0;
            minutesInput.value = 0;
        }

        const totalMinutes = (hours * 60) + minutes;

        totalDurationDisplay.textContent = formatDurationDisplay(totalMinutes);
        if (totalMinutesValue) totalMinutesValue.textContent = totalMinutes;
        if (hiddenField) hiddenField.value = totalMinutes;
    }

    function updatePriceDisplay() {
        const priceInput = document.querySelector('[id$="TextBox_Price"]');
        const priceDisplay = document.getElementById('PriceDisplay');
        if (priceInput && priceDisplay) {
            const value = parseFloat(priceInput.value) || 0;
            priceDisplay.textContent = '₱ ' + value.toFixed(2);
        }
    }

    // Function to format duration for display
    function formatDurationDisplay(totalMinutes) {
        if (totalMinutes <= 0) return "0 mins";

        const hrs = Math.floor(totalMinutes / 60);
        const mins = totalMinutes % 60;

        if (hrs > 0 && mins > 0) {
            return `${hrs} hr${hrs > 1 ? 's' : ''} ${mins} min${mins > 1 ? 's' : ''}`;
        } else if (hrs > 0) {
            return `${hrs} hr${hrs > 1 ? 's' : ''}`;
        } else {
            return `${mins} min${mins > 1 ? 's' : ''}`;
        }
    }

    // Update price display
    if (priceInput) {
        priceInput.addEventListener('input', function () {
            const value = parseFloat(this.value) || 0;
            priceDisplay.textContent = '₱ ' + value.toFixed(2);
        });
    }

    // Add event listeners for duration inputs
    if (hoursInput && minutesInput) {
        hoursInput.addEventListener('input', updateDurationDisplay);
        minutesInput.addEventListener('input', updateDurationDisplay);
        hoursInput.addEventListener('change', updateDurationDisplay);
        minutesInput.addEventListener('change', updateDurationDisplay);

        // Initialize display on page load
        updateDurationDisplay();
    }

    function bindPricingEvents() {
        const hoursInput = document.querySelector('[id$="TextBox_Hours"]');
        const minutesInput = document.querySelector('[id$="TextBox_Minutes_Only"]');
        const priceInput = document.querySelector('[id$="TextBox_Price"]');

        if (hoursInput && minutesInput) {
            hoursInput.addEventListener('input', updateDurationDisplay);
            minutesInput.addEventListener('input', updateDurationDisplay);
            hoursInput.addEventListener('change', updateDurationDisplay);
            minutesInput.addEventListener('change', updateDurationDisplay);
        }

        if (priceInput) {
            priceInput.addEventListener('input', updatePriceDisplay);
            priceInput.addEventListener('change', updatePriceDisplay);
        }

        // Immediately recalculate displays when controls update
        updateDurationDisplay();
        updatePriceDisplay();
    }

    // ASP.NET AJAX lifecycle hook: triggers on initial load and after UpdatePanel refreshes
    if (typeof Sys !== 'undefined' && Sys.WebForms && Sys.WebForms.PageRequestManager) {
        Sys.WebForms.PageRequestManager.getInstance().add_pageLoaded(function () {
            bindPricingEvents();
        });
    } else {
        document.addEventListener('DOMContentLoaded', bindPricingEvents);
    }
});

function ValidatePricing() {
    var hoursInput = document.querySelector('[id$="TextBox_Hours"]');
    var minutesInput = document.querySelector('[id$="TextBox_Minutes_Only"]');
    var price = document.querySelector('[id$="TextBox_Price"]');
    var customerType = document.querySelector('[id$="DropDownList_CustomerType"]');

    if (!customerType || !customerType.value) {
        alert('Please select a customer tier.');
        return false;
    }

    var hours = parseInt(hoursInput.value) || 0;
    var minutes = parseInt(minutesInput.value) || 0;

    if (hours > 24 || (hours === 24 && minutes > 0)) {
        alert('Maximum duration allowed is 24 hours (1,440 minutes).');
        return false;
    }

    if (minutes > 59) {
        alert('Minutes must be between 0 and 59.');
        return false;
    }

    var totalMinutes = (hours * 60) + minutes;

    if (totalMinutes <= 0) {
        alert('Please enter a valid duration (minimum 1 minute).');
        return false;
    }

    if (totalMinutes > 1440) {
        alert('Maximum duration allowed is 24 hours (1,440 minutes).');
        return false;
    }

    if (!price || !price.value || parseFloat(price.value) < 0) {
        alert('Please enter a valid price.');
        return false;
    }

    return true;
}




function editPricingRule(buttonElement) {
    // 1. Extract data attributes from clicked button
    const id = buttonElement.getAttribute('data-id');
    const tier = buttonElement.getAttribute('data-tier');
    const totalMinutes = parseInt(buttonElement.getAttribute('data-minutes')) || 0;
    const price = buttonElement.getAttribute('data-price');

    // 2. Select form controls
    const hiddenId = document.querySelector('[id$="HiddenField_PriceId"]');
    const ddlTier = document.querySelector('[id$="DropDownList_CustomerType"]');
    const txtHours = document.querySelector('[id$="TextBox_Hours"]');
    const txtMinutes = document.querySelector('[id$="TextBox_Minutes_Only"]');
    const txtPrice = document.querySelector('[id$="TextBox_Price"]');
    const btnSubmit = document.querySelector('[id$="LinkButton_Submit"]');

    // 3. Populate form controls
    if (hiddenId) hiddenId.value = id;
    if (ddlTier) ddlTier.value = tier;

    // Split total minutes into Hours and Minutes
    if (txtHours) txtHours.value = Math.floor(totalMinutes / 60);
    if (txtMinutes) txtMinutes.value = totalMinutes % 60;
    if (txtPrice) txtPrice.value = parseFloat(price).toFixed(2);

    // 4. Update submit button text
    if (btnSubmit) {
        btnSubmit.innerHTML = '<i class="fa fa-save"></i> Update Rule';
    }

    // 5. Trigger input events to refresh duration & price display calculations
    if (txtHours) txtHours.dispatchEvent(new Event('input'));
    if (txtPrice) txtPrice.dispatchEvent(new Event('input'));
}