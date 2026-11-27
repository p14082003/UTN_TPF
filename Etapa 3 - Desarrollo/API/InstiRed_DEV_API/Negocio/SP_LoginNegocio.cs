using Dominio.DTOs;
using MySqlConnector;
using Negocio.Helpers;

namespace Negocio
{
    public class SP_LoginNegocio
    {
        public SP_Login_out login(SP_Login_in ingreso)
        {
            validar(ingreso);
            AccesoDatos datos = new AccesoDatos();
            try
            {
                datos.setearProcedimiento("sp_Validar_Usuario_Login");
                datos.setearParametro("@p_email", ingreso.Email.Trim().ToLowerInvariant());
                // El SP requiere este parámetro; la comparación se hace únicamente en la API.
                datos.setearParametro("@p_clave", ingreso.Clave);
                datos.ejecutarLectura();
                MySqlDataReader? lector = datos.Lector;
                if (lector == null)
                {
                    throw new InvalidOperationException("No se inicializó el lector de datos.");
                }

                if (!lector.Read() || lector["Clave"] == DBNull.Value)
                {
                    return new SP_Login_out { Ok = false };
                }

                string hashGuardado = (string)lector["Clave"];
                if (!ContraseñaHelper.verificar(hashGuardado, ingreso.Clave))
                {
                    return new SP_Login_out { Ok = false };
                }

                var respuesta = new SP_Login_out
                {
                    Ok = true,
                    IdUsuario = Convert.ToInt32(lector["IdUsuario"]),
                    Email = (string)lector["Email"],
                    IdTipoUsuario = Convert.ToInt32(lector["IdTipoUsuario"])
                };
                if (lector["IdPersona"] != DBNull.Value)
                {
                    respuesta.IdPersona = Convert.ToInt32(lector["IdPersona"]);
                }
                if (lector["IdResponsable"] != DBNull.Value)
                {
                    respuesta.IdResponsable = Convert.ToInt32(lector["IdResponsable"]);
                }
                if (lector["IdAlumno"] != DBNull.Value)
                {
                    respuesta.IdAlumno = Convert.ToInt32(lector["IdAlumno"]);
                }
                if (lector["IdInstitucion"] != DBNull.Value)
                {
                    respuesta.IdInstitucion = Convert.ToInt32(lector["IdInstitucion"]);
                }
                if (lector["IdAdministradorInstitucion"] != DBNull.Value)
                {
                    respuesta.IdAdministradorInstitucion = Convert.ToInt32(lector["IdAdministradorInstitucion"]);
                }
                if (lector["Nombre"] != DBNull.Value)
                {
                    respuesta.Nombre = (string)lector["Nombre"];
                }
                if (lector["Apellido"] != DBNull.Value)
                {
                    respuesta.Apellido = (string)lector["Apellido"];
                }
                if (respuesta.IdTipoUsuario == 2 && respuesta.IdInstitucion.HasValue)
                {
                    // Se comprueba después de verificar la clave y también aplica a administradores adicionales.
                    datos.setearConsulta("SELECT estado FROM Instituciones WHERE id = @idInstitucion");
                    datos.setearParametro("@idInstitucion", respuesta.IdInstitucion.Value);
                    datos.ejecutarLectura();
                    var lectorInstitucion = datos.Lector;
                    if (lectorInstitucion == null)
                    {
                        throw new InvalidOperationException("No se inicializó el lector de la institución.");
                    }
                    if (!lectorInstitucion.Read() || !Convert.ToBoolean(lectorInstitucion["estado"]))
                    {
                        throw new UnauthorizedAccessException("Tu institución está dada de baja. Contactate con DevCo para consultar su reactivación.");
                    }
                }
                return respuesta;
            }
            catch (MySqlException ex)
            {
                if (ex.SqlState == "45000")
                {
                    // Usuario inexistente o inactivo: no se confirma el login.
                    return new SP_Login_out { Ok = false };
                }
                throw;
            }
            finally
            {
                datos.cerrarConexion();
            }
        }

        private void validar(SP_Login_in ingreso)
        {
            if (ingreso == null)
            {
                throw new ArgumentException("Debe indicar los datos de ingreso.");
            }
            if (string.IsNullOrWhiteSpace(ingreso.Email))
            {
                throw new ArgumentException("El email es obligatorio.");
            }
            if (ingreso.Email.Trim().Length > 100)
            {
                throw new ArgumentException("El email no puede superar los 100 caracteres.");
            }
            if (string.IsNullOrWhiteSpace(ingreso.Clave))
            {
                throw new ArgumentException("La clave es obligatoria.");
            }
            if (ingreso.Clave.Length > 255)
            {
                throw new ArgumentException("La clave no puede superar los 255 caracteres.");
            }
        }
    }
}
