// TailorBlend PWA Install Prompt Handler
(function () {
    'use strict';

    console.log('[PWA] Install script loading...');

    let deferredPrompt = null;
    let isInstallable = false;

    // Debug: Log initial state
    console.log('[PWA] iOS:', /iPad|iPhone|iPod/.test(navigator.userAgent));
    console.log('[PWA] User Agent:', navigator.userAgent);

    // Listen for the beforeinstallprompt event
    window.addEventListener('beforeinstallprompt', (e) => {
        console.log('[PWA] Install prompt available');

        // Prevent the default mini-infobar from appearing on mobile
        e.preventDefault();

        // Store the event for later use
        deferredPrompt = e;
        isInstallable = true;

        // Notify Blazor that install is available
        updateInstallButtonVisibility();
    });

    // Listen for successful installation
    window.addEventListener('appinstalled', () => {
        console.log('[PWA] App installed successfully');
        deferredPrompt = null;
        isInstallable = false;
        updateInstallButtonVisibility();
    });

    // Check if running on iOS
    function isIOS() {
        return /iPad|iPhone|iPod/.test(navigator.userAgent) &&
               !window.MSStream; // Exclude Windows Phone
    }

    // Check if app is already installed (standalone mode)
    function isRunningStandalone() {
        return (
            window.matchMedia('(display-mode: standalone)').matches ||
            window.navigator.standalone === true
        );
    }

    // Update button visibility based on install state
    function updateInstallButtonVisibility() {
        const button = document.getElementById('pwa-install-button');
        console.log('[PWA] updateInstallButtonVisibility called, button:', !!button);

        if (!button) {
            // Button doesn't exist yet - will be rendered by Blazor
            // Try again in the next tick if we know it's installable
            if ((isInstallable || isIOS()) && !isRunningStandalone()) {
                console.log('[PWA] Button not yet rendered, retrying in 100ms...');
                setTimeout(updateInstallButtonVisibility, 100);
            }
            return;
        }

        if (isRunningStandalone()) {
            // App is installed and running standalone
            button.style.display = 'none';
        } else if (isInstallable) {
            // App is installable
            button.style.display = '';
        } else {
            // Not installable (already installed or not supported)
            button.style.display = 'none';
        }
    }

    // Expose API for Blazor
    window.pwaInstall = {
        isInstallable: function () {
            const standalone = isRunningStandalone();
            const ios = isIOS();
            const result = (isInstallable || ios) && !standalone;

            console.log('[PWA] isInstallable() called:', {
                deferredPrompt: !!deferredPrompt,
                isInstallable,
                ios,
                standalone,
                result
            });

            return result;
        },

        isIOS: function () {
            const result = isIOS();
            console.log('[PWA] isIOS() called:', result);
            return result;
        },

        isStandalone: function () {
            return isRunningStandalone();
        },

        showPrompt: async function () {
            // iOS requires manual installation
            if (isIOS()) {
                console.log('[PWA] iOS detected - showing manual installation instructions');
                alert(
                    'Add TailorBlend to Home Screen:\n\n' +
                    '1. Tap the Share button (⬆️)\n' +
                    '2. Scroll down and tap "Add to Home Screen"\n' +
                    '3. Tap "Add" to confirm\n\n' +
                    'The app will appear on your home screen and work offline!'
                );
                return {
                    success: true,
                    message: 'iOS installation instructions shown'
                };
            }

            // Chrome/Chromium: Use beforeinstallprompt
            if (!deferredPrompt) {
                console.warn('[PWA] Install prompt not available');
                return {
                    success: false,
                    error: 'Install prompt not available. You may have already installed the app.'
                };
            }

            try {
                // Show the install prompt
                deferredPrompt.prompt();

                // Wait for the user's response
                const choiceResult = await deferredPrompt.userChoice;

                if (choiceResult.outcome === 'accepted') {
                    console.log('[PWA] User accepted installation');
                    return { success: true, installed: true };
                } else {
                    console.log('[PWA] User dismissed installation');
                    return { success: true, installed: false };
                }
            } catch (err) {
                console.error('[PWA] Install prompt error:', err);
                return {
                    success: false,
                    error: err.message || 'Installation failed'
                };
            } finally {
                // Clear the deferred prompt (can only be used once)
                deferredPrompt = null;
                isInstallable = false;
                updateInstallButtonVisibility();
            }
        },

        // Manually update button visibility (called from Blazor)
        updateButton: function () {
            updateInstallButtonVisibility();
        }
    };

    // Check initial state after DOM loads
    if (document.readyState === 'loading') {
        console.log('[PWA] DOM still loading, waiting for DOMContentLoaded...');
        document.addEventListener('DOMContentLoaded', updateInstallButtonVisibility);
    } else {
        console.log('[PWA] DOM already loaded, checking button visibility...');
        updateInstallButtonVisibility();
    }

    console.log('[PWA] Install script initialization complete');
})();
