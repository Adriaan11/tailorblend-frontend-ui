// TailorBlend PWA - Service Worker Registration
// Registers the service worker and handles updates

(function() {
  'use strict';

  if ('serviceWorker' in navigator) {
    window.addEventListener('load', function() {
      navigator.serviceWorker.register('/service-worker.js', {
        scope: '/'
      })
      .then(function(registration) {
        console.log('[PWA] ServiceWorker registered:', registration.scope);

        // Check for updates every hour
        setInterval(function() {
          registration.update();
        }, 60 * 60 * 1000);
      })
      .catch(function(error) {
        console.warn('[PWA] ServiceWorker registration failed:', error);
      });
    });
  }
})();
