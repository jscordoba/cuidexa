namespace Cuidexa.Web.Services;

// BCrypt: incluye salting automático por contraseña y es deliberadamente lento
// (resistente a fuerza bruta), a diferencia del SHA256 simple usado antes.
public static class PasswordHasher
{
    public const int LongitudMinima = 8;

    // Único punto de la regla mínima de contraseña — antes solo la aplicaba
    // SuperAdminController al crear el primer Admin de un centro; ahora la
    // comparten todos los puntos de alta/cambio de contraseña (Empleado y
    // CuentaDispositivo), para no poder crear una cuenta con una contraseña
    // trivial que anule el coste de BCrypt.
    public static void Validar(string password)
    {
        if (string.IsNullOrWhiteSpace(password) || password.Length < LongitudMinima)
        {
            throw new InvalidOperationException($"La contraseña debe tener al menos {LongitudMinima} caracteres.");
        }
    }

    public static string Hash(string password) => BCrypt.Net.BCrypt.HashPassword(password, workFactor: 11);

    public static bool Verify(string password, string hash) => BCrypt.Net.BCrypt.Verify(password, hash);
}
