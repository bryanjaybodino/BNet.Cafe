// =============================================================================
// BNetPageLoader - Global Page Loading Overlay
// =============================================================================
class BNetPageLoader {
    static getOverlay() {
        let loader = document.getElementById('bnet-page-loading-overlay');
        if (!loader) {
            loader = document.createElement('div');
            loader.id = 'bnet-page-loading-overlay';
            loader.className = 'bnet-page-loader-overlay';
            loader.innerHTML = '<div class="bnet-page-loader-spinner"></div>';
            document.body.appendChild(loader);
        }
        return loader;
    }

    static show() {
        const loader = BNetPageLoader.getOverlay();
        if (loader) loader.classList.add('active');
    }

    static hide() {
        const loader = document.getElementById('bnet-page-loading-overlay');
        if (loader) loader.classList.remove('active');
    }

    static navigateTo(url) {
        BNetPageLoader.show();
        window.location.href = url;
    }
}

// Global page unload listener
window.addEventListener('beforeunload', () => {
    BNetPageLoader.show();
});

// Legacy global function bindings
function navigateTo(url) {
    BNetPageLoader.navigateTo(url);
}

function showPageLoading() {
    BNetPageLoader.show();
}

function hidePageLoading() {
    BNetPageLoader.hide();
}