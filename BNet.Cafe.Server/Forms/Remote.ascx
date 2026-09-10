<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="Remote.ascx.cs" Inherits="BNet.Cafe.Server.Forms.Remote" %>
<%@ Register Src="~/Forms/Modals/RemoteMessage.ascx" TagPrefix="uc" TagName="RemoteMessage" %>

<link href="Assets/Pages/Remote.css" rel="stylesheet" />
<asp:ScriptManagerProxy ID="ScriptManagerProxy1" runat="server">
    <Scripts>
        <asp:ScriptReference Path="~/Assets/Pages/Remote.js" />
        <asp:ScriptReference Path="~/Assets/Pages/RemoteMessaging.js" />
    </Scripts>
</asp:ScriptManagerProxy>

<asp:UpdatePanel ID="UpdatePanel1" runat="server">
    <ContentTemplate>
        <!-- Remote Message Modal -->
        <uc:RemoteMessage ID="RemoteMessageModal" runat="server" />
    </ContentTemplate>
</asp:UpdatePanel>


<div class="computers-app" id="app">
    <main class="main">
        <div class="device-list" id="deviceList">
            <div class="no-devices" id="noDevices">Waiting for connected PCs…</div>
        </div>
    </main>

    <aside class="details-sidebar" id="detailsSidebar">
        <br />
        <div class="info-panel" id="infoPanel">
            <div class="sec-label">Station Hardware & System</div>
            <div class="info-scroll">
                <div class="info-row">
                    <div class="info-key">Account Name</div>
                    <div class="info-val" id="Username">—</div>
                </div>
                <div class="info-row">
                    <div class="info-key">Machine Name</div>
                    <div class="info-val" id="MachineName">—</div>
                </div>
                <div class="info-row">
                    <div class="info-key">Workgroup</div>
                    <div class="info-val" id="Workgroup">—</div>
                </div>
                <div class="info-row">
                    <div class="info-key">Windows</div>
                    <div class="info-val" id="Windows">—</div>
                </div>
                <div class="info-row">
                    <div class="info-key">Windows Version</div>
                    <div class="info-val" id="WindowsVersion">—</div>
                </div>
                <div class="info-row">
                    <div class="info-key">OS Version</div>
                    <div class="info-val" id="OSVersion">—</div>
                </div>
                <div class="info-row">
                    <div class="info-key">Architecture</div>
                    <div class="info-val" id="OSArchitecture">—</div>
                </div>
                <div class="info-row">
                    <div class="info-key">Serial Number</div>
                    <div class="info-val" id="SerialNumber">—</div>
                </div>
                <div class="info-row">
                    <div class="info-key">Processor Count</div>
                    <div class="info-val" id="ProcessorCount">—</div>
                </div>
                <div class="info-row">
                    <div class="info-key">Screen Count</div>
                    <div class="info-val" id="ScreenCount">—</div>
                </div>
            </div>
            <!-- Action Buttons Layout -->
            <div class="sidebar-actions">
                <a class="remote-launch-btn" id="remoteLaunchBtn" href="#">🖥 Launch Remote Control</a>
                <button type="button" class="btn-sidebar-action" id="remoteMsgBtn" onclick="openRemoteMessageModal(currentClient)">💬 Message Client</button>
            </div>
        </div>
        <div class="sidebar-foot">⬡ &nbsp;<span id="clock">--:--:--</span></div>
    </aside>
</div>
