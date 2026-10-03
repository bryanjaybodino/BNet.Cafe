<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="DownloadApp.ascx.cs" Inherits="BNet.Cafe.Server.Forms.DownloadApp" %>

<style>
    /* Main Container Card */
    .download-card-container {
        background-color: var(--bg-light-secondary);
        border: 1px solid var(--border-light);
        border-radius: 16px;
        padding: 32px 36px;
        display: flex;
        justify-content: space-between;
        align-items: flex-start;
        gap: 24px;
        width: 100%;
        box-sizing: border-box;
        box-shadow: 0 4px 20px rgba(0, 0, 0, 0.03);
        font-family: inherit;
    }

    /* Left Section Details */
    .download-info-section {
        display: flex;
        flex-direction: column;
        flex: 1;
    }

    .download-title-group {
        display: flex;
        align-items: baseline;
        gap: 10px;
        margin-bottom: 12px;
    }

    .download-title-main {
        font-size: 28px;
        font-weight: 900;
        letter-spacing: -0.5px;
        color: var(--text-light);
        text-transform: uppercase;
        margin: 0;
    }

    .download-title-sub {
        font-size: 13px;
        font-weight: 700;
        color: #4f46e5;
        text-transform: uppercase;
        letter-spacing: 0.5px;
    }

    .download-description {
        font-size: 13px;
        color: var(--text-light-secondary);
        text-transform: uppercase;
        letter-spacing: 0.3px;
        margin: 0 0 20px 0;
        font-weight: 500;
    }

    /* Bullet Points List */
    .download-features-list {
        display: flex;
        flex-direction: column;
        gap: 10px;
        list-style: none;
        padding: 0;
        margin: 0 0 20px 0;
    }

    .feature-item {
        display: flex;
        align-items: center;
        gap: 10px;
        font-size: 13px;
        font-weight: 700;
        text-transform: uppercase;
        letter-spacing: 0.4px;
        color: var(--text-light);
    }

    /* Green Check SVG Icon */
    .check-icon {
        width: 18px;
        height: 18px;
        background-color: #10b981;
        color: #ffffff;
        border-radius: 50%;
        display: flex;
        align-items: center;
        justify-content: center;
        flex-shrink: 0;
    }

    .check-icon svg {
        width: 12px;
        height: 12px;
        stroke-width: 3;
    }

    /* Pill Badge for Application Version */
    .version-badge {
        background-color: #18181b;
        color: #ffffff;
        padding: 6px 16px;
        border-radius: 20px;
        font-size: 12px;
        font-weight: 700;
        letter-spacing: 0.5px;
        display: inline-block;
    }

    /* SSL Bypass Instructions Box */
    .ssl-instruction-box {
        background-color: #fef3c7;
        border: 1px solid #fde047;
        border-radius: 12px;
        padding: 14px 16px;
        font-size: 12px;
        color: #78350f;
        line-height: 1.5;
        margin-top: 10px;
    }

    .ssl-instruction-title {
        font-weight: 700;
        margin-bottom: 4px;
        display: flex;
        align-items: center;
        gap: 6px;
        text-transform: uppercase;
        font-size: 11px;
        letter-spacing: 0.5px;
    }

    .ssl-flag-link {
        color: #b45309;
        font-weight: 700;
        text-decoration: underline;
        word-break: break-all;
        cursor: pointer;
    }

    .ssl-flag-link:hover {
        color: #451a03;
    }

    /* Right Section - QR Code & Action Button */
    .download-action-section {
        display: flex;
        flex-direction: column;
        align-items: center;
        gap: 16px;
        flex-shrink: 0;
    }

    /* Floating White Card for QR Code */
    .qr-code-card {
        background-color: #ffffff;
        border-radius: 16px;
        padding: 16px;
        display: flex;
        flex-direction: column;
        align-items: center;
        box-shadow: 0 10px 25px -5px rgba(0, 0, 0, 0.1), 0 8px 10px -6px rgba(0, 0, 0, 0.05);
        border: 1px solid rgba(0, 0, 0, 0.05);
    }

    .qr-code-img {
        width: 130px;
        height: 130px;
        object-fit: contain;
        border-radius: 4px;
    }

    .qr-caption {
        margin-top: 12px;
        font-size: 12px;
        font-weight: 600;
        color: #94a3b8;
        text-transform: uppercase;
        letter-spacing: 0.5px;
    }

    /* Download Installer Button */
    .btn-download-installer {
        background-color: #3b82f6;
        color: #ffffff;
        border: none;
        padding: 10px 20px;
        border-radius: 12px;
        font-size: 13px;
        font-weight: 700;
        text-transform: uppercase;
        letter-spacing: 0.5px;
        display: flex;
        align-items: center;
        gap: 8px;
        cursor: pointer;
        transition: background-color 0.2s ease, transform 0.1s ease;
        box-shadow: 0 4px 12px rgba(59, 130, 246, 0.3);
        text-decoration: none;
    }

    .btn-download-installer:hover {
        background-color: #2563eb;
        transform: translateY(-1px);
    }

    .btn-download-installer svg {
        width: 16px;
        height: 16px;
        stroke-width: 2.5;
    }

    /* Responsive Scaling */
    @media (max-width: 768px) {
        .download-card-container {
            flex-direction: column;
            align-items: flex-start;
            padding: 24px;
        }

        .download-action-section {
            width: 100%;
            margin-top: 12px;
        }

        .qr-code-card {
            width: 100%;
            box-sizing: border-box;
        }
    }
</style>

<!-- App Download UserControl Outer Box -->
<div class="download-card-container">

    <!-- Left Column: Information & Details -->
    <div class="download-info-section">
        <div class="download-title-group">
            <h2 class="download-title-main">Download</h2>
            <span class="download-title-sub">BNet Cafe Timer</span>
        </div>

        <p class="download-description">
            Scan the QR code below to install the app instantly on your device.
        </p>

        <ul class="download-features-list">
            <li class="feature-item">
                <span class="check-icon">
                    <svg viewBox="0 0 24 24" fill="none" stroke="currentColor">
                        <polyline points="20 6 9 17 4 12"></polyline>
                    </svg>
                </span>
                <span>Fast Installation</span>
            </li>

            <li class="feature-item">
                <span class="check-icon">
                    <svg viewBox="0 0 24 24" fill="none" stroke="currentColor">
                        <polyline points="20 6 9 17 4 12"></polyline>
                    </svg>
                </span>
                <span>Secure Download</span>
            </li>

            <li class="feature-item">
                <span class="check-icon">
                    <svg viewBox="0 0 24 24" fill="none" stroke="currentColor">
                        <polyline points="20 6 9 17 4 12"></polyline>
                    </svg>
                </span>
                <span>Works on Android and Windows Devices.</span>
            </li>

            <li class="feature-item" style="margin-top: 4px;">
                <span class="check-icon">
                    <svg viewBox="0 0 24 24" fill="none" stroke="currentColor">
                        <polyline points="20 6 9 17 4 12"></polyline>
                    </svg>
                </span>
                <span class="version-badge">Application Version</span>
            </li>
        </ul>

        <!-- SSL / PWA Edge Flag Notice -->
        <div class="ssl-instruction-box">
            <div class="ssl-instruction-title">
                ⚠️ Installation Warning (Local IP / HTTPS)
            </div>
            If the install button shows "Not Applicable", open 
            <a href="chrome://flags/#unsafely-treat-insecure-origin-as-secure" 
               class="ssl-flag-link" 
               onclick="openFlagLink(event, 'chrome://flags/#unsafely-treat-insecure-origin-as-secure');">
               chrome://flags/#unsafely-treat-insecure-origin-as-secure
            </a>, 
            set it to <b>Enabled</b>, and add <code><%= Request.Url.Scheme %>://<%= Request.Url.Authority %></code> to the allowed list.
        </div>
    </div>

    <!-- Right Column: QR Code & Download Button -->
    <div class="download-action-section">
        <!-- Floating QR Box -->
        <div class="qr-code-card">
            <asp:Image ID="imgQrCode" runat="server" CssClass="qr-code-img" AlternateText="QR Code" ImageUrl="~/Images/zion-qr-sample.png" />
            <span class="qr-caption">Scan to Download</span>
        </div>

        <!-- Download Action Button -->
        <asp:HyperLink ID="HyperLink_Url" runat="server" NavigateUrl="~/App/Installation.aspx" CssClass="btn-download-installer">
              <svg viewBox="0 0 24 24" fill="none" stroke="currentColor">
                  <path d="M21 15v4a2 2 0 0 1-2 2H5a2 2 0 0 1-2-2v-4"></path>
                  <polyline points="7 10 12 15 17 10"></polyline>
                  <line x1="12" y1="15" x2="12" y2="3"></line>
              </svg>
              <span>Download Installer</span>
        </asp:HyperLink>
    </div>

</div>

<script type="text/javascript">
    function openFlagLink(e, flagUrl) {
        e.preventDefault();
        try {
            window.open(flagUrl, '_blank');
        } catch (err) {
            navigator.clipboard.writeText(flagUrl);
            alert("Copied link to clipboard: " + flagUrl + "\n\nPlease paste it into your browser's address bar.");
        }
    }
</script>
