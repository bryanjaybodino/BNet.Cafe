<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="Clients.ascx.cs" Inherits="BNet.Cafe.Server.Forms.Clients" %>
<!DOCTYPE html>
<html lang="en">
<head>
    <meta charset="UTF-8">
    <meta name="viewport" content="width=device-width, initial-scale=1.0">
    <title>BNet Cafe - Client Monitoring</title>
    <style>
        :root {
            --bg-color: #0f172a;
            --card-bg: #1e293b;
            --accent-color: #3b82f6;
            --text-main: #f8fafc;
            --text-muted: #94a3b8;
            --online-color: #22c55e;
        }

        body {
            margin: 0;
            padding: 20px;
            font-family: 'Segoe UI', Tahoma, Geneva, Verdana, sans-serif;
            background-color: var(--bg-color);
            color: var(--text-main);
        }

        header {
            display: flex;
            justify-content: space-between;
            align-items: center;
            margin-bottom: 25px;
            padding-bottom: 15px;
            border-bottom: 1px solid #334155;
        }

        h1 { margin: 0; font-size: 1.5rem; }

        .status-badge {
            font-size: 0.85rem;
            padding: 4px 12px;
            border-radius: 999px;
            background: #334155;
        }
        .status-badge.connected { background: #166534; color: #4ade80; }

        /* Grid Layout for PC Cards */
        .pc-grid {
            display: grid;
            grid-template-columns: repeat(auto-fill, minmax(280px, 1fr));
            gap: 20px;
        }

        /* PC Card Styling */
        .pc-card {
            background: var(--card-bg);
            border-radius: 12px;
            padding: 16px;
            border: 1px solid #334155;
            display: flex;
            flex-direction: column;
            gap: 12px;
            box-shadow: 0 4px 6px -1px rgba(0,0,0,0.3);
            transition: transform 0.2s, border-color 0.2s;
        }

        .pc-card:hover {
            transform: translateY(-2px);
            border-color: var(--accent-color);
        }

        .pc-header {
            display: flex;
            justify-content: space-between;
            align-items: center;
        }

        .pc-title {
            font-size: 1.1rem;
            font-weight: 600;
        }

        .indicator {
            width: 10px;
            height: 10px;
            border-radius: 50%;
            background-color: var(--online-color);
            box-shadow: 0 0 8px var(--online-color);
        }

        .pc-details {
            font-size: 0.85rem;
            color: var(--text-muted);
            display: flex;
            flex-direction: column;
            gap: 4px;
        }

        .btn-view {
            margin-top: auto;
            padding: 8px 16px;
            background-color: var(--accent-color);
            color: white;
            border: none;
            border-radius: 6px;
            cursor: pointer;
            font-weight: 600;
            transition: background 0.2s;
        }

        .btn-view:hover {
            background-color: #2563eb;
        }

        /* Modal Viewer for Remote Desktop */
        .modal-overlay {
            display: none;
            position: fixed;
            top: 0; left: 0; width: 100%; height: 100%;
            background: rgba(0, 0, 0, 0.85);
            z-index: 1000;
            justify-content: center;
            align-items: center;
            flex-direction: column;
        }

        .modal-overlay.active { display: flex; }

        .modal-header {
            width: 90%;
            max-width: 1280px;
            display: flex;
            justify-content: space-between;
            align-items: center;
            margin-bottom: 10px;
        }

        .btn-close {
            background: #ef4444;
            color: white;
            border: none;
            padding: 6px 16px;
            border-radius: 6px;
            cursor: pointer;
            font-weight: bold;
        }

        .screen-container {
            position: relative;
            max-width: 90%;
            max-height: 80vh;
            border: 2px solid #334155;
            background: #000;
        }

        .screen-container img {
            display: block;
            max-width: 100%;
            max-height: 80vh;
            object-fit: contain;
        }
    </style>
</head>
<body>

    <header>
        <h1>BNet Cafe Dashboard</h1>
        <div id="connectionStatus" class="status-badge">Connecting to Server...</div>
    </header>

    <div id="pcGrid" class="pc-grid">
        <!-- Cards will be dynamically created here -->
    </div>

    <!-- Viewer Modal -->
    <div id="viewerModal" class="modal-overlay">
        <div class="modal-header">
            <h2 id="activePcTitle">Viewing Remote Screen</h2>
            <button class="btn-close" onclick="closeViewer()">Close Stream</button>
        </div>
        <div class="screen-container">
            <img id="remoteDisplay" src="" alt="Live Remote Desktop Feed" />
        </div>
    </div>

    <script>
        const WS_URL = "ws://" + window.location.hostname + ":2050";
        let socket = null;
        let activeAgent = null;

        const pcGrid = document.getElementById('pcGrid');
        const connectionStatus = document.getElementById('connectionStatus');
        const viewerModal = document.getElementById('viewerModal');
        const remoteDisplay = document.getElementById('remoteDisplay');
        const activePcTitle = document.getElementById('activePcTitle');

        function connectWebSocket() {
            socket = new WebSocket(WS_URL);
            socket.binaryType = "arraybuffer";

            socket.onopen = () => {
                connectionStatus.textContent = "Connected";
                connectionStatus.classList.add("connected");
            };

            socket.onclose = () => {
                connectionStatus.textContent = "Disconnected - Reconnecting...";
                connectionStatus.classList.remove("connected");
                setTimeout(connectWebSocket, 3000);
            };

            socket.onmessage = (event) => {
                if (event.data instanceof ArrayBuffer) {
                    handleBinaryMessage(new Uint8Array(event.data));
                }
            };
        }

        function handleBinaryMessage(bytes) {
            const msgType = bytes[0];

            // 0x10: JPEG Frame Payload
            if (msgType === 0x10 && activeAgent) {
                const imageBytes = bytes.subarray(1);
                const blob = new Blob([imageBytes], { type: 'image/jpeg' });
                remoteDisplay.src = URL.createObjectURL(blob);
            }
            // 0x11: Client List Updated (JSON)
            else if (msgType === 0x11) {
                const jsonText = new TextDecoder().decode(bytes.subarray(1));
                const clients = JSON.parse(jsonText);
                renderPCCards(clients);
            }
        }

        function renderPCCards(clients) {
            pcGrid.innerHTML = '';

            if (!clients || clients.length === 0) {
                pcGrid.innerHTML = '<p style="color: var(--text-muted)">No active PCs connected.</p>';
                return;
            }

            clients.forEach((client, index) => {
                const pcName = client.MachineName || `PC ${index + 1}`;
                const accountName = client.AccountName || pcName;

                const card = document.createElement('div');
                card.className = 'pc-card';
                card.innerHTML = `
                    <div class="pc-header">
                        <span class="pc-title">${pcName}</span>
                        <span class="indicator"></span>
                    </div>
                    <div class="pc-details">
                        <span><strong>Account:</strong> ${accountName}</span>
                        <span><strong>OS:</strong> ${client.Windows || 'Windows'}</span>
                        <span><strong>Monitors:</strong> ${client.ScreenCount || 1}</span>
                    </div>
                    <button class="btn-view" onclick="openViewer('${accountName}', '${pcName}')">View Screen</button>
                `;
                pcGrid.appendChild(card);
            });
        }

        function openViewer(accountName, pcName) {
            activeAgent = accountName;
            activePcTitle.textContent = `Viewing - ${pcName}`;
            viewerModal.classList.add('active');

            // Send SUBSCRIBE payload (0x21 + room key)
            const roomKey = `screen_1_${accountName.toLowerCase()}`;
            const encoder = new TextEncoder();
            const keyBytes = encoder.encode(roomKey);
            
            const payload = new Uint8Array(1 + keyBytes.length);
            payload[0] = 0x21; // SUBSCRIBE Opcode
            payload.set(keyBytes, 1);

            if (socket && socket.readyState === WebSocket.OPEN) {
                socket.send(payload.buffer);
            }
        }

        function closeViewer() {
            viewerModal.classList.remove('active');
            remoteDisplay.src = '';
            activeAgent = null;
        }

        // Mouse Input Listener for Remote Control
        remoteDisplay.addEventListener('mousedown', (e) => sendInput(e, 'mousedown'));
        remoteDisplay.addEventListener('mouseup', (e) => sendInput(e, 'mouseup'));
        remoteDisplay.addEventListener('mousemove', (e) => {
            if (e.buttons > 0) sendInput(e, 'mousemove');
        });

        function sendInput(e, type) {
            if (!activeAgent || socket.readyState !== WebSocket.OPEN) return;

            const rect = remoteDisplay.getBoundingClientRect();
            const inputData = {
                TargetAgent: activeAgent,
                type: type,
                x: Math.round(e.clientX - rect.left),
                y: Math.round(e.clientY - rect.top),
                button: e.button,
                frameW: Math.round(rect.width),
                frameH: Math.round(rect.height)
            };

            const jsonBytes = new TextEncoder().encode(JSON.stringify(inputData));
            const payload = new Uint8Array(1 + jsonBytes.length);
            payload[0] = 0x20; // INPUT Opcode
            payload.set(jsonBytes, 1);

            socket.send(payload.buffer);
        }

        // Initialize connection
        connectWebSocket();
    </script>
</body>
</html>