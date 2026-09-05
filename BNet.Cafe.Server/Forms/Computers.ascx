<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="Computers.ascx.cs" Inherits="BNet.Cafe.Server.Forms.Computers" %>

<style>
    :root {
        --bg: var(--bg-light);
        --bg2: var(--bg-light-secondary);
        --bg3: var(--bg-light-tertiary);
        --border: var(--border-light);
        --text: var(--text-light);
        --text2: var(--text-light-secondary);
        --text3: var(--text-light-secondary);
        --accent: var(--primary);
        --accent-glow: rgba(59, 130, 246, 0.15);
        --green: #16a34a;
        --green-bg: #dcfce7;
        --purple: var(--secondary);
        --ease: cubic-bezier(.4,0,.2,1);
        --sidebar-w: 340px;
    }

    .computers-app {
        display: flex;
        flex-direction: row;
        height: 100%;
        width: 100%;
        position: relative;
        overflow: hidden;
    }

    .main {
        flex: 1;
        display: flex;
        flex-direction: column;
        background: var(--bg);
        overflow: hidden;
        min-height: 0;
    }

    .sec-label {
        padding: 0 0 12px 0;
        font-size: 11px;
        font-family: monospace;
        font-weight: 700;
        text-transform: uppercase;
        letter-spacing: 1px;
        color: var(--text3);
    }

    /* ── Station Grid Layout ── */
    .device-list {
        padding-bottom: 16px;
        overflow-y: auto;
        display: grid;
        grid-template-columns: repeat(auto-fill, minmax(240px, 1fr));
        gap: 16px;
        align-content: start;
        flex: 1;
    }

    .no-devices {
        grid-column: 1 / -1;
        padding: 40px 20px;
        font-size: 13px;
        font-family: monospace;
        color: var(--text3);
        text-align: center;
        background: var(--bg2);
        border: 1px dashed var(--border);
        border-radius: 12px;
    }

    .device-entry {
        display: flex;
        flex-direction: column;
        gap: 12px;
        padding: 16px;
        border-radius: 12px;
        cursor: pointer;
        background: var(--bg2);
        border: 1px solid var(--border);
        position: relative;
        box-shadow: 0 1px 3px rgba(0, 0, 0, 0.05);
        transition: none;
    }

        .device-entry:hover {
            border-color: var(--accent);
            box-shadow: 0 4px 12px var(--accent-glow);
        }

        .device-entry.active {
            background: var(--bg3);
            border-color: var(--accent);
        }

    .device-top {
        display: flex;
        align-items: center;
        justify-content: space-between;
    }

    .device-avatar-wrap {
        display: flex;
        align-items: center;
        gap: 10px;
    }

    .avatar {
        width: 36px;
        height: 36px;
        border-radius: 8px;
        background: linear-gradient(135deg, var(--accent) 0%, var(--purple) 100%);
        display: flex;
        align-items: center;
        justify-content: center;
        font-size: 14px;
        font-weight: 800;
        color: #fff;
    }

    .status-badge {
        display: inline-flex;
        align-items: center;
        gap: 5px;
        padding: 4px 8px;
        border-radius: 20px;
        font-size: 11px;
        font-family: monospace;
        font-weight: 700;
        background: var(--green-bg);
        color: var(--green);
        border: 1px solid rgba(22, 163, 74, 0.2);
    }

    .activity-dot {
        width: 7px;
        height: 7px;
        border-radius: 50%;
        background: var(--green);
    }

    .device-body {
        display: flex;
        flex-direction: column;
        gap: 4px;
        background: var(--bg3);
        padding: 10px 12px;
        border-radius: 8px;
        border: 1px solid var(--border);
    }

    .device-body-label {
        font-size: 9px;
        font-family: monospace;
        text-transform: uppercase;
        font-weight: 700;
        color: var(--text3);
        letter-spacing: 0.5px;
    }

    .device-sub {
        font-size: 12px;
        color: var(--text2);
        font-family: monospace;
        font-weight: 600;
        word-break: break-word;
        line-height: 1.4;
    }

    /* Fixed Sidebar on Left Side Above Topbar */
    .details-sidebar {
        position: fixed;
        top: 0;
        left: 0;
        bottom: 0;
        width: var(--sidebar-w);
        background: var(--bg2);
        border-right: 1px solid var(--border);
        display: flex;
        flex-direction: column;
        overflow: hidden;
        z-index: 2000;
        box-shadow: 4px 0 16px rgba(0, 0, 0, 0.15);
        transform: translateX(-100%);
        transition: none;
        padding-top: 20px;
    }

        .details-sidebar.visible {
            transform: translateX(0);
        }

    .remote-launch-btn {
        display: inline-flex;
        align-items: center;
        justify-content: center;
        gap: 6px;
        padding: 10px 14px;
        margin: 16px 14px 4px;
        border-radius: 8px;
        background: var(--accent);
        color: #fff;
        border: none;
        font-size: 11px;
        font-family: monospace;
        font-weight: 700;
        cursor: pointer;
        text-decoration: none;
        transition: none;
    }

    .info-panel {
        flex: 1;
        overflow-y: auto;
        display: flex;
        flex-direction: column;
        padding: 12px;
    }

    .info-scroll {
        padding: 0 4px 8px;
    }

    .info-row {
        padding: 8px 10px;
        border-bottom: 1px solid var(--bg3);
    }

    .info-key {
        font-size: 9px;
        font-family: monospace;
        font-weight: 700;
        color: var(--text3);
        text-transform: uppercase;
    }

    .info-val {
        font-size: 12px;
        font-family: monospace;
        color: var(--text);
        word-break: break-all;
        margin-top: 2px;
    }

    .sidebar-foot {
        padding: 12px 16px;
        border-top: 1px solid var(--border);
        font-size: 11px;
        font-family: monospace;
        color: var(--text3);
    }
</style>

<div class="computers-app" id="app">
    <main class="main">
        <div class="device-list" id="deviceList">
            <div class="no-devices" id="noDevices">Waiting for connected PCs…</div>
        </div>
    </main>

    <aside class="details-sidebar" id="detailsSidebar">
        <a class="remote-launch-btn" id="remoteLaunchBtn" href="#">🖥 Launch Remote Control</a>
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
                <div class="info-row">
                    <div class="info-key">Active Window</div>
                    <div class="info-val" id="ActiveWindow">—</div>
                </div>
            </div>
        </div>
        <div class="sidebar-foot">⬡ &nbsp;<span id="clock">--:--:--</span></div>
    </aside>
</div>

<script>
    'use strict';
    var currentClient = '';
    var clientDataMap = new Map();
    var clientActivityMap = new Map();

    function tick() {
        var clock = document.getElementById('clock');
        if (clock) clock.textContent = new Date().toLocaleTimeString('en-US', { hour12: false });
    }
    setInterval(tick, 1000);
    tick();

    var detailsSidebar = document.getElementById('detailsSidebar');

    // Hide sidebar when clicking outside of it
    document.addEventListener('click', function (e) {
        if (!detailsSidebar.classList.contains('visible')) return;

        var isClickInsideSidebar = detailsSidebar.contains(e.target);
        var isClickOnDeviceCard = e.target.closest('.device-entry');

        if (!isClickInsideSidebar && !isClickOnDeviceCard) {
            deselectDevice();
        }
    });

    const socketUrl = 'ws://192.168.1.2:2050/ws/browser';
    const textDecoder = new TextDecoder('utf-8');
    let ws = null;

    function connectWebSocket() {
        ws = new WebSocket(socketUrl);
        ws.binaryType = 'arraybuffer';

        ws.onopen = function () {
            console.log('Connected to server via WebSocket');
        };

        ws.onmessage = function (event) {
            if (event.data instanceof ArrayBuffer) {
                const bytes = new Uint8Array(event.data);
                if (bytes.length === 0) return;

                const messageType = bytes[0];
                const payloadBytes = bytes.subarray(1);

                if (messageType === 0x11) {
                    try {
                        const jsonString = textDecoder.decode(payloadBytes);
                        const clients = JSON.parse(jsonString);
                        handleClientListUpdate(clients);
                    } catch (err) {
                        console.error('Failed to parse CLIENT_LIST JSON:', err);
                    }
                } else if (messageType === 0x12) {
                    try {
                        const jsonString = textDecoder.decode(payloadBytes);
                        const activity = JSON.parse(jsonString);
                        handleActivityUpdate(activity);
                    } catch (err) {
                        console.error('Failed to parse ACTIVITY JSON:', err);
                    }
                }
            }
        };

        ws.onclose = function () {
            setTimeout(connectWebSocket, 3000);
        };

        ws.onerror = function (err) {
            console.error('WebSocket error:', err);
        };
    }

    function handleClientListUpdate(clients) {
        clientDataMap.clear();
        clients.forEach(function (client) {
            if (client.AccountName) {
                clientDataMap.set(client.AccountName.toLowerCase(), client);
            }
        });

        renderDevices(clients);

        if (currentClient && clientDataMap.has(currentClient.toLowerCase())) {
            updateInfo(clientDataMap.get(currentClient.toLowerCase()));
        }
    }

    function handleActivityUpdate(activity) {
        if (!activity || !activity.accountName) return;
        const accountKey = activity.accountName.toLowerCase();
        clientActivityMap.set(accountKey, activity);

        var subEl = document.getElementById('sub-' + accountKey);
        if (subEl) {
            subEl.textContent = activity.windowTitle || activity.appName || 'Online';
        }

        if (currentClient && currentClient.toLowerCase() === accountKey) {
            document.getElementById('ActiveWindow').textContent = activity.windowTitle || '—';
        }
    }

    function renderDevices(clients) {
        var list = document.getElementById('deviceList');
        if (!list) return;
        list.innerHTML = '';

        if (clients.length === 0) {
            var ph = document.createElement('div');
            ph.className = 'no-devices';
            ph.id = 'noDevices';
            ph.textContent = 'Waiting for connected PCs…';
            list.appendChild(ph);
            return;
        }

        clients.forEach(function (item) {
            var name = item.AccountName;
            if (!name) return;
            var accountKey = name.toLowerCase();

            var el = document.createElement('div');
            el.className = 'device-entry';
            if (currentClient.toLowerCase() === accountKey) el.classList.add('active');
            el.dataset.name = name;

            var activity = clientActivityMap.get(accountKey);
            var statusText = activity ? (activity.windowTitle || activity.appName) : 'Idle / Online';

            el.innerHTML =
                '<div class="device-top">' +
                '<div class="device-avatar-wrap">' +
                '<div class="avatar">' + escapeHtml(name).toUpperCase() + '</div>' +
                '</div>' +
                '<div class="status-badge"><span class="activity-dot"></span>Active</div>' +
                '</div>' +
                '<div class="device-body">' +
                '<div class="device-body-label">Active Task</div>' +
                '<div class="device-sub" id="sub-' + accountKey + '">' + escapeHtml(statusText) + '</div>' +
                '</div>';

            el.addEventListener('click', function (e) {
                e.stopPropagation();
                selectDevice(name);
            });
            list.appendChild(el);
        });
    }

    function selectDevice(name) {
        currentClient = name;
        var accountKey = name.toLowerCase();

        document.querySelectorAll('.device-entry').forEach(function (el) {
            el.classList.toggle('active', el.dataset.name.toLowerCase() === accountKey);
        });

        detailsSidebar.classList.add('visible');
        document.getElementById('remoteLaunchBtn').href = 'Remote.html?client=' + encodeURIComponent(name);

        if (clientDataMap.has(accountKey)) {
            updateInfo(clientDataMap.get(accountKey));
        }

        if (clientActivityMap.has(accountKey)) {
            const act = clientActivityMap.get(accountKey);
            document.getElementById('ActiveWindow').textContent = act.windowTitle || '—';
        } else {
            document.getElementById('ActiveWindow').textContent = '—';
        }
    }

    function deselectDevice() {
        currentClient = '';
        detailsSidebar.classList.remove('visible');
        document.querySelectorAll('.device-entry').forEach(function (el) { el.classList.remove('active'); });
    }

    function updateInfo(item) {
        document.getElementById('Username').textContent = item.AccountName || '—';
        document.getElementById('MachineName').textContent = item.MachineName || '—';
        document.getElementById('Workgroup').textContent = item.WorkGroup || '—';
        document.getElementById('Windows').textContent = item.Windows || '—';
        document.getElementById('WindowsVersion').textContent = item.WindowsVersion || '—';
        document.getElementById('OSVersion').textContent = item.OSVersion || '—';
        document.getElementById('OSArchitecture').textContent = item.OSArchitecture || '—';
        document.getElementById('SerialNumber').textContent = item.SerialNumber || '—';
        document.getElementById('ProcessorCount').textContent = item.ProcessorCount || '—';
        document.getElementById('ScreenCount').textContent = item.ScreenCount || '—';
    }

    function escapeHtml(str) {
        if (!str) return '';
        return String(str).replace(/&/g, "&amp;").replace(/</g, "&lt;").replace(/>/g, "&gt;");
    }

    connectWebSocket();
</script>
