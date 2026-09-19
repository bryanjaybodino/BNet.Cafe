<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="Login.aspx.cs" Inherits="BNet.Cafe.Server.Login" %>

<!DOCTYPE html>

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title>Sign In - BNet Cafe</title>
    <meta name="viewport" content="width=device-width, initial-scale=1.0" />
    <style>
        * {
            box-sizing: border-box;
        }

        html, body {
            height: 100%;
            margin: 0;
            padding: 0;
        }

        body {
            font-family: -apple-system, BlinkMacSystemFont, "Segoe UI", Roboto, Helvetica, Arial, sans-serif;
            background-color: #f4f6f8;
            display: flex;
            align-items: center;
            justify-content: center;
            padding: 16px;
        }

        #form1 {
            width: 100%;
            display: flex;
            justify-content: center;
            align-items: center;
        }

        .login-card {
            background: #ffffff;
            padding: 40px 32px;
            border-radius: 12px;
            box-shadow: 0 4px 20px rgba(0, 0, 0, 0.08);
            text-align: center;
            width: 100%;
            max-width: 400px;
        }

            .login-card h2 {
                margin: 0 0 12px 0;
                color: #1a1a1a;
                font-size: 24px;
                font-weight: 600;
            }

            .login-card p {
                color: #666666;
                font-size: 14px;
                line-height: 1.5;
                margin: 0 0 28px 0;
            }

        .btn-google {
            display: flex;
            align-items: center;
            justify-content: center;
            width: 100%;
            background-color: #ffffff;
            color: #3c4043;
            border: 1px solid #dadce0;
            border-radius: 6px;
            padding: 12px 16px;
            font-size: 15px;
            font-weight: 500;
            text-decoration: none;
            cursor: pointer;
            transition: background-color 0.2s, box-shadow 0.2s, border-color 0.2s;
        }

            .btn-google:hover {
                background-color: #f8f9fa;
                border-color: #c6c9ce;
                box-shadow: 0 1px 3px rgba(60, 64, 67, 0.15);
            }

            .btn-google svg {
                margin-right: 12px;
                width: 20px;
                height: 20px;
                flex-shrink: 0;
            }

        /* Mobile Optimization */
        @media (max-width: 480px) {
            .login-card {
                padding: 28px 20px;
                border-radius: 8px;
            }

                .login-card h2 {
                    font-size: 20px;
                }

                .login-card p {
                    font-size: 13px;
                    margin-bottom: 20px;
                }

            .btn-google {
                font-size: 14px;
                padding: 10px 14px;
            }
        }
    </style>
</head>
<body>
    <form id="form1" runat="server">
        <div class="login-card">

            <h2>Welcome Back</h2>
            <p>Please sign in with your Google account to continue.</p>
            <asp:LinkButton ID="btnGoogleSignIn" runat="server" OnClick="btnGoogleSignIn_Click" CssClass="btn-google">
                    <svg viewBox="0 0 48 48">
                        <path fill="#EA4335" d="M24 9.5c3.54 0 6.71 1.22 9.21 3.6l6.85-6.85C35.9 2.38 30.47 0 24 0 14.62 0 6.51 5.38 2.56 13.22l7.98 6.19C12.43 13.72 17.74 9.5 24 9.5z"/>
                        <path fill="#4285F4" d="M46.98 24.55c0-1.57-.15-3.09-.38-4.55H24v9.02h12.94c-.58 2.96-2.26 5.48-4.78 7.18l7.73 6c4.51-4.18 7.09-10.36 7.09-17.65z"/>
                        <path fill="#FBBC05" d="M10.53 28.59c-.48-1.45-.76-2.99-.76-4.59s.27-3.14.76-4.59l-7.98-6.19C.92 16.46 0 20.12 0 24c0 3.88.92 7.54 2.56 10.78l7.97-6.19z"/>
                        <path fill="#34A853" d="M24 48c6.48 0 11.93-2.13 15.89-5.81l-7.73-6c-2.15 1.45-4.92 2.3-8.16 2.3-6.26 0-11.57-4.22-13.47-9.91l-7.98 6.19C6.51 42.62 14.62 48 24 48z"/>
                    </svg>
                    Sign in with Google
            </asp:LinkButton>

        </div>
    </form>
</body>
</html>