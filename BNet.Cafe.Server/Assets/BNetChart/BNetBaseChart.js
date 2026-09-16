// =============================================================================
// BNet Core Base Chart Class (RESPONSIVE FIXED)
// =============================================================================
class BNetBaseChart {
    constructor(options) {
        this.container = typeof options.container === 'string'
            ? document.querySelector(options.container)
            : options.container;
        this.data = options.data || [];
        this.padding = options.padding || 40;
        this.viewBox = options.viewBox || "0 0 600 240";
        this.tooltip = this.getOrCreateTooltip();
        this.svg = null;
    }

    initSVG() {
        if (!this.container) return;

        if (this.container.tagName && this.container.tagName.toLowerCase() === 'svg') {
            this.svg = this.container;
            this.svg.innerHTML = '';
        } else {
            this.container.innerHTML = '';
            this.svg = document.createElementNS("http://www.w3.org/2000/svg", "svg");
            this.container.appendChild(this.svg);
        }

        // Essential for proper scaling on mobile devices
        this.svg.setAttribute("viewBox", this.viewBox);
        this.svg.setAttribute("preserveAspectRatio", "xMidYMid meet");
        this.svg.style.width = "100%";
        this.svg.style.height = "100%";
    }

    getOrCreateTooltip() {
        let tooltip = document.getElementById("BNetChartTooltip");
        if (!tooltip) {
            tooltip = document.createElement("div");
            tooltip.id = "BNetChartTooltip";
            tooltip.className = "chart-tooltip";
            document.body.appendChild(tooltip);
        }
        return tooltip;
    }

    showTooltip(evt, text) {
        if (!this.tooltip) return;
        this.tooltip.innerText = text;
        this.tooltip.style.opacity = "1";
        this.tooltip.style.left = `${evt.clientX + 12}px`;
        this.tooltip.style.top = `${evt.clientY - 28}px`;
    }

    hideTooltip() {
        if (this.tooltip) this.tooltip.style.opacity = "0";
    }

    renderEmpty(width = 600, height = 240) {
        if (!this.svg) return;
        this.svg.innerHTML = '';
        const emptyText = document.createElementNS("http://www.w3.org/2000/svg", "text");
        emptyText.setAttribute("x", width / 2);
        emptyText.setAttribute("y", height / 2);
        emptyText.setAttribute("text-anchor", "middle");
        emptyText.setAttribute("fill", "var(--text-light-secondary, #888)");
        emptyText.setAttribute("font-size", "14");
        emptyText.textContent = "No data available";
        this.svg.appendChild(emptyText);
    }

    updateData(newData) {
        this.data = newData;
        this.render();
    }

    render() {
        console.warn("render() must be implemented by subclass");
    }
}