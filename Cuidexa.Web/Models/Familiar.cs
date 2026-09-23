namespace Cuidexa.Web.Models;

// Acceso de solo lectura para un familiar de un residente concreto (Fase 10
// — portal de familiares). Deliberadamente NO implementa ITieneCentro: su
// alcance real es un único Residente (más estrecho que un Centro entero), y
// vive en su propio esquema de cookie ("Familiar", ver Program.cs) — mismo
// motivo que SuperAdmin/Dispositivo, para no forzar al resto del código a
// contemplar "este acceso no tiene CentroId" como caso especial.
public class Familiar : IProtegidaContraFuerzaBruta
{
    public int Id { get; set; }

    public int ResidenteId { get; set; }
    public Residente? Residente { get; set; }

    public string Nombre { get; set; } = string.Empty;
    public string Relacion { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public bool Activo { get; set; } = true;
    public DateTime FechaCreacion { get; set; } = DateTime.UtcNow;

    public int IntentosFallidos { get; set; }
    public DateTime? BloqueadoHasta { get; set; }
}
