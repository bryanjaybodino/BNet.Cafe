<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="Login.aspx.cs" Inherits="BNet.Cafe.Server.Login" ClientIDMode="AutoID" EnableViewStateMac="true" ViewStateEncryptionMode="Always" ValidateRequest="true" Async="true" AsyncTimeout="999999999" %>

<!DOCTYPE html>
<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title>Sign In - BNet Cafe</title>
    <meta name="viewport" content="width=device-width, initial-scale=1.0" />
    <% Response.Write(BNet.Cafe.Server.Services.FileCssHelper.StyleSheetVersion("Assets/Login/Style.css")); %>
</head>
<body>
    <div class="page">

        <!-- Brand Panel -->
        <div class="brand-panel">
            <div class="brand-logo">
                <svg width="28" height="28" viewBox="0 0 24 24" fill="none" xmlns="http://www.w3.org/2000/svg">
                    <path d="M21 15C21 15.5304 20.7893 16.0391 20.4142 16.4142C20.0391 16.7893 19.5304 17 19 17H7L3 21V5C3 4.46957 3.21071 3.96086 3.58579 3.58579C3.96086 3.21071 4.46957 3 5 3H19C19.5304 3 20.0391 3.21071 20.4142 3.58579C20.7893 3.96086 21 4.46957 21 5V15Z" stroke="white" stroke-width="2" stroke-linecap="round" stroke-linejoin="round" />
                    <circle cx="8.5" cy="10" r="1" fill="white" />
                    <circle cx="12" cy="10" r="1" fill="white" />
                    <circle cx="15.5" cy="10" r="1" fill="white" />
                </svg>
            </div>
            <h1 class="brand-headline">BNet Cafe Management</h1>
            <p class="brand-sub">Monitor workstations, manage sessions, and view analytics in real time.</p>
            <div class="brand-stats">
                <div class="stat">
                    <div class="stat-num">100%</div>
                    <div class="stat-lbl">Free Software</div>
                </div>
                <div class="stat-divider"></div>
                <div class="stat">
                    <div class="stat-num">Realtime</div>
                    <div class="stat-lbl">Sync</div>
                </div>
            </div>
        </div>

        <!-- Form Panel -->
        <div class="form-panel">
            <div class="form-box">

                <p class="form-eyebrow">Management Portal</p>
                <h2 class="form-heading">Welcome back</h2>
                <p class="form-desc">Sign in to access your administrative dashboard.</p>

                <form id="form1" runat="server">
                    <asp:ScriptManager ID="ScriptManager1" EnableCdn="false" EnablePageMethods="true" EnablePartialRendering="true" AsyncPostBackTimeout="99999999" ScriptMode="Release" ValidateRequestMode="Enabled" EnableScriptLocalization="true" EnableScriptGlobalization="true" LoadScriptsBeforeUI="false" CompositeScript-ScriptMode="Release" CompositeScript-ResourceUICultures="Release" runat="server"></asp:ScriptManager>
                    <asp:UpdatePanel ID="UpdatePanel1" runat="server" UpdateMode="Conditional">
                        <ContentTemplate>

                            <!-- Single Error Notification Box -->
                            <div id="errorBox" class="error-box" style="display: none;" role="alert">
                                <svg viewBox="0 0 24 24" width="16" height="16">
                                    <path d="M1 21h22L12 2 1 21zM13 18h-2v-2h2v2zm0-4h-2v-4h2v4z" />
                                </svg>
                                <span id="errorMsg"></span>
                            </div>

                            <!-- Email Input -->
                            <div class="field">
                                <label for="TextBox_Email">Username or Email</label>
                                <div class="input-wrap">
                                    <span class="field-icon">
                                        <svg width="16" height="16" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round">
                                            <rect x="2" y="4" width="20" height="16" rx="2" />
                                            <path d="m22 7-8.97 5.7a1.94 1.94 0 0 1-2.06 0L2 7" />
                                        </svg>
                                    </span>
                                    <asp:TextBox ID="TextBox_Email" runat="server" CssClass="field-input" placeholder="Enter your username or email" autocomplete="username" />
                                </div>
                            </div>

                            <!-- Password Input -->
                            <div class="field">
                                <label for="TextBox_Password">Password</label>
                                <div class="input-wrap">
                                    <span class="field-icon">
                                        <svg width="16" height="16" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round">
                                            <rect x="3" y="11" width="18" height="11" rx="2" />
                                            <path d="M7 11V7a5 5 0 0 1 10 0v4" />
                                        </svg>
                                    </span>
                                    <asp:TextBox ID="TextBox_Password" runat="server" TextMode="SingleLine" CssClass="field-input" autocomplete="current-password" />
                                    <button type="button" class="pw-toggle" id="pwToggleBtn" onclick="togglePw()" title="Show / hide password">
                                        <svg id="pwIconHide" width="18" height="18" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round">
                                            <path d="M17.94 17.94A10.07 10.07 0 0 1 12 20c-7 0-11-8-11-8a18.45 18.45 0 0 1 5.06-5.94" />
                                            <path d="M9.9 4.24A9.12 9.12 0 0 1 12 4c7 0 11 8 11 8a18.5 18.5 0 0 1-2.16 3.19" />
                                            <line x1="1" y1="1" x2="23" y2="23" />
                                        </svg>
                                        <svg id="pwIconShow" width="18" height="18" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round" style="display: none;">
                                            <path d="M1 12s4-8 11-8 11 8 11 8-4 8-11 8-11-8-11-8z" />
                                            <circle cx="12" cy="12" r="3" />
                                        </svg>
                                    </button>
                                </div>
                            </div>

                            <div class="options">
                                <a href="ForgotPassword.aspx" style="display: none" class="forgot">Forgot password?</a>
                            </div>

                            <!-- Standard Submit Button -->
                            <asp:LinkButton ID="LinkButton_Login" runat="server" CssClass="submit-btn" OnClick="LinkButton_Login_Click" OnClientClick="return handleSubmit()">
                                <span class="btn-spinner"></span>
                                <span class="btn-label">Sign In</span>
                            </asp:LinkButton>

                            <div class="divider">
                                <span>OR</span>
                            </div>

                            <!-- Google OAuth Button -->
                            <asp:LinkButton ID="btnGoogleSignIn" runat="server" OnClick="btnGoogleSignIn_Click" CssClass="btn-google">
                                <svg viewBox="0 0 48 48">
                                    <path fill="#EA4335" d="M24 9.5c3.54 0 6.71 1.22 9.21 3.6l6.85-6.85C35.9 2.38 30.47 0 24 0 14.62 0 6.51 5.38 2.56 13.22l7.98 6.19C12.43 13.72 17.74 9.5 24 9.5z"/>
                                    <path fill="#4285F4" d="M46.98 24.55c0-1.57-.15-3.09-.38-4.55H24v9.02h12.94c-.58 2.96-2.26 5.48-4.78 7.18l7.73 6c4.51-4.18 7.09-10.36 7.09-17.65z"/>
                                    <path fill="#FBBC05" d="M10.53 28.59c-.48-1.45-.76-2.99-.76-4.59s.27-3.14.76-4.59l-7.98-6.19C.92 16.46 0 20.12 0 24c0 3.88.92 7.54 2.56 10.78l7.97-6.19z"/>
                                    <path fill="#34A853" d="M24 48c6.48 0 11.93-2.13 15.89-5.81l-7.73-6c-2.15 1.45-4.92 2.3-8.16 2.3-6.26 0-11.57-4.22-13.47-9.91l-7.98 6.19C6.51 42.62 14.62 48 24 48z"/>
                                </svg>
                                Sign in with Google
                            </asp:LinkButton>

                        </ContentTemplate>
                    </asp:UpdatePanel>
                </form>

                <div class="form-footer">
                    &copy; <%= DateTime.Now.Year %> BNet Cafe &middot; All rights reserved
                    <br />
                    <span class="status-pill"><span class="status-dot"></span>All systems operational</span>
                </div>

            </div>
        </div>
    </div>
</body>
</html>
