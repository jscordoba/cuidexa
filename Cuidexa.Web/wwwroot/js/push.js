// Registro del service worker + activación de notificaciones push (Fase 7).
// El permiso del navegador SOLO se pide con un gesto explícito del usuario
// (clic en el botón) — nunca al cargar la página, evita el bloqueo/penalización
// que aplican los navegadores a los sitios que piden permiso sin interacción.

function urlBase64ToUint8Array(base64String) {
    const padding = '='.repeat((4 - (base64String.length % 4)) % 4);
    const base64 = (base64String + padding).replace(/-/g, '+').replace(/_/g, '/');
    const raw = atob(base64);
    return Uint8Array.from([...raw].map((c) => c.charCodeAt(0)));
}

(async function () {
    if (!('serviceWorker' in navigator) || !('PushManager' in window)) return;

    const registro = await navigator.serviceWorker.register('/sw.js');
    const boton = document.getElementById('botonNotificaciones');
    if (!boton) return;

    function actualizarBoton(estado) {
        if (estado === 'granted') {
            boton.hidden = true;
        } else if (estado === 'denied') {
            boton.hidden = true;
        } else {
            boton.hidden = false;
        }
    }

    actualizarBoton(Notification.permission);

    boton.addEventListener('click', async () => {
        const permiso = await Notification.requestPermission();
        actualizarBoton(permiso);
        if (permiso !== 'granted') return;

        const vapidPublicKey = document.querySelector('meta[name="vapid-public-key"]')?.content;
        if (!vapidPublicKey) return;

        const suscripcion = await registro.pushManager.subscribe({
            userVisibleOnly: true,
            applicationServerKey: urlBase64ToUint8Array(vapidPublicKey)
        });

        const claves = suscripcion.toJSON().keys;
        const tokenCsrf = document.querySelector('meta[name="request-verification-token"]')?.content;
        // data-endpoint-suscribir: SuperAdmin/Familiar tienen su propia
        // acción de suscripción (esquema de cookie distinto de Empleado) —
        // por defecto, /Push/Suscribir sigue sirviendo a los roles operativos.
        const endpointSuscribir = boton.dataset.endpointSuscribir || '/Push/Suscribir';
        await fetch(endpointSuscribir, {
            method: 'POST',
            headers: { 'Content-Type': 'application/json', 'X-CSRF-TOKEN': tokenCsrf },
            body: JSON.stringify({ endpoint: suscripcion.endpoint, p256dh: claves.p256dh, auth: claves.auth })
        });
    });
})();
