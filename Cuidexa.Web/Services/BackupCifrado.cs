using System.Security.Cryptography;

namespace Cuidexa.Web.Services;

// Cifrado de ficheros de backup (AES-256-GCM), mismo formato/razonamiento
// que EncryptionService pero operando sobre bytes de fichero en vez de
// cadenas de texto de un campo. Clave independiente de "Encryption:Key" a
// propósito (separación de claves por propósito) — ver "Backup:ClaveCifrado".
public static class BackupCifrado
{
    private const int NonceSizeBytes = 12;
    private const int TagSizeBytes = 16;

    public static async Task CifrarArchivoAsync(string rutaOrigen, string rutaDestino, byte[] clave)
    {
        var plainBytes = await File.ReadAllBytesAsync(rutaOrigen);

        var nonce = RandomNumberGenerator.GetBytes(NonceSizeBytes);
        var cipherBytes = new byte[plainBytes.Length];
        var tag = new byte[TagSizeBytes];

        using (var aesGcm = new AesGcm(clave, TagSizeBytes))
        {
            aesGcm.Encrypt(nonce, plainBytes, cipherBytes, tag);
        }

        var resultado = new byte[NonceSizeBytes + cipherBytes.Length + TagSizeBytes];
        Buffer.BlockCopy(nonce, 0, resultado, 0, NonceSizeBytes);
        Buffer.BlockCopy(cipherBytes, 0, resultado, NonceSizeBytes, cipherBytes.Length);
        Buffer.BlockCopy(tag, 0, resultado, NonceSizeBytes + cipherBytes.Length, TagSizeBytes);

        await File.WriteAllBytesAsync(rutaDestino, resultado);
    }

    public static async Task DescifrarArchivoAsync(string rutaOrigen, string rutaDestino, byte[] clave)
    {
        var data = await File.ReadAllBytesAsync(rutaOrigen);
        var nonce = data.AsSpan(0, NonceSizeBytes);
        var cipherBytes = data.AsSpan(NonceSizeBytes, data.Length - NonceSizeBytes - TagSizeBytes);
        var tag = data.AsSpan(data.Length - TagSizeBytes, TagSizeBytes);
        var plainBytes = new byte[cipherBytes.Length];

        using var aesGcm = new AesGcm(clave, TagSizeBytes);
        aesGcm.Decrypt(nonce, cipherBytes, tag, plainBytes);

        await File.WriteAllBytesAsync(rutaDestino, plainBytes);
    }
}
