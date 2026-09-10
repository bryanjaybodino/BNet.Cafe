(function () {
    // 1. Wait until the window load completes completely
    window.addEventListener('load', function () {

        // 2. Delay execution by exactly 5 seconds after load
        setTimeout(function () {
            const endpoint = window.location.protocol + '//' + window.location.hostname + ':2050/sse';
            let evtSource = null;

            // 3. Set a connection timeout (5 seconds after initiating connection)
            const connectionTimeout = setTimeout(function () {
                if (evtSource && evtSource.readyState === EventSource.CONNECTING) {
                    console.warn('SSE connection timed out. Closing source to prevent page lag.');
                    evtSource.close();
                }
            }, 5000);

            try {
                evtSource = new EventSource(endpoint);

                evtSource.onopen = function () {
                    // Clear connection timeout once successfully connected
                    clearTimeout(connectionTimeout);
                };

                evtSource.onmessage = function (event) {
                    const LinkButton_Refresh = document.querySelector('[id$="LinkButton_Refresh"]');
                    if (LinkButton_Refresh) {
                        LinkButton_Refresh.click();
                    }
                };

                evtSource.onerror = function (err) {
                    clearTimeout(connectionTimeout);
                    // Close immediately on error to stop infinite reconnection attempts
                    console.error('SSE Error/Down. Aborting connection:', err);
                    evtSource.close();
                };
            } catch (e) {
                clearTimeout(connectionTimeout);
                console.error('Failed to initialize SSE:', e);
            }
        }, 5000); // <-- 5-second initial delay before connecting
    });
})();