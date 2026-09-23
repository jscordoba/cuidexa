// Pad de firma reutilizable: captura un trazo en un <canvas> y lo vuelca
// como PNG en base64 a un <input type="hidden"> justo antes de enviar el
// formulario — no depende de ninguna librería externa.
function iniciarFirmaPad(canvasId, inputId, botonLimpiarId, formId) {
    const canvas = document.getElementById(canvasId);
    if (!canvas) return;
    const ctx = canvas.getContext('2d');
    const input = document.getElementById(inputId);
    const boton = document.getElementById(botonLimpiarId);
    const form = document.getElementById(formId);

    ctx.lineWidth = 2;
    ctx.lineCap = 'round';
    ctx.strokeStyle = '#1a1a2e';

    let dibujando = false;
    let trazoVacio = true;

    function posicion(evento) {
        const rect = canvas.getBoundingClientRect();
        const punto = evento.touches ? evento.touches[0] : evento;
        return {
            x: (punto.clientX - rect.left) * (canvas.width / rect.width),
            y: (punto.clientY - rect.top) * (canvas.height / rect.height)
        };
    }

    function empezar(evento) {
        evento.preventDefault();
        dibujando = true;
        const p = posicion(evento);
        ctx.beginPath();
        ctx.moveTo(p.x, p.y);
    }

    function trazar(evento) {
        if (!dibujando) return;
        evento.preventDefault();
        const p = posicion(evento);
        ctx.lineTo(p.x, p.y);
        ctx.stroke();
        trazoVacio = false;
    }

    function terminar() {
        dibujando = false;
    }

    canvas.addEventListener('mousedown', empezar);
    canvas.addEventListener('mousemove', trazar);
    window.addEventListener('mouseup', terminar);
    canvas.addEventListener('touchstart', empezar, { passive: false });
    canvas.addEventListener('touchmove', trazar, { passive: false });
    canvas.addEventListener('touchend', terminar);

    if (boton) {
        boton.addEventListener('click', function () {
            ctx.clearRect(0, 0, canvas.width, canvas.height);
            trazoVacio = true;
        });
    }

    if (form) {
        form.addEventListener('submit', function (evento) {
            if (trazoVacio) {
                evento.preventDefault();
                alert('Captura la firma antes de guardar.');
                return;
            }
            input.value = canvas.toDataURL('image/png');
        });
    }
}
