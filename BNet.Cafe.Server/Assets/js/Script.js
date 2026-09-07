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

    // Format content based on input type (Array vs String)
    let contentHtml = '';
    if (Array.isArray(message)) {
        const items = message.map(msg => `<li>${msg}</li>`).join('');
        contentHtml = `<ul class="alert-list">${items}</ul>`;
    } else {
        contentHtml = `<span>${message}</span>`;
    }

    alertBox.innerHTML = `
        <i class="fa-solid ${iconClass} alert-icon"></i>
        <div class="alert-content">${contentHtml}</div>
    `;

    alertBox.classList.add('show');

    setTimeout(() => {
        alertBox.classList.remove('show');
    }, 5000); // Extended slightly for easier reading of multiple lines
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

    // Look for Select2 or VirtualSelect container
    var nextContainer = element.nextElementSibling;
    var hasCustomSelect = nextContainer && (
        nextContainer.classList.contains('select2-container') ||
        nextContainer.classList.contains('v-select-container')
    );

    if (hasCustomSelect) {
        nextContainer.classList.remove('is-valid', 'is-invalid');
    }

    if (value === null) {
        element.classList.add('is-invalid');
        if (hasCustomSelect) nextContainer.classList.add('is-invalid');
        ShowAlert('Invalid input detected. HTML/script tags are not allowed.');
        return false;
    }

    if (value !== '' && value !== '0') {
        element.classList.add('is-valid');
        if (hasCustomSelect) nextContainer.classList.add('is-valid');
        return true;
    } else {
        element.classList.add('is-invalid');
        if (hasCustomSelect) nextContainer.classList.add('is-invalid');
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




// =============================================================================
// Virtual Infinite Scroll Dropdown Component
// =============================================================================
class VirtualSelect {
    constructor(options) {
        this.container = typeof options.container === 'string'
            ? document.querySelector(options.container)
            : options.container;
        this.data = options.data || [];
        this.pageSize = options.pageSize || 20;
        this.placeholder = options.placeholder || 'Select an option...';
        this.onChange = options.onChange || null;

        this.filteredData = [];
        this.displayedCount = 0;
        this.selectedValue = null;
        this.selectedText = '';
        this.searchQuery = '';

        this.initDOM();
        this.bindEvents();
        this.filterAndReset('');
    }

    initDOM() {
        this.container.classList.add('v-select-container');
        this.container.innerHTML = `
            <div class="v-select-trigger">
                <span class="v-select-value-text v-select-placeholder">${this.placeholder}</span>
                <i class="fa-solid fa-chevron-down v-select-arrow"></i>
            </div>
            <div class="v-select-dropdown">
                <div class="v-select-search-wrapper">
                    <input type="text" class="v-select-search-input" placeholder="Search..." />
                </div>
                <ul class="v-select-options-list"></ul>
            </div>
        `;

        this.trigger = this.container.querySelector('.v-select-trigger');
        this.valueText = this.container.querySelector('.v-select-value-text');
        this.dropdown = this.container.querySelector('.v-select-dropdown');
        this.searchInput = this.container.querySelector('.v-select-search-input');
        this.optionsList = this.container.querySelector('.v-select-options-list');
    }

    bindEvents() {
        // Toggle Dropdown
        this.trigger.addEventListener('click', (e) => {
            e.stopPropagation();
            this.toggle();
        });

        // Prevent dropdown click from closing dropdown
        this.dropdown.addEventListener('click', (e) => e.stopPropagation());

        // Client-side Search Engine (Debounced)
        let debounceTimer;
        this.searchInput.addEventListener('input', (e) => {
            clearTimeout(debounceTimer);
            debounceTimer = setTimeout(() => {
                const cleanQuery = sanitizeInput(e.target.value) || '';
                this.filterAndReset(cleanQuery);
            }, 150);
        });

        // Infinite Scroll Pagination Event
        this.optionsList.addEventListener('scroll', () => {
            const { scrollTop, scrollHeight, clientHeight } = this.optionsList;
            if (scrollTop + clientHeight >= scrollHeight - 15) {
                this.loadMore();
            }
        });

        // Select Option Event (Delegated)
        this.optionsList.addEventListener('click', (e) => {
            const item = e.target.closest('.v-select-option');
            if (item) {
                this.selectItem(item.dataset.value, item.textContent);
            }
        });

        // Close dropdown when clicking outside
        document.addEventListener('click', () => this.close());
    }

    toggle() {
        if (this.container.classList.contains('disabled')) return;
        const isOpen = this.container.classList.contains('open');
        // Close all other instances if multiple exist on the page
        document.querySelectorAll('.v-select-container.open').forEach(el => el.classList.remove('open'));

        if (!isOpen) {
            this.container.classList.add('open');
            this.searchInput.focus();
        }
    }

    close() {
        this.container.classList.remove('open');
    }

    filterAndReset(query) {
        this.searchQuery = query.toLowerCase().trim();
        this.displayedCount = 0;
        this.optionsList.innerHTML = '';

        if (this.searchQuery === '') {
            this.filteredData = this.data;
        } else {
            // Local client-side search across values and text labels
            this.filteredData = this.data.filter(item =>
                String(item.label).toLowerCase().includes(this.searchQuery) ||
                String(item.value).toLowerCase().includes(this.searchQuery)
            );
        }

        if (this.filteredData.length === 0) {
            this.optionsList.innerHTML = `<div class="v-select-empty">No results found</div>`;
            return;
        }

        this.loadMore();
    }

    loadMore() {
        if (this.displayedCount >= this.filteredData.length) return;

        const fragment = document.createDocumentFragment();
        const nextBatch = this.filteredData.slice(
            this.displayedCount,
            this.displayedCount + this.pageSize
        );

        nextBatch.forEach(item => {
            const li = document.createElement('li');
            li.className = 'v-select-option';
            if (String(item.value) === String(this.selectedValue)) {
                li.classList.add('selected');
            }
            li.dataset.value = item.value;
            li.textContent = item.label;
            fragment.appendChild(li);
        });

        this.optionsList.appendChild(fragment);
        this.displayedCount += nextBatch.length;
    }

    selectItem(value, label) {
        this.selectedValue = value;
        this.selectedText = label;

        this.valueText.textContent = label;
        this.valueText.classList.remove('v-select-placeholder');

        // Update CSS state in DOM
        this.optionsList.querySelectorAll('.v-select-option').forEach(el => {
            el.classList.toggle('selected', el.dataset.value === String(value));
        });

        this.close();

        if (typeof this.onChange === 'function') {
            this.onChange({ value, label });
        }
    }

    // Public method to set programmatic options or update dataset
    updateData(newData) {
        this.data = newData;
        this.filterAndReset(this.searchQuery);
    }
}

// =============================================================================
// Example Initialization with Large Mock Dataset (10,000+ Items)
// =============================================================================
document.addEventListener('DOMContentLoaded', () => {
    const targetElement = document.getElementById('customDropdown');
    if (!targetElement) return;

    // Mock Generating 10,000 records
    const mockLargeData = Array.from({ length: 10000 }, (_, i) => ({
        value: `val_${i + 1}`,
        label: `Option ${i + 1} - System Entry Node #${1000 + i}`
    }));

    // Instantiate virtual dropdown
    window.mySelect = new VirtualSelect({
        container: '#customDropdown',
        data: mockLargeData,
        pageSize: 25, // Renders 25 items at a time on scroll
        placeholder: '-- Select a System Entry --',
        onChange: (selected) => {
            console.log('User selected:', selected);
        }
    });
});


// =============================================================================
// Reusable Auto-Binding Function for ASP.NET / Native <select> Elements
// =============================================================================
function initVirtualSelects(selector = 'select.v-select-enable') {
    const selectElements = document.querySelectorAll(selector);

    selectElements.forEach((aspDropdown) => {
        // Prevent duplicate initializations
        if (aspDropdown.dataset.vSelectInitialized === "true") return;

        // 1. Extract data and initial selection from native <select> options
        const extractedData = [];
        const initialSelectedValue = aspDropdown.value;
        const placeholder = aspDropdown.getAttribute('data-placeholder') ||
            (aspDropdown.options[0] ? aspDropdown.options[0].text : 'Select an option...');

        Array.from(aspDropdown.options).forEach((opt) => {
            if (opt.value !== "") {
                extractedData.push({
                    value: opt.value,
                    label: opt.text
                });
            }
        });

        // 2. Hide the native dropdown instead of clearing its options
        aspDropdown.style.display = 'none';

        // 3. Dynamically create and insert the UI container element right after the native select
        const container = document.createElement('div');
        if (aspDropdown.disabled) {
            container.classList.add('disabled');
        }
        aspDropdown.parentNode.insertBefore(container, aspDropdown.nextSibling);

        // 4. Instantiate VirtualSelect
        const vsInstance = new VirtualSelect({
            container: container,
            data: extractedData,
            pageSize: 25,
            placeholder: placeholder,
            onChange: function (selected) {
                // Update native dropdown value so ASP.NET receives it on postback
                aspDropdown.value = selected.value;

                // Trigger standard HTML change event if other scripts rely on it
                aspDropdown.dispatchEvent(new Event('change', { bubbles: true }));

                // Optional: Uncomment below if instant WebForms PostBack is required:
                // if (aspDropdown.name) __doPostBack(aspDropdown.name, '');
            }
        });

        // 5. Sync initial selection state
        if (initialSelectedValue) {
            const existingItem = extractedData.find(item => item.value === initialSelectedValue);
            if (existingItem) {
                vsInstance.selectItem(existingItem.value, existingItem.label);
            }
        }

        // Mark as initialized and store reference on the DOM element
        aspDropdown.dataset.vSelectInitialized = "true";
        aspDropdown.virtualSelect = vsInstance;
    });
}

// Automatically bind on DOM ready
document.addEventListener('DOMContentLoaded', () => {
    initVirtualSelects();
});

// Support ASP.NET AJAX UpdatePanel partial postbacks
if (typeof Sys !== 'undefined' && Sys.WebForms && Sys.WebForms.PageRequestManager) {
    Sys.WebForms.PageRequestManager.getInstance().add_endRequest(() => {
        initVirtualSelects();
    });
}