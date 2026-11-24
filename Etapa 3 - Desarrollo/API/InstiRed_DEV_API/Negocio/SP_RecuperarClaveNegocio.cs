using Dominio.DTOs;
using Negocio.Helpers;
using System.Net.Mail;
using System.Security.Cryptography;

namespace Negocio
{
    public class SP_RecuperarClaveNegocio
    {
        public SP_RecuperarClave_out recuperar(SP_RecuperarClave_in registro)
        {
            string email = registro?.Email?.Trim().ToLowerInvariant() ?? "";
            if (email.Length == 0 || email.Length > 100 ||
                !MailAddress.TryCreate(email, out var direccion) || direccion.Address != email)
            {
                throw new ArgumentException("Ingresá un correo electrónico válido de hasta 100 caracteres.");
            }

            var usuario = ProcedimientoHelper.ejecutar("sp_Olvide_Clave", datos =>
            {
                datos.setearParametro("@p_email", email);
                var lector = ProcedimientoHelper.leerFila(datos);
                return new SP_RecuperarClave_out
                {
                    IdUsuario = Convert.ToInt32(lector["IdUsuario"]),
                    Email = (string)lector["Email"],
                    Nombre = lector["Nombre"] as string,
                    Apellido = lector["Apellido"] as string
                };
            });

            usuario.NuevaClaveGenerada = GenerarClave();
            string hash = ContraseñaHelper.generarHash(usuario.NuevaClaveGenerada);
            ProcedimientoHelper.ejecutar("sp_Actualizar_Clave", datos =>
            {
                datos.setearParametro("@p_idUsuario", usuario.IdUsuario);
                datos.setearParametro("@p_clave", hash);
                datos.ejecutarAccion();
            });
            return usuario;
        }

        private static string GenerarClave()
        {
            const string caracteres = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789!@#$%*-_+";
            var clave = new char[12];
            do
            {
                for (int i = 0; i < clave.Length; i++)
                {
                    clave[i] = caracteres[RandomNumberGenerator.GetInt32(caracteres.Length)];
                }
            }
            while (!clave.Any(char.IsUpper) || !clave.Any(char.IsLower) || !clave.Any(char.IsDigit) ||
                !clave.Any(c => char.IsPunctuation(c) || char.IsSymbol(c)));
            return new string(clave);
        }
    }
}
