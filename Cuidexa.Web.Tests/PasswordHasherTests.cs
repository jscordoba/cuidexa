using Cuidexa.Web.Services;

namespace Cuidexa.Web.Tests;

public class PasswordHasherTests
{
    [Fact]
    public void Hash_LuegoVerify_ConLaMismaContraseña_DevuelveTrue()
    {
        var hash = PasswordHasher.Hash("Demo1234!");

        Assert.True(PasswordHasher.Verify("Demo1234!", hash));
    }

    [Fact]
    public void Verify_ConContraseñaIncorrecta_DevuelveFalse()
    {
        var hash = PasswordHasher.Hash("Demo1234!");

        Assert.False(PasswordHasher.Verify("otra-contraseña", hash));
    }

    [Fact]
    public void Hash_GeneraUnSaltDistintoCadaVez()
    {
        // BCrypt incluye salt aleatorio — dos hashes de la misma contraseña
        // nunca deben coincidir, aunque ambos verifiquen correctamente.
        var hash1 = PasswordHasher.Hash("Demo1234!");
        var hash2 = PasswordHasher.Hash("Demo1234!");

        Assert.NotEqual(hash1, hash2);
        Assert.True(PasswordHasher.Verify("Demo1234!", hash1));
        Assert.True(PasswordHasher.Verify("Demo1234!", hash2));
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("1234567")] // 7 caracteres, uno menos del mínimo
    public void Validar_ConContraseñaDemasiadoCorta_Lanza(string password)
    {
        Assert.Throws<InvalidOperationException>(() => PasswordHasher.Validar(password));
    }

    [Fact]
    public void Validar_ConContraseñaDeLongitudMinima_NoLanza()
    {
        var excepcion = Record.Exception(() => PasswordHasher.Validar("12345678"));

        Assert.Null(excepcion);
    }
}
