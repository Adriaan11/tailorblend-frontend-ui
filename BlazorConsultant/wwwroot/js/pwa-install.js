// TailorBlend PWA Install Prompt Handler
(function () {
    'use strict';

    let deferredPrompt = null;
    let isInstallable = false;

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
        if (!button) {
            // Button doesn't exist yet - will be rendered by Blazor
            // Try again in the next tick if we know it's installable
            if (isInstallable && !isRunningStandalone()) {
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
            // Show button if:
            // 1. Chrome/Chromium: beforeinstallprompt event fired
            // 2. iOS: Not yet installed (not in standalone mode)
            if (isRunningStandalone()) {
                return false; // Already installed
            }
            return isInstallable || isIOS();
        },

        isIOS: function () {
            return isIOS();
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
        document.addEventListener('DOMContentLoaded', updateInstallButtonVisibility);
    } else {
        updateInstallButtonVisibility();
    }
})();
