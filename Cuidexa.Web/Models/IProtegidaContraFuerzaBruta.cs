namespace Cuidexa.Web.Models;

// Implementada por toda cuenta con login propio (Empleado, CuentaDispositivo,
// SuperAdmin) — permite a Services/BloqueoCuenta.cs aplicar la misma lógica
// de bloqueo tras fallos repetidos una sola vez, en vez de repetirla en los
// tres controladores de login.
public interface IProtegidaContraFuerzaBruta
{
    int IntentosFallidos { get; set; }
    DateTime? BloqueadoHasta { get; set; }
}
