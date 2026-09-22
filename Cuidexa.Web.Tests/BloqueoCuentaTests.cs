using Cuidexa.Web.Models;
using Cuidexa.Web.Services;

namespace Cuidexa.Web.Tests;

public class BloqueoCuentaTests
{
    // SuperAdmin es la implementación más simple de IProtegidaContraFuerzaBruta
    // disponible (sin FKs ni propiedades de navegación que preparar).
    private static SuperAdmin NuevaCuenta() => new() { Nombre = "Test", Email = "test@test.local" };

    [Fact]
    public void EstaBloqueada_SinBloqueoPrevio_DevuelveFalse()
    {
        var cuenta = NuevaCuenta();

        Assert.False(BloqueoCuenta.EstaBloqueada(cuenta));
    }

    [Fact]
    public void EstaBloqueada_ConBloqueoYaExpirado_DevuelveFalse()
    {
        var cuenta = NuevaCuenta();
        cuenta.BloqueadoHasta = DateTime.UtcNow.AddMinutes(-1);

        Assert.False(BloqueoCuenta.EstaBloqueada(cuenta));
    }

    [Fact]
    public void EstaBloqueada_ConBloqueoFuturo_DevuelveTrue()
    {
        var cuenta = NuevaCuenta();
        cuenta.BloqueadoHasta = DateTime.UtcNow.AddMinutes(5);

        Assert.True(BloqueoCuenta.EstaBloqueada(cuenta));
    }

    [Fact]
    public void RegistrarFallo_PorDebajoDelUmbral_NoBloqueaLaCuenta()
    {
        var cuenta = NuevaCuenta();

        for (var i = 0; i < BloqueoCuenta.UmbralIntentos - 1; i++)
        {
            BloqueoCuenta.RegistrarFallo(cuenta);
        }

        Assert.Equal(BloqueoCuenta.UmbralIntentos - 1, cuenta.IntentosFallidos);
        Assert.False(BloqueoCuenta.EstaBloqueada(cuenta));
    }

    [Fact]
    public void RegistrarFallo_AlAlcanzarElUmbral_BloqueaLaCuenta()
    {
        var cuenta = NuevaCuenta();

        for (var i = 0; i < BloqueoCuenta.UmbralIntentos; i++)
        {
            BloqueoCuenta.RegistrarFallo(cuenta);
        }

        Assert.True(BloqueoCuenta.EstaBloqueada(cuenta));
        Assert.True(cuenta.BloqueadoHasta > DateTime.UtcNow.Add(BloqueoCuenta.DuracionBloqueo).AddSeconds(-5));
    }

    [Fact]
    public void Desbloquear_ReseteaIntentosYQuitaElBloqueo()
    {
        var cuenta = NuevaCuenta();
        for (var i = 0; i < BloqueoCuenta.UmbralIntentos; i++)
        {
            BloqueoCuenta.RegistrarFallo(cuenta);
        }

        BloqueoCuenta.Desbloquear(cuenta);

        Assert.Equal(0, cuenta.IntentosFallidos);
        Assert.Null(cuenta.BloqueadoHasta);
        Assert.False(BloqueoCuenta.EstaBloqueada(cuenta));
    }

    [Fact]
    public void MinutosRestantes_NuncaDevuelveCero()
    {
        var cuenta = NuevaCuenta();
        cuenta.BloqueadoHasta = DateTime.UtcNow.AddSeconds(10);

        Assert.Equal(1, BloqueoCuenta.MinutosRestantes(cuenta));
    }
}
