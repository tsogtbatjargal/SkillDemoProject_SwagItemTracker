self.addEventListener('install', event => {
    event.waitUntil(
        caches.open('F5CAA').then(cache => {
            return cache.addAll([
                '/',
                '/Home',
                '/css/site.css',
                '/js/site.js',
                // Add other files or routes to cache
            ]);
        })
    );
});

self.addEventListener('fetch', event => {
    event.respondWith(
        caches.match(event.request).then(response => {
            return response || fetch(event.request);
        })
    );
});
