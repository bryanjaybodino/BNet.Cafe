'use strict';
var currentClient = '';
var clientDataMap = new Map();
var clientActivityMap = new Map();

function tick() {
    var clock = document.getElementById('clock');
    if (clock) clock.textContent = new Date().toLocaleTimeString('en-US', { hour12: false });
}
setInterval(tick, 1000);

// Safe helper to get sidebar dynamically
function getDetailsSidebar() {
    return document.getElementById('detailsSidebar');
}

// Hide sidebar when clicking outside of it
document.addEventListener('click', function (e) {
    var detailsSidebar = getDetailsSidebar();
    if (!detailsSidebar || !detailsSidebar.classList.contains('visible')) return;

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
            clientDataMap.set(client.AccountName.toUpperCase(), client);
        }
    });

    renderDevices(clients);

    if (currentClient && clientDataMap.has(currentClient.toUpperCase())) {
        updateInfo(clientDataMap.get(currentClient.toUpperCase()));
    }
}

function handleActivityUpdate(activity) {
    if (!activity || !activity.accountName) return;
    const accountKey = activity.accountName.toUpperCase();
    clientActivityMap.set(accountKey, activity);

    var subEl = document.getElementById('sub-' + accountKey);
    if (subEl) {
        subEl.textContent = activity.windowTitle || activity.appName || 'Online';
    }

    if (currentClient && currentClient.toUpperCase() === accountKey) {
        var activeWinEl = document.getElementById('ActiveWindow');
        if (activeWinEl) {
            activeWinEl.textContent = activity.windowTitle || '—';
        }
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
        var accountKey = name.toUpperCase();

        var el = document.createElement('div');
        el.className = 'device-entry';
        if (currentClient.toUpperCase() === accountKey) el.classList.add('active');
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
    currentClient = name ? name.toUpperCase() : ''; // Transform to uppercase
    var accountKey = currentClient;

    document.querySelectorAll('.device-entry').forEach(function (el) {
        el.classList.toggle('active', el.dataset.name.toUpperCase() === accountKey);
    });

    var detailsSidebar = getDetailsSidebar();
    if (detailsSidebar) {
        detailsSidebar.classList.add('visible');
    }

    var launchBtn = document.getElementById('remoteLaunchBtn');
    if (launchBtn) {
        launchBtn.href = 'Remote.html?client=' + encodeURIComponent(currentClient);
    }

    if (clientDataMap.has(accountKey)) {
        updateInfo(clientDataMap.get(accountKey));
    }

    var activeWinEl = document.getElementById('ActiveWindow');
    if (activeWinEl) {
        if (clientActivityMap.has(accountKey)) {
            const act = clientActivityMap.get(accountKey);
            activeWinEl.textContent = act.windowTitle || '—';
        } else {
            activeWinEl.textContent = '—';
        }
    }
}

function deselectDevice() {
    currentClient = '';
    var detailsSidebar = getDetailsSidebar();
    if (detailsSidebar) {
        detailsSidebar.classList.remove('visible');
    }
    document.querySelectorAll('.device-entry').forEach(function (el) {
        el.classList.remove('active');
    });
}

function updateInfo(item) {
    var setElText = function (id, val) {
        var el = document.getElementById(id);
        if (el) el.textContent = val || '—';
    };

    setElText('Username', item.AccountName ? item.AccountName.toUpperCase() : null);
    setElText('MachineName', item.MachineName);
    setElText('Workgroup', item.WorkGroup);
    setElText('Windows', item.Windows);
    setElText('WindowsVersion', item.WindowsVersion);
    setElText('OSVersion', item.OSVersion);
    setElText('OSArchitecture', item.OSArchitecture);
    setElText('SerialNumber', item.SerialNumber);
    setElText('ProcessorCount', item.ProcessorCount);
    setElText('ScreenCount', item.ScreenCount);
}

function escapeHtml(str) {
    if (!str) return '';
    return String(str).replace(/&/g, "&amp;").replace(/</g, "&lt;").replace(/>/g, "&gt;");
}

// Ensure DOM and components are loaded before connecting
document.addEventListener('DOMContentLoaded', function () {
    tick();
    connectWebSocket();
});