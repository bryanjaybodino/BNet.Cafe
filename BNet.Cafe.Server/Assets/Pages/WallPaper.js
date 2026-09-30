let wallpaperList = [];

document.addEventListener('DOMContentLoaded', function () {
    initWallpaperUploader();
});

function initWallpaperUploader() {
    const dropZone = document.getElementById('dropZone');
    const fileInput = document.getElementById('fileInput');

    if (!dropZone || !fileInput) return;

    dropZone.addEventListener('click', () => fileInput.click());

    fileInput.addEventListener('change', (e) => {
        handleWallpaperFiles(e.target.files);
        fileInput.value = ''; // Reset input to allow selecting deleted images again
    });

    ['dragenter', 'dragover'].forEach(eventName => {
        dropZone.addEventListener(eventName, (e) => {
            e.preventDefault();
            e.stopPropagation();
            dropZone.classList.add('dragover');
        }, false);
    });

    ['dragleave', 'drop'].forEach(eventName => {
        dropZone.addEventListener(eventName, (e) => {
            e.preventDefault();
            e.stopPropagation();
            dropZone.classList.remove('dragover');
        }, false);
    });

    dropZone.addEventListener('drop', (e) => {
        const dt = e.dataTransfer;
        handleWallpaperFiles(dt.files);
    });
}

function handleWallpaperFiles(files) {
    const validTypes = ['image/jpeg', 'image/png', 'image/webp'];
    const maxSize = 10 * 1024 * 1024; // 10MB per wallpaper file limit

    Array.from(files).forEach(file => {
        if (!validTypes.includes(file.type)) {
            ShowAlert(`"${file.name}" is not a valid wallpaper format (JPG, PNG, WEBP).`);
            return;
        }

        if (file.size > maxSize) {
            ShowAlert(`"${file.name}" exceeds the 10MB size limit.`);
            return;
        }

        const reader = new FileReader();
        reader.onload = function (e) {
            wallpaperList.push({
                id: 'wp_' + Date.now() + '_' + Math.random().toString(36).substring(2, 7),
                name: file.name,
                base64: e.target.result
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
    if (countSpan) countSpan.textContent = `${wallpaperList.length} Wallpaper(s) Selected`;

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
    wallpaperList = wallpaperList.filter(wp => wp.id !== id);
    renderWallpaperPreviews();
}

function clearAllWallpapers() {
    wallpaperList = [];
    renderWallpaperPreviews();
}

function ValidateWallpaperUpload() {
    if (wallpaperList.length === 0) {
        ShowAlert('Please select at least one wallpaper to upload.');
        return false;
    }

    const hiddenField = document.querySelector('[id$="HiddenField_WallpaperData"]');
    if (hiddenField) {
        hiddenField.value = JSON.stringify(wallpaperList);
    }

    disableSubmitButton();
    return true;
}