// Service worker de Cuidexa (Fase 7) — instalabilidad de la PWA, push reales
// y, desde Fase 10 (comercialización), una pantalla propia de "sin conexión"
// para navegaciones de página completa. Deliberadamente SIN caché de datos
// (avisos/turnos/residentes...): cachearlos mostraría información obsoleta
// en una app donde la vigencia de los datos importa — solo se cachea
// offline.html, una página estática que no cambia.

const CACHE_NAME = 'cuidexa-offline-v1';
const OFFLINE_URL = '/offline.html';

self.addEventListener('install', (event) => {
    event.waitUntil(caches.open(CACHE_NAME).then((cache) => cache.add(OFFLINE_URL)));
    self.skipWaiting();
});

self.addEventListener('activate', (event) => {
    event.waitUntil(
        caches.keys()
            .then((claves) => Promise.all(claves.filter((k) => k !== CACHE_NAME).map((k) => caches.delete(k))))
            .then(() => self.clients.claim())
    );
});

// Solo intercepta navegaciones de página completa (cargar/recargar una
// pantalla): si falla por falta de red, se sirve offline.html en vez del
// error genérico del navegador. El resto de peticiones (datos, POST,
// assets) no se tocan — sin caché de datos, a propósito.
self.addEventListener('fetch', (event) => {
    if (event.request.mode === 'navigate') {
        event.respondWith(
            fetch(event.request).catch(() => caches.match(OFFLINE_URL))
        );
    }
});

self.addEventListener('push', (event) => {
    const datos = event.data ? event.data.json() : {};
    const titulo = datos.titulo || 'Cuidexa';
    const opciones = {
        body: datos.cuerpo || '',
        icon: '/icons/icon.svg',
        badge: '/icons/icon.svg',
        requireInteraction: !!datos.urgente,
        data: { url: datos.url || '/' }
    };
    event.waitUntil(self.registration.showNotification(titulo, opciones));
});

self.addEventListener('notificationclick', (event) => {
    event.notification.close();
    const url = event.notification.data?.url || '/';
    event.waitUntil(
        self.clients.matchAll({ type: 'window' }).then((clientList) => {
            for (const cliente of clientList) {
                if (cliente.url.endsWith(url) && 'focus' in cliente) {
                    return cliente.focus();
                }
            }
            if (self.clients.openWindow) {
                return self.clients.openWindow(url);
            }
        })
    );
});
