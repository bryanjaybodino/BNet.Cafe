<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="ComputerMap.ascx.cs" Inherits="BNet.Cafe.Server.Forms.ComputerMap" %>

<style>
    /* ==========================================
   COMPUTER MAP / FLOOR PLAN LAYOUT
   ========================================== */
    .map-toolbar {
        display: flex;
        justify-content: space-between;
        align-items: center;
        background-color: var(--bg-light-secondary);
        border: 1px solid var(--border-light);
        border-radius: 8px;
        padding: 12px 16px;
        margin-bottom: 16px;
    }

    .map-legend {
        display: flex;
        gap: 16px;
        font-size: 13px;
    }

    .legend-item {
        display: flex;
        align-items: center;
        gap: 6px;
    }

    .status-dot {
        width: 10px;
        height: 10px;
        border-radius: 50%;
        display: inline-block;
    }

        .status-dot.green {
            background-color: #10b981;
        }

        .status-dot.red {
            background-color: #ef4444;
        }

        .status-dot.yellow {
            background-color: #f59e0b;
        }

        .status-dot.gray {
            background-color: #64748b;
        }

    .map-controls {
        display: flex;
        align-items: center;
        gap: 8px;
    }

    .zoom-controls, .history-controls {
        display: flex;
        align-items: center;
        background-color: var(--bg-light-tertiary);
        border: 1px solid var(--border-light);
        border-radius: 6px;
        padding: 2px;
    }

    .action-btn {
        background: none;
        border: none;
        color: var(--text-light);
        padding: 6px 10px;
        cursor: pointer;
        font-size: 12px;
        border-radius: 4px;
        display: inline-flex;
        align-items: center;
        justify-content: center;
    }

        .action-btn:hover:not(:disabled) {
            background-color: var(--border-light);
        }

        .action-btn:disabled {
            opacity: 0.4;
            cursor: not-allowed;
        }

    .zoom-display {
        font-size: 12px;
        font-weight: 600;
        padding: 0 8px;
        min-width: 45px;
        text-align: center;
    }

    .floor-plan-container {
        position: relative;
        width: 100%;
        height: 650px;
        background-color: var(--bg-light-secondary);
        border: 1px solid var(--border-light);
        border-radius: 12px;
        overflow: hidden;
        user-select: none;
    }

    .floor-plan-viewport {
        position: absolute;
        width: 3000px;
        height: 3000px;
        top: 0;
        left: 0;
        transform-origin: 0 0;
        background-image: radial-gradient(var(--border-light) 1px, transparent 0);
        background-size: 20px 20px;
    }

    .seat-card {
        position: absolute;
        width: 110px;
        height: 90px;
        background-color: var(--bg-light-tertiary);
        border: 2px solid var(--border-light);
        border-radius: 10px;
        display: flex;
        flex-direction: column;
        align-items: center;
        justify-content: center;
        cursor: pointer;
        user-select: none;
        box-shadow: 0 2px 5px rgba(0,0,0,0.05);
        transition: box-shadow 0.1s ease;
        z-index: 2;
    }

        .seat-card.edit-mode {
            cursor: grab;
            border-style: dashed;
        }

            .seat-card.edit-mode:active {
                cursor: grabbing;
            }

        .seat-card.selected {
            outline: 2px solid #3b82f6 !important;
            outline-offset: 3px;
            box-shadow: 0 0 12px rgba(59, 130, 246, 0.5);
        }

        .seat-card .seat-icon {
            font-size: 22px;
            margin-bottom: 4px;
        }

        .seat-card .seat-name {
            font-size: 12px;
            font-weight: 700;
        }

        .seat-card .seat-status {
            font-size: 10px;
            font-weight: 600;
            text-transform: uppercase;
        }

        /* Status variants */
        .seat-card.available {
            border-color: #10b981;
            color: #10b981;
        }

        .seat-card.occupied {
            border-color: #ef4444;
            color: #ef4444;
        }

        .seat-card.paused {
            border-color: #f59e0b;
            color: #f59e0b;
        }

        .seat-card.offline {
            border-color: #64748b;
            color: #64748b;
        }

    /* Selection Box & Alignment Lines */
    .selection-box {
        position: absolute;
        border: 1px dashed #3b82f6;
        background-color: rgba(59, 130, 246, 0.15);
        pointer-events: none;
        z-index: 10;
        display: none;
    }

    .guide-line {
        position: absolute;
        background-color: #ef4444;
        z-index: 9;
        pointer-events: none;
        display: none;
    }

        .guide-line.v-line {
            width: 1px;
            top: 0;
            bottom: 0;
        }

        .guide-line.h-line {
            height: 1px;
            left: 0;
            right: 0;
        }
</style>

<asp:UpdatePanel ID="UpdatePanel1" runat="server">
    <ContentTemplate>
        <!-- Hidden field to send updated coordinates back to Server on save -->
        <asp:HiddenField ID="HiddenField_Positions" runat="server" ClientIDMode="Static" />

        <div class="map-toolbar">
            <div class="map-legend">
                <span class="legend-item"><i class="status-dot green"></i>Available</span>
                <span class="legend-item"><i class="status-dot red"></i>Occupied</span>
                <span class="legend-item"><i class="status-dot yellow"></i>Paused</span>
                <span class="legend-item"><i class="status-dot gray"></i>Offline</span>
            </div>

            <div class="map-controls">
                <!-- Undo / Redo Controls -->
                <div class="history-controls">
                    <button type="button" id="btnUndo" class="action-btn" onclick="undo()" title="Undo (Ctrl+Z)" disabled>
                        <i class="fa-solid fa-rotate-left"></i>
                    </button>
                    <button type="button" id="btnRedo" class="action-btn" onclick="redo()" title="Redo (Ctrl+Y)" disabled>
                        <i class="fa-solid fa-rotate-right"></i>
                    </button>
                </div>

                <!-- Zoom Controls -->
                <div class="zoom-controls">
                    <button type="button" class="action-btn" onclick="adjustZoom(-0.1)"><i class="fa-solid fa-minus"></i></button>
                    <span class="zoom-display" id="zoomText">100%</span>
                    <button type="button" class="action-btn" onclick="adjustZoom(0.1)"><i class="fa-solid fa-plus"></i></button>
                    <button type="button" class="action-btn" onclick="resetZoom()"><i class="fa-solid fa-arrow-rotate-left"></i></button>
                </div>

                <button type="button" id="btnEditMode" class="btn btn-secondary" onclick="toggleEditMode()">
                    <i class="fa-solid fa-arrows-up-down-left-right"></i>Enable Drag & Drop
                </button>

                <asp:Button ID="Button_SaveLayout" runat="server" Text="Save Layout"
                    CssClass="btn btn-primary" OnClick="Button_SaveLayout_Click" OnClientClick="prepareLayoutSave()" />
            </div>
        </div>

        <!-- Interactive Floor Plan Canvas Wrapper -->
        <div class="floor-plan-container" id="floorPlanContainer">
            <div class="floor-plan-viewport" id="floorPlanViewport">
                <div class="selection-box" id="selectionBox"></div>
                <div class="guide-line v-line" id="guideVLine"></div>
                <div class="guide-line h-line" id="guideHLine"></div>

                <asp:Repeater ID="Repeater_Computers" runat="server">
                    <ItemTemplate>
                        <div class="seat-card <%# Eval("StatusClass") %>"
                            data-id="<%# Eval("DBId") %>"
                            data-name="<%# Eval("DBComputerName") %>"
                            style="left: <%# Eval("PosX") %>px; top: <%# Eval("PosY") %>px;">
                            <div class="seat-icon">
                                <i class="fa-solid fa-desktop"></i>
                            </div>
                            <div class="seat-info">
                                <span class="seat-name"><%# Eval("DBComputerName") %></span>
                                <span class="seat-status"><%# Eval("StatusText") %></span>
                            </div>
                        </div>
                    </ItemTemplate>
                </asp:Repeater>
            </div>
        </div>
    </ContentTemplate>
</asp:UpdatePanel>

<script>
    let isEditMode = false;
    let zoomLevel = 1.0;
    let selectedSeats = new Set();

    // History Stacks
    const undoStack = [];
    const redoStack = [];

    // DOM References
    let floorContainer, viewport, selectionBox, guideVLine, guideHLine, btnUndo, btnRedo;

    function initMapElements() {
        floorContainer = document.getElementById('floorPlanContainer');
        viewport = document.getElementById('floorPlanViewport');
        selectionBox = document.getElementById('selectionBox');
        guideVLine = document.getElementById('guideVLine');
        guideHLine = document.getElementById('guideHLine');
        btnUndo = document.getElementById('btnUndo');
        btnRedo = document.getElementById('btnRedo');

        if (!floorContainer || !viewport) return;

        // Apply visual edit mode styles if edit mode was active prior to postback
        if (isEditMode) {
            const seats = document.querySelectorAll('.seat-card');
            seats.forEach(s => s.classList.add('edit-mode'));
            const btn = document.getElementById('btnEditMode');
            if (btn) {
                btn.classList.replace('btn-secondary', 'btn-danger');
                btn.innerHTML = '<i class="fa-solid fa-xmark"></i> Lock Drag & Drop';
            }
        }

        applyZoom();
        bindEvents();
        updateHistoryUI();
    }

    // Handle Page / UpdatePanel Lifecycle
    document.addEventListener('DOMContentLoaded', function () {
        initMapElements();
    });

    if (typeof Sys !== 'undefined' && Sys.WebForms && Sys.WebForms.PageRequestManager) {
        Sys.WebForms.PageRequestManager.getInstance().add_endRequest(function () {
            initMapElements();
        });
    }

    // ==========================================
    // 1. UNDO / REDO SYSTEM
    // ==========================================
    function getCanvasState() {
        const state = [];
        document.querySelectorAll('.seat-card').forEach(seat => {
            state.push({
                id: seat.getAttribute('data-id'),
                left: seat.style.left,
                top: seat.style.top
            });
        });
        return state;
    }

    function saveState() {
        undoStack.push(getCanvasState());
        redoStack.length = 0; // Clear redo history on new action
        updateHistoryUI();
    }

    function applyState(state) {
        state.forEach(item => {
            const seat = document.querySelector(`.seat-card[data-id="${item.id}"]`);
            if (seat) {
                seat.style.left = item.left;
                seat.style.top = item.top;
            }
        });
    }

    function undo() {
        if (undoStack.length === 0 || !isEditMode) return;

        redoStack.push(getCanvasState());
        const previousState = undoStack.pop();
        applyState(previousState);
        updateHistoryUI();
    }

    function redo() {
        if (redoStack.length === 0 || !isEditMode) return;

        undoStack.push(getCanvasState());
        const nextState = redoStack.pop();
        applyState(nextState);
        updateHistoryUI();
    }

    function updateHistoryUI() {
        if (btnUndo) btnUndo.disabled = undoStack.length === 0 || !isEditMode;
        if (btnRedo) btnRedo.disabled = redoStack.length === 0 || !isEditMode;
    }

    // Keyboard Shortcuts for Undo/Redo
    document.addEventListener('keydown', function (e) {
        if (!isEditMode) return;

        if (e.ctrlKey || e.metaKey) {
            if (e.key.toLowerCase() === 'z') {
                if (e.shiftKey) {
                    redo();
                } else {
                    undo();
                }
                e.preventDefault();
            } else if (e.key.toLowerCase() === 'y') {
                redo();
                e.preventDefault();
            }
        }
    });

    // ==========================================
    // 2. ZOOM CONTROLS
    // ==========================================
    function adjustZoom(delta) {
        zoomLevel = Math.min(Math.max(0.4, zoomLevel + delta), 2.0);
        applyZoom();
    }

    function resetZoom() {
        zoomLevel = 1.0;
        applyZoom();
    }

    function applyZoom() {
        if (!viewport) return;
        viewport.style.transform = `scale(${zoomLevel})`;
        const zoomText = document.getElementById('zoomText');
        if (zoomText) zoomText.innerText = `${Math.round(zoomLevel * 100)}%`;
    }

    function handleWheelZoom(e) {
        if (e.ctrlKey) {
            e.preventDefault();
            adjustZoom(e.deltaY < 0 ? 0.1 : -0.1);
        }
    }

    // ==========================================
    // 3. TOGGLE EDIT MODE
    // ==========================================
    function toggleEditMode() {
        isEditMode = !isEditMode;
        const btn = document.getElementById('btnEditMode');
        const saveBtn = document.getElementById('Button_SaveLayout');
        const seats = document.querySelectorAll('.seat-card');

        clearSelection();

        if (isEditMode) {
            if (btn) {
                btn.classList.replace('btn-secondary', 'btn-danger');
                btn.innerHTML = '<i class="fa-solid fa-xmark"></i> Lock Drag & Drop';
            }
            if (saveBtn) saveBtn.classList.remove('d-none');
            seats.forEach(s => s.classList.add('edit-mode'));
        } else {
            if (btn) {
                btn.classList.replace('btn-danger', 'btn-secondary');
                btn.innerHTML = '<i class="fa-solid fa-arrows-up-down-left-right"></i> Enable Drag & Drop';
            }
            if (saveBtn) saveBtn.classList.add('d-none');
            seats.forEach(s => s.classList.remove('edit-mode'));
        }

        updateHistoryUI();
    }

    // ==========================================
    // 4. EVENT BINDING & INTERACTION ENGINE
    // ==========================================
    function clearSelection() {
        selectedSeats.forEach(seat => seat.classList.remove('selected'));
        selectedSeats.clear();
    }

    function hideGuides() {
        if (guideVLine) guideVLine.style.display = 'none';
        if (guideHLine) guideHLine.style.display = 'none';
    }

    function bindEvents() {
        if (floorContainer) {
            floorContainer.removeEventListener('wheel', handleWheelZoom);
            floorContainer.addEventListener('wheel', handleWheelZoom, { passive: false });
        }

        if (viewport) {
            viewport.removeEventListener('mousedown', handleMouseDown);
            viewport.addEventListener('mousedown', handleMouseDown);
        }
    }

    function handleMouseDown(e) {
        if (!isEditMode) return;

        const targetSeat = e.target.closest('.seat-card');

        // Clicked on empty space: Start Selection Box
        if (!targetSeat) {
            if (!e.ctrlKey && !e.shiftKey) clearSelection();

            const rect = viewport.getBoundingClientRect();
            const startX = (e.clientX - rect.left) / zoomLevel;
            const startY = (e.clientY - rect.top) / zoomLevel;

            selectionBox.style.left = `${startX}px`;
            selectionBox.style.top = `${startY}px`;
            selectionBox.style.width = '0px';
            selectionBox.style.height = '0px';
            selectionBox.style.display = 'block';

            function onMouseMove(e) {
                const currentX = (e.clientX - rect.left) / zoomLevel;
                const currentY = (e.clientY - rect.top) / zoomLevel;

                const left = Math.min(startX, currentX);
                const top = Math.min(startY, currentY);
                const width = Math.abs(currentX - startX);
                const height = Math.abs(currentY - startY);

                selectionBox.style.left = `${left}px`;
                selectionBox.style.top = `${top}px`;
                selectionBox.style.width = `${width}px`;
                selectionBox.style.height = `${height}px`;

                // Calculate intersections
                document.querySelectorAll('.seat-card').forEach(seat => {
                    const sLeft = seat.offsetLeft;
                    const sTop = seat.offsetTop;
                    const sRight = sLeft + seat.offsetWidth;
                    const sBottom = sTop + seat.offsetHeight;

                    if (sLeft < left + width && sRight > left && sTop < top + height && sBottom > top) {
                        seat.classList.add('selected');
                        selectedSeats.add(seat);
                    } else if (!e.ctrlKey) {
                        seat.classList.remove('selected');
                        selectedSeats.delete(seat);
                    }
                });
            }

            function onMouseUp() {
                selectionBox.style.display = 'none';
                document.removeEventListener('mousemove', onMouseMove);
                document.removeEventListener('mouseup', onMouseUp);
            }

            document.addEventListener('mousemove', onMouseMove);
            document.addEventListener('mouseup', onMouseUp);
            return;
        }

        // Clicked on a Seat: Toggle or Move Selection Group
        if (e.ctrlKey || e.shiftKey) {
            if (selectedSeats.has(targetSeat)) {
                targetSeat.classList.remove('selected');
                selectedSeats.delete(targetSeat);
            } else {
                targetSeat.classList.add('selected');
                selectedSeats.add(targetSeat);
            }
        } else {
            if (!selectedSeats.has(targetSeat)) {
                clearSelection();
                targetSeat.classList.add('selected');
                selectedSeats.add(targetSeat);
            }
        }

        // Snapshot state before dragging starts (for Undo)
        saveState();

        const startMouseX = e.clientX;
        const startMouseY = e.clientY;

        const initialPositions = new Map();
        selectedSeats.forEach(seat => {
            initialPositions.set(seat, {
                left: seat.offsetLeft,
                top: seat.offsetTop
            });
        });

        // Collect alignment targets (unselected seats)
        const otherSeats = Array.from(document.querySelectorAll('.seat-card')).filter(s => !selectedSeats.has(s));

        function onDragMove(e) {
            const deltaX = (e.clientX - startMouseX) / zoomLevel;
            const deltaY = (e.clientY - startMouseY) / zoomLevel;

            let snapDx = 0;
            let snapDy = 0;
            let showV = false;
            let showH = false;
            let vLinePos = 0;
            let hLinePos = 0;

            const primarySeat = targetSeat;
            const primaryInit = initialPositions.get(primarySeat);
            let rawLeft = primaryInit.left + deltaX;
            let rawTop = primaryInit.top + deltaY;

            // Alignment Snap Checks (Threshold 6px)
            const snapThreshold = 6;
            const pWidth = primarySeat.offsetWidth;
            const pHeight = primarySeat.offsetHeight;

            for (const other of otherSeats) {
                const oLeft = other.offsetLeft;
                const oTop = other.offsetTop;
                const oRight = oLeft + other.offsetWidth;
                const oBottom = oTop + other.offsetHeight;

                // Vertical Alignment (X-Axis)
                if (Math.abs(rawLeft - oLeft) < snapThreshold) {
                    snapDx = oLeft - rawLeft;
                    showV = true; vLinePos = oLeft;
                } else if (Math.abs(rawLeft + pWidth - oRight) < snapThreshold) {
                    snapDx = oRight - pWidth - rawLeft;
                    showV = true; vLinePos = oRight;
                }

                // Horizontal Alignment (Y-Axis)
                if (Math.abs(rawTop - oTop) < snapThreshold) {
                    snapDy = oTop - rawTop;
                    showH = true; hLinePos = oTop;
                } else if (Math.abs(rawTop + pHeight - oBottom) < snapThreshold) {
                    snapDy = oBottom - pHeight - rawTop;
                    showH = true; hLinePos = oBottom;
                }
            }

            // Draw Snap Lines
            if (showV) {
                guideVLine.style.left = `${vLinePos}px`;
                guideVLine.style.display = 'block';
            } else {
                guideVLine.style.display = 'none';
            }

            if (showH) {
                guideHLine.style.top = `${hLinePos}px`;
                guideHLine.style.display = 'block';
            } else {
                guideHLine.style.display = 'none';
            }

            // Apply calculated positions to whole selection
            selectedSeats.forEach(seat => {
                const init = initialPositions.get(seat);
                let newLeft = Math.max(0, init.left + deltaX + snapDx);
                let newTop = Math.max(0, init.top + deltaY + snapDy);

                // Snap to 10px grid if not snapping to alignment lines
                if (!showV) newLeft = Math.round(newLeft / 10) * 10;
                if (!showH) newTop = Math.round(newTop / 10) * 10;

                seat.style.left = `${newLeft}px`;
                seat.style.top = `${newTop}px`;
            });
        }

        function onDragEnd() {
            hideGuides();
            document.removeEventListener('mousemove', onDragMove);
            document.removeEventListener('mouseup', onDragEnd);
        }

        document.addEventListener('mousemove', onDragMove);
        document.addEventListener('mouseup', onDragEnd);
    }

    // ==========================================
    // 5. PREPARE PAYLOAD FOR SERVER
    // ==========================================
    function prepareLayoutSave() {
        const seats = document.querySelectorAll('.seat-card');
        const positions = [];

        seats.forEach(seat => {
            positions.push({
                id: seat.getAttribute('data-id'),
                x: parseInt(seat.style.left, 10) || 0,
                y: parseInt(seat.style.top, 10) || 0
            });
        });

        document.getElementById('HiddenField_Positions').value = JSON.stringify(positions);
    }
</script>