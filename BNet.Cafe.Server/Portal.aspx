<%@ Page Language="C#" AutoEventWireup="true" Async="true" EnableEventValidation="false" AsyncTimeout="1000000000" CodeBehind="Portal.aspx.cs" Inherits="BNet.Cafe.Server.Portal" %>

<!DOCTYPE html>
<html lang="en" xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <script>
        (function () {
            var savedTheme = localStorage.getItem('theme') || 'dark';
            document.documentElement.setAttribute('data-theme', savedTheme);
            document.documentElement.classList.add('no-transitions');
        })();
    </script>
    <meta charset="UTF-8" />
    <meta name="viewport" content="width=device-width, initial-scale=1.0, maximum-scale=1.0, user-scalable=no" />
    <title>BNet Cafe - Portal</title>
    <!-- Assets -->
    <link href="Assets/BNetTable/Style.css" rel="stylesheet" />
    <link href="Assets/Pages/Style.css" rel="stylesheet" />
    <link href="Assets/fontawesome/font-awesome.min.css" rel="stylesheet" />
    <style>
        :root {
            --portal-bg: #0d1117;
            --portal-card-bg: rgba(22, 27, 34, 0.75);
            --portal-card-border: rgba(255, 255, 255, 0.08);
            --portal-accent: #6366f1;
            --portal-accent-gradient: linear-gradient(135deg, #6366f1 0%, #a855f7 100%);
            --portal-text-main: #f3f4f6;
            --portal-text-muted: #9ca3af;
        }

        [data-theme="light"] {
            --portal-bg: #f8fafc;
            --portal-card-bg: #ffffff;
            --portal-card-border: #e2e8f0;
            --portal-accent: #4f46e5;
            --portal-accent-gradient: linear-gradient(135deg, #4f46e5 0%, #7c3aed 100%);
            --portal-text-main: #0f172a;
            --portal-text-muted: #64748b;
        }

        * {
            box-sizing: border-box;
        }

        body {
            background-color: var(--portal-bg);
            color: var(--portal-text-main);
            margin: 0;
            padding: 0;
            min-height: 100vh;
            font-family: 'Plus Jakarta Sans', sans-serif;
        }

        .portal-layout {
            max-width: 900px;
            margin: 0 auto;
            padding: 16px;
        }

        /* Clean Topbar */
        .portal-nav {
            display: flex;
            align-items: center;
            justify-content: space-between;
            padding: 14px 20px;
            background: var(--portal-card-bg);
            backdrop-filter: blur(12px);
            -webkit-backdrop-filter: blur(12px);
            border: 1px solid var(--portal-card-border);
            border-radius: 16px;
            margin-bottom: 20px;
            box-shadow: 0 4px 20px rgba(0, 0, 0, 0.05);
        }

        .brand-logo {
            display: flex;
            align-items: center;
            gap: 10px;
            font-weight: 800;
            font-size: 18px;
            letter-spacing: -0.5px;
            color: var(--portal-text-main);
        }

            .brand-logo i {
                color: var(--portal-accent);
                font-size: 20px;
            }

        .nav-actions {
            display: flex;
            align-items: center;
            gap: 12px;
        }

        .btn-theme-toggle {
            background: none;
            border: 1px solid var(--portal-card-border);
            color: var(--portal-text-main);
            width: 38px;
            height: 38px;
            border-radius: 10px;
            cursor: pointer;
            display: flex;
            align-items: center;
            justify-content: center;
            transition: all 0.2s ease;
        }

        .btn-logout-modern {
            background: rgba(239, 68, 68, 0.1);
            color: #ef4444 !important;
            border: 1px solid rgba(239, 68, 68, 0.2);
            padding: 8px 16px;
            border-radius: 10px;
            font-weight: 600;
            font-size: 13px;
            cursor: pointer;
            transition: all 0.2s ease;
        }

            .btn-logout-modern:hover {
                background: #ef4444;
                color: #ffffff !important;
            }

        /* User Profile Hero Section (Main Body) */
        .user-hero-card {
            background: var(--portal-card-bg);
            border: 1px solid var(--portal-card-border);
            border-radius: 20px;
            padding: 24px;
            margin-bottom: 20px;
            display: flex;
            align-items: center;
            gap: 20px;
            box-shadow: 0 10px 30px rgba(0, 0, 0, 0.05);
        }

        .hero-avatar {
            width: 64px;
            height: 64px;
            border-radius: 18px;
            background: var(--portal-accent-gradient);
            color: #ffffff;
            display: flex;
            align-items: center;
            justify-content: center;
            font-weight: 800;
            font-size: 24px;
            box-shadow: 0 4px 15px rgba(99, 102, 241, 0.3);
            flex-shrink: 0;
        }

        .hero-details {
            display: flex;
            flex-direction: column;
            gap: 4px;
            overflow: hidden;
        }

        .hero-name {
            font-size: 20px;
            font-weight: 800;
            color: var(--portal-text-main);
            line-height: 1.2;
            word-break: break-word;
        }

        .hero-email {
            font-size: 13px;
            color: var(--portal-text-muted);
            word-break: break-all;
        }

        .hero-badge {
            display: inline-flex;
            align-items: center;
            gap: 6px;
            font-size: 11px;
            font-weight: 700;
            text-transform: uppercase;
            color: #10b981;
            background: rgba(16, 185, 129, 0.1);
            border: 1px solid rgba(16, 185, 129, 0.2);
            padding: 3px 8px;
            border-radius: 6px;
            width: fit-content;
            margin-top: 4px;
        }

        /* Balance Highlight Card */
        .balance-card {
            background: var(--portal-card-bg);
            border: 1px solid var(--portal-card-border);
            border-radius: 20px;
            padding: 24px;
            margin-bottom: 20px;
            position: relative;
            overflow: hidden;
            box-shadow: 0 10px 30px rgba(0, 0, 0, 0.05);
        }

            .balance-card::before {
                content: '';
                position: absolute;
                top: 0;
                right: 0;
                width: 150px;
                height: 150px;
                background: var(--portal-accent-gradient);
                opacity: 0.12;
                filter: blur(40px);
                border-radius: 50%;
                pointer-events: none;
            }

        .card-label {
            font-size: 12px;
            font-weight: 700;
            text-transform: uppercase;
            letter-spacing: 0.8px;
            color: var(--portal-accent);
            margin-bottom: 8px;
            display: flex;
            align-items: center;
            gap: 8px;
        }

        .balance-value {
            font-size: 36px;
            font-weight: 800;
            letter-spacing: -1px;
            color: var(--portal-text-main);
            margin-bottom: 6px;
        }

        .balance-footer {
            font-size: 13px;
            color: var(--portal-text-muted);
        }

            .balance-footer span {
                color: #10b981;
                font-weight: 700;
            }

        /* Mobile Responsive Adjustments */
        @media (max-width: 600px) {
            .portal-layout {
                padding: 12px;
            }

            .user-hero-card {
                padding: 18px;
                gap: 14px;
            }

            .hero-avatar {
                width: 52px;
                height: 52px;
                font-size: 20px;
                border-radius: 14px;
            }

            .hero-name {
                font-size: 17px;
            }

            .hero-email {
                font-size: 12px;
            }

            .balance-value {
                font-size: 28px;
            }
        }
    </style>
</head>
<body>
    <form id="form1" runat="server">
        <asp:ScriptManager ID="ScriptManager1" EnableCdn="false" EnablePageMethods="true" EnablePartialRendering="true" AsyncPostBackTimeout="99999999" ScriptMode="Release" ValidateRequestMode="Enabled" EnableScriptLocalization="true" EnableScriptGlobalization="true" LoadScriptsBeforeUI="false" CompositeScript-ScriptMode="Release" CompositeScript-ResourceUICultures="Release" runat="server"></asp:ScriptManager>
        <div class="portal-layout">

            <!-- Navbar Header -->
            <header class="portal-nav">
                <div class="brand-logo">
                    <i class="fa fa-cube"></i>
                    <span>BNet Portal</span>
                </div>

                <div class="nav-actions">
                    <button type="button" class="btn-theme-toggle" id="themeToggle" title="Toggle theme">
                        <i class="fa fa-moon"></i>
                    </button>

                    <asp:Button ID="btnLogout" runat="server" Text="Logout" CssClass="btn-logout-modern" OnClick="btnLogout_Click" />
                </div>
            </header>

            <!-- Main Content Area -->
            <main>
                <!-- User Information Section (Main Body) -->
                <div class="user-hero-card">
                    <div class="hero-avatar">
                        <asp:Literal ID="litAvatar" runat="server">U</asp:Literal>
                    </div>
                    <div class="hero-details">
                        <div class="hero-name">
                            <asp:Literal ID="litUserName" runat="server">User</asp:Literal>
                        </div>
                        <div class="hero-email">
                            <asp:Literal ID="litUserEmail" runat="server"></asp:Literal>
                        </div>
                        <div class="hero-badge">
                            <i class="fa fa-circle"></i>Active Account
                        </div>
                    </div>
                </div>

                <!-- Account Balance Hero Card -->
                <div class="balance-card">
                    <div class="card-label">
                        <i class="fa fa-clock"></i>Remaining Time Balance
                    </div>
                    <div class="balance-value">
                        <asp:Literal ID="litFormattedTime" runat="server">0 mins</asp:Literal>
                    </div>
                    <div class="balance-footer">
                        Total minutes available: <span>
                            <asp:Literal ID="litTotalMinutes" runat="server">0</asp:Literal>
                            mins</span>
                    </div>
                </div>

                <asp:UpdatePanel ID="UpdatePanel1" runat="server">
                    <ContentTemplate>
                        <asp:HiddenField ID="HiddenField_UserId" runat="server" />
                        <!-- Transaction Log Table -->
                        <div class="bnet-table-wrapper">
                            <div class="bnet-table-toolbar">
                                <div style="font-size: 16px; font-weight: 700; color: var(--portal-text-main); display: flex; align-items: center; gap: 8px;">
                                    <i class="fa fa-history"></i>Transaction History
                                </div>
                            </div>
                            <div class="bnet-table-container">
                                <asp:GridView ID="GridViewTable" runat="server" ShowHeaderWhenEmpty="true"
                                    AutoGenerateColumns="False" CssClass="bnet-table" GridLines="None"
                                    AllowPaging="True" OnPageIndexChanging="GridViewTable_PageIndexChanging">
                                    <Columns>
                                        <asp:BoundField DataField="DBDateCreated" HeaderText="Date" />
                                        <asp:TemplateField HeaderText="Time">
                                            <ItemTemplate>
                                                <asp:Label ID="Label_DBTimeCreated" runat="server" Text='<%#Eval("DBTimeCreated").ToString() %>'></asp:Label>
                                            </ItemTemplate>
                                        </asp:TemplateField>
                                        <asp:BoundField DataField="DBFormattedDuration" HeaderText="Duration" />
                                        <asp:BoundField DataField="DBAmount" HeaderText="Amount" />
                                        <asp:BoundField DataField="DBDescription" HeaderText="Description" />
                                    </Columns>
                                    <EmptyDataTemplate>
                                        <div class="bnet-table-empty-container">
                                            No balance transactions found.
                                        </div>
                                    </EmptyDataTemplate>
                                </asp:GridView>
                                <asp:Panel ID="Panel_Pagination" runat="server" CssClass="bnet-table-pagination-container" />
                            </div>
                        </div>
                    </ContentTemplate>
                </asp:UpdatePanel>
            </main>
        </div>
    </form>

    <script>
        // Theme Switcher & FontAwesome 
        document.getElementById('themeToggle').addEventListener('click', function () {
            var currentTheme = document.documentElement.getAttribute('data-theme') || 'dark';
            var newTheme = currentTheme === 'dark' ? 'light' : 'dark';

            document.documentElement.setAttribute('data-theme', newTheme);
            localStorage.setItem('theme', newTheme);

            var icon = this.querySelector('i');
            icon.className = newTheme === 'dark' ? 'fa fa-sun' : 'fa fa-moon';
        });

        // Initialize theme icon state on load
        (function () {
            var currentTheme = localStorage.getItem('theme') || 'dark';
            var icon = document.querySelector('#themeToggle i');
            if (icon) {
                icon.className = currentTheme === 'dark' ? 'fa fa-sun' : 'fa fa-moon';
            }
        })();
    </script>
</body>
</html>
