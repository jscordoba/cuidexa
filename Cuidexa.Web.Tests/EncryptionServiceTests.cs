using System.Security.Cryptography;
using Cuidexa.Web.Services;

namespace Cuidexa.Web.Tests;

public class EncryptionServiceTests
{
    private static byte[] NuevaClave() => RandomNumberGenerator.GetBytes(32); // AES-256

    [Fact]
    public void Encrypt_LuegoDecrypt_DevuelveElTextoOriginal()
    {
        var clave = NuevaClave();

        var cifrado = EncryptionService.Encrypt("12345678A", clave);
        var descifrado = EncryptionService.Decrypt(cifrado, clave);

        Assert.Equal("12345678A", descifrado);
    }

    [Fact]
    public void Encrypt_ConLaMismaClave_GeneraUnCifradoDistintoCadaVez()
    {
        // El nonce es aleatorio en cada llamada — mismo texto y clave nunca
        // deben producir el mismo texto cifrado.
        var clave = NuevaClave();

        var cifrado1 = EncryptionService.Encrypt("dato-sensible", clave);
        var cifrado2 = EncryptionService.Encrypt("dato-sensible", clave);

        Assert.NotEqual(cifrado1, cifrado2);
    }

    [Fact]
    public void Decrypt_ConClaveDistinta_Lanza()
    {
        var cifrado = EncryptionService.Encrypt("dato-sensible", NuevaClave());

        Assert.ThrowsAny<CryptographicException>(() => EncryptionService.Decrypt(cifrado, NuevaClave()));
    }

    [Fact]
    public void Decrypt_ConTextoCifradoManipulado_Lanza()
    {
        var clave = NuevaClave();
        var cifrado = EncryptionService.Encrypt("dato-sensible", clave);
        var bytes = Convert.FromBase64String(cifrado);
        bytes[^1] ^= 0xFF; // corrompe el último byte (parte del tag de autenticación)
        var cifradoManipulado = Convert.ToBase64String(bytes);

        Assert.ThrowsAny<CryptographicException>(() => EncryptionService.Decrypt(cifradoManipulado, clave));
    }
}
