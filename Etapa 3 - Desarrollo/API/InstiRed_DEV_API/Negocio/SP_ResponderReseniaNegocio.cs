using Dominio.DTOs;
using MySqlConnector;
using Negocio.Helpers;

namespace Negocio
{
    public class SP_ResponderReseniaNegocio
    {
        public void responder(SP_ResponderResenia_in registro)
        {
            validar(registro);
            AccesoDatos datos = new AccesoDatos();
            try
            {
                datos.setearProcedimiento("sp_Responder_Resenia");
                datos.setearParametro("@p_idAdministradorInstitucion", registro.IdAdministradorInstitucion);
                datos.setearParametro("@p_idResenia", registro.IdResenia);
                datos.setearParametro("@p_respuestaInstitucion", registro.RespuestaInstitucion.Trim());
                datos.ejecutarAccion();
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

        private void validar(SP_ResponderResenia_in registro)
        {
            if (registro == null)
            {
                throw new ArgumentException("Debe indicar los datos de la operación.");
            }
            if (registro.IdAdministradorInstitucion <= 0)
            {
                throw new ArgumentException("El campo IdAdministradorInstitucion debe ser mayor que cero.");
            }
            if (registro.IdResenia <= 0)
            {
                throw new ArgumentException("El campo IdResenia debe ser mayor que cero.");
            }
            if (string.IsNullOrWhiteSpace(registro.RespuestaInstitucion))
            {
                throw new ArgumentException("Debe ingresar una respuesta en la reseña.");
            }
            if (registro.RespuestaInstitucion.Trim().Length > 255)
            {
                throw new ArgumentException("La respuesta no puede superar los 255 caracteres.");
            }
        }
    }
}
