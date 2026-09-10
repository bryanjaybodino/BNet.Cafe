(function () {
    var endpoint = window.location.protocol + '//' + window.location.hostname + ':2050/sse';
    const evtSource = new EventSource(endpoint);
    evtSource.onmessage = function (event) {
        var LinkButton_Refresh = document.querySelector('[id$="LinkButton_Refresh"]');
        LinkButton_Refresh.click();
    };
})();