// =============================================================================
// BNetAlert Notification System
// =============================================================================
class BNetAlert {
    static show(message, type = 'error', duration = 5000) {
        let alertBox = document.getElementById('bnet-alert-box');
        if (!alertBox) {
            alertBox = document.createElement('div');
            alertBox.id = 'bnet-alert-box';
            alertBox.className = 'bnet-alert-box';
            document.body.appendChild(alertBox);
        }

        // Clear existing hide timer if trigger is fired rapidly
        if (alertBox._hideTimer) {
            clearTimeout(alertBox._hideTimer);
        }

        // Reset variant classes
        alertBox.className = 'bnet-alert-box';

        let iconClass = 'fa-triangle-exclamation';
        if (type === 'success') {
            alertBox.classList.add('bnet-alert-success');
            iconClass = 'fa-circle-check';
        } else if (type === 'warning') {
            alertBox.classList.add('bnet-alert-warning');
            iconClass = 'fa-triangle-exclamation';
        } else if (type === 'info') {
            alertBox.classList.add('bnet-alert-info');
            iconClass = 'fa-circle-info';
        } else {
            alertBox.classList.add('bnet-alert-error');
        }

        // Format content based on input type (Array vs String)
        let contentHtml = '';
        if (Array.isArray(message)) {
            const items = message.map(msg => `<li>${msg}</li>`).join('');
            contentHtml = `<ul class="bnet-alert-list">${items}</ul>`;
        } else {
            contentHtml = `<span>${message}</span>`;
        }

        alertBox.innerHTML = `
            <i class="fa-solid ${iconClass} bnet-alert-icon"></i>
            <div class="bnet-alert-content">${contentHtml}</div>
            <button type="button" class="bnet-alert-close" onclick="BNetAlert.hide()">&times;</button>
        `;

        requestAnimationFrame(() => {
            alertBox.classList.add('show');
        });

        alertBox._hideTimer = setTimeout(() => {
            BNetAlert.hide();
        }, duration);
    }

    static hide() {
        const alertBox = document.getElementById('bnet-alert-box');
        if (alertBox) {
            alertBox.classList.remove('show');
        }
    }
}

// Backward compatibility alias for legacy calls
function ShowAlert(message, type = 'error') {
    BNetAlert.show(message, type);
}