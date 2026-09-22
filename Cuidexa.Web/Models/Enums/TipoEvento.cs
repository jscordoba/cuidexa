namespace Cuidexa.Web.Models.Enums;

public enum TipoEvento
{
    Alta,
    Modificacion,
    Baja,
    Traslado,

    // Añadido al final a propósito: el enum se guarda como entero en la BD,
    // insertar en medio desplazaría los valores ya existentes.
    AvisoManual
}
