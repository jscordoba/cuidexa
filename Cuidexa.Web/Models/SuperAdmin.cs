namespace Cuidexa.Web.Models;

// Cuenta que crea Organizaciones/Centros nuevos (Fase 9). Deliberadamente
// fuera de Empleado: no tiene CentroId/OrganizacionId, opera por encima del
// modelo de tenant, así que necesita su propio esquema de autenticación
// (ver Program.cs) para no forzar a todo el resto del código a contemplar
// "este rol no tiene centro" como caso especial.
public class SuperAdmin : IProtegidaContraFuerzaBruta
{
    public int Id { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public bool Activo { get; set; } = true;

    // Bloqueo por fuerza bruta (ver Services/BloqueoCuenta.cs). Sin vía de
    // desbloqueo en la app (no hay "admin de SuperAdmin") — si se llega a
    // bloquear hace falta acceso directo a la base de datos.
    public int IntentosFallidos { get; set; }
    public DateTime? BloqueadoHasta { get; set; }
}
