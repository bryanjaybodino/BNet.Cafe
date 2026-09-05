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