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