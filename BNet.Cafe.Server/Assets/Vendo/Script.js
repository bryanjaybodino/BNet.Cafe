// --- 1. PROTECTIONS ---
document.addEventListener('contextmenu', e => e.preventDefault());
document.addEventListener('dragstart', e => e.preventDefault());

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

    // Digits are joined into ONE number ("2","0" -> 20) until Enter arrives.
    pendingDigits += digit;

    modalNumberDisplay.innerText = totalAmount + (parseInt(pendingDigits, 10) || 0);
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

    if (client.TimeStart && client.TimeStart.trim() !== "") {
        const start = Date.parse(client.TimeStart);
        const end = Date.parse(client.TimeEnd);

        if (!isNaN(start)) {
            let hours = 0;
            if (!isNaN(end)) {
                const durationMs = end - start;
                hours = Math.floor(durationMs / (1000 * 60 * 60));
            }

            const isAdministrator = (hours === 100);

            if (isAdministrator) {
                return "Administrator";
            } else {
                const isPaused = String(client.IsPaused).toUpperCase() === "TRUE";
                return isPaused ? "Paused" : "Occupied";
            }
        }
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
        const endpoint = window.location.protocol + '//' + window.location.hostname + ':2050/text';
        const response = await fetch(endpoint);
        let data = await response.text();
        try {
            data = JSON.parse(data);
        } catch (e) { }

        if (Array.isArray(data) && JSON.stringify(data) !== JSON.stringify(clientData)) {
            clientData = data;
            renderPSPMenu();
            syncArduinoState();   // lock/unlock coins depending on PC availability
        }
    } catch (err) {
        console.error(err);
    }
}

function renderPSPMenu() {
    pspCarousel.innerHTML = '';

    let filteredData = clientData.filter(client => {
        const status = getClientStatus(client);
        return status === "Available" || status === "Occupied";
    });

    filteredClientData = filteredData.sort((a, b) => {
        const nameA = a.ClientName || '';
        const nameB = b.ClientName || '';
        return nameA.localeCompare(nameB, undefined, { numeric: true, sensitivity: 'base' });
    });

    if (selectedIndex >= filteredClientData.length) {
        selectedIndex = Math.max(0, filteredClientData.length - 1);
    }

    filteredClientData.forEach((client, index) => {
        const status = getClientStatus(client);
        const statusClass = `status-${status.toLowerCase()}`;

        const formattedStart = formatDateTimeString(client.TimeStart);
        const formattedEnd = formatDateTimeString(client.TimeEnd);

        const card = document.createElement('div');
        card.className = `psp-card ${index === selectedIndex ? 'active' : ''} ${statusClass}`;

        card.innerHTML = `
                    <div class="pc-icon-wrapper">💻</div>
                    <div class="pc-title">${client.ClientName || `PC-${String(index + 1).padStart(2, '0')}`}</div>
                    <div class="pc-details">
                        ${client.IPAddress ? `${client.IPAddress}<br>` : ''}
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

function updateCarouselPosition(extraOffset = 0) {
    const cards = document.querySelectorAll('.psp-card');
    if (cards.length === 0) return;

    cards.forEach((card, idx) => {
        if (idx === selectedIndex) {
            card.classList.add('active');
        } else {
            card.classList.remove('active');
        }
    });


    const screenWidth = window.innerWidth;
    const cardWidth = cards[0].offsetWidth;
    const gap = window.innerWidth < 600 ? 12 : 28;

    const centerOffset = (screenWidth / 2) - (cardWidth / 2) - (selectedIndex * (cardWidth + gap));
    const totalTranslate = centerOffset + extraOffset;

    pspCarousel.style.transform = `translate3d(${totalTranslate}px, 0px, 0px)`;
    pspCarousel.style.webkitTransform = `translate3d(${totalTranslate}px, 0px, 0px)`;
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
    // Press Any Key screen: first key press dismisses it and unlocks the coin acceptor
    if (isPressAnyKeyVisible()) {
        e.preventDefault();
        hidePressAnyKey();      // hides the screen and sends 'U' (if a PC is available)
        return;
    }

    // Prevent number keys from triggering browser shortcuts or focus changes
    if (e.key >= '0' && e.key <= '9') {
        e.preventDefault();
        e.stopPropagation();
        handleNumberInput(e.key);
        return;
    }

    if (e.key === 'Enter' && (pendingDigits || isModalOpen())) {
        e.preventDefault();
        e.stopPropagation();
        handleEnterInput();
        return;
    }

    if (isModalOpen() || filteredClientData.length === 0) return;

    if (e.key === 'ArrowRight' || e.key === 'd' || e.key === 'D') {
        if (selectedIndex < filteredClientData.length - 1) {
            selectedIndex++;
            updateCarouselPosition();
        }
    } else if (e.key === 'ArrowLeft' || e.key === 'a' || e.key === 'A') {
        if (selectedIndex > 0) {
            selectedIndex--;
            updateCarouselPosition();
        }

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
        // Enter Fullscreen
        const docEl = document.documentElement;
        if (docEl.requestFullscreen) {
            docEl.requestFullscreen();
        } else if (docEl.webkitRequestFullscreen) { /* Safari */
            docEl.webkitRequestFullscreen();
        } else if (docEl.msRequestFullscreen) { /* IE11 */
            docEl.msRequestFullscreen();
        }
        if (icon) {
            icon.classList.remove('fa-expand');
            icon.classList.add('fa-compress');
        }
    } else {
        // Exit Fullscreen
        if (document.exitFullscreen) {
            document.exitFullscreen();
        } else if (document.webkitExitFullscreen) { /* Safari */
            document.webkitExitFullscreen();
        } else if (document.msExitFullscreen) { /* IE11 */
            document.msExitFullscreen();
        }
        if (icon) {
            icon.classList.remove('fa-compress');
            icon.classList.add('fa-expand');
        }
    }
}

// Keep icon synced if user exits via ESC key
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
let serialQueue = Promise.resolve();   // commands are sent one by one, in order

async function openSerial(allowPrompt) {
    if (!('serial' in navigator)) {
        console.error('Web Serial not available. Use Chrome/Edge on https:// or http://localhost');
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

// true  = coin acceptor is locked while the modal is open (one coin at a time)
// false = customer can keep inserting coins during the 5s countdown (they add up)
const LOCK_WHILE_MODAL_OPEN = false;

function shouldLock() {
    // Locked while: Press Any Key screen is visible, the coin modal is open,
    // or there is no available PC to credit.
    if (isPressAnyKeyVisible()) return true;
    if (LOCK_WHILE_MODAL_OPEN && isModalOpen()) return true;
    const hasPc = filteredClientData.some(c => {
        const st = getClientStatus(c);
        return st === "Available" || st === "Occupied";
    });
    return !hasPc;
}

function syncArduinoState() {
    sendSerialCommand(shouldLock() ? 'L' : 'U');
}

// ONE-TIME SETUP: click the Press Any Key screen once and choose the Arduino Leonardo.
// After that the browser remembers it and connects by itself.
document.addEventListener('click', function (e) {
    if (!e.target.closest || !e.target.closest('#pressAnyKeyScreen')) return;

    serialTask(async () => {
        if (await openSerial(true)) {
            hidePressAnyKey();   // hide the screen first so shouldLock() sees it as closed
            await serialWriter.write(new TextEncoder().encode(shouldLock() ? 'L' : 'U'));
        }
    });

});

if ('serial' in navigator) {
    navigator.serial.addEventListener('disconnect', () => { serialWriter = null; serialPort = null; });
    navigator.serial.addEventListener('connect', () => syncArduinoState());
}

// ---------------- Lock while the coin modal is open ----------------
const originalShowNumberModal = showNumberModal;
showNumberModal = function (pcName) {
    originalShowNumberModal(pcName);
    if (LOCK_WHILE_MODAL_OPEN) sendSerialCommand('L');
};

const originalHideNumberModal = hideNumberModal;
hideNumberModal = function () {
    originalHideNumberModal();
    // Unlock only if the Press Any Key screen is hidden AND a PC is available
    syncArduinoState();
};

// Page just loaded -> Press Any Key screen is visible -> lock coins
if (document.readyState === 'complete') showPressAnyKey();
else window.addEventListener('load', showPressAnyKey);



function getPressAnyKeyScreen() {
    return document.getElementById('pressAnyKeyScreen');
}

function isPressAnyKeyVisible() {
    const screen = getPressAnyKeyScreen();
    return !!screen && screen.style.display !== 'none';
}

function showPressAnyKey() {
    const screen = getPressAnyKeyScreen();
    if (screen) screen.style.display = 'flex';
    syncArduinoState();
}

function hidePressAnyKey() {
    const screen = getPressAnyKeyScreen();
    if (screen) screen.style.display = 'none';
    syncArduinoState();   // stays locked if there is no PC
}