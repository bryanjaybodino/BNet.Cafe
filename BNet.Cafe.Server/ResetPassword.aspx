<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="ResetPassword.aspx.cs" Inherits="BNet.Cafe.Server.ResetPassword" %>

<!DOCTYPE html>
<html lang="en">
<head runat="server">
    <meta charset="utf-8" />
    <meta name="viewport" content="width=device-width, initial-scale=1.0" />
    <link href="Assets/fontawesome/font-awesome.min.css" rel="stylesheet" />
    <title>Reset Password</title>



    <style>
        /* ==========================================================================
           1. RESET & BASE STYLES
           ========================================================================== */
        *, *::before, *::after {
            box-sizing: border-box;
            margin: 0;
            padding: 0;
        }

        body {
            font-family: 'Inter', -apple-system, BlinkMacSystemFont, "Segoe UI", Roboto, sans-serif;
            background-color: #f8fafc;
            color: #0f172a;
            display: flex;
            align-items: center;
            justify-content: center;
            min-height: 100vh;
            padding: 24px;
        }

        /* ==========================================================================
           2. FORM CONTAINER & CARD (EXPANDED SIZE)
           ========================================================================== */
        .reset-card {
            background: #ffffff;
            width: 100%;
            max-width: 480px; /* Increased from 420px */
            padding: 40px; /* Increased padding from 32px */
            border-radius: 20px;
            box-shadow: 0 10px 25px -5px rgba(0, 0, 0, 0.05), 0 8px 10px -6px rgba(0, 0, 0, 0.01);
            border: 1px solid #e2e8f0;
        }

        .card-header {
            text-align: center;
            margin-bottom: 32px;
        }

        .icon-badge {
            width: 56px;
            height: 56px;
            background: #eff6ff;
            color: #2563eb;
            border-radius: 14px;
            display: inline-flex;
            align-items: center;
            justify-content: center;
            font-size: 24px;
            margin-bottom: 18px;
        }

        .card-header h1 {
            font-size: 24px;
            font-weight: 700;
            color: #0f172a;
            margin-bottom: 8px;
        }

        .card-header p {
            font-size: 15px;
            color: #64748b;
        }

        /* ==========================================================================
           3. FORM INPUTS & LABELS
           ========================================================================== */
        .form-group {
            margin-bottom: 22px;
        }

            .form-group label {
                display: block;
                font-size: 14px;
                font-weight: 600;
                color: #334155;
                margin-bottom: 8px;
            }

        .required-star {
            color: #ef4444;
        }

        .form-control {
            width: 100%;
            padding: 14px 18px;
            font-size: 15px;
            font-family: inherit;
            color: #0f172a;
            background-color: #ffffff;
            border: 1.5px solid #cbd5e1;
            border-radius: 12px;
            outline: none;
            transition: border-color 0.2s ease, box-shadow 0.2s ease;
        }

            .form-control:focus {
                border-color: #2563eb;
                box-shadow: 0 0 0 4px rgba(37, 99, 235, 0.1);
            }

            .form-control.is-invalid {
                border-color: #ef4444 !important;
                background-color: #fef2f2;
            }

            .form-control.is-valid {
                border-color: #10b981 !important;
                background-color: #ecfdf5;
            }

        /* ==========================================================================
           4. BUTTON STYLES
           ========================================================================== */
        .btn-submit {
            width: 100%;
            padding: 14px 24px;
            font-size: 15px;
            font-weight: 600;
            color: #ffffff;
            background-color: #2563eb;
            border: none;
            border-radius: 12px;
            cursor: pointer;
            display: inline-flex;
            align-items: center;
            justify-content: center;
            gap: 10px;
            text-decoration: none;
            transition: background-color 0.2s ease, transform 0.1s ease;
            margin-top: 10px;
        }

            .btn-submit:hover {
                background-color: #1d4ed8;
            }

            .btn-submit:active {
                transform: scale(0.99);
            }

            .btn-submit.disabled {
                opacity: 0.7;
                pointer-events: none;
                cursor: not-allowed;
            }

        /* ==========================================================================
           5. MODERN TOAST NOTIFICATION STYLES
           ========================================================================== */
        .toast-wrapper {
            position: fixed;
            top: 24px;
            right: 24px;
            z-index: 10000;
            display: flex;
            flex-direction: column;
            gap: 12px;
            pointer-events: none;
        }

        .toast-card {
            pointer-events: auto;
            min-width: 320px;
            max-width: 400px;
            padding: 16px 18px;
            background: #ffffff;
            border-radius: 12px;
            box-shadow: 0 20px 25px -5px rgba(0, 0, 0, 0.1), 0 8px 10px -6px rgba(0, 0, 0, 0.05);
            display: flex;
            align-items: flex-start;
            gap: 12px;
            border: 1px solid #f1f5f9;
            animation: slideIn 0.3s cubic-bezier(0.16, 1, 0.3, 1) forwards;
            transition: all 0.25s ease;
        }

            .toast-card.toast-success {
                border-left: 4px solid #10b981;
            }

                .toast-card.toast-success .toast-icon {
                    color: #10b981;
                    background: #ecfdf5;
                }

            .toast-card.toast-error {
                border-left: 4px solid #ef4444;
            }

                .toast-card.toast-error .toast-icon {
                    color: #ef4444;
                    background: #fef2f2;
                }

        .toast-icon {
            width: 32px;
            height: 32px;
            border-radius: 8px;
            display: flex;
            align-items: center;
            justify-content: center;
            font-size: 16px;
            flex-shrink: 0;
        }

        .toast-body {
            flex: 1;
            font-size: 14px;
            color: #334155;
            line-height: 1.5;
            padding-top: 3px;
        }

        .toast-close-btn {
            background: none;
            border: none;
            color: #94a3b8;
            font-size: 18px;
            cursor: pointer;
            padding: 2px;
            line-height: 1;
        }

            .toast-close-btn:hover {
                color: #475569;
            }

        .toast-fade-out {
            opacity: 0;
            transform: translateX(30px);
        }

        @keyframes slideIn {
            from {
                opacity: 0;
                transform: translateX(100%);
            }

            to {
                opacity: 1;
                transform: translateX(0);
            }
        }


        /* Body reset */
        body {
            font-family: 'Inter', -apple-system, BlinkMacSystemFont, "Segoe UI", Roboto, sans-serif;
            background-color: #f8fafc;
            color: #0f172a;
            min-height: 100vh;
            margin: 0;
            padding: 24px;
            display: flex;
            align-items: center;
            justify-content: center;
        }

        /* ASP.NET Form spans full width */
        form#form1 {
            width: 100%;
            display: flex;
            justify-content: center;
        }

        /* Force ASP.NET UpdatePanel wrapper to center its inner content */
        #UpdatePanel1 {
            width: 100%;
            display: flex;
            justify-content: center;
            align-items: center;
        }

        /* Now you can freely adjust max-width to whatever size you want (e.g., 600px, 700px, 800px) */
        .reset-card {
            background: #ffffff;
            width: 100%;
            max-width: 500px; /* Adjust this value to set your desired card width */
            padding: 40px;
            border-radius: 20px;
            box-shadow: 0 10px 25px -5px rgba(0, 0, 0, 0.05), 0 8px 10px -6px rgba(0, 0, 0, 0.01);
            border: 1px solid #e2e8f0;
        }
    </style>

    <script>
        function ShowAlert(message, type) {
            var wrapper = document.getElementById('toastWrapper');
            if (!wrapper) {
                wrapper = document.createElement('div');
                wrapper.id = 'toastWrapper';
                wrapper.className = 'toast-wrapper';
                document.body.appendChild(wrapper);
            }

            var messages = Array.isArray(message) ? message : [message];

            messages.forEach(function (msg) {
                var toast = document.createElement('div');
                toast.className = 'toast-card toast-' + (type || 'error');

                var iconClass = type === 'success' ? 'fa-check' : 'fa-xmark';

                toast.innerHTML =
                    '<div class="toast-icon"><i class="fa-solid ' + iconClass + '"></i></div>' +
                    '<div class="toast-body">' + msg + '</div>' +
                    '<button type="button" class="toast-close-btn" onclick="dismissToast(this.parentElement)">&times;</button>';

                wrapper.appendChild(toast);

                setTimeout(function () {
                    dismissToast(toast);
                }, 4000);
            });

            resetSubmitButton();
        }

        function dismissToast(toast) {
            if (!toast) return;
            toast.classList.add('toast-fade-out');
            setTimeout(function () {
                if (toast.parentElement) {
                    toast.parentElement.removeChild(toast);
                }
            }, 250);
        }

        function Validate() {
            var passwordInput = document.querySelector('[id$="TextBox_Password"]');
            var confirmInput = document.querySelector('[id$="TextBox_ConfirmPassword"]');

            var errors = [];

            if (passwordInput) passwordInput.classList.remove('is-invalid', 'is-valid');
            if (confirmInput) confirmInput.classList.remove('is-invalid', 'is-valid');

            if (!passwordInput || !passwordInput.value.trim()) {
                if (passwordInput) passwordInput.classList.add('is-invalid');
                errors.push('Please enter a new password.');
            } else {
                passwordInput.classList.add('is-valid');
            }

            if (!confirmInput || !confirmInput.value.trim()) {
                if (confirmInput) confirmInput.classList.add('is-invalid');
                errors.push('Please confirm your new password.');
            } else if (passwordInput && passwordInput.value !== confirmInput.value) {
                confirmInput.classList.add('is-invalid');
                errors.push('Passwords do not match.');
            } else {
                confirmInput.classList.add('is-valid');
            }

            if (errors.length > 0) {
                ShowAlert(errors, 'error');
                return false;
            }

            disableSubmitButton();
            return true;
        }

        function disableSubmitButton() {
            var btn = document.querySelector('[id$="LinkButton_Submit"]');
            if (btn) {
                btn.classList.add('disabled');
                btn.innerHTML = '<i class="fa-solid fa-spinner fa-spin"></i> Resetting...';
            }
        }

        function resetSubmitButton() {
            var btn = document.querySelector('[id$="LinkButton_Submit"]');
            if (btn) {
                btn.classList.remove('disabled');
                btn.innerHTML = '<i class="fa-solid fa-floppy-disk"></i> Change Password';
            }
        }
    </script>
</head>
<body>
    <div id="toastWrapper" class="toast-wrapper"></div>

    <form id="form1" runat="server">
        <asp:ScriptManager ID="ScriptManager1" runat="server" />
        <asp:UpdatePanel ID="UpdatePanel1" runat="server">
            <ContentTemplate>
                <div class="reset-card">
                    <div class="card-header">
                        <div class="icon-badge">
                            <i class="fa-solid fa-key"></i>
                        </div>
                        <h1>Reset Password</h1>
                        <p>Enter your new password below.</p>
                    </div>

                    <div class="form-group">
                        <label for="<%= TextBox_Password.ClientID %>">New Password <span class="required-star">*</span></label>
                        <asp:TextBox ID="TextBox_Password" runat="server" TextMode="Password" CssClass="form-control" placeholder="Enter new password" MaxLength="100"></asp:TextBox>
                    </div>

                    <div class="form-group">
                        <label for="<%= TextBox_ConfirmPassword.ClientID %>">Confirm Password <span class="required-star">*</span></label>
                        <asp:TextBox ID="TextBox_ConfirmPassword" runat="server" TextMode="Password" CssClass="form-control" placeholder="Confirm new password" MaxLength="100"></asp:TextBox>
                    </div>

                    <asp:LinkButton ID="LinkButton_Submit" OnClick="LinkButton_Submit_Click" CssClass="btn-submit" OnClientClick="return Validate();" runat="server">
                        <i class="fa-solid fa-floppy-disk"></i> Change Password
                    </asp:LinkButton>
                </div>
            </ContentTemplate>
        </asp:UpdatePanel>
    </form>
</body>
</html>
