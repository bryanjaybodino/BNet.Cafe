<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="Installation.aspx.cs" Inherits="BNet.Cafe.Server.App.Installation" %>

<!DOCTYPE html>

<html lang="en">
<head runat="server">
    <meta charset="utf-8" />
    <title>Install App</title>
    <link rel="manifest" href="manifest.json">
    <meta name="viewport" content="width=device-width, initial-scale=1">
    <meta name="theme-color" content="#353535">

    <style>
        * {
            box-sizing: border-box;
            margin: 0;
            padding: 0;
        }

        html, body {
            height: 100%;
            font-family: -apple-system, BlinkMacSystemFont, "Segoe UI", Roboto, Oxygen, Ubuntu, Cantarell, sans-serif;
        }

        body {
            background: linear-gradient(135deg, #f0f4ff 0%, #e8ecf8 100%);
            display: flex;
            align-items: center;
            justify-content: center;
            min-height: 100vh;
            padding: 1rem;
        }

        .install-card {
            background: #ffffff;
            border-radius: 20px;
            box-shadow: 0 8px 40px rgba(0, 0, 0, 0.12);
            padding: 2.5rem 2rem;
            max-width: 420px;
            width: 100%;
            text-align: center;
        }

        .app-icon-wrapper {
            width: 90px;
            height: 90px;
            border-radius: 22px;
            background: #f3f4f6;
            display: inline-flex;
            align-items: center;
            justify-content: center;
            margin-bottom: 1.25rem;
            box-shadow: 0 2px 12px rgba(0, 0, 0, 0.08);
            overflow: hidden;
        }

            .app-icon-wrapper img {
                width: 56px;
                height: auto;
                object-fit: contain;
            }

        .install-title {
            font-size: 1.4rem;
            font-weight: 700;
            color: #1a1a2e;
            margin-bottom: 0.4rem;
        }

        .install-subtitle {
            font-size: 0.92rem;
            color: #6b7280;
            margin-bottom: 1.5rem;
            line-height: 1.4;
        }

        #message {
            font-size: 0.95rem;
            font-weight: 600;
            color: #3b5bdb;
            margin-bottom: 1.25rem;
            min-height: 1.4em;
        }

        /* Pure CSS Primary Button */
        .btn-install {
            background-color: #3b82f6;
            color: #ffffff;
            border: none;
            border-radius: 12px;
            padding: 0.85rem 1.5rem;
            font-weight: 600;
            font-size: 1rem;
            letter-spacing: 0.01em;
            cursor: pointer;
            width: 100%;
            margin-bottom: 1rem;
            display: inline-flex;
            align-items: center;
            justify-content: center;
            gap: 8px;
            box-shadow: 0 4px 12px rgba(59, 130, 246, 0.3);
            transition: background-color 0.2s ease, transform 0.1s ease, box-shadow 0.2s ease;
            user-select: none;
            text-decoration: none;
        }

            .btn-install:hover {
                background-color: #2563eb;
                box-shadow: 0 6px 16px rgba(37, 99, 235, 0.4);
            }

            .btn-install:active {
                transform: scale(0.97);
                background-color: #1d4ed8;
            }

        .btn-icon {
            width: 18px;
            height: 18px;
            stroke-width: 2.2;
            flex-shrink: 0;
        }

        /* Info Alert Callout Box */
        .info-alert {
            background: #eff6ff;
            border: 1px solid #bfdbfe;
            border-radius: 10px;
            color: #1e40af;
            font-size: 0.82rem;
            padding: 0.65rem 0.9rem;
            display: flex;
            align-items: flex-start;
            gap: 0.6rem;
            text-align: left;
            line-height: 1.4;
        }

        .info-icon {
            width: 16px;
            height: 16px;
            flex-shrink: 0;
            margin-top: 2px;
            color: #2563eb;
        }

        /* Utility Class for Hiding Elements */
        .d-none {
            display: none !important;
        }

        /* Spinner for loading state */
        .btn-spinner {
            width: 16px;
            height: 16px;
            border: 2px solid rgba(255, 255, 255, 0.4);
            border-top-color: #ffffff;
            border-radius: 50%;
            animation: btn-spin 0.8s linear infinite;
            display: inline-block;
            vertical-align: middle;
        }

        @keyframes btn-spin {
            to {
                transform: rotate(360deg);
            }
        }
    </style>
</head>

<body>
    <%-- Hidden control for server-side navigation --%>
    <asp:HyperLink ID="HyperLink_Url" Visible="false" NavigateUrl="~/Login.aspx" runat="server"></asp:HyperLink>

    <form id="form1" runat="server" autocomplete="off" style="display: contents;">
        <asp:ScriptManager ID="ScriptManager1" EnableCdn="false" EnablePartialRendering="true"
            AsyncPostBackTimeout="99999999" ScriptMode="Release" ValidateRequestMode="Enabled"
            EnableScriptLocalization="true" EnableScriptGlobalization="true"
            LoadScriptsBeforeUI="false" CompositeScript-ScriptMode="Release"
            CompositeScript-ResourceUICultures="Release" runat="server">
        </asp:ScriptManager>

        <div class="install-card">
            <!-- App Icon -->
            <div class="app-icon-wrapper">
                <asp:Image ID="Image_Icon" Width="56" runat="server" AlternateText="App Icon" />
            </div>

            <!-- Title & Description -->
            <div class="install-title">Install BNet Cafe Timer</div>
            <div class="install-subtitle">Add the app to your home screen for quick access.</div>

            <!-- Status Message -->
            <p id="message">Checking device support&hellip;</p>

            <!-- Install Button — type="button" via HTML span / button to prevent postback -->
            <button id="installBtn" type="button" class="btn-install d-none">
                <svg class="btn-icon" viewBox="0 0 24 24" fill="none" stroke="currentColor">
                    <path d="M21 15v4a2 2 0 0 1-2 2H5a2 2 0 0 1-2-2v-4"></path>
                    <polyline points="7 10 12 15 17 10"></polyline>
                    <line x1="12" y1="15" x2="12" y2="3"></line>
                </svg>
                <span>Install App</span>
            </button>

            <!-- Info Note -->
            <div class="info-alert">
                <svg class="info-icon" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2">
                    <circle cx="12" cy="12" r="10"></circle>
                    <line x1="12" y1="16" x2="12" y2="12"></line>
                    <line x1="12" y1="8" x2="12.01" y2="8"></line>
                </svg>
                <span>If the install button does not appear, make sure your browser is using a secure (HTTPS) connection.</span>
            </div>
        </div>
    </form>
</body>
</html>
