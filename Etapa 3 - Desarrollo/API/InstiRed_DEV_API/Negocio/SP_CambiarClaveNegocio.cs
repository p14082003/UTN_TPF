using Dominio.DTOs;
using Negocio.Helpers;

namespace Negocio
{
    public class SP_CambiarClaveNegocio
    {
        public void cambiar(SP_CambiarClave_in registro)
        {
            validar(registro);
            ProcedimientoHelper.ejecutar("sp_Usuario_ObtenerClave", datos =>
            {
                datos.setearParametro("@p_idUsuarioActor", registro.IdUsuarioActor);
                datos.ejecutarLectura();
                var lector = datos.Lector;
                if (lector == null)
                {
                    throw new InvalidOperationException("No se inicializó el lector de datos.");
                }
                if (!lector.Read() || !ContraseñaHelper.verificar(lector["Clave"] as string, registro.ClaveActual))
                {
                    throw new ArgumentException("La contraseña actual es incorrecta o la cuenta no está activa.");
                }
            });

            string hashNuevo = ContraseñaHelper.generarHash(registro.ClaveNueva);
            ProcedimientoHelper.ejecutar("sp_Usuario_CambiarClave", datos =>
            {
                datos.setearParametro("@p_idUsuarioActor", registro.IdUsuarioActor);
                datos.setearParametro("@p_clave", hashNuevo);
                datos.ejecutarAccion();
            });
        }

        private void validar(SP_CambiarClave_in registro)
        {
            if (registro == null || registro.IdUsuarioActor <= 0)
            {
                throw new ArgumentException("Debe indicar un usuario válido.");
            }
            if (string.IsNullOrWhiteSpace(registro.ClaveActual) || registro.ClaveActual.Length > 255)
            {
                throw new ArgumentException("Debe indicar la contraseña actual, de hasta 255 caracteres.");
            }
            string clave = registro.ClaveNueva;
            if (string.IsNullOrWhiteSpace(clave) || clave.Length < 8 || clave.Length > 20 ||
                !clave.Any(char.IsUpper) || !clave.Any(char.IsLower) || !clave.Any(char.IsDigit) ||
                !clave.Any(c => char.IsPunctuation(c) || char.IsSymbol(c)))
            {
                throw new ArgumentException("La contraseña nueva debe tener entre 8 y 20 caracteres e incluir una mayúscula, una minúscula, un número y un símbolo.");
            }
            if (clave == registro.ClaveActual)
            {
                throw new ArgumentException("La contraseña nueva tiene que ser distinta de la actual.");
            }
        }
    }
}
