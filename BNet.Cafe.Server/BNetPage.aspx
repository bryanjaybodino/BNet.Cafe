<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="BNetPage.aspx.cs" Inherits="BNet.Cafe.Server.BNetPage" %>

<!DOCTYPE html>
<html lang="en">
<head>
    <meta charset="UTF-8">
    <meta name="viewport" content="width=device-width, initial-scale=1.0">
    <title>BNet Cafe</title>
    <link rel="stylesheet" href="https://cdnjs.cloudflare.com/ajax/libs/font-awesome/6.4.0/css/all.min.css">
    <style>
        * {
            margin: 0;
            padding: 0;
            box-sizing: border-box;
        }

        :root {
            --primary: #3b82f6;
            --primary-dark: #1e40af;
            --secondary: #8b5cf6;
            --bg-light: #f8fafc;
            --bg-light-secondary: #ffffff;
            --bg-light-tertiary: #f1f5f9;
            --text-light: #1e293b;
            --text-light-secondary: #64748b;
            --border-light: #e2e8f0;
            --sidebar-light: #ffffff;
        }

        [data-theme="dark"] {
            --primary: #60a5fa;
            --primary-dark: #3b82f6;
            --secondary: #a78bfa;
            --bg-light: #111111;
            --bg-light-secondary: #151515;
            --bg-light-tertiary: #20201F;
            --text-light: #f1f5f9;
            --text-light-secondary: #cbd5e1;
            --border-light: #2a2a2a;
            --sidebar-light: #151515;
        }

        body {
            font-family: -apple-system, BlinkMacSystemFont, 'Segoe UI', Roboto, Oxygen, Ubuntu, Cantarell, sans-serif;
            background-color: var(--bg-light);
            color: var(--text-light);
            transition: none;
        }

        .container {
            display: flex;
            height: 100vh;
            overflow: hidden;
            position: relative;
        }

            .container::before {
                content: '';
                position: fixed;
                top: 0;
                left: 0;
                right: 0;
                bottom: 0;
                background-color: rgba(0, 0, 0, 0.5);
                backdrop-filter: blur(4px);
                opacity: 0;
                pointer-events: none;
                transition: none;
                z-index: 99;
            }

            .container.sidebar-open::before {
                opacity: 1;
                pointer-events: auto;
            }

        /* Sidebar */
        .sidebar {
            width: 260px;
            background-color: var(--sidebar-light);
            border-right: 1px solid var(--border-light);
            padding: 24px 0;
            overflow-y: auto;
            transition: none;
            position: fixed;
            height: 100vh;
            left: 0;
            z-index: 100;
        }

            .sidebar.collapsed {
                width: 80px;
            }

            .sidebar.mobile-hidden {
                left: -260px;
            }

        .logo {
            padding: 0 20px 30px;
            display: flex;
            align-items: center;
            gap: 12px;
            font-size: 20px;
            font-weight: 700;
            color: var(--primary);
        }

        .sidebar.collapsed .logo span {
            display: none;
        }

        .sidebar-menu {
            list-style: none;
        }

            .sidebar-menu li {
                margin: 8px 0;
                padding: 0 12px;
            }

            .sidebar-menu a {
                display: flex;
                align-items: center;
                gap: 12px;
                padding: 12px 16px;
                text-decoration: none;
                color: var(--text-light-secondary);
                border-radius: 8px;
                transition: none;
                font-size: 14px;
            }

                .sidebar-menu a:hover {
                    background-color: var(--bg-light-tertiary);
                    color: var(--primary);
                }

                .sidebar-menu a.active {
                    background-color: var(--primary);
                    color: white;
                }

            .sidebar-menu i {
                min-width: 20px;
                text-align: center;
                font-size: 18px;
            }

        .sidebar.collapsed .sidebar-menu a span {
            display: none;
        }

        /* Main Container */
        .main-wrapper {
            flex: 1;
            margin-left: 260px;
            display: flex;
            flex-direction: column;
            transition: none;
        }

            .main-wrapper.expanded {
                margin-left: 80px;
            }

        /* Topbar */
        .topbar {
            background-color: var(--bg-light-secondary);
            border-bottom: 1px solid var(--border-light);
            padding: 16px 24px;
            display: flex;
            justify-content: space-between;
            align-items: center;
            height: 70px;
            box-shadow: 0 2px 4px rgba(0, 0, 0, 0.05);
        }

        .topbar-left {
            display: flex;
            align-items: center;
            gap: 16px;
        }

        .toggle-btn {
            background: none;
            border: none;
            cursor: pointer;
            font-size: 18px;
            color: var(--text-light);
            transition: none;
            padding: 8px;
        }

            .toggle-btn:hover {
                color: var(--primary);
            }



        .topbar-right {
            display: flex;
            align-items: center;
            gap: 16px;
        }

        .theme-toggle {
            background: none;
            border: none;
            cursor: pointer;
            font-size: 18px;
            color: var(--text-light);
            transition: none;
            padding: 8px;
        }

            .theme-toggle:hover {
                color: var(--primary);
            }

        .user-menu {
            display: flex;
            align-items: center;
            gap: 12px;
            cursor: pointer;
            padding: 8px;
            border-radius: 8px;
            transition: none;
        }

            .user-menu:hover {
                background-color: var(--bg-light-tertiary);
            }

        .user-avatar {
            width: 36px;
            height: 36px;
            border-radius: 50%;
            background: linear-gradient(135deg, var(--primary), var(--secondary));
            display: flex;
            align-items: center;
            justify-content: center;
            color: white;
            font-weight: 600;
            font-size: 14px;
        }

        /* Main Content */
        .main-content {
            flex: 1;
            overflow-y: auto;
            padding: 32px 24px;
            background-color: var(--bg-light);
        }

        .content-header {
            margin-bottom: 32px;
        }

            .content-header h1 {
                font-size: 28px;
                font-weight: 700;
                margin-bottom: 8px;
                color: var(--text-light);
            }

            .content-header p {
                color: var(--text-light-secondary);
                font-size: 14px;
            }

        .cards-grid {
            display: grid;
            grid-template-columns: repeat(auto-fit, minmax(280px, 1fr));
            gap: 20px;
            margin-bottom: 32px;
        }

        .card {
            background-color: var(--bg-light-secondary);
            border: 1px solid var(--border-light);
            border-radius: 12px;
            padding: 24px;
            transition: none;
        }

            .card:hover {
                border-color: var(--primary);
                box-shadow: 0 4px 12px rgba(59, 130, 246, 0.1);
            }

        .card-icon {
            width: 48px;
            height: 48px;
            background-color: var(--bg-light-tertiary);
            border-radius: 10px;
            display: flex;
            align-items: center;
            justify-content: center;
            margin-bottom: 16px;
            color: var(--primary);
            font-size: 24px;
        }

        .card-title {
            font-size: 14px;
            color: var(--text-light-secondary);
            margin-bottom: 8px;
            text-transform: uppercase;
            letter-spacing: 0.5px;
            font-weight: 600;
        }

        .card-value {
            font-size: 32px;
            font-weight: 700;
            color: var(--text-light);
            margin-bottom: 12px;
        }

        .card-footer {
            font-size: 12px;
            color: var(--text-light-secondary);
        }

            .card-footer .positive {
                color: #10b981;
                font-weight: 600;
            }

        .chart-container {
            background-color: var(--bg-light-secondary);
            border: 1px solid var(--border-light);
            border-radius: 12px;
            padding: 24px;
            height: 300px;
            display: flex;
            align-items: center;
            justify-content: center;
            color: var(--text-light-secondary);
        }

        @media (max-width: 768px) {
            .sidebar {
                width: 260px;
                left: -260px;
                z-index: 101;
                box-shadow: 2px 0 8px rgba(0, 0, 0, 0.1);
            }

                .sidebar.mobile-visible {
                    left: 0;
                }

            .main-wrapper {
                margin-left: 0;
            }

                .main-wrapper.expanded {
                    margin-left: 0;
                }



            .cards-grid {
                grid-template-columns: 1fr;
            }

            .topbar {
                padding: 12px 16px;
                height: 60px;
            }

            .content-header h1 {
                font-size: 22px;
            }

            .main-content {
                padding: 16px 12px;
            }

            .card {
                padding: 16px;
            }

            .card-value {
                font-size: 24px;
            }

            .chart-container {
                height: 250px;
            }

            body.sidebar-open {
                overflow: hidden;
            }
        }
    </style>
</head>
<body>
    <div class="container">
        <!-- Sidebar -->
        <div class="sidebar" id="sidebar">
            <div class="logo">
                <i class="fas fa-cube"></i>
                <span>BNet Cafe</span>
            </div>
            <ul class="sidebar-menu">
                <li>
                    <asp:HyperLink ID="HyperLink_Dashboard" runat="server" NavigateUrl="?Form=Dashboard">
            <i class="fas fa-home"></i><span>Dashboard</span>
                    </asp:HyperLink>
                </li>
                <li>
                    <asp:HyperLink ID="HyperLink_Computers" runat="server" NavigateUrl="?Form=Computers">
            <i class="fas fa-computer"></i><span>Computers</span>
                    </asp:HyperLink>
                </li>
                <li>
                    <asp:HyperLink ID="HyperLink_Users" runat="server" NavigateUrl="?Form=Users">
            <i class="fas fa-users"></i><span>Users</span>
                    </asp:HyperLink>
                </li>
            </ul>
        </div>

        <!-- Main Wrapper -->
        <div class="main-wrapper" id="mainWrapper">
            <!-- Topbar -->
            <div class="topbar">
                <div class="topbar-left">
                    <button class="toggle-btn" id="toggleBtn" title="Toggle sidebar">
                        <i class="fas fa-bars"></i>
                    </button>
                </div>

                <div class="topbar-right">
                    <button class="theme-toggle" id="themeToggle" title="Toggle dark mode">
                        <i class="fas fa-moon"></i>
                    </button>
                    <div class="user-menu">
                        <div class="user-avatar">JD</div>
                        <div style="font-size: 13px;">
                            <div style="font-weight: 600; color: var(--text-light);">John Doe</div>
                            <div style="color: var(--text-light-secondary); font-size: 12px;">Admin</div>
                        </div>
                    </div>
                </div>
            </div>

            <!-- Main Content -->
            <div class="main-content">
                <asp:PlaceHolder ID="PlaceHolder_Container" runat="server"></asp:PlaceHolder>
            </div>
        </div>
    </div>

    <script>
        const sidebar = document.getElementById('sidebar');
        const mainWrapper = document.getElementById('mainWrapper');
        const toggleBtn = document.getElementById('toggleBtn');
        const themeToggle = document.getElementById('themeToggle');
        const container = document.querySelector('.container');
        const html = document.documentElement;
        const body = document.body;

        // Check if mobile
        const isMobile = () => window.innerWidth <= 768;

        // Sidebar Toggle
        toggleBtn.addEventListener('click', () => {
            if (isMobile()) {
                // Mobile: toggle sidebar visibility with blur
                sidebar.classList.toggle('mobile-visible');
                container.classList.toggle('sidebar-open');
            } else {
                // Desktop: collapse/expand sidebar
                sidebar.classList.toggle('collapsed');
                mainWrapper.classList.toggle('expanded');
            }
        });

        // Close sidebar when clicking on a menu item (mobile)
        document.querySelectorAll('.sidebar-menu a').forEach(link => {
            link.addEventListener('click', () => {
                if (isMobile()) {
                    sidebar.classList.remove('mobile-visible');
                    container.classList.remove('sidebar-open');
                }
            });
        });

        // Close sidebar when clicking on blur overlay (mobile)
        document.querySelector('.container').addEventListener('click', (e) => {
            if (isMobile() && e.target === container && sidebar.classList.contains('mobile-visible')) {
                sidebar.classList.remove('mobile-visible');
                container.classList.remove('sidebar-open');
            }
        });

        // Handle window resize
        window.addEventListener('resize', () => {
            if (!isMobile()) {
                sidebar.classList.remove('mobile-visible');
                container.classList.remove('sidebar-open');
            }
        });

        // Theme Toggle
        const currentTheme = localStorage.getItem('theme') || 'light';
        html.setAttribute('data-theme', currentTheme);
        updateThemeIcon(currentTheme);

        themeToggle.addEventListener('click', () => {
            const theme = html.getAttribute('data-theme') === 'light' ? 'dark' : 'light';
            html.setAttribute('data-theme', theme);
            localStorage.setItem('theme', theme);
            updateThemeIcon(theme);
        });

        function updateThemeIcon(theme) {
            themeToggle.innerHTML = theme === 'light' ? '<i class="fas fa-moon"></i>' : '<i class="fas fa-sun"></i>';
        }
    </script>
</body>
</html>
