using Dominio.DTOs;
using MySqlConnector;
using Negocio.Helpers;

namespace Negocio
{
    public class SP_RegistrarResponsableNegocio
    {
        public SP_RegistrarResponsable_out registrar(SP_RegistrarResponsable_in registro)
        {
            validar(registro);
            string hash = ContraseñaHelper.generarHash(registro.Clave);
            AccesoDatos datos = new AccesoDatos();
            try
            {
                datos.setearProcedimiento("sp_Registrar_Responsable");
                datos.setearParametro("@p_clave", hash);
                datos.setearParametro("@p_email", registro.Email.Trim().ToLowerInvariant());
                datos.setearParametro("@p_documento", registro.Documento.Trim());
                datos.setearParametro("@p_nombre", registro.Nombre?.Trim());
                datos.setearParametro("@p_apellido", registro.Apellido?.Trim());
                datos.setearParametro("@p_domicilio", registro.Domicilio?.Trim());
                datos.setearParametro("@p_fechaNacimiento", registro.FechaNacimiento?.Date);
                datos.setearParametro("@p_telefono", registro.Telefono?.Trim());
                datos.ejecutarLectura();
                MySqlDataReader? lector = datos.Lector;
                if (lector == null)
                {
                    throw new InvalidOperationException("No se inicializó el lector de datos.");
                }
                if (!lector.Read())
                {
                    throw new InvalidOperationException("El procedimiento no devolvió los identificadores del registro.");
                }

                SP_RegistrarResponsable_out respuesta = new SP_RegistrarResponsable_out();
                respuesta.IdResponsable = Convert.ToInt32(lector["idResponsable"]);
                respuesta.IdUsuario = Convert.ToInt32(lector["idUsuario"]);
                respuesta.IdPersona = Convert.ToInt32(lector["idPersona"]);
                return respuesta;
            }
            catch (MySqlException ex)
            {
                if (ex.SqlState == "45000")
                {
                    throw new ArgumentException(ex.Message);
                }
                if (ex.Number == 1062)
                {
                    throw new ArgumentException("Ya existe un registro con ese email o documento.");
                }
                if (ex.Number == 1452)
                {
                    throw new ArgumentException("No se pudo registrar al responsable porque falta una entidad relacionada.");
                }
                throw;
            }
            finally
            {
                datos.cerrarConexion();
            }
        }

        private void validar(SP_RegistrarResponsable_in registro)
        {
            if (registro == null)
            {
                throw new ArgumentException("Debe indicar los datos del responsable.");
            }
            if (string.IsNullOrWhiteSpace(registro.Clave))
            {
                throw new ArgumentException("La clave es obligatoria.");
            }
            if (registro.Clave.Length > 255)
            {
                throw new ArgumentException("La clave no puede superar los 255 caracteres.");
            }
            if (string.IsNullOrWhiteSpace(registro.Email))
            {
                throw new ArgumentException("El email es obligatorio.");
            }
            if (registro.Email.Trim().Length > 100)
            {
                throw new ArgumentException("El email no puede superar los 100 caracteres.");
            }
            if (string.IsNullOrWhiteSpace(registro.Documento))
            {
                throw new ArgumentException("El documento es obligatorio.");
            }
            if (registro.Documento.Trim().Length > 8 || registro.Documento.Trim().StartsWith("0"))
            {
                throw new ArgumentException("El documento no puede superar los 8 caracteres ni empezar con 0.");
            }
            // El SP decide si son obligatorios: una Persona existente puede reutilizarse.
            if (registro.Nombre != null && registro.Nombre.Trim().Length > 50)
            {
                throw new ArgumentException("El nombre no puede superar los 50 caracteres.");
            }
            if (registro.Apellido != null && registro.Apellido.Trim().Length > 50)
            {
                throw new ArgumentException("El apellido no puede superar los 50 caracteres.");
            }
            if (registro.Domicilio != null && registro.Domicilio.Trim().Length > 100)
            {
                throw new ArgumentException("El domicilio no puede superar los 100 caracteres.");
            }
            if (registro.Telefono != null && registro.Telefono.Trim().Length > 30)
            {
                throw new ArgumentException("El teléfono no puede superar los 30 caracteres.");
            }
            if (registro.FechaNacimiento != null && registro.FechaNacimiento.Value.Year < 1000)
            {
                throw new ArgumentException("La fecha de nacimiento no es válida.");
            }
        }
    }
}
