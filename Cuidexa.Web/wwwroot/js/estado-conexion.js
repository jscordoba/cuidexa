// Banner "sin conexión" — visible mientras navigator.onLine sea false, para
// que una pérdida de red se note de inmediato en vez de descubrirse cuando
// un formulario falla al enviarse sin explicación.
(function () {
    function actualizar() {
        var banner = document.getElementById('bannerSinConexion');
        if (!banner) return;
        banner.hidden = navigator.onLine;
    }

    window.addEventListener('online', actualizar);
    window.addEventListener('offline', actualizar);
    document.addEventListener('DOMContentLoaded', actualizar);
})();
