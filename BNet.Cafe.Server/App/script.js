
const ua = navigator.userAgent.toLowerCase();
const isSafari =
    ua.includes("safari") &&
    !ua.includes("chrome") &&
    !ua.includes("android");

// Real PWA support check
const supportsPWA = "BeforeInstallPromptEvent" in window;

// DOM elements
const installBtn = document.getElementById("installBtn");
const messageEl = document.getElementById("message");

// Hide button initially
installBtn.classList.add("d-none");


// ================================
//  SAFARI FALLBACK (installation instructions only)
// ================================
if (isSafari) {
    installBtn.classList.remove("d-none");
    installBtn.innerText = "How to Install";
    messageEl.innerText = "To install: Tap Share → Add to Home Screen.";

    installBtn.addEventListener("click", () => {
        alert(
            "Safari Installation Steps:\n\n" +
            "1. Tap the Share button (square with arrow)\n" +
            "2. Choose 'Add to Home Screen'\n" +
            "3. Confirm installation"
        );
    });
}


// ================================
//  UNSUPPORTED BROWSER HANDLING
// ================================
else if (!supportsPWA) {
    installBtn.classList.remove("d-none");
    installBtn.innerText = "Your device is not supported";
    installBtn.disabled = true;

    messageEl.innerText =
        "This browser does not support app installation.";
}


// ================================
//  FULL PWA INSTALL SUPPORT (Chrome / Edge / Brave / Opera)
// ================================
else {
    let deferredPrompt;
    let beforeInstallPromptFired = false;
    // When browser detects app can be installed
    window.addEventListener("beforeinstallprompt", (e) => {
        e.preventDefault();
        deferredPrompt = e;
        installBtn.classList.remove("d-none");
        messageEl.innerText = "Your app is ready for installation.";
        beforeInstallPromptFired = true; // mark that the event fired
    });

    // User clicks Install button
    installBtn.addEventListener("click", async () => { 
        if (!deferredPrompt) return; 

        // Show loading state immediately on click
        installBtn.disabled = true;
        installBtn.innerHTML = '<span class="btn-spinner"></span> Installing...';
        messageEl.innerText = "Installing application...";

        try {
            deferredPrompt.prompt(); 
            const { outcome } = await deferredPrompt.userChoice; 

            if (outcome === "accepted") { 
                messageEl.innerText = "Installing... Please wait.";
            } else {
                // User rejected/cancelled the prompt — restore button
                installBtn.disabled = false;
                installBtn.innerHTML = originalBtnContent;
                messageEl.innerText = "Installation cancelled.";
            }
        } catch (err) {
            console.error("Installation error:", err);
            installBtn.disabled = false;
            installBtn.innerHTML = originalBtnContent;
        } finally {
            deferredPrompt = null; 
        }
    });


    function checkIfInstalled() {
        if (window.matchMedia('(display-mode: standalone)').matches || window.navigator.standalone === true) {
            installBtn.innerText = "Installed";
            installBtn.disabled = true;
            messageEl.innerText = "Welcome back! The app is already installed.";
            return true;
        }
        return false;
    }


    // Installed event
    window.addEventListener("appinstalled", () => {

        //Visibily Check if the app is already on the mobile app windows
        document.addEventListener("visibilitychange", function onVisibilityChange() {
            if (document.visibilityState === "hidden") {
                // User switched away from the page
                window.location.href = "../login.aspx";
                document.removeEventListener("visibilitychange", onVisibilityChange);
            }
        });
    });



    // Check after a delay if the event did not fire
    setTimeout(() => {
        if (!beforeInstallPromptFired) {
            installBtn.classList.remove("d-none");
            installBtn.innerText = "Not Applicable";
            installBtn.disabled = true;
            messageEl.innerText = "Installation not available right now.";
        }
    }, 3000); // wait 3 seconds after page load
}




// ================================
//  Service worker registration
// ================================
if ("serviceWorker" in navigator) {
    navigator.serviceWorker
        .register("service-worker.js")
        //.then(() => console.log("Service worker registered"))
        .catch((err) =>
            console.error("Service worker registration failed:", err)
        );
}
