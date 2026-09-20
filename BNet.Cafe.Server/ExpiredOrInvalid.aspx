<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="ExpiredOrInvalid.aspx.cs" Inherits="BNet.Cafe.Server.ExpiredOrInvalid" %>

<!DOCTYPE html>
<html lang="en">
<head runat="server">
    <meta charset="utf-8" />
    <meta name="viewport" content="width=device-width, initial-scale=1.0" />
    <link href="Assets/fontawesome/font-awesome.min.css" rel="stylesheet" />
    <title>Link Expired or Invalid</title>

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

        /* ==========================================================================
           2. CARD CONTAINER
           ========================================================================== */
        .status-card {
            background: #ffffff;
            width: 100%;
            max-width: 500px;
            padding: 40px;
            border-radius: 20px;
            box-shadow: 0 10px 25px -5px rgba(0, 0, 0, 0.05), 0 8px 10px -6px rgba(0, 0, 0, 0.01);
            border: 1px solid #e2e8f0;
            text-align: center;
        }

        .icon-badge-warning {
            width: 64px;
            height: 64px;
            background: #fffbebf5;
            color: #d97706;
            border: 1px solid #fef3c7;
            border-radius: 16px;
            display: inline-flex;
            align-items: center;
            justify-content: center;
            font-size: 28px;
            margin-bottom: 20px;
        }

        .card-header h1 {
            font-size: 24px;
            font-weight: 700;
            color: #0f172a;
            margin-bottom: 10px;
        }

        .card-header p {
            font-size: 15px;
            color: #64748b;
            line-height: 1.6;
            margin-bottom: 28px;
        }

        /* ==========================================================================
           3. BUTTON & ACTION STYLES
           ========================================================================== */
        .btn-action {
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
            margin-bottom: 12px;
        }

            .btn-action:hover {
                background-color: #1d4ed8;
            }

            .btn-action:active {
                transform: scale(0.99);
            }

        .btn-secondary-action {
            width: 100%;
            padding: 14px 24px;
            font-size: 15px;
            font-weight: 600;
            color: #475569;
            background-color: #f1f5f9;
            border: 1px solid #e2e8f0;
            border-radius: 12px;
            cursor: pointer;
            display: inline-flex;
            align-items: center;
            justify-content: center;
            gap: 10px;
            text-decoration: none;
            transition: background-color 0.2s ease, color 0.2s ease;
        }

            .btn-secondary-action:hover {
                background-color: #e2e8f0;
                color: #0f172a;
            }
    </style>
</head>
<body>
    <form id="form1" runat="server">
        <div class="status-card">
            <div class="icon-badge-warning">
                <i class="fa-solid fa-triangle-exclamation"></i>
            </div>
            
            <div class="card-header">
                <h1>Link Expired or Invalid</h1>
                <p>The password reset link you clicked is no longer valid or has expired. Please request a new link to reset your password.</p>
            </div>


            <asp:HyperLink ID="HyperLink_Login" runat="server" NavigateUrl="~/Login.aspx" CssClass="btn-secondary-action">
                <i class="fa-solid fa-arrow-left"></i> Return to Login
            </asp:HyperLink>
        </div>
    </form>
</body>
</html>