const CACHE_NAME = 'vk-tourguide-v2';
const STATIC_ASSETS = [
  '/',
  '/index.html',
  '/admin.html',
  '/vendor.html',
  '/icon.svg',
  '/manifest.json'
];

const CACHE_VERSION = 'v2';
const MAX_CACHE_SIZE = 50;

// Install - cache static assets
self.addEventListener('install', event => {
  event.waitUntil(
    caches.open(CACHE_NAME)
      .then(cache => {
        console.log('[SW] Caching static assets');
        return cache.addAll(STATIC_ASSETS);
      })
      .then(() => self.skipWaiting())
  );
});

// Activate - clean old caches
self.addEventListener('activate', event => {
  event.waitUntil(
    caches.keys()
      .then(cacheNames => {
        return Promise.all(
          cacheNames
            .filter(name => name !== CACHE_NAME)
            .map(name => {
              console.log('[SW] Deleting old cache:', name);
              return caches.delete(name);
            })
        );
      })
      .then(() => self.clients.claim())
  );
});

// Fetch - cache-first for static, network-first for API
self.addEventListener('fetch', event => {
  const { request } = event;
  const url = new URL(request.url);

  // Skip non-GET requests
  if (request.method !== 'GET') return;

  // Skip Chrome extensions and devtools
  if (url.protocol === 'chrome-extension:') return;

  // API requests - network first, fallback to cache
  if (url.pathname.startsWith('/api/')) {
    event.respondWith(
      fetch(request)
        .then(response => {
          if (response.ok) {
            const clone = response.clone();
            caches.open(CACHE_NAME).then(cache => {
              cache.put(request, clone);
              trimCache();
            });
          }
          return response;
        })
        .catch(() => caches.match(request))
    );
    return;
  }

  // Map tiles - cache first with network fallback
  if (url.hostname.includes('tile') || url.hostname.includes('openstreetmap') || url.hostname.includes('carto')) {
    event.respondWith(
      caches.match(request)
        .then(response => {
          if (response) return response;
          return fetch(request)
            .then(networkResponse => {
              const clone = networkResponse.clone();
              caches.open(CACHE_NAME).then(cache => cache.put(request, clone));
              return networkResponse;
            });
        })
        .catch(() => new Response('', { status: 503 }))
    );
    return;
  }

  // Static assets (CDN) - cache first
  if (url.origin !== self.location.origin && (
    url.hostname.includes('unpkg.com') ||
    url.hostname.includes('cdnjs.cloudflare.com') ||
    url.hostname.includes('cdn.jsdelivr.net')
  )) {
    event.respondWith(
      caches.match(request)
        .then(response => {
          if (response) return response;
          return fetch(request)
            .then(networkResponse => {
              const clone = networkResponse.clone();
              caches.open(CACHE_NAME).then(cache => cache.put(request, clone));
              return networkResponse;
            });
        })
    );
    return;
  }

  // Local assets - cache first
  event.respondWith(
    caches.match(request)
      .then(response => {
        return response || fetch(request)
          .then(networkResponse => {
            if (networkResponse.ok) {
              const clone = networkResponse.clone();
              caches.open(CACHE_NAME).then(cache => cache.put(request, clone));
            }
            return networkResponse;
          });
      })
      .catch(() => {
        // Offline fallback for HTML pages
        if (request.headers.get('Accept')?.includes('text/html')) {
          return caches.match('/index.html');
        }
      })
  );
});

// Trim cache to prevent storage bloat
async function trimCache() {
  const cache = await caches.open(CACHE_NAME);
  const keys = await cache.keys();
  if (keys.length > MAX_CACHE_SIZE) {
    await cache.delete(keys[0]);
  }
}

// Background sync for analytics
self.addEventListener('sync', event => {
  if (event.tag === 'sync-analytics') {
    event.waitUntil(syncAnalytics());
  }
});

async function syncAnalytics() {
  // Sync any pending analytics data
  console.log('[SW] Syncing analytics...');
}
