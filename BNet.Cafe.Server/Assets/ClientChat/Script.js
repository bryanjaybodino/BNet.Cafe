const urlParams = new URLSearchParams(window.location.search);
const clientName = (urlParams.get('ClientName') || '').toUpperCase();
const userId = urlParams.get('userId') || '';

document.getElementById('clientTitle').textContent = clientName || 'Client Chat';

const encoder = new TextEncoder();
const decoder = new TextDecoder('utf-8');
let agentWs = null;
let cooldownTimer = null;
const LOCK_DURATION = 3; // Lock time in seconds

function getHandlerEndpoint(handlerName) {
    return /\.aspx$/i.test(window.location.pathname)
        ? window.location.pathname.replace(/[^\/]+\.aspx$/i, 'Ashx/' + handlerName)
        : window.location.pathname.replace(/[^\/]+$/i, 'Ashx/' + handlerName);
}

function setStatus(text, isConnected) {
    const statusText = document.getElementById('connectionStatus');
    const statusDot = document.getElementById('statusDot');

    statusText.textContent = text;
    if (isConnected) {
        statusDot.style.backgroundColor = 'var(--accent-pickleball)';
        statusDot.style.boxShadow = '0 0 6px var(--accent-pickleball)';
        statusText.style.color = 'var(--text-light)';
    } else {
        statusDot.style.backgroundColor = 'var(--accent-pingpong)';
        statusDot.style.boxShadow = '0 0 6px var(--accent-pingpong)';
        statusText.style.color = 'var(--text-light-secondary)';
    }
}

// Load non-deleted chat history for this computer
// Load non-deleted chat history for this computer
function loadChatHistory() {
    const endpoint = getHandlerEndpoint('GetChatMessagesHandler.ashx') + '?isDeleted=false';

    fetch(endpoint)
        .then(res => res.json())
        .then(res => {
            if (res && res.success && Array.isArray(res.data)) {
                const logs = document.getElementById('chatLogs');
                logs.innerHTML = ''; // clear current logs

                // Sort items by numeric ID ascending
                res.data.sort((a, b) => Number(a.id) - Number(b.id));

                // Filter messages meant for this specific client or show all active messages
                const filtered = res.data.filter(item =>
                    !item.computerName || item.computerName.toUpperCase() === clientName
                );

                filtered.forEach(item => {
                    // Determine whether message was sent by client (outgoing) or received (incoming)
                    const isOutgoing = item.computerName && item.computerName.toUpperCase() === clientName;
                    appendBubble(item.message, isOutgoing ? 'outgoing' : 'incoming');
                });
            }
        })
        .catch(err => console.error('Error fetching chat history:', err));
}
function connectAgent() {
    agentWs = new WebSocket(`ws://${window.location.hostname}:2050/ws/agent`);
    agentWs.binaryType = 'arraybuffer';

    agentWs.onopen = () => {
        setStatus('Connected', true);

        sendPayload(0x03, {
            ClientName: clientName,
            windowTitle: "Chat Client Online"
        });
    };

    agentWs.onmessage = (event) => {
        if (!(event.data instanceof ArrayBuffer)) return;
        const bytes = new Uint8Array(event.data);
        const type = bytes[0];

        if (type === 0x02) {
            const messageText = decoder.decode(bytes.subarray(1));
            appendBubble(messageText, 'incoming');
        }
    };

    agentWs.onclose = () => {
        setStatus('Disconnected (Retrying)', false);
        setTimeout(connectAgent, 3000);
    };
}

function sendPayload(frameType, obj) {
    if (!agentWs || agentWs.readyState !== WebSocket.OPEN) return;
    const jsonBytes = encoder.encode(JSON.stringify(obj));
    const payload = new Uint8Array(1 + jsonBytes.length);
    payload[0] = frameType;
    payload.set(jsonBytes, 1);
    agentWs.send(payload.buffer);
}

function appendBubble(text, type) {
    const logs = document.getElementById('chatLogs');
    const bubble = document.createElement('div');
    bubble.className = `chat-bubble ${type}`;
    bubble.textContent = text;
    logs.appendChild(bubble);
    logs.scrollTop = logs.scrollHeight;
}

// Anti-Spam Lock Logic
function checkAntiSpamLock() {
    const lastSentTime = localStorage.getItem('chat_last_sent_timestamp');
    if (!lastSentTime) return;

    const timePassed = Math.floor((Date.now() - parseInt(lastSentTime, 10)) / 1000);
    const remaining = LOCK_DURATION - timePassed;

    if (remaining > 0) {
        startCooldown(remaining);
    }
}

function startCooldown(seconds) {
    const input = document.getElementById('messageInput');
    const sendBtn = document.querySelector('[id$="LinkButton_SaveMessage"]');

    if (!input) return;

    // Set readOnly instead of disabled so the input value remains clearly visible
    input.readOnly = true;
    if (sendBtn) sendBtn.classList.add('disabled');

    if (cooldownTimer) clearInterval(cooldownTimer);

    let currentRemaining = seconds;

    // Set value and placeholder so the countdown is visible in all browsers
    input.value = `Please wait ${currentRemaining}s before sending again...`;

    cooldownTimer = setInterval(() => {
        currentRemaining--;

        if (currentRemaining <= 0) {
            clearInterval(cooldownTimer);
            input.readOnly = false;
            input.value = ''; // Clear countdown text
            input.placeholder = "Type a message...";
            if (sendBtn) sendBtn.classList.remove('disabled');
            input.focus();
        } else {
            input.value = `Please wait ${currentRemaining}s before sending again...`;
        }
    }, 1000);
}

function handleSend(e) {
    if (e) e.preventDefault();

    const input = document.getElementById('messageInput');
    const sendBtn = document.querySelector('[id$="LinkButton_SaveMessage"]');

    if (!input || input.readOnly || input.disabled || (sendBtn && sendBtn.classList.contains('disabled'))) {
        return;
    }

    const message = input.value.trim();
    if (!message) return;

    localStorage.setItem('chat_last_sent_timestamp', Date.now().toString());

    // 1. Send via WebSocket
    sendPayload(0x03, {
        ClientName: clientName,
        ChatMessage: message,
        windowTitle: "Chat Client"
    });

    // 2. Save to Database asynchronously via ASHX Handler
    const saveEndpoint = getHandlerEndpoint('SaveChatMessageHandler.ashx');
    fetch(saveEndpoint, {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify({
            message: message,
            computerName: clientName,
            userId: userId
        })
    })
        .then(res => res.json())
        .then(data => {
            if (!data.success) {
                console.error("Failed to persist message:", data.error);
            }
        })
        .catch(err => console.error("Error saving message:", err));

    appendBubble(message, 'outgoing');

    // 3. Start Cooldown
    startCooldown(LOCK_DURATION);
}

// Attach listener cleanly
const saveBtnEl = document.querySelector('[id$="LinkButton_SaveMessage"]');
if (saveBtnEl) {
    saveBtnEl.onclick = handleSend;
}

document.getElementById('messageInput').addEventListener('keypress', (e) => {
    if (e.key === 'Enter') {
        e.preventDefault();
        handleSend(e);
    }
});
// Initialize WebSocket, fetch existing history, and verify active lock state
connectAgent();
loadChatHistory();
checkAntiSpamLock();

// Disable right-click context menu
document.addEventListener('contextmenu', function (e) {
    e.preventDefault();
});

// Disable keyboard shortcuts for Inspect Element / DevTools
document.addEventListener('keydown', function (e) {
    // Prevent F12
    if (e.key === 'F12') {
        e.preventDefault();
    }
    // Prevent Ctrl+Shift+I, Ctrl+Shift+J, Ctrl+Shift+C
    if (e.ctrlKey && e.shiftKey && (e.key === 'I' || e.key === 'i' || e.key === 'J' || e.key === 'j' || e.key === 'C' || e.key === 'c')) {
        e.preventDefault();
    }
    // Prevent Ctrl+U (View Source)
    if (e.ctrlKey && (e.key === 'U' || e.key === 'u')) {
        e.preventDefault();
    }
});