using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;

namespace Negocio.Helpers
{
    public static class ContraseñaHelper
    {
        private static readonly object contexto = new object();
        private static readonly PasswordHasher<object> hasher = new PasswordHasher<object>(
            Options.Create(new PasswordHasherOptions
            {
                CompatibilityMode = PasswordHasherCompatibilityMode.IdentityV3,
                IterationCount = 220000
            }));

        public static string generarHash(string clave)
        {
            if (string.IsNullOrWhiteSpace(clave))
            {
                throw new ArgumentException("La clave es obligatoria.");
            }
            if (clave.Length > 255)
            {
                throw new ArgumentException("La clave no puede superar los 255 caracteres.");
            }
            // PasswordHasher genera un salt aleatorio y lo incluye en el valor guardado.
            return hasher.HashPassword(contexto, clave);
        }

        public static bool verificar(string? hashGuardado, string? clave)
        {
            if (string.IsNullOrWhiteSpace(hashGuardado) || string.IsNullOrWhiteSpace(clave) || clave.Length > 255)
            {
                return false;
            }
            try
            {
                PasswordVerificationResult resultado = hasher.VerifyHashedPassword(contexto, hashGuardado, clave);
                return resultado == PasswordVerificationResult.Success ||
                    resultado == PasswordVerificationResult.SuccessRehashNeeded;
            }
            catch (FormatException)
            {
                // Un valor en texto plano o con formato inválido no autentica al usuario.
                return false;
            }
        }
    }
}
