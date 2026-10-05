function ShowAlert(message, type) {
    var wrapper = document.getElementById('toastWrapper');
    if (!wrapper) {
        wrapper = document.createElement('div');
        wrapper.id = 'toastWrapper';
        wrapper.className = 'toast-wrapper';
        document.body.appendChild(wrapper);
    }

    var messages = Array.isArray(message) ? message : [message];

    messages.forEach(function (msg) {
        var toast = document.createElement('div');
        toast.className = 'toast-card toast-' + (type || 'error');

        var iconClass = type === 'success' ? 'fa-check' : 'fa-xmark';

        toast.innerHTML =
            '<div class="toast-icon"><i class="fa-solid ' + iconClass + '"></i></div>' +
            '<div class="toast-body">' + msg + '</div>' +
            '<button type="button" class="toast-close-btn" onclick="dismissToast(this.parentElement)">&times;</button>';

        wrapper.appendChild(toast);

        setTimeout(function () {
            dismissToast(toast);
        }, 4000);
    });

    resetSubmitButton();
}

function dismissToast(toast) {
    if (!toast) return;
    toast.classList.add('toast-fade-out');
    setTimeout(function () {
        if (toast.parentElement) {
            toast.parentElement.removeChild(toast);
        }
    }, 250);
}

function Validate() {
    var passwordInput = document.querySelector('[id$="TextBox_Password"]');
    var confirmInput = document.querySelector('[id$="TextBox_ConfirmPassword"]');

    var errors = [];

    if (passwordInput) passwordInput.classList.remove('is-invalid', 'is-valid');
    if (confirmInput) confirmInput.classList.remove('is-invalid', 'is-valid');

    if (!passwordInput || !passwordInput.value.trim()) {
        if (passwordInput) passwordInput.classList.add('is-invalid');
        errors.push('Please enter a new password.');
    } else {
        passwordInput.classList.add('is-valid');
    }

    if (!confirmInput || !confirmInput.value.trim()) {
        if (confirmInput) confirmInput.classList.add('is-invalid');
        errors.push('Please confirm your new password.');
    } else if (passwordInput && passwordInput.value !== confirmInput.value) {
        confirmInput.classList.add('is-invalid');
        errors.push('Passwords do not match.');
    } else {
        confirmInput.classList.add('is-valid');
    }

    if (errors.length > 0) {
        ShowAlert(errors, 'error');
        return false;
    }

    disableSubmitButton();
    return true;
}

function disableSubmitButton() {
    var btn = document.querySelector('[id$="LinkButton_Submit"]');
    if (btn) {
        btn.classList.add('disabled');
        btn.innerHTML = '<i class="fa-solid fa-spinner fa-spin"></i> Resetting...';
    }
}

function resetSubmitButton() {
    var btn = document.querySelector('[id$="LinkButton_Submit"]');
    if (btn) {
        btn.classList.remove('disabled');
        btn.innerHTML = '<i class="fa-solid fa-floppy-disk"></i> Change Password';
    }
}


// Disable right-click context menu
document.addEventListener('contextmenu', function (e) {
    e.preventDefault();
});

// Disable keyboard shortcuts for Inspect Element / DevTools
document.addEventListener('keydown', function (e) {
    // Prevent F12
    if (e.key === 'F12') {
        e.preventDefault();
    }
    // Prevent Ctrl+Shift+I, Ctrl+Shift+J, Ctrl+Shift+C
    if (e.ctrlKey && e.shiftKey && (e.key === 'I' || e.key === 'i' || e.key === 'J' || e.key === 'j' || e.key === 'C' || e.key === 'c')) {
        e.preventDefault();
    }
    // Prevent Ctrl+U (View Source)
    if (e.ctrlKey && (e.key === 'U' || e.key === 'u')) {
        e.preventDefault();
    }
});
