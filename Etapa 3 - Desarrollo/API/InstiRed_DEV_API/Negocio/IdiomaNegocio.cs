using System.Data;
using Dominio;
using MySqlConnector;

namespace Negocio
{
    public class IdiomaNegocio
    {
        public List<Idioma> listar()
        {
            List<Idioma> lista = new List<Idioma>();
            AccesoDatos datos = new AccesoDatos();
            try
            {
                datos.setearConsulta("SELECT `id`, `idioma` FROM `Idiomas` ORDER BY `id`");
                datos.ejecutarLectura();
                MySqlDataReader? lector = datos.Lector;
                if (lector == null)
                {
                    throw new InvalidOperationException("No se inicializó el lector de datos.");
                }
                while (lector.Read())
                {
                    lista.Add(leerRegistro(lector));
                }
                return lista;
            }
            finally
            {
                datos.cerrarConexion();
            }
        }

        public Idioma? obtenerPorId(int id)
        {
            if (id <= 0 || id > 127)
            {
                throw new ArgumentException("El campo Id debe ser mayor que cero y no superar 127.");
            }
            AccesoDatos datos = new AccesoDatos();
            try
            {
                datos.setearConsulta("SELECT `id`, `idioma` FROM `Idiomas` WHERE `id` = @clave0");
                datos.setearParametro("@clave0", id);
                datos.ejecutarLectura();
                MySqlDataReader? lector = datos.Lector;
                if (lector == null)
                {
                    throw new InvalidOperationException("No se inicializó el lector de datos.");
                }
                if (lector.Read())
                {
                    return leerRegistro(lector);
                }
                return null;
            }
            finally
            {
                datos.cerrarConexion();
            }
        }

        public int agregar(Idioma nuevo)
        {
            validar(nuevo);
            AccesoDatos datos = new AccesoDatos();
            try
            {
                datos.setearConsulta("INSERT INTO `Idiomas` (`idioma`) VALUES (@idioma)");
                datos.setearParametro("@idioma", nuevo.NombreIdioma);
                datos.ejecutarAccion();
                return Convert.ToInt32(datos.UltimoIdInsertado);
            }
            catch (MySqlException ex)
            {
                if (ex.Number == 1062)
                {
                    throw new ConstraintException("Ya existe un registro de Idiomas con esos datos únicos.", ex);
                }
                if (ex.Number == 1452)
                {
                    throw new ConstraintException("No se puede guardar el registro de Idiomas porque una de las entidades relacionadas no existe.", ex);
                }
                if (ex.Number == 1451)
                {
                    throw new ConstraintException("No se puede modificar o eliminar el registro de Idiomas porque aparece en otras entidades.", ex);
                }
                throw;
            }
            finally
            {
                datos.cerrarConexion();
            }
        }

        public void actualizar(Idioma registro)
        {
            validar(registro);
            int id = registro.Id;
            if (id <= 0 || id > 127)
            {
                throw new ArgumentException("El campo Id debe ser mayor que cero y no superar 127.");
            }
            AccesoDatos datos = new AccesoDatos();
            try
            {
                datos.setearConsulta("UPDATE `Idiomas` SET `idioma` = @idioma WHERE `id` = @clave0");
                datos.setearParametro("@idioma", registro.NombreIdioma);
                datos.setearParametro("@clave0", id);
                if (datos.ejecutarAccion() == 0)
                {
                    throw new KeyNotFoundException("No se encontró el registro de Idiomas que se desea actualizar.");
                }
            }
            catch (MySqlException ex)
            {
                if (ex.Number == 1062)
                {
                    throw new ConstraintException("Ya existe un registro de Idiomas con esos datos únicos.", ex);
                }
                if (ex.Number == 1452)
                {
                    throw new ConstraintException("No se puede guardar el registro de Idiomas porque una de las entidades relacionadas no existe.", ex);
                }
                if (ex.Number == 1451)
                {
                    throw new ConstraintException("No se puede modificar o eliminar el registro de Idiomas porque aparece en otras entidades.", ex);
                }
                throw;
            }
            finally
            {
                datos.cerrarConexion();
            }
        }

        public void eliminar(int id)
        {
            if (id <= 0 || id > 127)
            {
                throw new ArgumentException("El campo Id debe ser mayor que cero y no superar 127.");
            }
            AccesoDatos datos = new AccesoDatos();
            try
            {
                datos.setearConsulta("DELETE FROM `Idiomas` WHERE `id` = @clave0");
                datos.setearParametro("@clave0", id);

                if (datos.ejecutarAccion() == 0)
                {
                    throw new KeyNotFoundException("No se encontró el registro de Idiomas solicitado.");
                }
            }
            catch (MySqlException ex)
            {
                if (ex.Number == 1062)
                {
                    throw new ConstraintException("Ya existe un registro de Idiomas con esos datos únicos.", ex);
                }
                if (ex.Number == 1452)
                {
                    throw new ConstraintException("No se puede guardar el registro de Idiomas porque una de las entidades relacionadas no existe.", ex);
                }
                if (ex.Number == 1451)
                {
                    throw new ConstraintException("No se puede modificar o eliminar el registro de Idiomas porque aparece en otras entidades.", ex);
                }
                throw;
            }
            finally
            {
                datos.cerrarConexion();
            }
        }

        private Idioma leerRegistro(MySqlDataReader lector)
        {
            Idioma registro = new Idioma();
            registro.Id = Convert.ToInt32(lector["id"]);
            registro.NombreIdioma = (string)lector["idioma"];
            return registro;
        }

        private void validar(Idioma registro)
        {
            if (registro == null)
            {
                throw new ArgumentException("Debe indicar los datos de Idioma.");
            }
            if (string.IsNullOrWhiteSpace(registro.NombreIdioma))
            {
                throw new ArgumentException("El campo NombreIdioma es obligatorio.");
            }
            if (registro.NombreIdioma.Length > 20)
            {
                throw new ArgumentException("El campo NombreIdioma no puede superar los 20 caracteres.");
            }
        }
    }
}
