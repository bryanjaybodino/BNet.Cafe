// =============================================================================
// BNet Donut Chart Component (FIXED)
// =============================================================================
class BNetDonutChart extends BNetBaseChart {
    constructor(options) {
        super(options);
        // Set chart-specific properties
        this.viewBox = options.viewBox || "0 0 200 200";
        this.radius = options.radius || 65;
        this.cx = options.cx || 100;
        this.cy = options.cy || 100;
        this.labelKey = options.labelKey || 'label';
        this.valueKey = options.valueKey || 'value';
        this.colorKey = options.colorKey || 'color';
        this.onLegendUpdate = options.onLegendUpdate || null;

        // NOW initialize and render
        this.initSVG();
        this.render();
    }

    render() {
        this.initSVG();
        const total = this.data.reduce((acc, item) => acc + (item[this.valueKey] || 0), 0);

        if (total <= 0) {
            this.renderEmpty(200, 200);
            return;
        }

        let accumulatedAngle = 0;

        this.data.forEach(item => {
            const val = item[this.valueKey] || 0;
            if (val <= 0) return;

            const angle = (val / total) * 360;
            const startAngle = accumulatedAngle;
            const endAngle = accumulatedAngle + angle;
            accumulatedAngle += angle;

            const effectiveEndAngle = angle >= 360 ? startAngle + 359.999 : endAngle;

            const x1 = this.cx + this.radius * Math.cos(Math.PI * (startAngle - 90) / 180);
            const y1 = this.cy + this.radius * Math.sin(Math.PI * (startAngle - 90) / 180);
            const x2 = this.cx + this.radius * Math.cos(Math.PI * (effectiveEndAngle - 90) / 180);
            const y2 = this.cy + this.radius * Math.sin(Math.PI * (effectiveEndAngle - 90) / 180);

            const largeArcFlag = angle > 180 ? 1 : 0;
            const pathData = `M ${x1} ${y1} A ${this.radius} ${this.radius} 0 ${largeArcFlag} 1 ${x2} ${y2}`;

            const path = document.createElementNS("http://www.w3.org/2000/svg", "path");
            path.setAttribute("d", pathData);
            path.setAttribute("fill", "none");
            path.setAttribute("stroke", item[this.colorKey] || "#3b82f6");
            path.setAttribute("stroke-width", "20");
            path.setAttribute("stroke-linecap", "butt");
            path.setAttribute("class", "donut-segment");

            path.addEventListener("mousemove", (e) => {
                this.showTooltip(e, `${item[this.labelKey]}: ${val} (${((val / total) * 100).toFixed(1)}%)`);
            });
            path.addEventListener("mouseleave", () => this.hideTooltip());

            this.svg.appendChild(path);
        });

        // Center Total Indicator
        const text = document.createElementNS("http://www.w3.org/2000/svg", "text");
        text.setAttribute("x", this.cx);
        text.setAttribute("y", this.cy + 5);
        text.setAttribute("text-anchor", "middle");
        text.setAttribute("fill", "var(--text-light, #333)");
        text.setAttribute("font-size", "18");
        text.setAttribute("font-weight", "bold");
        text.textContent = total;
        this.svg.appendChild(text);

        if (typeof this.onLegendUpdate === 'function') {
            this.onLegendUpdate(this.data, total);
        }
    }
}