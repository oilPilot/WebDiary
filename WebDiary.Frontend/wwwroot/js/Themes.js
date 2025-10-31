
function toggleTheme(theme) {
        document.documentElement.setAttribute('data-bs-theme', theme);
        localStorage.setItem('theme', theme);
    }
function toggleLastTheme() {
        if (!localStorage.getItem('theme'))
            localStorage.setItem('theme') = window.matchMedia('(prefers-color-scheme: dark)').matches;
        document.documentElement.setAttribute('data-bs-theme', localStorage.getItem('theme'));
    }
function getTheme() {
        if (!localStorage.getItem('theme'))
            localStorage.setItem('theme') = window.matchMedia('(prefers-color-scheme: dark)').matches;
        return localStorage.getItem('theme')
    }
function getAndSetLastTheme() {
        var lastTheme = localStorage.getItem('theme');
        if (!lastTheme)
            lastTheme = window.matchMedia('(prefers-color-scheme: dark)').matches;
        document.documentElement.setAttribute('data-bs-theme', lastTheme);
        return lastTheme;
    }




