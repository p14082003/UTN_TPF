using Dominio.DTOs;
using MySqlConnector;
using Negocio.Helpers;

namespace Negocio
{
    public class SP_RegistrarReseniaNegocio
    {
        public void registrar(SP_RegistrarResenia_in registro)
        {
            validar(registro);
            AccesoDatos datos = new AccesoDatos();
            try
            {
                datos.setearProcedimiento("sp_Registrar_Resenia");
                datos.setearParametro("@p_comentario", registro.Comentario);
                datos.setearParametro("@p_calificacion", registro.Calificacion);
                datos.setearParametro("@p_idPostulacion", registro.IdPostulacion);
                datos.setearParametro("@p_idResponsable", registro.IdResponsable);
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

        private void validar(SP_RegistrarResenia_in registro)
        {
            if (registro == null)
            {
                throw new ArgumentException("Debe indicar los datos de la operación.");
            }
            if (string.IsNullOrWhiteSpace(registro.Comentario))
            {
                throw new ArgumentException("El comentario es obligatorio.");
            }
            if (registro.Comentario.Length > 255)
            {
                throw new ArgumentException("El comentario no puede superar los 255 caracteres.");
            }
            if (registro.Calificacion < 1 || registro.Calificacion > 5)
            {
                throw new ArgumentException("La calificación debe estar entre 1 y 5.");
            }
            if (registro.IdPostulacion <= 0)
            {
                throw new ArgumentException("El campo IdPostulacion debe ser mayor que cero.");
            }
            if (registro.IdResponsable <= 0)
            {
                throw new ArgumentException("El campo IdResponsable debe ser mayor que cero.");
            }
        }
    }
}
