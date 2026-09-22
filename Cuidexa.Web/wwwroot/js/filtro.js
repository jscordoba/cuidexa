// Filtro de texto simple para listados que pueden crecer mucho (residentes,
// historiales, avisos...). Sin recarga de página ni llamada al servidor:
// con los volúmenes de datos de este MVP, filtrar en el cliente es
// suficiente y más simple que paginar/buscar en el backend.
function activarFiltro(inputSelector, itemSelector, opciones) {
    const input = document.querySelector(inputSelector);
    if (!input) return;

    const contenedor = opciones && opciones.contenedor ? document.querySelector(opciones.contenedor) : document;
    const vacioSelector = opciones && opciones.vacio;

    function aplicar() {
        const texto = input.value.trim().toLowerCase();
        const items = contenedor.querySelectorAll(itemSelector);
        let visibles = 0;

        items.forEach(function (item) {
            const coincide = texto === '' || item.textContent.toLowerCase().includes(texto);
            item.hidden = !coincide;
            if (coincide) visibles++;
        });

        if (vacioSelector) {
            const vacio = document.querySelector(vacioSelector);
            if (vacio) vacio.hidden = !(texto !== '' && visibles === 0);
        }
    }

    input.addEventListener('input', aplicar);
}
