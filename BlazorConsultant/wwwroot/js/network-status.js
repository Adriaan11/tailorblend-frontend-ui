// Network Status Monitor for TailorBlend PWA
// Displays offline banner and handles connection changes

let offlineBanner = null;
let isOnline = navigator.onLine;

// Initialize network monitoring
window.addEventListener('load', () => {
    // Check initial state
    if (!navigator.onLine) {
        showOfflineBanner();
    }

    // Listen for online/offline events
    window.addEventListener('online', handleOnline);
    window.addEventListener('offline', handleOffline);

    // Monitor connection quality
    if ('connection' in navigator) {
        navigator.connection.addEventListener('change', handleConnectionChange);
    }
});

// Show offline banner
function showOfflineBanner() {
    if (offlineBanner) return; // Already showing

    offlineBanner = document.createElement('div');
    offlineBanner.className = 'offline-banner';
    offlineBanner.innerHTML = `
        <span>You're offline</span>
        <span style="margin-left: auto; font-size: 0.75rem;">Changes will sync when reconnected</span>
    `;

    document.body.appendChild(offlineBanner);
    console.log('[Network] Offline mode activated');
}

// Hide offline banner
function hideOfflineBanner() {
    if (offlineBanner) {
        offlineBanner.style.animation = 'fadeOut 0.3s ease';
        setTimeout(() => {
            if (offlineBanner && offlineBanner.parentNode) {
                offlineBanner.remove();
            }
            offlineBanner = null;
        }, 300);

        console.log('[Network] Online mode restored');
    }
}

// Handle online event
function handleOnline() {
    isOnline = true;
    hideOfflineBanner();

    // Show brief success notification
    showConnectionNotification('Back online', 'success');

    // Trigger sync if service worker supports it
    if ('serviceWorker' in navigator && 'sync' in registration) {
        navigator.serviceWorker.ready
            .then(registration => registration.sync.register('sync-messages'))
            .catch(err => console.log('[Network] Background sync failed:', err));
    }
}

// Handle offline event
function handleOffline() {
    isOnline = false;
    showOfflineBanner();
    showConnectionNotification('Connection lost', 'warning');
}

// Handle connection quality changes
function handleConnectionChange() {
    const connection = navigator.connection || navigator.mozConnection || navigator.webkitConnection;

    if (connection) {
        const { effectiveType, downlink, rtt } = connection;

        console.log('[Network] Connection changed:', {
            type: effectiveType,
            downlink: downlink + ' Mbps',
            latency: rtt + ' ms'
        });

        // Warn on slow connections
        if (effectiveType === 'slow-2g' || effectiveType === '2g') {
            showConnectionNotification('Slow connection detected', 'warning', 5000);
        }
    }
}

// Show temporary connection notification
function showConnectionNotification(message, type = 'info', duration = 3000) {
    const notification = document.createElement('div');
    notification.style.cssText = `
        position: fixed;
        top: calc(var(--mobile-nav-height, 64px) + 16px);
        left: 16px;
        right: 16px;
        padding: 12px 16px;
        background: ${type === 'success' ? '#10b981' : type === 'warning' ? '#f59e0b' : '#3b82f6'};
        color: white;
        border-radius: 8px;
        font-size: 14px;
        font-weight: 600;
        box-shadow: 0 4px 12px rgba(0, 0, 0, 0.15);
        z-index: 1100;
        animation: slideDown 0.3s ease;
        display: flex;
        align-items: center;
        gap: 8px;
    `;

    const icon = type === 'success' ? '✓' : type === 'warning' ? '⚠' : 'ℹ';
    notification.innerHTML = `<span style="font-size: 16px;">${icon}</span><span>${message}</span>`;

    document.body.appendChild(notification);

    setTimeout(() => {
        notification.style.animation = 'fadeOut 0.3s ease';
        setTimeout(() => notification.remove(), 300);
    }, duration);
}

// Export status for use in app
window.networkStatus = {
    isOnline: () => isOnline,
    getConnectionInfo: () => {
        if ('connection' in navigator) {
            const conn = navigator.connection;
            return {
                type: conn.effectiveType,
                downlink: conn.downlink,
                rtt: conn.rtt,
                saveData: conn.saveData
            };
        }
        return null;
    }
};

// Add fadeOut animation
const style = document.createElement('style');
style.textContent = `
    @keyframes fadeOut {
        from {
            opacity: 1;
            transform: translateY(0);
        }
        to {
            opacity: 0;
            transform: translateY(-10px);
        }
    }
`;
document.head.appendChild(style);
