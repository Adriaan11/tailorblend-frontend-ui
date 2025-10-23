// Minimal service worker for PWA installability
// No offline caching - Blazor Server requires active connection

const CACHE_NAME = 'tailorblend-v1';

// Install event - minimal setup
self.addEventListener('install', event => {
    console.log('Service Worker installing.');
    self.skipWaiting();
});

// Activate event - clean up old caches
self.addEventListener('activate', event => {
    console.log('Service Worker activating.');
    event.waitUntil(
        caches.keys().then(cacheNames => {
            return Promise.all(
                cacheNames
                    .filter(cacheName => cacheName !== CACHE_NAME)
                    .map(cacheName => caches.delete(cacheName))
            );
        })
    );
    return self.clients.claim();
});

// Fetch event - pass through to network (no offline caching)
// Blazor Server requires active SignalR connection
self.addEventListener('fetch', event => {
    // Let all requests pass through to network
    // No caching strategy for Blazor Server
    event.respondWith(fetch(event.request));
});
