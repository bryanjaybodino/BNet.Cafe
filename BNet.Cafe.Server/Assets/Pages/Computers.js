(function () {


    let serverTimeOffset = 0;

    // Fetch server time to calculate offset on load
    function syncServerTime() {


        var endpoint = /\.aspx$/i.test(window.location.pathname)
            ? window.location.pathname.replace(/[^\/]+\.aspx$/i, 'Ashx/GetTimeServiceHandler.ashx')
            : window.location.pathname.replace(/[^\/]+$/i, 'Ashx/GetTimeServiceHandler.ashx');


        fetch(endpoint)
            .then(response => {
                if (!response.ok) throw new Error('Network response was not ok');
                return response.json();
            })
            .then(data => {
                const serverTime = new Date(data.serverTime);
                const localTime = new Date();
                serverTimeOffset = serverTime - localTime;
            })
            .catch(err => console.warn('Failed to sync server time, falling back to local clock:', err));
    }

    syncServerTime();


    // Live Running Time Counter using synchronized server time
    function updateRunningTimers() {
        const timerElements = document.querySelectorAll('.running-timer[data-start-time]');

        timerElements.forEach(el => {
            const rawStart = el.getAttribute('data-start-time');
            if (!rawStart) return;

            const startTime = new Date(rawStart);
            if (isNaN(startTime.getTime())) return;

            const synchronizedNow = new Date(new Date().getTime() + serverTimeOffset);
            let diffMs = synchronizedNow - startTime;
            if (diffMs < 0) diffMs = 0;

            const totalSeconds = Math.floor(diffMs / 1000);
            const hrs = String(Math.floor(totalSeconds / 3600)).padStart(2, '0');
            const mins = String(Math.floor((totalSeconds % 3600) / 60)).padStart(2, '0');
            const secs = String(totalSeconds % 60).padStart(2, '0');

            el.textContent = `${hrs}:${mins}:${secs}`;
        });
    }

    setInterval(updateRunningTimers, 1000);

    // 1. Wait until the window load completes completely
    window.addEventListener('load', function () {
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
    });
})();