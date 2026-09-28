// 1. Convert Password input to type=password (bypasses TextMode=SingleLine limitation)
(function () {
    var pw = document.getElementById('TextBox_Password');
    if (pw) {
        pw.setAttribute('type', 'password');
        pw.setAttribute('placeholder', 'Enter your password');
    }
})();

// 2. SVG Eye Password Toggle
function togglePw() {
    var inp = document.getElementById('TextBox_Password');
    var iconOff = document.getElementById('pwIconHide');
    var iconOn = document.getElementById('pwIconShow');
    if (!inp) return;

    if (inp.type === 'password') {
        inp.type = 'text';
        if (iconOff) iconOff.style.display = 'none';
        if (iconOn) iconOn.style.display = '';
    } else {
        inp.type = 'password';
        if (iconOff) iconOff.style.display = '';
        if (iconOn) iconOn.style.display = 'none';
    }
    inp.focus();
}

// 3. Login Lock Enforcement
var _lockUnlockCheck = null;
var _loginTimerInterval = null;

function enforceLoginLock(seconds) {
    var emailEl = document.getElementById('TextBox_Email');
    var pwEl = document.getElementById('TextBox_Password');
    var btnEl = document.getElementById('LinkButton_Login');

    if (emailEl) { emailEl.disabled = true; emailEl.setAttribute('readonly', 'readonly'); }
    if (pwEl) { pwEl.disabled = true; pwEl.setAttribute('readonly', 'readonly'); }
    if (btnEl) { btnEl.disabled = true; btnEl.style.pointerEvents = 'none'; btnEl.style.opacity = '0.5'; }

    showErrorWithTimer('Account locked. Try again in', seconds);

    if (_lockUnlockCheck) clearInterval(_lockUnlockCheck);
    _lockUnlockCheck = setInterval(function () {
        if (!_loginTimerInterval) {
            clearInterval(_lockUnlockCheck);
            _lockUnlockCheck = null;
            if (emailEl) { emailEl.disabled = false; emailEl.removeAttribute('readonly'); }
            if (pwEl) { pwEl.disabled = false; pwEl.removeAttribute('readonly'); }
            if (btnEl) { btnEl.disabled = false; btnEl.style.pointerEvents = ''; btnEl.style.opacity = ''; }
            hideError();
        }
    }, 500);
}

function showErrorWithTimer(message, seconds) {
    if (_loginTimerInterval) clearInterval(_loginTimerInterval);
    var remaining = seconds;

    function update() {
        var m = Math.floor(remaining / 60);
        var s = remaining % 60;
        var timeStr = m > 0 ? m + 'm ' + (s < 10 ? '0' : '') + s + 's' : s + 's';
        showError(message + ' ' + timeStr);
        if (remaining <= 0) {
            clearInterval(_loginTimerInterval);
            _loginTimerInterval = null;
        }
        remaining--;
    }

    update();
    _loginTimerInterval = setInterval(update, 1000);
}

// 4. Server Lock Initialization check on load
document.addEventListener('DOMContentLoaded', function () {
    if (typeof window.loginLockedSeconds !== 'undefined' && window.loginLockedSeconds > 0) {
        enforceLoginLock(window.loginLockedSeconds);
    }
});

// 5. Unified Error Box Handler
function showError(msg) {
    var box = document.getElementById('errorBox');
    var txt = document.getElementById('errorMsg');
    if (!box || !txt) return;
    txt.textContent = msg;
    box.style.display = 'flex';
}

function hideError() {
    if (_loginTimerInterval) { clearInterval(_loginTimerInterval); _loginTimerInterval = null; }
    var box = document.getElementById('errorBox');
    if (box) box.style.display = 'none';
}

// 6. Form Submission Validation
function handleSubmit() {
    hideError();
    var u = document.getElementById('TextBox_Email');
    var p = document.getElementById('TextBox_Password');

    if (u) {
        var emailVal = u.value.trim();
        if (!emailVal || !emailVal.includes('@')) {
            showError('Please enter a valid email address.');
            return false;
        }
    }

    if (p && !p.value) {
        showError('Please enter your password.');
        return false;
    }

    var btn = document.getElementById('LinkButton_Login');
    if (btn) btn.classList.add('loading');
    return true;
}

// 7. Reset button state after partial postback
(function wireEndRequest() {
    if (typeof Sys === 'undefined') { window.addEventListener('load', wireEndRequest); return; }
    Sys.WebForms.PageRequestManager.getInstance().add_endRequest(function () {
        var btn = document.getElementById('LinkButton_Login');
        if (btn) btn.classList.remove('loading');
    });
})();