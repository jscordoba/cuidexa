using System.Security.Cryptography;
using System.Text;

namespace Cuidexa.Web.Services;

// Cifrado simétrico (AES-256-GCM) para campos sensibles en reposo (documento de
// identidad, teléfono, email, contacto de emergencia). AES-GCM es autenticado:
// si el texto cifrado se manipula, Decrypt lanza en vez de devolver basura.
//
// La clave vive en appsettings.json ("Encryption:Key") solo para simplificar
// este MVP. Antes de producción, mover a un secreto gestionado (Key Vault,
// variables de entorno del entorno de despliegue) y prever rotación de clave.
public static class EncryptionService
{
    private const int NonceSizeBytes = 12;
    private const int TagSizeBytes = 16;

    public static string Encrypt(string plaintext, byte[] key)
    {
        var nonce = RandomNumberGenerator.GetBytes(NonceSizeBytes);
        var plainBytes = Encoding.UTF8.GetBytes(plaintext);
        var cipherBytes = new byte[plainBytes.Length];
        var tag = new byte[TagSizeBytes];

        using var aesGcm = new AesGcm(key, TagSizeBytes);
        aesGcm.Encrypt(nonce, plainBytes, cipherBytes, tag);

        var result = new byte[NonceSizeBytes + cipherBytes.Length + TagSizeBytes];
        Buffer.BlockCopy(nonce, 0, result, 0, NonceSizeBytes);
        Buffer.BlockCopy(cipherBytes, 0, result, NonceSizeBytes, cipherBytes.Length);
        Buffer.BlockCopy(tag, 0, result, NonceSizeBytes + cipherBytes.Length, TagSizeBytes);

        return Convert.ToBase64String(result);
    }

    public static string Decrypt(string ciphertextBase64, byte[] key)
    {
        var data = Convert.FromBase64String(ciphertextBase64);
        var nonce = data.AsSpan(0, NonceSizeBytes);
        var cipherBytes = data.AsSpan(NonceSizeBytes, data.Length - NonceSizeBytes - TagSizeBytes);
        var tag = data.AsSpan(data.Length - TagSizeBytes, TagSizeBytes);
        var plainBytes = new byte[cipherBytes.Length];

        using var aesGcm = new AesGcm(key, TagSizeBytes);
        aesGcm.Decrypt(nonce, cipherBytes, tag, plainBytes);

        return Encoding.UTF8.GetString(plainBytes);
    }
}
