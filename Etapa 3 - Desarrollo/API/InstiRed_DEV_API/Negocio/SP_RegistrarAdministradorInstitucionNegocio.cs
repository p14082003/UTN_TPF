using Dominio.DTOs;
using MySqlConnector;
using Negocio.Helpers;
using System.Text.RegularExpressions;

namespace Negocio
{
    public class SP_RegistrarAdministradorInstitucionNegocio
    {
        public SP_RegistrarAdministradorInstitucion_out registrar(SP_RegistrarAdministradorInstitucion_in registro)
        {
            validar(registro);
            string hash = ContraseñaHelper.generarHash(registro.Clave);
            string codigoSeguimiento = "PAI-" + Guid.NewGuid().ToString("N").Substring(0, 26).ToUpperInvariant();
            AccesoDatos datos = new AccesoDatos();
            try
            {
                datos.setearProcedimiento("sp_AdministradorInstitucion_Registrar");
                datos.setearParametro("@p_nombre", registro.Nombre.Trim());
                datos.setearParametro("@p_apellido", registro.Apellido.Trim());
                datos.setearParametro("@p_documento", registro.Documento.Trim());
                datos.setearParametro("@p_domicilio", registro.Domicilio.Trim());
                datos.setearParametro("@p_fechaNacimiento", registro.FechaNacimiento.Date);
                datos.setearParametro("@p_clave", hash);
                datos.setearParametro("@p_email", registro.Email.Trim().ToLowerInvariant());
                datos.setearParametro("@p_telefono", registro.Telefono.Trim());
                datos.setearParametro("@p_nombreInstitucion", registro.NombreInstitucion.Trim());
                datos.setearParametro("@p_cuitInstitucion", registro.CuitInstitucion.Trim());
                datos.setearParametro("@p_rutaConstanciaFiscal", registro.RutaConstanciaFiscal);
                datos.setearParametro("@p_rutaHabilitacion", registro.RutaHabilitacion);
                datos.setearParametro("@p_rutaConstanciaLaboral", registro.RutaConstanciaLaboral);
                datos.setearParametro("@p_codigoSeguimiento", codigoSeguimiento);
                datos.setearParametroSalida("@p_idPostulacion", MySqlDbType.Int32);
                datos.ejecutarAccion();
                object? idGenerado = datos.obtenerParametroSalida("@p_idPostulacion");
                if (idGenerado == null || Convert.ToInt32(idGenerado) <= 0)
                {
                    throw new InvalidOperationException("El procedimiento no devolvió el ID de la postulación.");
                }
                return new SP_RegistrarAdministradorInstitucion_out
                {
                    IdPostulacion = Convert.ToInt32(idGenerado),
                    CodigoSeguimiento = codigoSeguimiento
                };
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

        private void validar(SP_RegistrarAdministradorInstitucion_in registro)
        {
            if (registro == null)
            {
                throw new ArgumentException("Debe indicar los datos de la operación.");
            }
            if (string.IsNullOrWhiteSpace(registro.Nombre))
            {
                throw new ArgumentException("El campo Nombre es obligatorio.");
            }
            if (registro.Nombre.Trim().Length > 50)
            {
                throw new ArgumentException("El campo Nombre no puede superar los 50 caracteres.");
            }
            if (string.IsNullOrWhiteSpace(registro.Apellido))
            {
                throw new ArgumentException("El campo Apellido es obligatorio.");
            }
            if (registro.Apellido.Trim().Length > 50)
            {
                throw new ArgumentException("El campo Apellido no puede superar los 50 caracteres.");
            }
            if (string.IsNullOrWhiteSpace(registro.Documento))
            {
                throw new ArgumentException("El campo Documento es obligatorio.");
            }
            if (registro.Documento.Trim().Length > 8)
            {
                throw new ArgumentException("El campo Documento no puede superar los 8 caracteres.");
            }
            if (!Regex.IsMatch(registro.Documento.Trim(), "^[0-9]{7,8}$"))
            {
                throw new ArgumentException("El documento debe tener 7 u 8 dígitos.");
            }
            if (string.IsNullOrWhiteSpace(registro.Domicilio))
            {
                throw new ArgumentException("El campo Domicilio es obligatorio.");
            }
            if (registro.Domicilio.Trim().Length > 100)
            {
                throw new ArgumentException("El campo Domicilio no puede superar los 100 caracteres.");
            }
            if (registro.FechaNacimiento.Year < 1000)
            {
                throw new ArgumentException("La fecha de nacimiento no es válida.");
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
                throw new ArgumentException("El campo Email es obligatorio.");
            }
            if (registro.Email.Trim().Length > 100)
            {
                throw new ArgumentException("El campo Email no puede superar los 100 caracteres.");
            }
            if (string.IsNullOrWhiteSpace(registro.Telefono))
            {
                throw new ArgumentException("El campo Telefono es obligatorio.");
            }
            if (registro.Telefono.Trim().Length > 30)
            {
                throw new ArgumentException("El campo Telefono no puede superar los 30 caracteres.");
            }
            if (string.IsNullOrWhiteSpace(registro.NombreInstitucion))
            {
                throw new ArgumentException("El campo NombreInstitucion es obligatorio.");
            }
            if (registro.NombreInstitucion.Trim().Length > 100)
            {
                throw new ArgumentException("El campo NombreInstitucion no puede superar los 100 caracteres.");
            }
            if (string.IsNullOrWhiteSpace(registro.CuitInstitucion))
            {
                throw new ArgumentException("El campo CuitInstitucion es obligatorio.");
            }
            if (registro.CuitInstitucion.Trim().Length > 11)
            {
                throw new ArgumentException("El campo CuitInstitucion no puede superar los 11 caracteres.");
            }
            if (!Regex.IsMatch(registro.CuitInstitucion.Trim(), "^[0-9]{11}$"))
            {
                throw new ArgumentException("El CUIT debe tener 11 dígitos, sin guiones.");
            }
            if (string.IsNullOrWhiteSpace(registro.RutaConstanciaFiscal))
            {
                throw new ArgumentException("El campo RutaConstanciaFiscal es obligatorio.");
            }
            if (registro.RutaConstanciaFiscal.Length > 255)
            {
                throw new ArgumentException("El campo RutaConstanciaFiscal no puede superar los 255 caracteres.");
            }
            if (string.IsNullOrWhiteSpace(registro.RutaHabilitacion))
            {
                throw new ArgumentException("El campo RutaHabilitacion es obligatorio.");
            }
            if (registro.RutaHabilitacion.Length > 255)
            {
                throw new ArgumentException("El campo RutaHabilitacion no puede superar los 255 caracteres.");
            }
            if (string.IsNullOrWhiteSpace(registro.RutaConstanciaLaboral))
            {
                throw new ArgumentException("El campo RutaConstanciaLaboral es obligatorio.");
            }
            if (registro.RutaConstanciaLaboral.Length > 255)
            {
                throw new ArgumentException("El campo RutaConstanciaLaboral no puede superar los 255 caracteres.");
            }
        }
    }
}
