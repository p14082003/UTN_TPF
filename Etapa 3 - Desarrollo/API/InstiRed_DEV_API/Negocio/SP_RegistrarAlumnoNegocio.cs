using Dominio.DTOs;
using MySqlConnector;
using Negocio.Helpers;

namespace Negocio
{
    public class SP_RegistrarAlumnoNegocio
    {
        public SP_RegistrarAlumno_out registrar(SP_RegistrarAlumno_in registro)
        {
            validar(registro);
            AccesoDatos datos = new AccesoDatos();
            try
            {
                datos.setearProcedimiento("sp_Registrar_Alumno");
                datos.setearParametro("@p_idResponsable", registro.IdResponsable);
                datos.setearParametro("@p_idRol", registro.IdRol);
                datos.setearParametro("@p_documento", registro.Documento.Trim());
                datos.setearParametro("@p_nombre", registro.Nombre?.Trim());
                datos.setearParametro("@p_apellido", registro.Apellido?.Trim());
                datos.setearParametro("@p_domicilio", registro.Domicilio?.Trim());
                datos.setearParametro("@p_fechaNacimiento", registro.FechaNacimiento?.Date);
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
                SP_RegistrarAlumno_out respuesta = new SP_RegistrarAlumno_out();
                respuesta.IdAlumno = Convert.ToInt32(lector["idAlumno"]);
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
                    throw new ArgumentException("Ya existe un registro con esos datos únicos.");
                }
                if (ex.Number == 1452)
                {
                    throw new ArgumentException("Una de las entidades relacionadas no existe.");
                }
                throw;
            }
            finally
            {
                datos.cerrarConexion();
            }
        }

        private void validar(SP_RegistrarAlumno_in registro)
        {
            if (registro == null)
            {
                throw new ArgumentException("Debe indicar los datos de la operación.");
            }
            if (registro.IdResponsable <= 0)
            {
                throw new ArgumentException("El campo IdResponsable debe ser mayor que cero.");
            }
            if (registro.IdRol != null && (registro.IdRol < -128 || registro.IdRol > 127))
            {
                throw new ArgumentException("El rol debe estar entre -128 y 127 o quedar sin especificar.");
            }
            if (string.IsNullOrWhiteSpace(registro.Documento))
            {
                throw new ArgumentException("El campo Documento es obligatorio.");
            }
            if (registro.Documento.Trim().Length > 8)
            {
                throw new ArgumentException("El campo Documento no puede superar los 8 caracteres.");
            }
            if (registro.Documento.Trim().StartsWith("0"))
            {
                throw new ArgumentException("El documento no puede empezar con 0.");
            }
            if (registro.Nombre != null && registro.Nombre.Trim().Length > 50)
            {
                throw new ArgumentException("El campo Nombre no puede superar los 50 caracteres.");
            }
            if (registro.Apellido != null && registro.Apellido.Trim().Length > 50)
            {
                throw new ArgumentException("El campo Apellido no puede superar los 50 caracteres.");
            }
            if (registro.Domicilio != null && registro.Domicilio.Trim().Length > 100)
            {
                throw new ArgumentException("El campo Domicilio no puede superar los 100 caracteres.");
            }
            if (registro.FechaNacimiento != null && registro.FechaNacimiento.Value.Year < 1000)
            {
                throw new ArgumentException("La fecha de nacimiento no es válida.");
            }
        }
    }
}
