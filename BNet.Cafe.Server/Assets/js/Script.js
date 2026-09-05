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