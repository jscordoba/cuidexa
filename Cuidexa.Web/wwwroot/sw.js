// Service worker de Cuidexa (Fase 7) — solo instalabilidad de la PWA y push
// reales. Deliberadamente sin caché offline: cachear avisos/turnos mostraría
// datos obsoletos en una app donde la vigencia de la información importa.

self.addEventListener('install', () => {
    self.skipWaiting();
});

self.addEventListener('activate', (event) => {
    event.waitUntil(self.clients.claim());
});

// Passthrough — requisito de instalabilidad en algunos navegadores, sin caché.
self.addEventListener('fetch', () => { });

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
