<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="SeatMap.ascx.cs" Inherits="BNet.Cafe.Server.Forms.SeatMap" %>

<link href="Assets/Pages/SeatMap.css" rel="stylesheet" />
<asp:UpdatePanel ID="UpdatePanel1" runat="server">
    <ContentTemplate>
        <asp:LinkButton ID="LinkButton_Refresh" OnClick="LinkButton_Refresh_Click" runat="server"></asp:LinkButton>
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