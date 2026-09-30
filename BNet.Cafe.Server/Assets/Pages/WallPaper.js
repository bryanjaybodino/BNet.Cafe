// Global scope binding to prevent re-declaration errors
var wallpaperList = window.wallpaperList || [];
let deleteModalInstance = null;
let pendingDeleteId = null;

// Modal Manager
function getDeleteWallpaperModal() {
    if (!deleteModalInstance) {
        deleteModalInstance = new BNetModal('#deleteWallpaperModal');
    }
    return deleteModalInstance;
}

function openDeleteWallpaperModal(id, name) {
    pendingDeleteId = id;
    const nameTarget = document.getElementById('deleteWallpaperTargetName');
    if (nameTarget) nameTarget.innerText = name;
    getDeleteWallpaperModal().open();
}

function closeDeleteWallpaperModal() {
    pendingDeleteId = null;
    getDeleteWallpaperModal().close();
}

// Handle UpdatePanel Async Postback Life Cycle
document.addEventListener('DOMContentLoaded', function () {
    syncWallpaperState();

    // Re-run after ASP.NET UpdatePanel postbacks complete
    if (typeof Sys !== 'undefined' && Sys.WebForms && Sys.WebForms.PageRequestManager) {
        Sys.WebForms.PageRequestManager.getInstance().add_endRequest(function () {
            syncWallpaperState();
        });
    }
});

// Global Delegation for Drag & Drop and File Selection
document.addEventListener('click', function (e) {
    const dropZone = e.target.closest('#dropZone');
    const fileInput = document.getElementById('fileInput');
    if (dropZone && fileInput && e.target !== fileInput) {
        fileInput.click();
    }
});

document.addEventListener('change', function (e) {
    if (e.target && e.target.id === 'fileInput') {
        handleWallpaperFiles(e.target.files);
        e.target.value = '';
    }
});

document.addEventListener('dragover', function (e) {
    const dropZone = e.target.closest('#dropZone');
    if (dropZone) {
        e.preventDefault();
        dropZone.classList.add('dragover');
    }
});

document.addEventListener('dragleave', function (e) {
    const dropZone = e.target.closest('#dropZone');
    if (dropZone) {
        e.preventDefault();
        dropZone.classList.remove('dragover');
    }
});

document.addEventListener('drop', function (e) {
    const dropZone = e.target.closest('#dropZone');
    if (dropZone) {
        e.preventDefault();
        dropZone.classList.remove('dragover');
        if (e.dataTransfer && e.dataTransfer.files) {
            handleWallpaperFiles(e.dataTransfer.files);
        }
    }
});

function syncWallpaperState() {
    const hiddenField = document.querySelector('[id$="HiddenField_WallpaperData"]');
    if (hiddenField && hiddenField.value) {
        try {
            const grid = document.getElementById('wallpaperGrid');
            if (grid) grid.innerHTML = '';

            const serverItems = JSON.parse(hiddenField.value);

            // Keep only pending local uploads that don't already exist on the server by file name
            const pendingUploads = wallpaperList.filter(wp =>
                !wp.isExisting && !serverItems.some(serverWp => serverWp.name === wp.name)
            );

            wallpaperList = [...serverItems, ...pendingUploads];
            renderWallpaperPreviews();
        } catch (e) {
            console.error('Error parsing wallpaper data:', e);
        }
    } else if (wallpaperList.length > 0) {
        renderWallpaperPreviews();
    }
}

function handleWallpaperFiles(files) {
    const validTypes = ['image/jpeg', 'image/png', 'image/webp'];
    const maxSize = 10 * 1024 * 1024;

    Array.from(files).forEach(file => {
        if (!validTypes.includes(file.type)) {
            const msg = `"${file.name}" is not a valid wallpaper format (JPG, PNG, WEBP).`;
            typeof ShowAlert === 'function' ? ShowAlert(msg) : alert(msg);
            return;
        }

        if (file.size > maxSize) {
            const msg = `"${file.name}" exceeds the 10MB size limit.`;
            typeof ShowAlert === 'function' ? ShowAlert(msg) : alert(msg);
            return;
        }

        // Check duplicates across ALL existing wallpapers (server + pending)
        const isDuplicate = wallpaperList.some(wp => wp.name === file.name);
        if (isDuplicate) {
            const msg = `"${file.name}" is already in the list.`;
            typeof ShowAlert === 'function' ? ShowAlert(msg) : alert(msg);
            return;
        }

        const reader = new FileReader();
        reader.onload = function (e) {
            wallpaperList.push({
                id: 'new_' + Date.now() + '_' + Math.random().toString(36).substring(2, 7),
                name: file.name,
                base64: e.target.result,
                isExisting: false
            });
            renderWallpaperPreviews();
        };
        reader.readAsDataURL(file);
    });
}

function renderWallpaperPreviews() {
    const grid = document.getElementById('wallpaperGrid');
    const header = document.getElementById('previewHeader');
    const countSpan = document.getElementById('imageCount');

    if (!grid) return;

    grid.innerHTML = '';

    if (wallpaperList.length === 0) {
        if (header) header.classList.add('d-none');
        return;
    }

    if (header) header.classList.remove('d-none');
    if (countSpan) countSpan.textContent = `${wallpaperList.length} Wallpaper(s) Total`;

    wallpaperList.forEach((wp) => {
        const card = document.createElement('div');
        card.className = 'wallpaper-card';
        card.innerHTML = `
            <img src="${wp.base64}" alt="${wp.name}">
            <button type="button" class="remove-btn" title="Remove wallpaper" onclick="removeWallpaper('${wp.id}')">
                <i class="fa-solid fa-xmark"></i>
            </button>
            <div class="file-info">${wp.name}</div>
        `;
        grid.appendChild(card);
    });
}

function removeWallpaper(id) {
    const item = wallpaperList.find(wp => wp.id === id);
    if (!item) return;

    if (item.isExisting) {
        openDeleteWallpaperModal(id, item.name);
    } else {
        wallpaperList = wallpaperList.filter(wp => wp.id !== id);
        renderWallpaperPreviews();
    }
}

function confirmWallpaperDeletion() {
    if (!pendingDeleteId) return;

    const item = wallpaperList.find(wp => wp.id === pendingDeleteId);
    if (!item) return;

    const targetField = document.querySelector('[id$="HiddenField_DeleteTarget"]');
    const deleteBtn = document.querySelector('[id$="LinkButton_Delete"]');
    const wallpaperDataField = document.querySelector('[id$="HiddenField_WallpaperData"]');

    if (targetField && deleteBtn) {
        targetField.value = item.name;

        // Clear large Base64 hidden field payload before deleting to avoid huge HTTP POST requests
        if (wallpaperDataField) {
            wallpaperDataField.value = '';
        }

        // UI Feedback on Modal Delete Button
        const confirmBtn = document.getElementById('btnConfirmDelete');
        if (confirmBtn) {
            confirmBtn.classList.add('disabled');
            confirmBtn.style.pointerEvents = 'none';
            confirmBtn.innerHTML = '<i class="fa fa-spinner fa-spin"></i> Deleting...';
        }

        // Trigger LinkButton click natively
        deleteBtn.click();
    }
}

function clearAllWallpapers() {
    wallpaperList = wallpaperList.filter(wp => wp.isExisting);
    renderWallpaperPreviews();
}

function ValidateWallpaperUpload() {
    if (wallpaperList.length === 0) {
        if (typeof ShowAlert === 'function') {
            ShowAlert('Please select at least one wallpaper.');
        } else {
            alert('Please select at least one wallpaper.');
        }
        return false;
    }
    const hiddenField = document.querySelector('[id$="HiddenField_WallpaperData"]');
    if (hiddenField) {
        hiddenField.value = JSON.stringify(wallpaperList);
    }

    showSubmitLoading();
    return true;
}

function showSubmitLoading() {
    const submitBtn = document.querySelector('[id$="LinkButton_Submit"]');
    if (submitBtn) {
        submitBtn.classList.add('disabled');
        submitBtn.style.pointerEvents = 'none';
        submitBtn.innerHTML = '<i class="fa fa-spinner fa-spin"></i> Saving...';
    }
}