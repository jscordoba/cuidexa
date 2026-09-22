// Convierte cada <tr data-href="..."> que coincida con el selector en una
// fila navegable con un solo clic — evita depender de un <a> dentro de la
// celda (que obliga a verse como enlace azul subrayado para ser reconocible
// como clicable). El estilo de "clicable" (cursor, resaltado) lo pone la
// clase .tabla-clicable en site.css; esto solo añade el comportamiento.
function activarFilasClicables(filaSelector) {
    document.querySelectorAll(filaSelector).forEach(function (fila) {
        if (!fila.dataset.href) return;

        fila.setAttribute('role', 'link');
        fila.setAttribute('tabindex', '0');

        fila.addEventListener('click', function () {
            window.location.href = fila.dataset.href;
        });
        fila.addEventListener('keydown', function (evento) {
            if (evento.key === 'Enter' || evento.key === ' ') {
                evento.preventDefault();
                window.location.href = fila.dataset.href;
            }
        });
    });
}
