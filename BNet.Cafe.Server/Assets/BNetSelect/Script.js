// =============================================================================
// Virtual Infinite Scroll Select Component
// =============================================================================
class BNetSelect {
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
        this.container.classList.add('bnet-select-container');
        this.container.innerHTML = `
            <div class="bnet-select-trigger">
                <span class="bnet-select-value-text bnet-select-placeholder">${this.placeholder}</span>
                <i class="fa-solid fa-chevron-down bnet-select-arrow"></i>
            </div>
            <div class="bnet-select-dropdown">
                <div class="bnet-select-search-wrapper">
                    <input type="text" class="bnet-select-search-input" placeholder="Search..." />
                </div>
                <ul class="bnet-select-options-list"></ul>
            </div>
        `;

        this.trigger = this.container.querySelector('.bnet-select-trigger');
        this.valueText = this.container.querySelector('.bnet-select-value-text');
        this.dropdown = this.container.querySelector('.bnet-select-dropdown');
        this.searchInput = this.container.querySelector('.bnet-select-search-input');
        this.optionsList = this.container.querySelector('.bnet-select-options-list');
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
                const cleanQuery = typeof sanitizeInput === 'function' ? sanitizeInput(e.target.value) : e.target.value;
                this.filterAndReset(cleanQuery || '');
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
            const item = e.target.closest('.bnet-select-option');
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
        document.querySelectorAll('.bnet-select-container.open').forEach(el => el.classList.remove('open'));

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
            this.optionsList.innerHTML = `<div class="bnet-select-empty">No results found</div>`;
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
            li.className = 'bnet-select-option';
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

    selectItem(value, label, triggerChange = true) {
        this.selectedValue = value;
        this.selectedText = label;

        this.valueText.textContent = label;
        this.valueText.classList.remove('bnet-select-placeholder');

        this.optionsList.querySelectorAll('.bnet-select-option').forEach(el => {
            el.classList.toggle('selected', el.dataset.value === String(value));
        });

        this.close();

        // Only fire onChange if explicitly allowed
        if (triggerChange && typeof this.onChange === 'function') {
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
    const targetElement = document.getElementById('customSelect');
    if (!targetElement) return;

    // Mock Generating 10,000 records
    const mockLargeData = Array.from({ length: 10000 }, (_, i) => ({
        value: `val_${i + 1}`,
        label: `Option ${i + 1} - System Entry Node #${1000 + i}`
    }));

    // Instantiate virtual select
    window.mySelect = new BNetSelect({
        container: '#customSelect',
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
function initBNetSelects(selector = 'select.bnet-select') {
    const selectElements = document.querySelectorAll(selector);

    selectElements.forEach((aspSelect) => {
        if (aspSelect.dataset.bnetSelectInitialized === "true") return;

        const extractedData = [];
        const initialSelectedValue = aspSelect.value;
        const placeholder = aspSelect.getAttribute('data-placeholder') ||
            (aspSelect.options[0] ? aspSelect.options[0].text : 'Select an option...');

        Array.from(aspSelect.options).forEach((opt) => {
            if (opt.value !== "") {
                extractedData.push({
                    value: opt.value,
                    label: opt.text
                });
            }
        });

        aspSelect.style.display = 'none';

        const container = document.createElement('div');
        if (aspSelect.disabled) {
            container.classList.add('disabled');
        }
        aspSelect.parentNode.insertBefore(container, aspSelect.nextSibling);

        const vsInstance = new BNetSelect({
            container: container,
            data: extractedData,
            pageSize: 25,
            placeholder: placeholder,
            onChange: function (selected) {
                // Prevent trigger if the value hasn't actually changed
                if (aspSelect.value === selected.value) return;

                aspSelect.value = selected.value;

                // ASP.NET AutoPostBack check:
                // Dispatching the native 'change' event will trigger ASP.NET's 
                // inline 'onchange' handler automatically.
                aspSelect.dispatchEvent(new Event('change', { bubbles: true }));
            }
        });

        // Sync initial selection state WITHOUT triggering the onChange callback
        if (initialSelectedValue) {
            const existingItem = extractedData.find(item => item.value === initialSelectedValue);
            if (existingItem) {
                // Pass 'false' as the 3rd argument to prevent recursive postbacks
                vsInstance.selectItem(existingItem.value, existingItem.label, false);
            }
        }

        aspSelect.dataset.bnetSelectInitialized = "true";
        aspSelect.bnetSelect = vsInstance;
        aspSelect.BNetSelect = vsInstance;
    });
}

// Automatically bind on DOM ready
document.addEventListener('DOMContentLoaded', () => {
    initBNetSelects();
});

// Support ASP.NET AJAX UpdatePanel partial postbacks
if (typeof Sys !== 'undefined' && Sys.WebForms && Sys.WebForms.PageRequestManager) {
    Sys.WebForms.PageRequestManager.getInstance().add_endRequest(() => {
        initBNetSelects();
    });
}