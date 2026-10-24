// --- 1. PROTECTIONS ---
document.addEventListener('contextmenu', e => e.preventDefault());
document.addEventListener('dragstart', e => e.preventDefault());

// --- IDLE TIMER FOR ATTRACT SCREEN ---
let idleTimer = null;
let lockCountdownInterval = null;
let lockRemainingSeconds = 60;
const IDLE_TIMEOUT_MS = 10000; // 16 seconds of inactivity before countdown starts

function resetIdleTimer() {
    if (idleTimer) {
        clearTimeout(idleTimer);
        idleTimer = null;
    }
    if (lockCountdownInterval) {
        clearInterval(lockCountdownInterval);
        lockCountdownInterval = null;
    }

    // Hide the idle countdown footer while active/interacting
    const idleFooter = document.getElementById('idleCountdownFooter');
    if (idleFooter) {
        idleFooter.style.display = 'none';
    }

    const psp = document.getElementById('pspContainer');
    const isMainMenuViewable = psp && psp.style.display !== 'none';

    if (isMainMenuViewable && !isModalOpen()) {
        idleTimer = setTimeout(() => {
            startLockCountdown();
        }, IDLE_TIMEOUT_MS);
    }
}

function startLockCountdown() {
    const idleFooter = document.getElementById('idleCountdownFooter');
    const bannerIcon = document.getElementById('bannerIcon');
    const bannerText = document.getElementById('bannerText');

    lockRemainingSeconds = 60; // Countdown duration before locking
    if (bannerIcon) bannerIcon.className = 'fas fa-lock';
    if (bannerText) bannerText.innerText = `LOCKING IN ${lockRemainingSeconds}s`;

    // Only reveal the footer when the countdown actually begins
    if (idleFooter) idleFooter.style.display = 'flex';

    speakText("System is Locking soon");

    if (lockCountdownInterval) clearInterval(lockCountdownInterval);

    lockCountdownInterval = setInterval(() => {
        lockRemainingSeconds--;
        if (bannerText) bannerText.innerText = `LOCKING IN ${lockRemainingSeconds}s`;

        if (lockRemainingSeconds <= 0) {
            clearInterval(lockCountdownInterval);
            lockCountdownInterval = null;
            if (idleFooter) idleFooter.style.display = 'none';

            // Trigger full lock screen
            showPressAnyKey();
        }
    }, 1000);
}
function showPressAnyKey() {
    if (idleTimer) clearTimeout(idleTimer);
    if (lockCountdownInterval) clearInterval(lockCountdownInterval);

    const idleFooter = document.getElementById('idleCountdownFooter');
    if (idleFooter) idleFooter.style.display = 'none';

    const screen = document.getElementById('pressAnyKeyScreen');
    if (screen) screen.style.display = 'block'; // Show full lock screen

    const psp = document.getElementById('pspContainer');
    if (psp) psp.style.display = 'none';

    // --- ADD SOUND WHEN LOCKED ---
    speakText("System is locked now");

    syncArduinoState();
}

function hidePressAnyKey() {
    const screen = document.getElementById('pressAnyKeyScreen');
    if (screen) screen.style.display = 'none'; // Completely hide lock overlay so mouse/swipe works

    const psp = document.getElementById('pspContainer');
    if (psp) psp.style.removeProperty('display');

    requestAnimationFrame(() => updateCarouselPosition(0));
    syncArduinoState();
    resetIdleTimer();

    // --- ADD SOUND WHEN UNLOCKED ---
    speakText("Choose PC");
}

// Bind meaningful user interaction events (removed 'mousemove' to prevent jitter/constant resets)
['mousedown', 'keydown', 'touchstart', 'wheel', 'click'].forEach(event => {
    window.addEventListener(event, resetIdleTimer, { passive: true });
});
// --- NUMBER TYPING & MODAL LOGIC ---
let totalAmount = 0;          // coins already committed (each coin ends with Enter)
let pendingDigits = "";       // digits of the coin currently being typed by the Arduino
let modalCloseTimer = null;
let modalCountdownInterval = null;
let remainingSeconds = 5;

const numberModal = document.getElementById('numberModal');
const modalNumberDisplay = document.getElementById('modalNumberDisplay');
const modalTimerText = document.getElementById('modalTimerText');
const modalPcTarget = document.getElementById('modalPcTarget');

function isModalOpen() {
    return numberModal.classList.contains('visible');
}

function showNumberModal(pcName) {
    if (idleTimer) clearTimeout(idleTimer); // Prevent idle trigger while modal is open
    modalPcTarget.innerText = pcName || "PC-01";
    numberModal.classList.add('visible');
}

function hideNumberModal() {
    const insertedAmount = parseInt(modalNumberDisplay.innerText, 10) || 0;
    const activeCard = document.querySelector('.psp-card.active');

    numberModal.classList.remove('visible');

    if (modalCloseTimer) clearTimeout(modalCloseTimer);
    if (modalCountdownInterval) clearInterval(modalCountdownInterval);

    // If coins were inserted, pass data to hidden fields and click the LinkButton
    if (insertedAmount > 0 && activeCard) {
        const titleEl = activeCard.querySelector('.pc-title');
        const computerName = titleEl ? titleEl.innerText : "PC-01";

        const amountField = document.querySelector('[id$="HiddenField_ModalAmount"]');
        const nameField = document.querySelector('[id$="HiddenField_ModalComputerName"]');
        const triggerBtn = document.querySelector('[id$="LinkButton_ProcessVendo"]');

        if (amountField) amountField.value = insertedAmount;
        if (nameField) nameField.value = computerName;

        if (triggerBtn) {
            triggerBtn.click(); // Triggers the C# server-side Click event
        }
    }

    totalAmount = 0;
    pendingDigits = "";
    resetIdleTimer();
}

function resetModalCloseTimer() {
    if (modalCloseTimer) clearTimeout(modalCloseTimer);
    if (modalCountdownInterval) clearInterval(modalCountdownInterval);

    remainingSeconds = 5;
    modalTimerText.innerText = `Closing in ${remainingSeconds}s`;

    modalCountdownInterval = setInterval(() => {
        remainingSeconds--;
        if (remainingSeconds > 0) {
            modalTimerText.innerText = `Closing in ${remainingSeconds}s`;
        } else {
            modalTimerText.innerText = `Closing...`;
        }
    }, 1000);

    modalCloseTimer = setTimeout(() => {
        hideNumberModal();
    }, 5000);
}

function handleNumberInput(digit) {
    const activeCard = document.querySelector('.psp-card.active');
    let currentPcName = "PC-01";
    if (activeCard) {
        const titleEl = activeCard.querySelector('.pc-title');
        if (titleEl) currentPcName = titleEl.innerText;
    }

    // Play coin sound on every input
    playCoinSound();

    // Instead of string concatenation ("1" + "1" = "11"), 
    // we add them numerically (1 + 1 = 2)
    const currentPending = parseInt(pendingDigits, 10) || 0;
    const incomingDigit = parseInt(digit, 10) || 0;
    pendingDigits = currentPending + incomingDigit;

    modalNumberDisplay.innerText = totalAmount + pendingDigits;
    showNumberModal(currentPcName);
    resetModalCloseTimer();
}
// Enter = the Arduino finished typing one coin -> add it to the total
function handleEnterInput() {
    if (pendingDigits) {
        totalAmount += parseInt(pendingDigits, 10) || 0;
        pendingDigits = "";
    }
    modalNumberDisplay.innerText = totalAmount;
    resetModalCloseTimer();
}

// --- 2. DYNAMIC WALLPAPER DISCOVERY & SLIDER ---
const folderPath = 'Uploads/Wallpapers/';
const imageExtensions = ['.png', '.jpg', '.jpeg', '.webp'];
const sliderContainer = document.getElementById('slider');
const loadingText = document.getElementById('loadingText');

let validImages = [];
let slideIndex = 0;

function checkImageExists(url) {
    return new Promise((resolve) => {
        const img = new Image();
        img.onload = () => resolve(true);
        img.onerror = () => resolve(false);
        img.src = url;
    });
}

async function discoverImages() {
    let index = 1;
    while (true) {
        let found = false;
        for (const ext of imageExtensions) {
            const testPath = `${folderPath}${index}${ext}`;
            if (await checkImageExists(testPath)) {
                validImages.push(testPath);
                found = true;
                break;
            }
        }
        if (!found) break;
        index++;
    }

    if (validImages.length > 0) {
        if (loadingText) loadingText.remove();
        initSlider();
    } else {
        if (loadingText) loadingText.innerText = 'No wallpapers found in Uploads/Wallpapers/';
    }
}

function initSlider() {
    validImages.forEach((src, idx) => {
        const slide = document.createElement('div');
        slide.classList.add('slide');
        if (idx === 0) slide.classList.add('active');
        slide.style.backgroundImage = `url('${src}')`;
        sliderContainer.appendChild(slide);
    });

    if (validImages.length > 1) {
        setInterval(nextSlide, 10000);
    }
}

function nextSlide() {
    const slides = document.querySelectorAll('.slide');
    slides[slideIndex].classList.remove('active');
    slideIndex = (slideIndex + 1) % slides.length;
    slides[slideIndex].classList.add('active');
}

// --- 3. STATUS CALCULATION ---
function getClientStatus(client) {
    if (!client) return "Offline";

    // Use pre-calculated status from handler (supports both camelCase and PascalCase)
    const status = client.status || client.Status;
    if (status) {
        return status;
    }

    return "Available";
}

// --- 4. FETCH CLIENT DATA & CAROUSEL RENDERING ---
const pspContainer = document.getElementById('pspContainer');
const pspCarousel = document.getElementById('pspCarousel');
let selectedIndex = 0;
let clientData = [];
let filteredClientData = [];

async function fetchClientData() {
    try {
        var endpoint = /\.aspx$/i.test(window.location.pathname)
            ? window.location.pathname.replace(/[^\/]+\.aspx$/i, 'Ashx/GetComputerHandler.ashx')
            : window.location.pathname.replace(/[^\/]+$/i, 'Ashx/GetComputerHandler.ashx');

        const response = await fetch(endpoint);
        const data = await response.json();

        if (Array.isArray(data)) {
            if (JSON.stringify(data) !== JSON.stringify(clientData)) {
                clientData = data;
                renderPSPMenu();
                renderAttractGrid();  // display-only board on the Press Any Key screen
                syncArduinoState();   // lock/unlock coins depending on PC availability
            }
        }
    } catch (err) {
        console.error('Failed to fetch computer data from handler:', err);
    }
}
function renderPSPMenu() {
    pspCarousel.innerHTML = '';

    let filteredData = clientData.filter(client => {
        const status = getClientStatus(client);
        return status === "Available" || status === "Occupied" || status === "Offline" || status === "Paused" || status === "Administrator";
    });

    filteredClientData = filteredData.sort((a, b) => {
        const nameA = a.clientName || a.ClientName || '';
        const nameB = b.clientName || b.ClientName || '';
        return nameA.localeCompare(nameB, undefined, { numeric: true, sensitivity: 'base' });
    });

    if (selectedIndex >= filteredClientData.length) {
        selectedIndex = Math.max(0, filteredClientData.length - 1);
    }

    filteredClientData.forEach((client, index) => {
        const status = getClientStatus(client);
        const statusClass = `status-${status.toLowerCase()}`;

        const timeStart = client.timeStart || client.TimeStart;
        const timeEnd = client.timeEnd || client.TimeEnd;
        const ipAddress = client.ipAddress || client.IPAddress;
        const clientName = client.clientName || client.ClientName;

        const formattedStart = formatDateTimeString(timeStart);
        const formattedEnd = formatDateTimeString(timeEnd);

        const card = document.createElement('div');
        card.className = `psp-card ${index === selectedIndex ? 'active' : ''} ${statusClass}`;

        card.innerHTML = `
            <div class="pc-icon-wrapper">💻</div>
            <div class="pc-title">${clientName || `PC-${String(index + 1).padStart(2, '0')}`}</div>
            <div class="pc-details">
                ${ipAddress && ipAddress !== '-' ? `${ipAddress}<br>` : ''}
                ${formattedStart ? `${formattedStart}<br>` : ''}
                ${formattedEnd ? `${formattedEnd}` : ''}
            </div>
            <div class="psp-badge ${statusClass}">
                ${status}
            </div>
        `;

        card.addEventListener('click', () => {
            if (isModalOpen()) return;
            if (Math.abs(currentDeltaX) < 10) {
                selectedIndex = index;
                updateCarouselPosition(0);
            }
        });

        pspCarousel.appendChild(card);
    });

    requestAnimationFrame(() => updateCarouselPosition(0));
}
let lastSpokenPC = "";
function updateCarouselPosition(extraOffset = 0) {
    const cards = document.querySelectorAll('.psp-card');
    if (cards.length === 0) return;

    cards.forEach((card, idx) => {
        card.classList.toggle('active', idx === selectedIndex);
    });

    if (pspContainer.style.display === 'none') return;

    const target = cards[selectedIndex];

    // --- ADD SOUND WHEN SELECTING / MOVING TO A PC ---
    if (target) {
        const titleEl = target.querySelector('.pc-title');
        const pcName = titleEl ? titleEl.innerText : `PC-${String(selectedIndex + 1).padStart(2, '0')}`;

        if (pcName !== lastSpokenPC) {
            lastSpokenPC = pcName;
            speakText(pcName); // e.g. says "PC-01"
        }
    }

    const cardCenter = (target.offsetLeft - pspCarousel.offsetLeft) + (target.offsetWidth / 2);
    const totalTranslate = (window.innerWidth / 2) - cardCenter + extraOffset;

    pspCarousel.style.transform = `translate3d(${totalTranslate}px, 0px, 0px)`;
    pspCarousel.style.webkitTransform = `translate3d(${totalTranslate}px, 0px, 0px)`;

    syncArduinoState();
}

// --- 5. TOUCH / SWIPE DRAG ENGINE ---
let startX = 0;
let currentDeltaX = 0;
let isDragging = false;

function onPointerDown(e) {
    if (isModalOpen() || filteredClientData.length === 0) return;
    isDragging = true;
    startX = e.clientX || (e.touches && e.touches[0].clientX);
    currentDeltaX = 0;
    pspCarousel.style.transition = 'none';
}

function onPointerMove(e) {
    if (!isDragging || isModalOpen()) return;
    const currentX = e.clientX || (e.touches && e.touches[0].clientX);
    currentDeltaX = currentX - startX;
    updateCarouselPosition(currentDeltaX);
}

function onPointerUp() {
    if (!isDragging) return;
    isDragging = false;

    pspCarousel.style.transition = 'transform 0.4s cubic-bezier(0.25, 1, 0.5, 1)';
    const swipeThreshold = window.innerWidth * 0.12;

    if (currentDeltaX < -swipeThreshold && selectedIndex < filteredClientData.length - 1) {
        selectedIndex++;
    } else if (currentDeltaX > swipeThreshold && selectedIndex > 0) {
        selectedIndex--;
    }

    updateCarouselPosition(0);
}

pspContainer.addEventListener('mousedown', onPointerDown);
window.addEventListener('mousemove', onPointerMove);
window.addEventListener('mouseup', onPointerUp);

pspContainer.addEventListener('touchstart', onPointerDown, { passive: true });
window.addEventListener('touchmove', onPointerMove, { passive: true });
window.addEventListener('touchend', onPointerUp);

window.addEventListener('resize', () => updateCarouselPosition(0));
window.addEventListener('orientationchange', () => setTimeout(() => updateCarouselPosition(0), 150));

let isWheelCoolingDown = false;
pspContainer.addEventListener('wheel', (e) => {
    if (isModalOpen() || filteredClientData.length === 0 || isWheelCoolingDown) return;

    if (e.deltaY > 0 || e.deltaX > 0) {
        if (selectedIndex < filteredClientData.length - 1) {
            selectedIndex++;
            updateCarouselPosition();
        }
    } else {
        if (selectedIndex > 0) {
            selectedIndex--;
            updateCarouselPosition();
        }
    }

    isWheelCoolingDown = true;
    setTimeout(() => { isWheelCoolingDown = false; }, 200);
});
document.addEventListener('keydown', (e) => {
    // 1. If the "Press Any Key" screen is visible
    if (isPressAnyKeyVisible()) {
        // If the user presses Left/Right arrow, numbers, or Ctrl+A / Ctrl+C, dismiss the attract screen
        if (e.key === 'ArrowLeft' || e.key === 'ArrowRight' || (e.key >= '0' && e.key <= '9') || (e.ctrlKey && (e.key.toLowerCase() === 'a' || e.key.toLowerCase() === 'c'))) {
            e.preventDefault();

            // Execute the serial port routine (matching what the click event does)
            serialTask(async () => {
                if (await openSerial(true)) {
                    hidePressAnyKey();
                    await serialWriter.write(new TextEncoder().encode(shouldLock() ? 'L' : 'U'));
                }
            });
            return;
        }
        // Ignore all other keys while the attract screen is showing
        e.preventDefault();
        return;
    }

    // --- Block number typing if the currently selected PC is Offline or Admin ---
    if (e.key >= '0' && e.key <= '9') {
        if (filteredClientData && filteredClientData.length > 0 && selectedIndex >= 0 && selectedIndex < filteredClientData.length) {
            const currentClient = filteredClientData[selectedIndex];
            const currentStatus = getClientStatus(currentClient);
            if (currentStatus === "Offline" || currentStatus === "Administrator") {
                e.preventDefault();
                e.stopPropagation();
                return;
            }
        }

        e.preventDefault();
        e.stopPropagation();
        handleNumberInput(e.key);
        return;
    }

    // --- Enter input for coins ---
    if (e.key === 'Enter' && (pendingDigits || isModalOpen())) {
        if (filteredClientData && filteredClientData.length > 0 && selectedIndex >= 0 && selectedIndex < filteredClientData.length) {
            const currentClient = filteredClientData[selectedIndex];
            const currentStatus = getClientStatus(currentClient);
            if (currentStatus === "Offline" || currentStatus === "Administrator" || currentStatus === "Paused") {
                e.preventDefault();
                e.stopPropagation();
                return;
            }
        }

        e.preventDefault();
        e.stopPropagation();
        handleEnterInput();
        return;
    }

    if (isModalOpen() || filteredClientData.length === 0) return;

    // --- Carousel Navigation (Arrow Keys & Ctrl Shortcuts) ---
    if (e.key === 'ArrowRight' || (e.ctrlKey && e.key.toLowerCase() === 'c')) {
        e.preventDefault();
        if (selectedIndex < filteredClientData.length - 1) {
            selectedIndex++;
            updateCarouselPosition();
        }
    } else if (e.key === 'ArrowLeft' || (e.ctrlKey && e.key.toLowerCase() === 'a')) {
        e.preventDefault();
        if (selectedIndex > 0) {
            selectedIndex--;
            updateCarouselPosition();
        }
    } else {
        // Block and swallow any other unexpected keystrokes
        e.preventDefault();
        e.stopPropagation();
    }
});
// --- 6. INITIALIZATION & SSE ---
window.addEventListener('load', function () {
    const endpoint = window.location.protocol + '//' + window.location.hostname + ':2050/sse';
    let evtSource = null;

    try {
        evtSource = new EventSource(endpoint);

        evtSource.onmessage = function (event) {
            fetchClientData();

            const LinkButton_Refresh = document.querySelector('[id$="LinkButton_Refresh"]');
            if (LinkButton_Refresh) {
                LinkButton_Refresh.click();
            }
        };

        evtSource.onerror = function (err) {
            console.error('SSE Error/Disconnected:', err);
        };
    } catch (e) {
        console.error('Failed to initialize SSE:', e);
    }
    resetIdleTimer();
});

function formatDateTimeString(dateStr) {
    if (!dateStr || dateStr.trim() === "") return "";

    if (dateStr.includes("9999-12-31")) {
        return "∞";
    }

    const d = new Date(dateStr);
    if (isNaN(d.getTime())) return dateStr;

    const months = ["Jan", "Feb", "Mar", "Apr", "May", "Jun", "Jul", "Aug", "Sep", "Oct", "Nov", "Dec"];

    const month = months[d.getMonth()];
    const day = String(d.getDate()).padStart(2, '0');

    let hours = d.getHours();
    const minutes = String(d.getMinutes()).padStart(2, '0');
    const ampm = hours >= 12 ? 'PM' : 'AM';

    hours = hours % 12;
    hours = hours ? hours : 12;
    const formattedHours = String(hours).padStart(2, '0');

    return `${month} ${day} – ${formattedHours}:${minutes} ${ampm}`;
}

discoverImages();
fetchClientData();

function toggleFullScreen() {
    const icon = document.getElementById('fullscreenIcon');

    if (!document.fullscreenElement && !document.webkitFullscreenElement && !document.msFullscreenElement) {
        const docEl = document.documentElement;
        if (docEl.requestFullscreen) {
            docEl.requestFullscreen();
        } else if (docEl.webkitRequestFullscreen) {
            docEl.webkitRequestFullscreen();
        } else if (docEl.msRequestFullscreen) {
            docEl.msRequestFullscreen();
        }
        if (icon) {
            icon.classList.remove('fa-expand');
            icon.classList.add('fa-compress');
        }
    } else {
        if (document.exitFullscreen) {
            document.exitFullscreen();
        } else if (document.webkitExitFullscreen) {
            document.webkitExitFullscreen();
        } else if (document.msExitFullscreen) {
            document.msExitFullscreen();
        }
        if (icon) {
            icon.classList.remove('fa-compress');
            icon.classList.add('fa-expand');
        }
    }
}

document.addEventListener('fullscreenchange', () => {
    const icon = document.getElementById('fullscreenIcon');
    if (!icon) return;

    if (document.fullscreenElement) {
        icon.classList.remove('fa-expand');
        icon.classList.add('fa-compress');
    } else {
        icon.classList.remove('fa-compress');
        icon.classList.add('fa-expand');
    }
});

// ---------------- Web Serial ----------------
let serialPort = null;
let serialWriter = null;
let serialQueue = Promise.resolve();

async function openSerial(allowPrompt) {
    if (!('serial' in navigator)) {
        console.error('Web Serial not available.');
        return false;
    }
    if (serialWriter) return true;

    if (!serialPort) {
        const ports = await navigator.serial.getPorts();
        serialPort = ports[0] || null;
        if (!serialPort && allowPrompt) serialPort = await navigator.serial.requestPort();
        if (!serialPort) return false;
    }
    if (!serialPort.writable) await serialPort.open({ baudRate: 9600 });
    serialWriter = serialPort.writable.getWriter();
    return true;
}

function serialTask(task) {
    serialQueue = serialQueue.then(async () => {
        try {
            await task();
        } catch (err) {
            console.error('Serial error:', err);
            try { if (serialWriter) serialWriter.releaseLock(); } catch (e) { }
            try { if (serialPort) await serialPort.close(); } catch (e) { }
            serialWriter = null;
            serialPort = null;
        }
    });
}

function sendSerialCommand(command) {
    serialTask(async () => {
        if (await openSerial(false)) {
            await serialWriter.write(new TextEncoder().encode(command));
        }
    });
}

const LOCK_WHILE_MODAL_OPEN = false;

function shouldLock() {
    if (isPressAnyKeyVisible()) return true;
    if (LOCK_WHILE_MODAL_OPEN && isModalOpen()) return true;

    if (filteredClientData && filteredClientData.length > 0 && selectedIndex >= 0 && selectedIndex < filteredClientData.length) {
        const selectedClient = filteredClientData[selectedIndex];
        const status = getClientStatus(selectedClient);
        if (status === "Offline" || status === "Administrator" || status === "Paused") {
            return true;
        }
    }

    const hasValidPc = clientData.some(c => {
        const st = getClientStatus(c);
        return st === "Available" || st === "Occupied";
    });

    return !hasValidPc;
}

function syncArduinoState() {
    sendSerialCommand(shouldLock() ? 'L' : 'U');
}

document.addEventListener('click', function (e) {
    if (!e.target.closest || !e.target.closest('#pressAnyKeyScreen')) return;

    serialTask(async () => {
        if (await openSerial(true)) {
            hidePressAnyKey();
            await serialWriter.write(new TextEncoder().encode(shouldLock() ? 'L' : 'U'));
        }
    });
});

if ('serial' in navigator) {
    navigator.serial.addEventListener('disconnect', () => { serialWriter = null; serialPort = null; });
    navigator.serial.addEventListener('connect', () => syncArduinoState());
}

const originalShowNumberModal = showNumberModal;
showNumberModal = function (pcName) {
    originalShowNumberModal(pcName);
    if (LOCK_WHILE_MODAL_OPEN) sendSerialCommand('L');
};

const originalHideNumberModal = hideNumberModal;
hideNumberModal = function () {
    originalHideNumberModal();
    syncArduinoState();
};

if (document.readyState === 'complete') showPressAnyKey();
else window.addEventListener('load', showPressAnyKey);

function getPressAnyKeyScreen() {
    return document.getElementById('pressAnyKeyScreen');
}

function isPressAnyKeyVisible() {
    const screen = getPressAnyKeyScreen();
    return !!screen && screen.style.display !== 'none';
}


function escapeHtml(s) {
    return String(s == null ? '' : s).replace(/[&<>"']/g, c => ({ '&': '&amp;', '<': '&lt;', '>': '&gt;', '"': '&quot;', "'": '&#39;' }[c]));
}


let serverTimeOffset = null;

async function fetchServerTime() {
    var endpoint = /\.aspx$/i.test(window.location.pathname)
        ? window.location.pathname.replace(/[^\/]+\.aspx$/i, 'Ashx/GetTimeServiceHandler.ashx')
        : window.location.pathname.replace(/[^\/]+$/i, 'Ashx/GetTimeServiceHandler.ashx');

    const response = await fetch(endpoint, { cache: 'no-store' });
    if (!response.ok) throw new Error('Network response was not ok');
    const data = await response.json();
    return new Date(data.serverTime).getTime();
}

async function syncServerTime() {
    try {
        const serverNow = await fetchServerTime();
        serverTimeOffset = serverNow - (performance.timeOrigin + performance.now());
    } catch (err) {
        console.warn('Failed to sync server time:', err);
    }
}

function getServerNow() {
    return performance.timeOrigin + performance.now() + (serverTimeOffset || 0);
}

function getRemainingSeconds(client) {
    const endStr = client.timeEnd || client.TimeEnd;
    if (!endStr || String(endStr).includes('9999-12-31') || String(endStr).trim() === "") return null;

    const end = Date.parse(String(endStr).replace(" ", "T"));
    if (isNaN(end)) return 0;

    return Math.max(0, Math.floor((end - getServerNow()) / 1000));
}

function getUserType(client) {
    const raw = client.userType || client.UserType || client.memberType || client.accountType || '';
    const t = String(raw).trim();
    if (/vip/i.test(t)) return { text: 'VIP', cls: 'vip', icon: 'fa-crown' };
    if (t && !/guest/i.test(t)) return { text: /member/i.test(t) ? 'MEMBER' : t, cls: 'member', icon: 'fa-user' };
    return { text: 'GUEST', cls: 'guest', icon: 'fa-user' };
}

function buildAttractCard(client, index) {
    const status = getClientStatus(client);
    const running = (status === 'Occupied' || status === 'Paused' || status === 'Administrator');

    const theme = status === 'Paused' ? 'theme-orange'
        : status === 'Administrator' ? 'theme-purple'
            : status === 'Occupied' ? 'theme-red'
                : status === 'Available' ? 'theme-green' : 'theme-gray';

    const pill = status === 'Occupied' ? { cls: 'pill-occupied', text: 'OCCUPIED' }
        : status === 'Paused' ? { cls: 'pill-paused', text: 'PAUSED' }
            : status === 'Administrator' ? { cls: 'pill-admin', text: 'ADMINISTRATOR' }
                : status === 'Available' ? { cls: 'pill-available', text: 'AVAILABLE' }
                    : { cls: 'pill-offline', text: 'OFFLINE' };

    const user = getUserType(client);
    const name = client.ClientName || `PC-${String(index + 1).padStart(2, '0')}`;
    const endAttr = (running && client.TimeEnd) ? escapeHtml(client.TimeEnd) : '';

    return `
        <div class="atc-wrap ${theme}">
          <div class="atc"><div class="atc-in">
            <div class="atc-top">
              <div class="atc-pill ${pill.cls}">${pill.text}</div>
              <div class="atc-identity">
                <i class="fas fa-desktop"></i>
                <div class="atc-name">${escapeHtml(name)}</div>
              </div>
            </div>
            <div class="atc-time">
              <div class="atc-label"><i class="far fa-clock"></i> TIME</div>
              <div class="atc-digits" data-end="${endAttr}" data-running="${running ? 1 : 0}"></div>
              <div class="atc-units"><span>Hour</span><span>|</span><span>Minutes</span><span>|</span><span>Seconds</span></div>
            </div>
          </div></div>
        </div>`;
}
function renderAttractGrid() {
    const grid = document.getElementById('attractGrid');
    if (!grid) return;

    const list = clientData.slice().sort((a, b) =>
        (a.ClientName || '').localeCompare(b.ClientName || '', undefined, { numeric: true, sensitivity: 'base' }));

    const count = list.length;
    if (count === 0) {
        grid.innerHTML = '';
        return;
    }

    let cols = 4; // default fallback

    // Tailored symmetric layouts for specific counts:
    if (count <= 4) {
        cols = count;
    } else {
        // Dynamic scaling for in-between numbers
        const screenAspect = window.innerWidth / Math.max(1, window.innerHeight);
        cols = Math.ceil(Math.sqrt(count * screenAspect * 0.7));
        cols = Math.max(2, Math.min(cols, count));
    }

    grid.style.setProperty('--cols', cols);

    grid.innerHTML = list.map(buildAttractCard).join('');
    tickAttractTimes();
}
window.addEventListener('resize', () => {
    if (isPressAnyKeyVisible()) {
        renderAttractGrid();
    }
});

function tickAttractTimes() {
    document.querySelectorAll('#attractGrid .atc-digits').forEach(el => {
        let html;
        if (el.dataset.running !== '1') {
            html = '<span>00</span><span class="sep">:</span><span>00</span><span class="sep">:</span><span>00</span>';
        } else {
            const secs = getRemainingSeconds({ TimeEnd: el.dataset.end });
            if (secs === null) {
                html = '<span class="inf">&infin;</span>';
            } else {
                const p = n => String(n).padStart(2, '0');
                html = `<span>${p(Math.floor(secs / 3600))}</span><span class="sep">:</span>` +
                    `<span>${p(Math.floor((secs % 3600) / 60))}</span><span class="sep">:</span>` +
                    `<span>${p(secs % 60)}</span>`;
            }
        }
        if (el.innerHTML !== html) el.innerHTML = html;
    });
}

syncServerTime().then(() => {
    if (typeof tickAttractTimes === 'function') tickAttractTimes();
});
setInterval(syncServerTime, 5 * 60 * 1000);

setInterval(() => { if (isPressAnyKeyVisible()) tickAttractTimes(); }, 1000);


// --- HELPER: Text-to-Speech Function ---
function speakText(text) {
    if ('speechSynthesis' in window) {
        window.speechSynthesis.cancel();

        const utterance = new SpeechSynthesisUtterance(text);
        utterance.rate = 1.5; // Speed (0.1 to 10)
        utterance.pitch = 2.0; // Pitch (0 to 2)
        window.speechSynthesis.speak(utterance);
    }
}
// --- SHARED AUDIO CONTEXT (Prevents browser audio crash on rapid drops) ---
let sharedAudioCtx = null;

function getAudioContext() {
    if (!sharedAudioCtx) {
        const AudioCtxClass = window.AudioContext || window.webkitAudioContext;
        if (AudioCtxClass) {
            sharedAudioCtx = new AudioCtxClass();
        }
    }
    if (sharedAudioCtx && sharedAudioCtx.state === 'suspended') {
        sharedAudioCtx.resume();
    }
    return sharedAudioCtx;
}

function playCoinSound() {
    try {
        const ctx = getAudioContext();
        if (!ctx) return;

        const now = ctx.currentTime;

        // Master gain container
        const masterGain = ctx.createGain();
        masterGain.gain.setValueAtTime(0.35, now);
        masterGain.connect(ctx.destination);

        // Crisp dual metallic coin frequencies (tuned for a clearer coin clink)
        const metallicFrequencies = [2400, 3100];

        metallicFrequencies.forEach((freq, i) => {
            const osc = ctx.createOscillator();
            const gain = ctx.createGain();

            osc.type = 'sine';
            osc.frequency.setValueAtTime(freq, now);
            // Natural metallic pitch decay
            osc.frequency.exponentialRampToValueAtTime(freq * 0.9, now + 0.12);

            // Clean decay envelope
            gain.gain.setValueAtTime(0.12, now);
            gain.gain.exponentialRampToValueAtTime(0.0001, now + (i === 0 ? 0.1 : 0.15));

            osc.connect(gain);
            gain.connect(masterGain);

            osc.start(now);
            osc.stop(now + 0.16);
        });

        // Short impact thud/clink component
        const thudOsc = ctx.createOscillator();
        const thudGain = ctx.createGain();

        thudOsc.type = 'triangle';
        thudOsc.frequency.setValueAtTime(600, now);
        thudOsc.frequency.exponentialRampToValueAtTime(200, now + 0.04);

        thudGain.gain.setValueAtTime(0.15, now);
        thudGain.gain.exponentialRampToValueAtTime(0.0001, now + 0.04);

        thudOsc.connect(thudGain);
        thudGain.connect(masterGain);

        thudOsc.start(now);
        thudOsc.stop(now + 0.05);

    } catch (e) {
        console.warn("Web Audio API not supported or blocked:", e);
    }
}