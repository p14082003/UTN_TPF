using Dominio.DTOs;
using MySqlConnector;
using Negocio.Helpers;

namespace Negocio
{
    public class SP_CrearAdministradorInstitucionAdicionalNegocio
    {
        public SP_CrearAdministradorInstitucionAdicional_out crear(SP_CrearAdministradorInstitucionAdicional_in registro)
        {
            validar(registro);
            return ProcedimientoHelper.ejecutar("sp_AdministradorInstitucion_CrearAdicional", datos =>
            {
                datos.setearParametro("@p_idUsuarioActor", registro.IdUsuarioActor);
                datos.setearParametro("@p_nombre", registro.Nombre.Trim());
                datos.setearParametro("@p_apellido", registro.Apellido.Trim());
                datos.setearParametro("@p_documento", registro.Documento.Trim());
                datos.setearParametro("@p_domicilio", registro.Domicilio.Trim());
                datos.setearParametro("@p_fechaNacimiento", registro.FechaNacimiento.Date);
                datos.setearParametro("@p_clave", ContraseñaHelper.generarHash(registro.Clave));
                datos.setearParametro("@p_email", registro.Email.Trim().ToLowerInvariant());
                datos.setearParametro("@p_telefono", registro.Telefono.Trim());
                datos.setearParametroSalida("@p_idAdministrador", MySqlDbType.Int32);
                datos.ejecutarAccion();
                int id = ProcedimientoHelper.leerIdSalida(datos, "@p_idAdministrador");
                return new SP_CrearAdministradorInstitucionAdicional_out { IdAdministrador = id };
            });
        }

        private void validar(SP_CrearAdministradorInstitucionAdicional_in registro)
        {
            if (registro == null)
            {
                throw new ArgumentException("Debe indicar los datos de la operación.");
            }
            if (registro.IdUsuarioActor <= 0)
            {
                throw new ArgumentException("El campo IdUsuarioActor debe ser mayor que cero.");
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
        }
    }
}
