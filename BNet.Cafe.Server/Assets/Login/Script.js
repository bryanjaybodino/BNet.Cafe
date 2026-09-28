// 1. Password Field Initialization
(function () {
    var pw = document.getElementById('TextBox_Password');
    if (pw) {
        pw.setAttribute('type', 'password');
        pw.setAttribute('placeholder', 'Enter your password');
    }
})();

// 2. SVG Eye Toggle
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

// 3. Login Lock Enforcement & LocalStorage Backup
var _loginTimerInterval = null;

function enforceLoginLock(seconds) {
    var sec = parseInt(seconds, 10);
    if (isNaN(sec) || sec <= 0) {
        unlockUI();
        return;
    }

    // Save end time in localStorage as client fallback on refresh
    var lockEndTime = Date.now() + (sec * 1000);
    localStorage.setItem('bnet_login_lock_until', lockEndTime.toString());

    lockUI();
    runCountdown();
}

function lockUI() {
    var emailEl = document.getElementById('TextBox_Email');
    var pwEl = document.getElementById('TextBox_Password');
    var btnEl = document.getElementById('LinkButton_Login');

    if (emailEl) { emailEl.disabled = true; emailEl.setAttribute('readonly', 'readonly'); }
    if (pwEl) { pwEl.disabled = true; pwEl.setAttribute('readonly', 'readonly'); }
    if (btnEl) { btnEl.disabled = true; btnEl.style.pointerEvents = 'none'; btnEl.style.opacity = '0.5'; }
}

function unlockUI() {
    localStorage.removeItem('bnet_login_lock_until');
    if (_loginTimerInterval) { clearInterval(_loginTimerInterval); _loginTimerInterval = null; }

    var emailEl = document.getElementById('TextBox_Email');
    var pwEl = document.getElementById('TextBox_Password');
    var btnEl = document.getElementById('LinkButton_Login');

    if (emailEl) { emailEl.disabled = false; emailEl.removeAttribute('readonly'); }
    if (pwEl) { pwEl.disabled = false; pwEl.removeAttribute('readonly'); }
    if (btnEl) { btnEl.disabled = false; btnEl.style.pointerEvents = ''; btnEl.style.opacity = ''; }

    hideError();
}

function runCountdown() {
    if (_loginTimerInterval) clearInterval(_loginTimerInterval);

    function update() {
        var lockUntil = parseInt(localStorage.getItem('bnet_login_lock_until') || '0', 10);
        var now = Date.now();
        var remainingMs = lockUntil - now;

        if (remainingMs <= 0) {
            unlockUI();
            return;
        }

        var remainingSec = Math.ceil(remainingMs / 1000);
        var m = Math.floor(remainingSec / 60);
        var s = remainingSec % 60;
        var timeStr = m > 0 ? m + 'm ' + (s < 10 ? '0' : '') + s + 's' : s + 's';

        showError('Too many failed attempts. Try again in ' + timeStr);
    }

    update();
    _loginTimerInterval = setInterval(update, 1000);
}

// 4. Auto-Check Storage Lock state on DOM Load / Refresh
document.addEventListener('DOMContentLoaded', function () {
    var lockUntil = parseInt(localStorage.getItem('bnet_login_lock_until') || '0', 10);
    if (lockUntil > Date.now()) {
        lockUI();
        runCountdown();
    }
});

// 5. Global Error Box Controllers
function showError(msg) {
    var box = document.getElementById('errorBox');
    var txt = document.getElementById('errorMsg');
    if (!box || !txt) return;
    txt.textContent = msg;
    box.style.display = 'flex';
}

function hideError() {
    var box = document.getElementById('errorBox');
    if (box) box.style.display = 'none';
}

// 6. Form Submission Handler
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