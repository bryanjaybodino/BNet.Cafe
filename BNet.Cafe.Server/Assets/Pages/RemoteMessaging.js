'use strict';

/**
 * Server Messaging Client
 * Handles dedicated text messaging outbound to target PCs via /ws/server
 */
const textEncoder = new TextEncoder();
let serverWs = null;

/**
 * Initialize and manage the connection to /ws/server
 */
function connectServerWebSocket() {

    var endpoint = 'ws://' + window.location.hostname + ':2050/ws/server';
    serverWs = new WebSocket(endpoint);
    serverWs.binaryType = 'arraybuffer';

    serverWs.onopen = function () {
        console.log('Connected to /ws/server WebSocket');
    };

    serverWs.onclose = function () {
        console.warn('Disconnected from /ws/server. Retrying in 3 seconds...');
        setTimeout(connectServerWebSocket, 3000);
    };

    serverWs.onerror = function (err) {
        console.error('Server WebSocket error:', err);
    };
}

/**
 * Send a text message to a specific client PC
 * @param {string} targetClient - The ClientName/Machine identifier (e.g. "pc1")
 * @param {string} messageContent - The message string to send
 */
function sendTextMessageToPC(targetClient, messageContent) {
    if (!serverWs || serverWs.readyState !== WebSocket.OPEN) {
        console.error('Cannot send message: /ws/server WebSocket is not connected.');
        return false;
    }

    if (!targetClient || !messageContent) {
        console.error('Target client and message content are required.');
        return false;
    }

    // Build payload matching C# ServerTextMessage class
    const payloadObject = {
        TargetClient: targetClient,
        Message: messageContent
    };

    const jsonString = JSON.stringify(payloadObject);
    const jsonBytes = textEncoder.encode(jsonString);

    // Frame layout: [0x02][UTF-8 JSON Payload]
    const fullPayload = new Uint8Array(1 + jsonBytes.length);
    fullPayload[0] = 0x02; // Message Type 0x02 for text
    fullPayload.set(jsonBytes, 1);

    serverWs.send(fullPayload.buffer);
    console.log(`Message successfully dispatched to target: ${targetClient}`);
    return true;
}

// Auto-connect on script load
connectServerWebSocket();