using Dominio.DTOs;
using MySqlConnector;
using Negocio.Helpers;

namespace Negocio
{
    public class SP_CrearInstitucionNegocio
    {
        public SP_CrearInstitucion_out crear(SP_CrearInstitucion_in registro)
        {
            validar(registro);
            return ProcedimientoHelper.ejecutar("sp_Institucion_Crear", datos =>
            {
                datos.setearParametro("@p_idAdministrador", registro.IdAdministrador);
                datos.setearParametro("@p_direccion", registro.Direccion.Trim());
                datos.setearParametro("@p_email", registro.Email.Trim().ToLowerInvariant());
                datos.setearParametro("@p_latitud", registro.Latitud);
                datos.setearParametro("@p_longitud", registro.Longitud);
                datos.setearParametro("@p_descripcion", registro.Descripcion.Trim());
                datos.setearParametro("@p_idTipoInstitucion", registro.IdTipoInstitucion);
                datos.setearParametro("@p_movilidad", registro.Movilidad);
                datos.setearParametro("@p_visual", registro.Visual);
                datos.setearParametro("@p_auditiva", registro.Auditiva);
                datos.setearParametro("@p_verbal", registro.Verbal);
                datos.setearParametro("@p_intelectual", registro.Intelectual);
                datos.setearParametro("@p_sindromeDown", registro.SindromeDown);
                datos.setearParametro("@p_autismo", registro.Autismo);
                datos.setearParametroSalida("@p_idInstitucion", MySqlDbType.Int32);
                datos.ejecutarAccion();
                int id = ProcedimientoHelper.leerIdSalida(datos, "@p_idInstitucion");
                return new SP_CrearInstitucion_out { IdInstitucion = id };
            });
        }

        private void validar(SP_CrearInstitucion_in registro)
        {
            if (registro == null)
            {
                throw new ArgumentException("Debe indicar los datos de la operación.");
            }
            if (registro.IdAdministrador <= 0)
            {
                throw new ArgumentException("El campo IdAdministrador debe ser mayor que cero.");
            }
            if (string.IsNullOrWhiteSpace(registro.Direccion))
            {
                throw new ArgumentException("El campo Direccion es obligatorio.");
            }
            if (registro.Direccion.Trim().Length > 100)
            {
                throw new ArgumentException("El campo Direccion no puede superar los 100 caracteres.");
            }
            if (string.IsNullOrWhiteSpace(registro.Email))
            {
                throw new ArgumentException("El campo Email es obligatorio.");
            }
            if (registro.Email.Trim().Length > 100)
            {
                throw new ArgumentException("El campo Email no puede superar los 100 caracteres.");
            }
            if (registro.Latitud == null || !double.IsFinite(registro.Latitud.Value) || registro.Latitud < -90 || registro.Latitud > 90)
            {
                throw new ArgumentException("El campo Latitud debe estar entre -90 y 90.");
            }
            if (registro.Longitud == null || !double.IsFinite(registro.Longitud.Value) || registro.Longitud < -180 || registro.Longitud > 180)
            {
                throw new ArgumentException("El campo Longitud debe estar entre -180 y 180.");
            }
            if (string.IsNullOrWhiteSpace(registro.Descripcion))
            {
                throw new ArgumentException("El campo Descripcion es obligatorio.");
            }
            if (registro.Descripcion.Trim().Length > 3000)
            {
                throw new ArgumentException("El campo Descripcion no puede superar los 3000 caracteres.");
            }
            if (registro.IdTipoInstitucion <= 0 || registro.IdTipoInstitucion > 127)
            {
                throw new ArgumentException("El campo IdTipoInstitucion debe estar entre 1 y 127.");
            }
            if (registro.Movilidad == null || registro.Visual == null || registro.Auditiva == null ||
                registro.Verbal == null || registro.Intelectual == null || registro.SindromeDown == null || registro.Autismo == null)
            {
                throw new ArgumentException("Debe indicar los siete valores de accesibilidad.");
            }
        }
    }
}
