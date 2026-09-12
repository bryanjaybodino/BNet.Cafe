// =============================================================================
// BNetDatePicker Component (Single & Range with Working Shortcuts & UI Formatting)
// =============================================================================
class BNetDatePicker {
    constructor(options) {
        this.container = typeof options.container === 'string'
            ? document.querySelector(options.container)
            : options.container;

        this.mode = options.mode || 'single';
        this.placeholder = options.placeholder || (this.mode === 'range' ? 'Select date range...' : 'Select date...');
        this.onChange = options.onChange || null;

        this.currentDate = new Date();
        this.viewYear = this.currentDate.getFullYear();
        this.viewMonth = this.currentDate.getMonth();

        this.selectedStartDate = null;
        this.selectedEndDate = null;
        this.hoverDate = null;

        this.initDOM();
        this.bindEvents();
        this.renderCalendar();
    }

    initDOM() {
        this.container.classList.add('bnet-datepicker-container');
        if (this.mode === 'range') {
            this.container.classList.add('mode-range');
        }

        const sidebarHTML = this.mode === 'range' ? `
            <div class="bnet-dp-sidebar">
                <button type="button" class="bnet-dp-shortcut" data-range="today">Today</button>
                <button type="button" class="bnet-dp-shortcut" data-range="yesterday">Yesterday</button>
                <button type="button" class="bnet-dp-shortcut" data-range="last7">Last 7 Days</button>
                <button type="button" class="bnet-dp-shortcut" data-range="last40">Last 40 Days</button>
                <button type="button" class="bnet-dp-shortcut" data-range="thisMonth">This Month</button>
                <button type="button" class="bnet-dp-shortcut" data-range="lastMonth">Last Month</button>
                <button type="button" class="bnet-dp-shortcut" data-range="thisYear">This Year</button>
                <button type="button" class="bnet-dp-shortcut" data-range="lastYear">Last Year</button>
            </div>
        ` : '';

        this.container.innerHTML = `
            <div class="bnet-datepicker-trigger">
                <span class="bnet-datepicker-value-text bnet-datepicker-placeholder">${this.placeholder}</span>
                <i class="fa-solid fa-calendar-days bnet-datepicker-icon"></i>
            </div>
            <div class="bnet-datepicker-dropdown ${this.mode === 'range' ? 'has-sidebar' : ''}">
                ${sidebarHTML}
                <div class="bnet-dp-calendar-body">
                    <div class="bnet-datepicker-header">
                        <button type="button" class="bnet-dp-btn bnet-dp-prev"><i class="fa-solid fa-chevron-left"></i></button>
                        <span class="bnet-dp-month-year"></span>
                        <button type="button" class="bnet-dp-next bnet-dp-btn"><i class="fa-solid fa-chevron-right"></i></button>
                    </div>
                    <div class="bnet-datepicker-weekdays">
                        <span>Su</span><span>Mo</span><span>Tu</span><span>We</span><span>Th</span><span>Fr</span><span>Sa</span>
                    </div>
                    <div class="bnet-datepicker-days"></div>
                </div>
            </div>
        `;

        this.trigger = this.container.querySelector('.bnet-datepicker-trigger');
        this.valueText = this.container.querySelector('.bnet-datepicker-value-text');
        this.dropdown = this.container.querySelector('.bnet-datepicker-dropdown');
        this.monthYearText = this.container.querySelector('.bnet-dp-month-year');
        this.daysContainer = this.container.querySelector('.bnet-datepicker-days');
        this.prevBtn = this.container.querySelector('.bnet-dp-prev');
        this.nextBtn = this.container.querySelector('.bnet-dp-next');
        this.sidebar = this.container.querySelector('.bnet-dp-sidebar');
    }

    bindEvents() {
        this.trigger.addEventListener('click', (e) => {
            e.stopPropagation();
            this.toggle();
        });

        this.dropdown.addEventListener('click', (e) => e.stopPropagation());

        this.prevBtn.addEventListener('click', () => {
            this.viewMonth--;
            if (this.viewMonth < 0) {
                this.viewMonth = 11;
                this.viewYear--;
            }
            this.renderCalendar();
        });

        this.nextBtn.addEventListener('click', () => {
            this.viewMonth++;
            if (this.viewMonth > 11) {
                this.viewMonth = 0;
                this.viewYear++;
            }
            this.renderCalendar();
        });

        this.daysContainer.addEventListener('click', (e) => {
            const cell = e.target.closest('.bnet-dp-day:not(.empty)');
            if (!cell) return;
            this.handleDateSelect(cell.dataset.date);
        });

        this.daysContainer.addEventListener('mouseover', (e) => {
            if (this.mode !== 'range' || !this.selectedStartDate || this.selectedEndDate) return;
            const cell = e.target.closest('.bnet-dp-day:not(.empty)');
            if (cell) {
                this.hoverDate = cell.dataset.date;
                this.highlightRange();
            }
        });

        if (this.sidebar) {
            this.sidebar.addEventListener('click', (e) => {
                const btn = e.target.closest('.bnet-dp-shortcut');
                if (btn) {
                    this.applyShortcut(btn.dataset.range);
                }
            });
        }

        document.addEventListener('click', () => this.close());
    }

    toggle() {
        if (this.container.classList.contains('disabled')) return;
        const isOpen = this.container.classList.contains('open');
        document.querySelectorAll('.bnet-datepicker-container.open').forEach(el => el.classList.remove('open'));

        if (!isOpen) {
            this.container.classList.add('open');
            this.adjustPosition();
        }
    }

    close() {
        this.container.classList.remove('open');
    }

    adjustPosition() {
        this.dropdown.classList.remove('align-right');

        // Skip dropdown edge shifting on mobile viewports
        if (window.innerWidth <= 480) {
            return;
        }

        const dropdownRect = this.dropdown.getBoundingClientRect();
        const viewportWidth = window.innerWidth || document.documentElement.clientWidth;

        if (dropdownRect.right > viewportWidth) {
            this.dropdown.classList.add('align-right');
        }
    }

    formatDate(date) {
        const y = date.getFullYear();
        const m = String(date.getMonth() + 1).padStart(2, '0');
        const d = String(date.getDate()).padStart(2, '0');
        return `${y}-${m}-${d}`;
    }

    formatDisplayDate(dateStr) {
        if (!dateStr) return '';
        const [y, m, d] = dateStr.split('-');
        const months = ["Jan", "Feb", "Mar", "Apr", "May", "Jun", "Jul", "Aug", "Sep", "Oct", "Nov", "Dec"];
        return `${months[parseInt(m, 10) - 1]} ${parseInt(d, 10)}, ${y}`;
    }

    applyShortcut(type) {
        const now = new Date();
        const y = now.getFullYear();
        const m = now.getMonth();
        const d = now.getDate();

        let start, end;

        switch (type) {
            case 'today':
                start = new Date(y, m, d);
                end = new Date(y, m, d);
                break;
            case 'yesterday':
                start = new Date(y, m, d - 1);
                end = new Date(y, m, d - 1);
                break;
            case 'last7':
                start = new Date(y, m, d - 6);
                end = new Date(y, m, d);
                break;
            case 'last40':
                start = new Date(y, m, d - 39);
                end = new Date(y, m, d);
                break;
            case 'thisMonth':
                start = new Date(y, m, 1);
                end = new Date(y, m + 1, 0);
                break;
            case 'lastMonth':
                start = new Date(y, m - 1, 1);
                end = new Date(y, m, 0);
                break;
            case 'thisYear':
                start = new Date(y, 0, 1);
                end = new Date(y, 11, 31);
                break;
            case 'lastYear':
                start = new Date(y - 1, 0, 1);
                end = new Date(y - 1, 11, 31);
                break;
        }

        this.selectedStartDate = this.formatDate(start);
        this.selectedEndDate = this.formatDate(end);

        this.viewYear = end.getFullYear();
        this.viewMonth = end.getMonth();

        if (this.sidebar) {
            this.sidebar.querySelectorAll('.bnet-dp-shortcut').forEach(b => {
                b.classList.toggle('active', b.dataset.range === type);
            });
        }

        this.updateValueDisplay();
        this.renderCalendar();
        this.close();
        this.triggerChange();
    }

    renderCalendar() {
        const months = ["January", "February", "March", "April", "May", "June", "July", "August", "September", "October", "November", "December"];
        this.monthYearText.textContent = `${months[this.viewMonth]} ${this.viewYear}`;

        this.daysContainer.innerHTML = '';
        const firstDay = new Date(this.viewYear, this.viewMonth, 1).getDay();
        const daysInMonth = new Date(this.viewYear, this.viewMonth + 1, 0).getDate();

        for (let i = 0; i < firstDay; i++) {
            const emptyCell = document.createElement('div');
            emptyCell.className = 'bnet-dp-day empty';
            this.daysContainer.appendChild(emptyCell);
        }

        const todayStr = this.formatDate(new Date());

        for (let day = 1; day <= daysInMonth; day++) {
            const dateObj = new Date(this.viewYear, this.viewMonth, day);
            const dateStr = this.formatDate(dateObj);

            const dayCell = document.createElement('div');
            dayCell.className = 'bnet-dp-day';
            dayCell.textContent = day;
            dayCell.dataset.date = dateStr;

            if (dateStr === todayStr) {
                dayCell.classList.add('today');
            }

            this.daysContainer.appendChild(dayCell);
        }

        this.highlightRange();
    }

    handleDateSelect(dateStr) {
        if (this.sidebar) {
            this.sidebar.querySelectorAll('.bnet-dp-shortcut').forEach(b => b.classList.remove('active'));
        }

        if (this.mode === 'single') {
            this.selectedStartDate = dateStr;
            this.selectedEndDate = null;
            this.updateValueDisplay();
            this.close();
            this.triggerChange();
        } else if (this.mode === 'range') {
            if (!this.selectedStartDate || (this.selectedStartDate && this.selectedEndDate)) {
                this.selectedStartDate = dateStr;
                this.selectedEndDate = null;
            } else if (this.selectedStartDate && !this.selectedEndDate) {
                if (dateStr < this.selectedStartDate) {
                    this.selectedEndDate = this.selectedStartDate;
                    this.selectedStartDate = dateStr;
                } else {
                    this.selectedEndDate = dateStr;
                }
                this.updateValueDisplay();
                this.close();
                this.triggerChange();
            }
        }
        this.highlightRange();
    }

    highlightRange() {
        const cells = this.daysContainer.querySelectorAll('.bnet-dp-day:not(.empty)');

        cells.forEach(cell => {
            const dateStr = cell.dataset.date;
            cell.classList.remove('selected', 'in-range', 'range-start', 'range-end');

            if (this.mode === 'single') {
                if (dateStr === this.selectedStartDate) {
                    cell.classList.add('selected');
                }
            } else if (this.mode === 'range') {
                if (dateStr === this.selectedStartDate) {
                    cell.classList.add('selected', 'range-start');
                }
                if (dateStr === this.selectedEndDate) {
                    cell.classList.add('selected', 'range-end');
                }

                const endComparison = this.selectedEndDate || this.hoverDate;
                if (this.selectedStartDate && endComparison) {
                    const start = this.selectedStartDate < endComparison ? this.selectedStartDate : endComparison;
                    const end = this.selectedStartDate < endComparison ? endComparison : this.selectedStartDate;

                    if (dateStr > start && dateStr < end) {
                        cell.classList.add('in-range');
                    }
                }
            }
        });
    }

    updateValueDisplay() {
        let displayText = '';

        if (this.mode === 'single' && this.selectedStartDate) {
            displayText = this.formatDisplayDate(this.selectedStartDate);
        } else if (this.mode === 'range' && this.selectedStartDate && this.selectedEndDate) {
            const startLabel = this.formatDisplayDate(this.selectedStartDate);
            const endLabel = this.formatDisplayDate(this.selectedEndDate);
            displayText = `${startLabel} &ndash; ${endLabel}`;
        }

        if (displayText) {
            this.valueText.innerHTML = displayText;
            this.valueText.classList.remove('bnet-datepicker-placeholder');
        } else {
            this.valueText.textContent = this.placeholder;
            this.valueText.classList.add('bnet-datepicker-placeholder');
        }
    }

    triggerChange() {
        if (typeof this.onChange === 'function') {
            this.onChange({
                value: this.getValue(),
                displayText: this.getDisplayText()
            });
        }
    }

    getValue() {
        if (this.mode === 'single') {
            return this.selectedStartDate || '';
        } else {
            return (this.selectedStartDate && this.selectedEndDate)
                ? `${this.selectedStartDate},${this.selectedEndDate}`
                : '';
        }
    }

    getDisplayText() {
        return this.valueText.textContent;
    }

    setValue(value) {
        if (!value) return;
        if (this.mode === 'single') {
            this.selectedStartDate = value;
            const [y, m] = value.split('-').map(Number);
            if (y && m) {
                this.viewYear = y;
                this.viewMonth = m - 1;
            }
        } else if (this.mode === 'range' && value.includes(',')) {
            const parts = value.split(',');
            this.selectedStartDate = parts[0];
            this.selectedEndDate = parts[1];
            const [y, m] = parts[0].split('-').map(Number);
            if (y && m) {
                this.viewYear = y;
                this.viewMonth = m - 1;
            }
        }
        this.updateValueDisplay();
        this.renderCalendar();
    }
}

function initBNetDatePickers(selector = 'input.bnet-datepicker') {
    const inputs = document.querySelectorAll(selector);

    inputs.forEach((input) => {
        if (input.dataset.bnetDpInitialized === "true") return;

        const mode = input.getAttribute('data-mode') || 'single';
        const placeholder = input.placeholder || (mode === 'range' ? 'Select date range...' : 'Select date...');

        input.style.display = 'none';

        const container = document.createElement('div');
        if (input.disabled) container.classList.add('disabled');
        input.parentNode.insertBefore(container, input.nextSibling);

        const dpInstance = new BNetDatePicker({
            container: container,
            mode: mode,
            placeholder: placeholder,
            onChange: function (data) {
                input.value = data.value;
                input.dispatchEvent(new Event('change', { bubbles: true }));
            }
        });

        if (input.value) {
            dpInstance.setValue(input.value);
        }

        input.dataset.bnetDpInitialized = "true";
        input.bnetDatePicker = dpInstance;
    });
}

document.addEventListener('DOMContentLoaded', () => {
    initBNetDatePickers();
});

if (typeof Sys !== 'undefined' && Sys.WebForms && Sys.WebForms.PageRequestManager) {
    Sys.WebForms.PageRequestManager.getInstance().add_endRequest(() => {
        initBNetDatePickers();
    });
}