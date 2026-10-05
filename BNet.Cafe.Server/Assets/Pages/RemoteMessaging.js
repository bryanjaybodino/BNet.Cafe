'use strict';

const textEncoderRemoteMessaging = new TextEncoder();
const textDecoderRemoteMessaging = new TextDecoder('utf-8');
let serverWsRemoteMessaging = null;
let browserWsRemoteMessaging = null;
let isFetchingNotifications = false;

// Determine handler endpoint path dynamically
function getHandlerEndpoint(handlerName) {
    return /\.aspx$/i.test(window.location.pathname)
        ? window.location.pathname.replace(/[^\/]+\.aspx$/i, 'Ashx/' + handlerName)
        : window.location.pathname.replace(/[^\/]+$/i, 'Ashx/' + handlerName);
}

// Convert date/time string to 'hh:mm tt' format (e.g., "03:45 PM")
function format12HourTime(timeStr, dateStr) {
    let dateObj = null;

    if (dateStr && timeStr) {
        dateObj = new Date(`${dateStr} ${timeStr}`);
    } else if (timeStr) {
        dateObj = new Date(`1970-01-01 ${timeStr}`);
    }

    if (!dateObj || isNaN(dateObj.getTime())) {
        if (timeStr && timeStr.includes(':')) {
            const parts = timeStr.split(':');
            let hours = parseInt(parts[0], 10);
            const minutes = parts[1] || '00';
            if (!isNaN(hours)) {
                const ampm = hours >= 12 ? 'PM' : 'AM';
                hours = hours % 12 || 12;
                const formattedHours = hours < 10 ? '0' + hours : hours;
                return `${formattedHours}:${minutes} ${ampm}`;
            }
        }
        return timeStr || '';
    }

    let hours = dateObj.getHours();
    const minutes = dateObj.getMinutes();
    const ampm = hours >= 12 ? 'PM' : 'AM';
    hours = hours % 12 || 12;
    const formattedHours = hours < 10 ? '0' + hours : hours;
    const formattedMinutes = minutes < 10 ? '0' + minutes : minutes;

    return `${formattedHours}:${formattedMinutes} ${ampm}`;
}

// Fetch active (non-deleted) chat messages from GetChatMessagesHandler.ashx
function fetchAndRenderNotifications(playSoundIfNew = false) {
    if (isFetchingNotifications) return;
    isFetchingNotifications = true;

    const endpoint = getHandlerEndpoint('GetChatMessagesHandler.ashx') + '?isDeleted=false';

    fetch(endpoint)
        .then(response => response.json())
        .then(res => {
            if (res && res.success && Array.isArray(res.data)) {
                const messages = res.data;

                // Only play sound if requested and active message count is greater than zero
                if (playSoundIfNew && messages.length > 0) {
                    playNotificationSound();
                }

                renderNotificationUI(messages);
            } else {
                renderNotificationUI([]);
            }
        })
        .catch(err => {
            console.error('Error fetching chat notifications:', err);
            renderNotificationUI([]);
        })
        .finally(() => {
            isFetchingNotifications = false;
        });
}

// Pure JS execution to delete single message called from ChatMessage.ascx modal
function deleteSingleMessageJS(msgId) {
    if (!msgId) return;

    const deleteEndpoint = getHandlerEndpoint('DeleteChatMessagesHandler.ashx') + '?id=' + encodeURIComponent(msgId);

    fetch(deleteEndpoint)
        .then(res => res.json())
        .then(res => {
            if (res && res.success) {
                // Close modal and refresh UI list
                if (typeof closeChatMessageModal === 'function') {
                    closeChatMessageModal();
                }
                fetchAndRenderNotifications();
            } else {
                alert(res.message || "Failed to delete message.");
            }
        })
        .catch(err => {
            console.error("Error deleting message:", err);
            alert("An error occurred while deleting the message.");
        });
}

// Delete all non-deleted messages via DeleteChatMessagesHandler.ashx
function clearAllChatNotifications() {
    const endpoint = getHandlerEndpoint('GetChatMessagesHandler.ashx') + '?isDeleted=false';

    fetch(endpoint)
        .then(res => res.json())
        .then(res => {
            if (res && res.success && Array.isArray(res.data) && res.data.length > 0) {
                const deletePromises = res.data.map(item => {
                    const msgId = item.id || item.Id || item.DBId;
                    const deleteEndpoint = getHandlerEndpoint('DeleteChatMessagesHandler.ashx') + '?id=' + encodeURIComponent(msgId);
                    return fetch(deleteEndpoint);
                });

                return Promise.all(deletePromises);
            }
        })
        .then(() => {
            fetchAndRenderNotifications();
            toggleNotifDropdown(false);
        })
        .catch(err => {
            console.error('Error clearing chat notifications:', err);
        });
}

// Render UI using database records
function renderNotificationUI(messages) {
    const badge = document.getElementById('notifBadge');
    const notifList = document.getElementById('notifList');

    // 1. Update Badge Count
    const count = messages.length;
    if (badge) {
        if (count > 0) {
            badge.textContent = count > 99 ? '99+' : count;
            badge.classList.remove('d-none');
            badge.classList.add('pulse');
        } else {
            badge.classList.add('d-none');
            badge.classList.remove('pulse');
        }
    }

    // 2. Render List
    if (!notifList) return;

    if (count === 0) {
        notifList.innerHTML = '<div class="notif-empty" id="emptyNotifMsg">No new messages</div>';
        return;
    }

    notifList.innerHTML = '';
    messages.forEach(item => {
        const div = document.createElement('div');
        div.className = 'notif-item';

        const msgId = item.id || item.Id || item.DBId;
        const computerName = item.computerName || item.ComputerName || 'Terminal';
        const messageText = item.message || item.Message || '';
        const timeCreated = item.timeCreated || item.TimeCreated || '';
        const dateCreated = item.dateCreated || item.DateCreated || '';

        const formattedTime = format12HourTime(timeCreated, dateCreated);

        div.innerHTML = `
            <div class="notif-item-title" style="display: flex; justify-content: space-between; align-items: center;">
                <span><i class="fas fa-comment-dots"></i> ${escapeHtml(computerName)}</span>
                <span class="notif-item-time" style="font-weight: normal; font-size: 11px; opacity: 0.8;">${escapeHtml(formattedTime)}</span>
            </div>
            <div class="notif-item-msg" style="margin-top: 4px;">${escapeHtml(messageText)}</div>
        `;

        // Open modal on click without postback
        div.onclick = (e) => {
            e.preventDefault();
            e.stopPropagation();
            toggleNotifDropdown(false);

            if (typeof openChatMessageModal === 'function') {
                openChatMessageModal(msgId, computerName, messageText, formattedTime);
            } else {
                console.error("openChatMessageModal function is not defined. Ensure ChatMessage.ascx is included on the page.");
            }
        };

        notifList.appendChild(div);
    });
}

// WebSocket Connection - Outbound to PC
function connectServerWebSocket() {
    if (serverWsRemoteMessaging && (serverWsRemoteMessaging.readyState === WebSocket.OPEN || serverWsRemoteMessaging.readyState === WebSocket.CONNECTING)) {
        return;
    }
    const endpoint = 'ws://' + window.location.hostname + ':2050/ws/server';
    serverWsRemoteMessaging = new WebSocket(endpoint);
    serverWsRemoteMessaging.binaryType = 'arraybuffer';

    serverWsRemoteMessaging.onclose = function () {
        setTimeout(connectServerWebSocket, 3000);
    };
}

function sendTextMessageToPC(targetClient, messageContent) {
    if (!serverWsRemoteMessaging || serverWsRemoteMessaging.readyState !== WebSocket.OPEN) return false;
    if (!targetClient || !messageContent) return false;

    const payloadObject = { TargetClient: targetClient, Message: messageContent };
    const jsonBytes = textEncoderRemoteMessaging.encode(JSON.stringify(payloadObject));
    const fullPayload = new Uint8Array(1 + jsonBytes.length);
    fullPayload[0] = 0x02;
    fullPayload.set(jsonBytes, 1);

    serverWsRemoteMessaging.send(fullPayload.buffer);
    return true;
}

// WebSocket Connection - Global Browser Inbound
function connectGlobalBrowserWebSocket() {
    if (browserWsRemoteMessaging && (browserWsRemoteMessaging.readyState === WebSocket.OPEN || browserWsRemoteMessaging.readyState === WebSocket.CONNECTING)) {
        return;
    }

    const endpoint = 'ws://' + window.location.hostname + ':2050/ws/browser';
    browserWsRemoteMessaging = new WebSocket(endpoint);
    browserWsRemoteMessaging.binaryType = 'arraybuffer';

    browserWsRemoteMessaging.onmessage = function (event) {
        if (!(event.data instanceof ArrayBuffer)) return;
        const bytes = new Uint8Array(event.data);
        const type = bytes[0];

        if (type === 0x12) {
            try {
                const data = JSON.parse(textDecoderRemoteMessaging.decode(bytes.subarray(1)));

                const clientName = data.ClientName || data.clientName || data.TargetClient || data.targetClient;
                const chatMessage = data.ChatMessage || data.chatMessage || data.Message || data.message;

                if (clientName && typeof chatMessage === 'string' && chatMessage.trim() !== '') {
                    // Fetch notifications and request sound playback if count > 0
                    setTimeout(() => {
                        fetchAndRenderNotifications(true);
                    }, 300);
                }
            } catch (e) {
                console.error("Error parsing real-time message notification", e);
            }
        }
    };

    browserWsRemoteMessaging.onclose = function () {
        setTimeout(connectGlobalBrowserWebSocket, 3000);
    };
}

function playNotificationSound() {
    try {
        const audioCtx = new (window.AudioContext || window.webkitAudioContext)();
        const osc = audioCtx.createOscillator();
        const gain = audioCtx.createGain();
        osc.connect(gain);
        gain.connect(audioCtx.destination);
        osc.frequency.value = 800;
        gain.gain.value = 0.1;
        osc.start();
        osc.stop(audioCtx.currentTime + 0.15);
    } catch (e) { }
}

function toggleNotifDropdown(show) {
    const dropdown = document.getElementById('notifDropdown');
    if (!dropdown) return;

    if (show === undefined) {
        dropdown.classList.toggle('show');
    } else if (show) {
        dropdown.classList.add('show');
    } else {
        dropdown.classList.remove('show');
    }
}

function escapeHtml(str) {
    return str ? String(str).replace(/[&<>"']/g, s => ({ '&': '&amp;', '<': '&lt;', '>': '&gt;', '"': '&quot;', "'": '&#39;' }[s])) : '';
}

// Bind Bell Button Events
function bindNotificationEvents() {
    const bellBtn = document.getElementById('notifBellBtn');
    if (bellBtn && !bellBtn.dataset.bound) {
        bellBtn.dataset.bound = 'true';
        bellBtn.addEventListener('click', (e) => {
            e.stopPropagation();
            toggleNotifDropdown();
        });
    }

    const clearBtn = document.getElementById('clearNotifsBtn');
    if (clearBtn && !clearBtn.dataset.bound) {
        clearBtn.dataset.bound = 'true';
        clearBtn.addEventListener('click', (e) => {
            e.stopPropagation();
            clearAllChatNotifications();
        });
    }
}

// Global Document Listeners
document.addEventListener('click', (e) => {
    const wrapper = document.getElementById('notificationWrapper');
    if (wrapper && !wrapper.contains(e.target)) {
        toggleNotifDropdown(false);
    }
});

// Initialization Logic
function initRemoteMessaging() {
    connectServerWebSocket();
    connectGlobalBrowserWebSocket();
    bindNotificationEvents();

    fetchAndRenderNotifications();
}

if (document.readyState === 'loading') {
    document.addEventListener('DOMContentLoaded', initRemoteMessaging);
} else {
    initRemoteMessaging();
}

// Handle ASP.NET AJAX UpdatePanel postbacks across forms
if (typeof Sys !== 'undefined' && Sys.WebForms && Sys.WebForms.PageRequestManager) {
    Sys.WebForms.PageRequestManager.getInstance().add_endRequest(() => {
        bindNotificationEvents();
    });
}