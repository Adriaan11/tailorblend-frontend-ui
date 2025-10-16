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
        }
    };

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
        });
    }
})();
