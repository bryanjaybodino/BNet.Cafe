class BNetModal {
    constructor(target) {
        // Find existing DOM element by ID or CSS selector
        this.overlay = typeof target === 'string' ? document.querySelector(target) : target;

        if (!this.overlay) {
            console.error(`BNetModal: Element "${target}" not found.`);
            return;
        }

        this.initDOM();
        this.bindEvents();
    }

    initDOM() {
        // Ensure overlay has correct base class
        if (!this.overlay.classList.contains('bnet-modal-overlay')) {
            this.overlay.classList.add('bnet-modal-overlay');
        }
    }

    bindEvents() {
        // Close on clicking close triggers or backdrop
        this.overlay.addEventListener('click', (e) => {
            if (
                e.target === this.overlay ||
                e.target.closest('.bnet-modal-close') ||
                e.target.closest('[data-bnet-dismiss]')
            ) {
                this.close();
            }
        });
    }

    open() {
        if (!this.overlay) return;

        // Auto-calculate stacked z-index if multiple modals are open
        const activeModals = document.querySelectorAll('.bnet-modal-overlay.active').length;
        this.overlay.style.zIndex = 10000 + (activeModals * 10);

        // Display flex first, then trigger opacity transition
        this.overlay.style.display = 'flex';
        requestAnimationFrame(() => {
            this.overlay.classList.add('active');
        });
    }

    close() {
        if (!this.overlay) return;

        this.overlay.classList.remove('active');

        // Wait for CSS transition before setting display: none
        setTimeout(() => {
            if (!this.overlay.classList.contains('active')) {
                this.overlay.style.display = 'none';
            }
        }, 200);
    }

    // Static initializer for HTML data attributes: <button data-bnet-target="#myModal">
    static initAutoBind() {
        document.addEventListener('click', (e) => {
            const trigger = e.target.closest('[data-bnet-target]');
            if (trigger) {
                const targetSelector = trigger.getAttribute('data-bnet-target');
                const modal = new BNetModal(targetSelector);
                modal.open();
            }
        });
    }
}

// Auto-bind click events globally on DOM load
document.addEventListener('DOMContentLoaded', () => {
    BNetModal.initAutoBind();
});