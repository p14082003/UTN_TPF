using MySqlConnector;

namespace Negocio.Helpers
{
    internal static class ProcedimientoHelper
    {
        public static void ejecutar(string nombre, Action<AccesoDatos> accion)
        {
            ejecutar<object?>(nombre, datos =>
            {
                accion(datos);
                return null;
            });
        }

        public static T ejecutar<T>(string nombre, Func<AccesoDatos, T> accion)
        {
            AccesoDatos datos = new AccesoDatos();
            try
            {
                datos.setearProcedimiento(nombre);
                return accion(datos);
            }
            catch (MySqlException ex)
            {
                if (ex.SqlState == "45000")
                {
                    throw new ArgumentException(ex.Message, ex);
                }
                if (ex.Number == 1062)
                {
                    throw new ArgumentException("Ya existe un registro con esos datos únicos.", ex);
                }
                if (ex.Number == 1452)
                {
                    throw new ArgumentException("Una de las entidades relacionadas no existe.", ex);
                }
                throw;
            }
            finally
            {
                datos.cerrarConexion();
            }
        }

        public static MySqlDataReader leerFila(AccesoDatos datos)
        {
            datos.ejecutarLectura();
            MySqlDataReader? lector = datos.Lector;
            if (lector == null || !lector.Read())
            {
                throw new InvalidOperationException("El procedimiento no devolvió el resultado esperado.");
            }
            return lector;
        }

        public static int leerIdSalida(AccesoDatos datos, string parametro)
        {
            int id = Convert.ToInt32(datos.obtenerParametroSalida(parametro));
            if (id <= 0)
            {
                throw new InvalidOperationException("El procedimiento no devolvió el ID generado.");
            }
            return id;
        }
    }
}
