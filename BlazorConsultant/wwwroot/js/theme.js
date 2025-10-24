(function () {
    function resolveInitialTheme() {
        try {
            const stored = window.localStorage ? localStorage.getItem('tb-theme') : null;
            if (stored === 'light' || stored === 'dark') {
                return stored;
            }
        } catch (err) {
            console.warn('TailorBlend: unable to read stored theme', err);
        }

        const prefersDark = window.matchMedia && window.matchMedia('(prefers-color-scheme: dark)').matches;
        return prefersDark ? 'dark' : 'light';
    }

    const initialTheme = resolveInitialTheme();
    document.documentElement.setAttribute('data-theme', initialTheme);

    // Update theme-color meta tag for PWA
    function updateThemeColor(theme) {
        const lightColor = '#70D1C7';
        const darkColor = '#75D5CA';
        const themeColor = theme === 'dark' ? darkColor : lightColor;

        // Update the single theme-color meta tag
        const metaTag = document.querySelector('meta[name="theme-color"]');
        if (metaTag) {
            metaTag.content = themeColor;
        }
    }

    window.tbTheme = {
        init: function () {
            return document.documentElement.getAttribute('data-theme') || initialTheme || 'light';
        },
        setTheme: function (theme) {
            const value = theme === 'dark' ? 'dark' : 'light';
            document.documentElement.setAttribute('data-theme', value);
            try {
                if (window.localStorage) {
                    localStorage.setItem('tb-theme', value);
                }
            } catch (err) {
                console.warn('TailorBlend: unable to persist theme', err);
            }
            // Update PWA theme color
            updateThemeColor(value);
        }
    };

    // Set initial theme color
    updateThemeColor(initialTheme);

    if (window.matchMedia) {
        window.matchMedia('(prefers-color-scheme: dark)').addEventListener('change', function (event) {
            try {
                const stored = window.localStorage ? localStorage.getItem('tb-theme') : null;
                if (stored === 'light' || stored === 'dark') {
                    return; // honor explicit preference
                }
            } catch {
                // ignore
            }

            const value = event.matches ? 'dark' : 'light';
            document.documentElement.setAttribute('data-theme', value);
            // Update PWA theme color when system preference changes
            updateThemeColor(value);
        });
    }
})();
