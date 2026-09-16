// =============================================================================
// BNet Line Chart Component (FIXED)
// =============================================================================
class BNetLineChart extends BNetBaseChart {
    constructor(options) {
        super(options);
        // Set chart-specific properties
        this.viewBox = options.viewBox || "0 0 600 240";
        this.width = options.width || 600;
        this.height = options.height || 240;
        this.xKey = options.xKey || 'date';
        this.yKey = options.yKey || 'amount';
        this.currencySymbol = options.currencySymbol !== undefined ? options.currencySymbol : "₱";
        this.strokeColor = options.strokeColor || "var(--primary)";

        // NOW initialize and render
        this.initSVG();
        this.render();
    }

    render() {
        this.initSVG();
        const total = this.data.reduce((acc, item) => acc + (item[this.yKey] || 0), 0);
        if (!this.data.length || total <= 0) {
            this.renderEmpty(this.width, this.height);
            return;
        }

        const maxVal = Math.max(...this.data.map(d => d[this.yKey])) * 1.1 || 1;
        const stepX = this.data.length > 1 ? (this.width - this.padding * 2) / (this.data.length - 1) : 0;

        const points = this.data.map((d, i) => ({
            x: this.padding + i * stepX,
            y: this.height - this.padding - (d[this.yKey] / maxVal) * (this.height - this.padding * 2),
            data: d
        }));

        // Render Y-Axis Gridlines & Labels
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
            label.textContent = `${this.currencySymbol}${val}`;
            this.svg.appendChild(label);
        }

        // Render Path
        const pathData = points.reduce((acc, pt, i) =>
            i === 0 ? `M ${pt.x} ${pt.y}` : `${acc} L ${pt.x} ${pt.y}`, "");

        const path = document.createElementNS("http://www.w3.org/2000/svg", "path");
        path.setAttribute("d", pathData);
        path.setAttribute("fill", "none");
        path.setAttribute("stroke", this.strokeColor);
        path.setAttribute("stroke-width", "3");
        this.svg.appendChild(path);

        // Render Data Dots & Tooltips
        points.forEach(pt => {
            const circle = document.createElementNS("http://www.w3.org/2000/svg", "circle");
            circle.setAttribute("cx", pt.x); circle.setAttribute("cy", pt.y);
            circle.setAttribute("r", "5");
            circle.setAttribute("fill", this.strokeColor);
            circle.style.cursor = "pointer";
            circle.addEventListener("mousemove", (e) => {
                const val = typeof pt.data[this.yKey] === 'number' ? pt.data[this.yKey].toFixed(2) : pt.data[this.yKey];
                this.showTooltip(e, `${pt.data[this.xKey]}: ${this.currencySymbol}${val}`);
            });
            circle.addEventListener("mouseleave", () => this.hideTooltip());
            this.svg.appendChild(circle);

            const xLabel = document.createElementNS("http://www.w3.org/2000/svg", "text");
            xLabel.setAttribute("x", pt.x); xLabel.setAttribute("y", this.height - 10);
            xLabel.setAttribute("text-anchor", "middle");
            xLabel.setAttribute("fill", "var(--text-light-secondary)");
            xLabel.setAttribute("font-size", "11");
            xLabel.textContent = pt.data[this.xKey];
            this.svg.appendChild(xLabel);
        });
    }
}