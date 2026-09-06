// =============================================================================
// DOM Elements & Theme Settings
// =============================================================================
const sidebar = document.getElementById('sidebar');
const mainWrapper = document.getElementById('mainWrapper');
const toggleBtn = document.getElementById('toggleBtn');
const themeToggle = document.getElementById('themeToggle');
const container = document.querySelector('.container');
const html = document.documentElement;

// Mobile Viewport Check
const isMobile = () => window.innerWidth <= 768;

// Sidebar Toggle Handler
if (toggleBtn) {
    toggleBtn.addEventListener('click', () => {
        if (isMobile()) {
            if (sidebar) sidebar.classList.toggle('mobile-visible');
            if (container) container.classList.toggle('sidebar-open');
        } else {
            if (sidebar) sidebar.classList.toggle('collapsed');
            if (mainWrapper) mainWrapper.classList.toggle('expanded');
        }
    });
}

// Mobile Overlay and Link Handlers
document.querySelectorAll('.sidebar-menu a').forEach(link => {
    link.addEventListener('click', () => {
        if (isMobile() && sidebar && container) {
            sidebar.classList.remove('mobile-visible');
            container.classList.remove('sidebar-open');
        }
    });
});

if (container) {
    container.addEventListener('click', (e) => {
        if (isMobile() && e.target === container && sidebar && sidebar.classList.contains('mobile-visible')) {
            sidebar.classList.remove('mobile-visible');
            container.classList.remove('sidebar-open');
        }
    });
}

window.addEventListener('resize', () => {
    if (!isMobile() && sidebar && container) {
        sidebar.classList.remove('mobile-visible');
        container.classList.remove('sidebar-open');
    }
});

// Theme Management
const currentTheme = localStorage.getItem('theme') || 'light';
html.setAttribute('data-theme', currentTheme);
updateThemeIcon(currentTheme);

if (themeToggle) {
    themeToggle.addEventListener('click', () => {
        const theme = html.getAttribute('data-theme') === 'light' ? 'dark' : 'light';
        html.setAttribute('data-theme', theme);
        localStorage.setItem('theme', theme);
        updateThemeIcon(theme);
    });
}

function updateThemeIcon(theme) {
    if (themeToggle) {
        themeToggle.innerHTML = theme === 'light' ? '<i class="fas fa-moon"></i>' : '<i class="fas fa-sun"></i>';
    }
}

// =============================================================================
// Alert Notification System
// =============================================================================
// ==========================================
// CUSTOM ALERT SYSTEM
// ==========================================
function ShowAlert(message, type = 'error') {
    let alertBox = document.getElementById('custom-alert-box');
    if (!alertBox) {
        alertBox = document.createElement('div');
        alertBox.id = 'custom-alert-box';
        alertBox.className = 'alert-box';
        document.body.appendChild(alertBox);
    }

    // Reset variant classes
    alertBox.classList.remove('alert-success');

    let iconClass = 'fa-triangle-exclamation';
    if (type === 'success') {
        alertBox.classList.add('alert-success');
        iconClass = 'fa-circle-check';
    }

    alertBox.innerHTML = `<i class="fa-solid ${iconClass}"></i> ${message}`;
    alertBox.classList.add('show');

    setTimeout(() => {
        alertBox.classList.remove('show');
    }, 4000);
}

// =============================================================================
// XSS Helper & Input Sanitization
// =============================================================================
function sanitizeInput(value) {
    if (value === null || value === undefined) return '';
    const str = String(value).trim();

    const xssPattern = /<script[\s\S]*?>[\s\S]*?<\/script>/gi;
    const tagPattern = /<[^>]+>/g;
    const eventPattern = /on\w+\s*=/gi;
    const jsProtocol = /javascript\s*:/gi;
    const dataProtocol = /data\s*:/gi;

    if (
        xssPattern.test(str) ||
        tagPattern.test(str) ||
        eventPattern.test(str) ||
        jsProtocol.test(str) ||
        dataProtocol.test(str)
    ) {
        return null; // Signal: Input contains illegal script pattern
    }

    return str;
}

// =============================================================================
// Validation Functions (Pure Vanilla JavaScript)
// =============================================================================
function requiredValidation(id) {
    var element = document.getElementById(id);
    if (!element) return false;

    var value = sanitizeInput(element.value);

    element.classList.remove('is-valid', 'is-invalid');

    // Vanilla JS lookup for Select2 container
    var select2Container = element.nextElementSibling;
    var hasSelect2 = select2Container && select2Container.classList.contains('select2-container');

    if (hasSelect2) {
        select2Container.classList.remove('is-valid', 'is-invalid');
    }

    if (value === null) {
        element.classList.add('is-invalid');
        if (hasSelect2) select2Container.classList.add('is-invalid');
        ShowAlert('Invalid input detected. HTML/script tags are not allowed.');
        return false;
    }

    if (value !== '') {
        element.classList.add('is-valid');
        if (hasSelect2) select2Container.classList.add('is-valid');
        return true;
    } else {
        element.classList.add('is-invalid');
        if (hasSelect2) select2Container.classList.add('is-invalid');
        return false;
    }
}

function emailValidation(id) {
    var text = document.getElementById(id);
    if (!text) return false;

    text.classList.remove('is-valid', 'is-invalid');

    var value = sanitizeInput(text.value);

    if (value === null) {
        text.classList.add('is-invalid');
        ShowAlert('Invalid input detected. HTML/script tags are not allowed.');
        return false;
    }

    let res = /^[\w-\.]+@([\w-]+\.)+[\w-]{2,4}$/;
    if (res.test(value)) {
        text.classList.add('is-valid');
        return true;
    } else {
        text.classList.add('is-invalid');
        ShowAlert('Please enter a valid email address.');
        return false;
    }
}

function compareValidation(text1, text2) {
    var validationTextarea = document.getElementById(text1);
    var text = document.getElementById(text2);
    if (!validationTextarea || !text) return false;

    text.classList.remove('is-valid', 'is-invalid');

    var v1 = sanitizeInput(validationTextarea.value);
    var v2 = sanitizeInput(text.value);

    if (v1 === null || v2 === null) {
        text.classList.add('is-invalid');
        ShowAlert('Invalid input detected. HTML/script tags are not allowed.');
        return false;
    }

    if (v1 === v2) {
        text.classList.add('is-valid');
        return true;
    } else {
        text.classList.add('is-invalid');
        ShowAlert('Fields do not match. Please try again.');
        return false;
    }
}

function RangeValidation(text1, text2) {
    var validationTextarea = document.getElementById(text1);
    var text = document.getElementById(text2);
    if (!validationTextarea || !text) return false;

    validationTextarea.classList.remove('is-valid', 'is-invalid');
    text.classList.remove('is-valid', 'is-invalid');

    var value1 = sanitizeInput(validationTextarea.value);
    var value2 = sanitizeInput(text.value);

    if (value1 === null || value2 === null) {
        validationTextarea.classList.add('is-invalid');
        text.classList.add('is-invalid');
        ShowAlert('Invalid input detected. HTML/script tags are not allowed.');
        return false;
    }

    var num1 = parseFloat(value1);
    var num2 = parseFloat(value2);
    var date1 = new Date(value1);
    var date2 = new Date(value2);

    if (!isNaN(num1) && !isNaN(num2) && value1 !== '' && value2 !== '') {
        if (num1 <= num2) {
            validationTextarea.classList.add('is-valid');
            text.classList.add('is-valid');
            return true;
        }
    } else if (!isNaN(date1) && !isNaN(date2)) {
        if (date1 <= date2) {
            validationTextarea.classList.add('is-valid');
            text.classList.add('is-valid');
            return true;
        }
    }

    validationTextarea.classList.add('is-invalid');
    text.classList.add('is-invalid');
    return false;
}

function RangeMinTodayValidation(text1) {
    var validationTextarea = document.getElementById(text1);
    if (!validationTextarea) return false;

    if (validationTextarea.getAttribute('type') !== 'date') return true;

    validationTextarea.classList.remove('is-valid', 'is-invalid');

    var value1 = sanitizeInput(validationTextarea.value);

    if (value1 === null) {
        validationTextarea.classList.add('is-invalid');
        ShowAlert('Invalid input detected. HTML/script tags are not allowed.');
        return false;
    }

    var date1 = new Date(value1);
    var today = new Date();
    today.setHours(0, 0, 0, 0);

    if (!isNaN(date1) && date1 >= today) {
        validationTextarea.classList.add('is-valid');
        return true;
    }

    validationTextarea.classList.add('is-invalid');
    return false;
}

// =============================================================================
// Input Filters & Formatters
// =============================================================================
function NumberOnly(e) {
    e.value = e.value.replace(/\D/g, '');
    e.value = e.value.replace(/^0+/, '');
}

function DecimalOnly(e) {
    e.value = e.value.replace(/[^0-9.]/g, '');
    if ((e.value.match(/\./g) || []).length > 1) {
        e.value = e.value.replace(/\.+$/, '');
    }
}

function formatNumberWithComma(e) {
    let value = e.value.replace(/,/g, '');
    if (value === '' || value === '.') return;
    const parts = value.split('.');
    let integerPart = parts[0];
    const decimalPart = parts[1];
    integerPart = integerPart.replace(/\B(?=(\d{3})+(?!\d))/g, ',');
    e.value = decimalPart !== undefined ? `${integerPart}.${decimalPart}` : integerPart;
}

function limitTo24(el) {
    if (el.value === "") return;
    let value = Number(el.value);
    if (value < 0) el.value = 0;
    if (value > 24) el.value = 24;
}


function navigateTo(url) {
    showPageLoading();
    window.location.href = url;
}

function showPageLoading() {
    var loader = document.getElementById('pageLoadingOverlay');
    if (loader) loader.classList.add('active');
}

function hidePageLoading() {
    var loader = document.getElementById('pageLoadingOverlay');
    if (loader) loader.classList.remove('active');
}

// 1. Show loader before browser unloads (handles standard link clicks & full postbacks)
window.addEventListener('beforeunload', function () {
    showPageLoading();
});
