// TailorBlend PWA - Minimal Service Worker
// Purpose: Enable "Add to Home Screen" on iOS (requires SW)
// Strategy: Network-first, no aggressive caching (Blazor Server requires live connection)

const CACHE_PREFIX = 'tailorblend-';
const CACHE_VERSION = 'v1';
const CACHE_NAME = `${CACHE_PREFIX}${CACHE_VERSION}`;
const OFFLINE_URL = '/offline.html';

// Minimal assets to cache (only for offline fallback)
const CRITICAL_ASSETS = [
  '/offline.html',
  '/icons/icon-192.png'
];

// Install event - cache critical assets only
self.addEventListener('install', event => {
  console.log('[ServiceWorker] Install');

  event.waitUntil(
    caches.open(CACHE_NAME)
      .then(cache => cache.addAll(CRITICAL_ASSETS))
      .then(() => self.skipWaiting())
  );
});

// Activate event - clean up old caches
self.addEventListener('activate', event => {
  console.log('[ServiceWorker] Activate');

  event.waitUntil(
    caches.keys().then(cacheNames => {
      return Promise.all(
        cacheNames
          .filter(name => name.startsWith(CACHE_PREFIX) && name !== CACHE_NAME)
          .map(name => caches.delete(name))
      );
    })
    .then(() => self.clients.claim())
  );
});

// Fetch event - network first, offline fallback only
self.addEventListener('fetch', event => {
  // Skip non-GET requests
  if (event.request.method !== 'GET') {
    return;
  }

  const url = new URL(event.request.url);

  // Skip Blazor SignalR connections (critical for Blazor Server)
  if (url.pathname.startsWith('/_blazor')) {
    return;
  }

  // Skip API requests (need live connection)
  if (url.pathname.startsWith('/api/')) {
    return;
  }

  // Network-first strategy for everything else
  event.respondWith(
    fetch(event.request)
      .catch(() => {
        // Only serve offline page for navigation requests
        if (event.request.mode === 'navigate') {
          return caches.match(OFFLINE_URL);
        }

        // Try cache for assets (icons, CSS)
        return caches.match(event.request);
      })
  );
});

console.log('[SW] Service worker loaded');
