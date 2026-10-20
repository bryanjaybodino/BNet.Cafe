document.addEventListener('DOMContentLoaded', function () {
    const hoursInput = document.querySelector('[id$="TextBox_Hours"]');
    const minutesInput = document.querySelector('[id$="TextBox_Minutes_Only"]');
    const pointsInput = document.querySelector('[id$="TextBox_Points"]');
    const totalDurationDisplay = document.getElementById('TotalDurationDisplay');
    const totalMinutesValue = document.getElementById('TotalMinutesValue');
    const pointsDisplay = document.getElementById('PointsDisplay');

    function updateDurationDisplay() {
        const totalDurationDisplay = document.getElementById('TotalDurationDisplay');
        const totalMinutesValue = document.getElementById('TotalMinutesValue');
        const hiddenField = document.querySelector('[id$="HiddenField_TotalMinutes"]');

        if (!hoursInput || !minutesInput || !totalDurationDisplay) return;

        let hours = parseInt(hoursInput.value) || 0;
        let minutes = parseInt(minutesInput.value) || 0;

        if (hours > 24) {
            hours = 24;
            hoursInput.value = 24;
        }

        if (minutes > 59) {
            minutes = 59;
            minutesInput.value = 59;
        }

        if (hours === 24 && minutes > 0) {
            minutes = 0;
            minutesInput.value = 0;
        }

        const totalMinutes = (hours * 60) + minutes;

        totalDurationDisplay.textContent = formatDurationDisplay(totalMinutes);
        if (totalMinutesValue) totalMinutesValue.textContent = totalMinutes;
        if (hiddenField) hiddenField.value = totalMinutes;
    }

    function updatePointsDisplay() {
        if (pointsInput && pointsDisplay) {
            const value = parseFloat(pointsInput.value) || 0;
            pointsDisplay.textContent = value.toFixed(2) + ' pts';
        }
    }

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

    if (pointsInput) {
        pointsInput.addEventListener('input', updatePointsDisplay);
    }

    if (hoursInput && minutesInput) {
        hoursInput.addEventListener('input', updateDurationDisplay);
        minutesInput.addEventListener('input', updateDurationDisplay);
        hoursInput.addEventListener('change', updateDurationDisplay);
        minutesInput.addEventListener('change', updateDurationDisplay);
        updateDurationDisplay();
    }

    function bindPointsEvents() {
        if (hoursInput && minutesInput) {
            hoursInput.addEventListener('input', updateDurationDisplay);
            minutesInput.addEventListener('input', updateDurationDisplay);
        }
        if (pointsInput) {
            pointsInput.addEventListener('input', updatePointsDisplay);
        }
        updateDurationDisplay();
        updatePointsDisplay();
    }

    if (typeof Sys !== 'undefined' && Sys.WebForms && Sys.WebForms.PageRequestManager) {
        Sys.WebForms.PageRequestManager.getInstance().add_pageLoaded(function () {
            bindPointsEvents();
        });
    }
});

function ValidatePointsRule() {
    var hoursInput = document.querySelector('[id$="TextBox_Hours"]');
    var minutesInput = document.querySelector('[id$="TextBox_Minutes_Only"]');
    var points = document.querySelector('[id$="TextBox_Points"]');
    var customerType = document.querySelector('[id$="DropDownList_CustomerType"]');

    if (!customerType || !customerType.value) {
        alert('Please select a customer tier.');
        return false;
    }

    var hours = parseInt(hoursInput.value) || 0;
    var minutes = parseInt(minutesInput.value) || 0;
    var totalMinutes = (hours * 60) + minutes;

    if (totalMinutes <= 0) {
        alert('Please enter a valid duration (minimum 1 minute).');
        return false;
    }

    if (!points || !points.value || parseFloat(points.value) < 0) {
        alert('Please enter a valid points amount.');
        return false;
    }

    // Trigger the loading spinner animation on the submit button
    disableSubmitButton();
    return true;
}

function editPointsRule(buttonElement) {
    const id = buttonElement.getAttribute('data-id');
    const tier = buttonElement.getAttribute('data-tier');
    const totalMinutes = parseInt(buttonElement.getAttribute('data-minutes')) || 0;
    const points = buttonElement.getAttribute('data-points');

    const hiddenId = document.querySelector('[id$="HiddenField_PointId"]');
    const ddlTier = document.querySelector('[id$="DropDownList_CustomerType"]');
    const txtHours = document.querySelector('[id$="TextBox_Hours"]');
    const txtMinutes = document.querySelector('[id$="TextBox_Minutes_Only"]');
    const txtPoints = document.querySelector('[id$="TextBox_Points"]');
    const btnSubmit = document.querySelector('[id$="LinkButton_Submit"]');

    if (hiddenId) hiddenId.value = id;
    if (ddlTier) ddlTier.value = tier;
    if (txtHours) txtHours.value = Math.floor(totalMinutes / 60);
    if (txtMinutes) txtMinutes.value = totalMinutes % 60;
    if (txtPoints) txtPoints.value = parseFloat(points).toFixed(2);

    if (btnSubmit) {
        btnSubmit.innerHTML = '<i class="fa fa-save"></i> Update Rule';
    }

    if (txtHours) txtHours.dispatchEvent(new Event('input'));
    if (txtPoints) txtPoints.dispatchEvent(new Event('input'));
}

function disableSubmitButton() {
    var btn = document.querySelector('[id$="LinkButton_Submit"]');
    if (btn) {
        btn.classList.add('disabled');
        btn.style.pointerEvents = 'none';
        btn.innerHTML = '<i class="fa fa-spinner fa-spin"></i> Saving...';
    }
}

// Restore button state after the ASP.NET AJAX UpdatePanel postback finishes
if (typeof Sys !== 'undefined' && Sys.WebForms && Sys.WebForms.PageRequestManager) {
    var prm = Sys.WebForms.PageRequestManager.getInstance();

    prm.add_endRequest(function (sender, args) {
        var btn = document.querySelector('[id$="LinkButton_Submit"]');
        if (btn) {
            btn.classList.remove('disabled');
            btn.style.pointerEvents = 'auto';
            btn.innerHTML = '<i class="fa fa-save"></i> Save Rule';
        }
    });
}