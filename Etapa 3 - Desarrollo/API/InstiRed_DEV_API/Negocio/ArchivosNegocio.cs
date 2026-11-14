using Negocio.Helpers;
using MySqlConnector;

namespace Negocio
{
    public class ArchivosNegocio
    {
        public string obtenerRutaAdministrador(int id, string tipo)
        {
            if (id <= 0)
            {
                throw new ArgumentException("El ID de la postulación debe ser mayor que cero.");
            }
            ArchivoHelper.validarTipo(tipo);
            string columna;

            if (tipo == "constancia-fiscal")
                columna = "rutaConstanciaFiscal";
            else if (tipo == "habilitacion")
                columna = "rutaHabilitacion";
            else
                columna = "rutaConstanciaLaboral";
            AccesoDatos datos = new AccesoDatos();
            try
            {
                // La columna se elige de la lista fija anterior, nunca de una ruta enviada por el cliente.
                datos.setearConsulta("SELECT " + columna + " FROM PostulacionesAdminInstitucion WHERE id = @id");
                datos.setearParametro("@id", id);
                datos.ejecutarLectura();
                MySqlDataReader? lector = datos.Lector;
                if (lector == null)
                {
                    throw new InvalidOperationException("No se inicializó el lector de datos.");
                }
                if (!lector.Read())
                {
                    throw new KeyNotFoundException("No se encontró la postulación solicitada.");
                }
                return (string)lector[columna];
            }
            finally
            {
                datos.cerrarConexion();
            }
        }
    }
}
