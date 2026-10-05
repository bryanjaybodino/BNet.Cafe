<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="ClientChat.aspx.cs" Inherits="BNet.Cafe.Server.ClientChat" %>

<!DOCTYPE html>
<html lang="en">
<head runat="server">
    <meta charset="UTF-8">
    <meta name="viewport" content="width=device-width, initial-scale=1.0">
    <title>Client Chat Screen</title>
    <% Response.Write(BNet.Cafe.Server.Services.FileCssHelper.StyleSheetVersion("Assets/bundle.min.css")); %>
    <style>
        body, html {
            height: 100%;
            margin: 0;
            padding: 0;
            overflow: hidden;
            background-color: var(--bg-light);
        }

        .full-chat-wrapper {
            display: flex;
            flex-direction: column;
            height: 100vh;
            width: 100vw;
        }

        .full-chat-header {
            padding: 20px;
            background: var(--bg-light-secondary);
            border-bottom: 1px solid var(--border-light);
            display: flex;
            align-items: center;
            justify-content: space-between;
        }

        .full-chat-logs {
            flex: 1;
            padding: 24px;
            overflow-y: auto;
            display: flex;
            flex-direction: column;
            gap: 12px;
            background: var(--bg-light);
        }

        .chat-bubble {
            max-width: 60%;
            padding: 12px 18px;
            border-radius: 12px;
            font-size: 15px;
            line-height: 1.4;
            word-wrap: break-word;
        }

        .chat-bubble.incoming {
            background: var(--bg-light-secondary);
            color: var(--text-light);
            align-self: flex-start;
            border: 1px solid var(--border-light);
        }

        .chat-bubble.outgoing {
            background: var(--primary);
            color: #ffffff;
            align-self: flex-end;
        }

        .full-chat-footer {
            padding: 20px;
            background: var(--bg-light-secondary);
            border-top: 1px solid var(--border-light);
            display: flex;
            gap: 12px;
        }

        .full-chat-footer input {
            flex: 1;
        }
    </style>
</head>
<body>
    <form id="form1" runat="server">
        <asp:ScriptManager ID="ScriptManager1" runat="server"></asp:ScriptManager>
        <asp:UpdatePanel ID="UpdatePanel1" runat="server">
            <ContentTemplate>
                <!-- Hidden inputs to pass data to C# postback -->
                <asp:TextBox ID="TextBox_ChatMessage" runat="server" Style="display:none;"></asp:TextBox>
                <asp:TextBox ID="TextBox_ComputerName" runat="server" Style="display:none;"></asp:TextBox>
                <asp:TextBox ID="TextBox_UserId" runat="server" Style="display:none;"></asp:TextBox>

                <!-- Hidden LinkButton to trigger C# server handler -->
                <asp:LinkButton ID="LinkButton_SaveMessage" runat="server" OnClick="LinkButton_SaveMessage_Click" Style="display:none;"></asp:LinkButton>

                <div class="full-chat-wrapper">
                    <div class="full-chat-header">
                        <h2 id="clientTitle">Client Chat</h2>
                        <span id="connectionStatus" style="color: #10b981;">Connecting...</span>
                    </div>

                    <div id="chatLogs" class="full-chat-logs"></div>

                    <div class="full-chat-footer">
                        <input type="text" id="messageInput" class="form-control" placeholder="Type a message..." autofocus>
                        <button type="button" id="sendBtn" class="btn btn-primary">Send</button>
                    </div>
                </div>
            </ContentTemplate>
        </asp:UpdatePanel>
    </form>

    <script>
        const urlParams = new URLSearchParams(window.location.search);
        const clientName = (urlParams.get('client')).toUpperCase();
        const userId = urlParams.get('userId') || '';

        document.getElementById('clientTitle').textContent = `${clientName}`;

        const encoder = new TextEncoder();
        const decoder = new TextDecoder('utf-8');
        let agentWs = null;

        function getHandlerEndpoint(handlerName) {
            return /\.aspx$/i.test(window.location.pathname)
                ? window.location.pathname.replace(/[^\/]+\.aspx$/i, 'Ashx/' + handlerName)
                : window.location.pathname.replace(/[^\/]+$/i, 'Ashx/' + handlerName);
        }

        // Load non-deleted chat history for this computer
        function loadChatHistory() {
            const endpoint = getHandlerEndpoint('GetChatMessagesHandler.ashx') + '?isDeleted=false';

            fetch(endpoint)
                .then(res => res.json())
                .then(res => {
                    if (res && res.success && Array.isArray(res.data)) {
                        const logs = document.getElementById('chatLogs');
                        logs.innerHTML = ''; // clear current logs

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
                document.getElementById('connectionStatus').textContent = 'Connected';
                document.getElementById('connectionStatus').style.color = '#10b981';

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
                document.getElementById('connectionStatus').textContent = 'Disconnected (Retrying)';
                document.getElementById('connectionStatus').style.color = '#ef4444';
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

        function handleSend() {
            const input = document.getElementById('messageInput');
            const message = input.value.trim();
            if (!message) return;

            // 1. Send via WebSocket
            sendPayload(0x03, {
                ClientName: clientName,
                ChatMessage: message,
                windowTitle: "Chat Client"
            });

            // 2. Set hidden ASP.NET field values and trigger LinkButton PostBack
            const txtMsg = document.getElementById('<%= TextBox_ChatMessage.ClientID %>');
            const txtComputer = document.getElementById('<%= TextBox_ComputerName.ClientID %>');
            const txtUser = document.getElementById('<%= TextBox_UserId.ClientID %>');
            const saveBtn = document.getElementById('<%= LinkButton_SaveMessage.ClientID %>');
            if (txtMsg && txtComputer && txtUser && saveBtn) {
                txtMsg.value = message;
                txtComputer.value = clientName;
                txtUser.value = userId;

                // Trigger the ASP.NET LinkButton click event
                saveBtn.click();
            }

            appendBubble(message, 'outgoing');
            input.value = '';
        }

        document.getElementById('sendBtn').onclick = handleSend;
        document.getElementById('messageInput').addEventListener('keypress', (e) => {
            if (e.key === 'Enter') handleSend();
        });

        // Initialize WebSocket and fetch existing history
        connectAgent();
        loadChatHistory();
    </script>
</body>
</html>