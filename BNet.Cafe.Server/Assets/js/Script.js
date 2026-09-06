const sidebar = document.getElementById('sidebar');
const mainWrapper = document.getElementById('mainWrapper');
const toggleBtn = document.getElementById('toggleBtn');
const themeToggle = document.getElementById('themeToggle');
const container = document.querySelector('.container');
const html = document.documentElement;
const body = document.body;

// Check if mobile
const isMobile = () => window.innerWidth <= 768;

// Sidebar Toggle
toggleBtn.addEventListener('click', () => {
    if (isMobile()) {
        // Mobile: toggle sidebar visibility with blur
        sidebar.classList.toggle('mobile-visible');
        container.classList.toggle('sidebar-open');
    } else {
        // Desktop: collapse/expand sidebar
        sidebar.classList.toggle('collapsed');
        mainWrapper.classList.toggle('expanded');
    }
});

// Close sidebar when clicking on a menu item (mobile)
document.querySelectorAll('.sidebar-menu a').forEach(link => {
    link.addEventListener('click', () => {
        if (isMobile()) {
            sidebar.classList.remove('mobile-visible');
            container.classList.remove('sidebar-open');
        }
    });
});

// Close sidebar when clicking on blur overlay (mobile)
document.querySelector('.container').addEventListener('click', (e) => {
    if (isMobile() && e.target === container && sidebar.classList.contains('mobile-visible')) {
        sidebar.classList.remove('mobile-visible');
        container.classList.remove('sidebar-open');
    }
});

// Handle window resize
window.addEventListener('resize', () => {
    if (!isMobile()) {
        sidebar.classList.remove('mobile-visible');
        container.classList.remove('sidebar-open');
    }
});

// Theme Toggle
const currentTheme = localStorage.getItem('theme') || 'light';
html.setAttribute('data-theme', currentTheme);
updateThemeIcon(currentTheme);

themeToggle.addEventListener('click', () => {
    const theme = html.getAttribute('data-theme') === 'light' ? 'dark' : 'light';
    html.setAttribute('data-theme', theme);
    localStorage.setItem('theme', theme);
    updateThemeIcon(theme);
});

function updateThemeIcon(theme) {
    themeToggle.innerHTML = theme === 'light' ? '<i class="fas fa-moon"></i>' : '<i class="fas fa-sun"></i>';
}




// =============================================================================
// Validation
// =============================================================================
function emailValidation(id) {
    var text = document.getElementById(id);
    if (!text) return false;

    text.classList.remove('is-valid', 'is-invalid', 'form-control');

    var value = sanitizeInput(text.value);

    if (value === null) {
        text.classList.add('is-invalid', 'form-control');
        ShowAlert('Invalid input detected. HTML/script tags are not allowed.');
        return false;
    }

    let res = /^[\w-\.]+@([\w-]+\.)+[\w-]{2,4}$/;
    if (res.test(value)) {
        text.classList.add('is-valid', 'form-control');
        return true;
    } else {
        text.classList.add('is-invalid', 'form-control');
        ShowAlert('Please enter a valid email address.');
        return false;
    }
}

function compareValidation(text1, text2) {
    var validationTextarea = document.getElementById(text1);
    var text = document.getElementById(text2);
    if (!validationTextarea || !text) return false;

    text.classList.remove('is-valid', 'is-invalid', 'form-control');

    var v1 = sanitizeInput(validationTextarea.value);
    var v2 = sanitizeInput(text.value);

    if (v1 === null || v2 === null) {
        text.classList.add('is-invalid', 'form-control');
        ShowAlert('Invalid input detected. HTML/script tags are not allowed.');
        return false;
    }

    if (v1 === v2) {
        text.classList.add('is-valid', 'form-control');
        return true;
    } else {
        text.classList.add('is-invalid', 'form-control');
        ShowAlert('Fields do not match. Please try again.');
        return false;
    }
}

function requiredValidation(id) {
    var element = document.getElementById(id);
    if (!element) return false;

    var value = sanitizeInput(element.value);

    element.classList.remove('is-valid', 'is-invalid');

    var select2Container = $(element).next('.select2-container');
    if (select2Container.length) {
        select2Container.removeClass('is-valid is-invalid');
    }

    if (value === null) {
        element.classList.add('is-invalid');
        if (select2Container.length) select2Container.addClass('is-invalid');
        ShowAlert('Invalid input detected. HTML/script tags are not allowed.');
        return false;
    }

    if (value !== '') {
        element.classList.add('is-valid');
        if (select2Container.length) select2Container.addClass('is-valid');
        return true;
    } else {
        element.classList.add('is-invalid');
        if (select2Container.length) select2Container.addClass('is-invalid');
        return false;
    }
}

function RangeValidation(text1, text2) {
    var validationTextarea = document.getElementById(text1);
    var text = document.getElementById(text2);
    if (!validationTextarea || !text) return false;

    validationTextarea.classList.remove('is-valid', 'is-invalid', 'form-control');
    text.classList.remove('is-valid', 'is-invalid', 'form-control');

    var value1 = sanitizeInput(validationTextarea.value);
    var value2 = sanitizeInput(text.value);

    if (value1 === null || value2 === null) {
        validationTextarea.classList.add('is-invalid', 'form-control');
        text.classList.add('is-invalid', 'form-control');
        ShowAlert('Invalid input detected. HTML/script tags are not allowed.');
        return false;
    }

    var num1 = parseFloat(value1);
    var num2 = parseFloat(value2);
    var date1 = new Date(value1);
    var date2 = new Date(value2);

    if (!isNaN(num1) && !isNaN(num2) && value1 !== '' && value2 !== '') {
        if (num1 <= num2) {
            validationTextarea.classList.add('is-valid', 'form-control');
            text.classList.add('is-valid', 'form-control');
            return true;
        }
    } else if (!isNaN(date1) && !isNaN(date2)) {
        if (date1 <= date2) {
            validationTextarea.classList.add('is-valid', 'form-control');
            text.classList.add('is-valid', 'form-control');
            return true;
        }
    }

    validationTextarea.classList.add('is-invalid', 'form-control');
    text.classList.add('is-invalid', 'form-control');
    return false;
}

function RangeMinTodayValidation(text1) {
    var validationTextarea = document.getElementById(text1);
    if (!validationTextarea) return false;

    if (validationTextarea.getAttribute('type') !== 'date') return true;

    validationTextarea.classList.remove('is-valid', 'is-invalid', 'form-control');

    var value1 = sanitizeInput(validationTextarea.value);

    if (value1 === null) {
        validationTextarea.classList.add('is-invalid', 'form-control');
        ShowAlert('Invalid input detected. HTML/script tags are not allowed.');
        return false;
    }

    var date1 = new Date(value1);
    var today = new Date();
    today.setHours(0, 0, 0, 0);

    if (!isNaN(date1) && date1 >= today) {
        validationTextarea.classList.add('is-valid', 'form-control');
        return true;
    }

    validationTextarea.classList.add('is-invalid', 'form-control');
    return false;
}

// =============================================================================
// Input filters
// =============================================================================
function NumberOnly(e) {
    e.value = e.value.replace(/\D/g, '');
    e.value = e.value.replace(/^0+/, '');
    var keyCodeEntered = (event.which) ? event.which : (window.event.keyCode) ? window.event.keyCode : -1;
    if ((keyCodeEntered > 47) && (keyCodeEntered < 58) && keyCodeEntered != 13) {
        return true;
    }
    return false;
}

function RangeValidation(e) {
    var el = document.getElementById(e.id);
    if (el.type == "number" && el.max && el.min) {
        let value = parseInt(el.value);
        el.value = value;
        let max = parseInt(el.max);
        let min = parseInt(el.min);
        if (value > max) el.value = el.max;
        if (value < min) el.value = el.min;
    }
}

function DecimalOnly(e) {
    var keyCodeEntered = (event.which) ? event.which :
        (window.event.keyCode) ? window.event.keyCode : -1;
    if ((keyCodeEntered > 47) && (keyCodeEntered < 58) && keyCodeEntered != 13) { return true; }
    else if (keyCodeEntered == 46) {
        if ((e.value) && (e.value.indexOf('.') >= 0)) return false; else; return true;
    }
    return false;
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
