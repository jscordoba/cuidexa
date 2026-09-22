using Cuidexa.Web.Models;

namespace Cuidexa.Web.Services;

// Bloqueo temporal tras fallos repetidos de login — complementa el rate
// limiting por IP (Program.cs) con un límite por CUENTA, que no se puede
// esquivar repartiendo los intentos entre varias IPs.
public static class BloqueoCuenta
{
    public const int UmbralIntentos = 5;
    public static readonly TimeSpan DuracionBloqueo = TimeSpan.FromMinutes(15);

    public static bool EstaBloqueada(IProtegidaContraFuerzaBruta cuenta) =>
        cuenta.BloqueadoHasta.HasValue && cuenta.BloqueadoHasta.Value > DateTime.UtcNow;

    // Minutos enteros redondeados hacia arriba, para el mensaje al usuario
    // ("inténtalo de nuevo en N minutos") — nunca "en 0 minutos".
    public static int MinutosRestantes(IProtegidaContraFuerzaBruta cuenta) =>
        Math.Max(1, (int)Math.Ceiling((cuenta.BloqueadoHasta!.Value - DateTime.UtcNow).TotalMinutes));

    public static void RegistrarFallo(IProtegidaContraFuerzaBruta cuenta)
    {
        cuenta.IntentosFallidos++;
        if (cuenta.IntentosFallidos >= UmbralIntentos)
        {
            cuenta.BloqueadoHasta = DateTime.UtcNow.Add(DuracionBloqueo);
        }
    }

    // Tras un login correcto, o cuando un Admin resetea la contraseña de la
    // cuenta (ver EmpleadoService/CuentaDispositivoService.ActualizarAsync) —
    // este segundo caso es la vía de "recuperación de cuenta" sin email.
    public static void Desbloquear(IProtegidaContraFuerzaBruta cuenta)
    {
        cuenta.IntentosFallidos = 0;
        cuenta.BloqueadoHasta = null;
    }
}
