// =============================================================================
// BNet Bar Chart Component (DYNAMIC MOBILE RESPONSIVE)
// =============================================================================
class BNetBarChart extends BNetBaseChart {
    constructor(options) {
        super(options);
        this.labelKey = options.labelKey || 'pc';
        this.valueKey = options.valueKey || 'count';
        this.barColor = options.barColor || "var(--secondary)";
        this.unitLabel = options.unitLabel || "rentals";
        this.height = options.height || 240;
        this.baseWidth = options.width || 800;

        this.initSVG();
        this.render();
    }

    render() {
        if (!this.data) this.data = [];

        // Ensure minimum 55px width allocation per terminal so labels are always readable
        const requiredWidth = Math.max(this.baseWidth, this.data.length * 55 + this.padding * 2);
        this.width = requiredWidth;
        this.viewBox = `0 0 ${this.width} ${this.height}`;

        this.initSVG();

        // Apply width directly to SVG element style for crisp rendering
        if (this.svg) {
            this.svg.style.minWidth = `${this.width}px`;
        }

        const totalCount = this.data.reduce((acc, item) => acc + (item[this.valueKey] || 0), 0);
        if (!this.data.length || totalCount <= 0) {
            this.renderEmpty(this.width, this.height);
            return;
        }

        const maxVal = Math.max(...this.data.map(d => d[this.valueKey])) * 1.15 || 1;
        const availableWidth = this.width - this.padding * 2;
        const gap = availableWidth / this.data.length;
        const barWidth = Math.min(45, gap * 0.6);

        // Render Axis Gridlines & Values
        for (let i = 0; i <= 4; i++) {
            const y = this.height - this.padding - (i / 4) * (this.height - this.padding * 2);
            const val = (maxVal * (i / 4)).toFixed(0);

            const line = document.createElementNS("http://www.w3.org/2000/svg", "line");
            line.setAttribute("x1", this.padding); line.setAttribute("y1", y);
            line.setAttribute("x2", this.width - this.padding); line.setAttribute("y2", y);
            line.setAttribute("stroke", "var(--border-light)");
            line.setAttribute("stroke-dasharray", "4");
            this.svg.appendChild(line);

            const label = document.createElementNS("http://www.w3.org/2000/svg", "text");
            label.setAttribute("x", this.padding - 10); label.setAttribute("y", y + 4);
            label.setAttribute("text-anchor", "end");
            label.setAttribute("fill", "var(--text-light-secondary)");
            label.setAttribute("font-size", "10");
            label.textContent = val;
            this.svg.appendChild(label);
        }

        // Render Bars & Labels
        this.data.forEach((item, i) => {
            const x = this.padding + i * gap + (gap - barWidth) / 2;
            const barHeight = (item[this.valueKey] / maxVal) * (this.height - this.padding * 2);
            const y = this.height - this.padding - barHeight;

            const rect = document.createElementNS("http://www.w3.org/2000/svg", "rect");
            rect.setAttribute("x", x); rect.setAttribute("y", y);
            rect.setAttribute("width", barWidth); rect.setAttribute("height", barHeight);
            rect.setAttribute("fill", this.barColor);
            rect.setAttribute("rx", "4");
            rect.style.cursor = "pointer";
            rect.addEventListener("mousemove", (e) => this.showTooltip(e, `${item[this.labelKey]}: ${item[this.valueKey]} ${this.unitLabel}`));
            rect.addEventListener("mouseleave", () => this.hideTooltip());
            this.svg.appendChild(rect);

            const xLabel = document.createElementNS("http://www.w3.org/2000/svg", "text");
            xLabel.setAttribute("x", x + barWidth / 2); xLabel.setAttribute("y", this.height - 12);
            xLabel.setAttribute("text-anchor", "middle");
            xLabel.setAttribute("fill", "var(--text-light-secondary)");
            xLabel.setAttribute("font-size", "11");
            xLabel.textContent = item[this.labelKey];
            this.svg.appendChild(xLabel);

            const valLabel = document.createElementNS("http://www.w3.org/2000/svg", "text");
            valLabel.setAttribute("x", x + barWidth / 2); valLabel.setAttribute("y", y - 8);
            valLabel.setAttribute("text-anchor", "middle");
            valLabel.setAttribute("fill", "var(--text-light)");
            valLabel.setAttribute("font-size", "11");
            valLabel.setAttribute("font-weight", "bold");
            valLabel.textContent = item[this.valueKey];
            this.svg.appendChild(valLabel);
        });
    }
}