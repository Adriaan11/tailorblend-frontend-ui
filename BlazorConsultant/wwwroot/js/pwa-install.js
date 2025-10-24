// PWA Installation Handler for TailorBlend
// Manages install prompt and service worker registration

let deferredPrompt;
let installButton;

// Initialize PWA features
window.addEventListener('load', async () => {
    // Register service worker
    if ('serviceWorker' in navigator) {
        try {
            const registration = await navigator.serviceWorker.register('/service-worker.js', {
                scope: '/'
            });

            console.log('[PWA] Service Worker registered:', registration.scope);

            // Check for updates every hour
            setInterval(() => {
                registration.update();
            }, 60 * 60 * 1000);

            // Handle service worker updates
            registration.addEventListener('updatefound', () => {
                const newWorker = registration.installing;

                newWorker.addEventListener('statechange', () => {
                    if (newWorker.state === 'installed' && navigator.serviceWorker.controller) {
                        // New service worker available, show update notification
                        showUpdateNotification();
                    }
                });
            });
        } catch (error) {
            console.error('[PWA] Service Worker registration failed:', error);
        }
    }

    // Listen for install prompt
    window.addEventListener('beforeinstallprompt', (e) => {
        console.log('[PWA] Install prompt available');
        e.preventDefault();
        deferredPrompt = e;

        // Show custom install button
        showInstallPromotion();
    });

    // Listen for app installed event
    window.addEventListener('appinstalled', () => {
        console.log('[PWA] App installed successfully');
        deferredPrompt = null;
        hideInstallPromotion();

        // Track installation (if analytics is set up)
        if (window.gtag) {
            gtag('event', 'pwa_installed', {
                event_category: 'engagement',
                event_label: 'PWA Installation'
            });
        }
    });

    // Check if already installed
    if (window.matchMedia('(display-mode: standalone)').matches) {
        console.log('[PWA] Running in standalone mode');
    }
});

// Show install promotion UI
function showInstallPromotion() {
    // Create install banner
    const banner = document.createElement('div');
    banner.id = 'pwa-install-banner';
    banner.innerHTML = `
        <div style="position: fixed; bottom: 88px; left: 16px; right: 16px; z-index: 1000; background: linear-gradient(135deg, #70d1c7, #5bbfb5); color: white; padding: 16px 20px; border-radius: 16px; box-shadow: 0 8px 24px rgba(112, 209, 199, 0.4); display: flex; align-items: center; justify-content: space-between; animation: slideUp 0.3s ease;">
            <div style="flex: 1;">
                <div style="font-weight: 700; font-size: 15px; margin-bottom: 4px;">Install TailorBlend</div>
                <div style="font-size: 13px; opacity: 0.9;">Get quick access and offline support</div>
            </div>
            <div style="display: flex; gap: 8px;">
                <button id="pwa-install-btn" style="background: white; color: #5bbfb5; border: none; padding: 10px 20px; border-radius: 999px; font-weight: 600; font-size: 14px; cursor: pointer;">
                    Install
                </button>
                <button id="pwa-dismiss-btn" style="background: rgba(255,255,255,0.2); color: white; border: none; padding: 10px 16px; border-radius: 999px; font-size: 14px; cursor: pointer;">
                    ✕
                </button>
            </div>
        </div>
    `;

    document.body.appendChild(banner);

    // Add event listeners
    document.getElementById('pwa-install-btn').addEventListener('click', installApp);
    document.getElementById('pwa-dismiss-btn').addEventListener('click', () => {
        banner.remove();
        // Remember dismissal for 7 days
        localStorage.setItem('pwa-install-dismissed', Date.now() + (7 * 24 * 60 * 60 * 1000));
    });

    // Check if previously dismissed
    const dismissed = localStorage.getItem('pwa-install-dismissed');
    if (dismissed && Date.now() < parseInt(dismissed)) {
        banner.remove();
    }
}

// Hide install promotion
function hideInstallPromotion() {
    const banner = document.getElementById('pwa-install-banner');
    if (banner) {
        banner.remove();
    }
}

// Install the app
async function installApp() {
    if (!deferredPrompt) {
        console.log('[PWA] No install prompt available');
        return;
    }

    // Show the install prompt
    deferredPrompt.prompt();

    // Wait for user response
    const { outcome } = await deferredPrompt.userChoice;
    console.log(`[PWA] User response: ${outcome}`);

    if (outcome === 'accepted') {
        console.log('[PWA] User accepted the install prompt');
    } else {
        console.log('[PWA] User dismissed the install prompt');
    }

    deferredPrompt = null;
    hideInstallPromotion();
}

// Show update notification
function showUpdateNotification() {
    const notification = document.createElement('div');
    notification.innerHTML = `
        <div style="position: fixed; top: 80px; left: 16px; right: 16px; z-index: 1100; background: #111827; color: white; padding: 16px 20px; border-radius: 12px; box-shadow: 0 8px 24px rgba(0, 0, 0, 0.3); display: flex; align-items: center; justify-content: space-between; animation: slideDown 0.3s ease; border: 1px solid rgba(112, 209, 199, 0.3);">
            <div style="flex: 1;">
                <div style="font-weight: 600; font-size: 14px; margin-bottom: 4px;">Update Available</div>
                <div style="font-size: 13px; opacity: 0.8;">A new version of TailorBlend is ready</div>
            </div>
            <button id="update-reload-btn" style="background: linear-gradient(135deg, #70d1c7, #5bbfb5); color: white; border: none; padding: 10px 20px; border-radius: 999px; font-weight: 600; font-size: 14px; cursor: pointer;">
                Update
            </button>
        </div>
    `;

    document.body.appendChild(notification);

    document.getElementById('update-reload-btn').addEventListener('click', () => {
        window.location.reload();
    });
}

// Add slideUp animation
const style = document.createElement('style');
style.textContent = `
    @keyframes slideUp {
        from {
            transform: translateY(100%);
            opacity: 0;
        }
        to {
            transform: translateY(0);
            opacity: 1;
        }
    }

    @keyframes slideDown {
        from {
            transform: translateY(-100%);
            opacity: 0;
        }
        to {
            transform: translateY(0);
            opacity: 1;
        }
    }
`;
document.head.appendChild(style);

// Export functions for use in Blazor components
window.pwaInstall = {
    prompt: installApp,
    isAvailable: () => deferredPrompt !== null,
    isStandalone: () => window.matchMedia('(display-mode: standalone)').matches
};
