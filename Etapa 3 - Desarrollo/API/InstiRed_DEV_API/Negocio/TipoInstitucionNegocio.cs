using System.Data;
using Dominio;
using MySqlConnector;

namespace Negocio
{
    public class TipoInstitucionNegocio
    {
        public List<TipoInstitucion> listar()
        {
            List<TipoInstitucion> lista = new List<TipoInstitucion>();
            AccesoDatos datos = new AccesoDatos();
            try
            {
                datos.setearConsulta("SELECT `id`, `tipoInstitucion` FROM `TipoInstitucion` ORDER BY `id`");
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

        public TipoInstitucion? obtenerPorId(int id)
        {
            if (id <= 0 || id > 127)
            {
                throw new ArgumentException("El campo Id debe ser mayor que cero y no superar 127.");
            }
            AccesoDatos datos = new AccesoDatos();
            try
            {
                datos.setearConsulta("SELECT `id`, `tipoInstitucion` FROM `TipoInstitucion` WHERE `id` = @clave0");
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

        public int agregar(TipoInstitucion nuevo)
        {
            validar(nuevo);
            AccesoDatos datos = new AccesoDatos();
            try
            {
                datos.setearConsulta("INSERT INTO `TipoInstitucion` (`tipoInstitucion`) VALUES (@tipoInstitucion)");
                datos.setearParametro("@tipoInstitucion", nuevo.TipoDeInstitucion);
                datos.ejecutarAccion();
                return Convert.ToInt32(datos.UltimoIdInsertado);
            }
            catch (MySqlException ex)
            {
                if (ex.Number == 1062)
                {
                    throw new ConstraintException("Ya existe un registro de TipoInstitucion con esos datos únicos.", ex);
                }
                if (ex.Number == 1452)
                {
                    throw new ConstraintException("No se puede guardar el registro de TipoInstitucion porque una de las entidades relacionadas no existe.", ex);
                }
                if (ex.Number == 1451)
                {
                    throw new ConstraintException("No se puede modificar o eliminar el registro de TipoInstitucion porque aparece en otras entidades.", ex);
                }
                throw;
            }
            finally
            {
                datos.cerrarConexion();
            }
        }

        public void actualizar(TipoInstitucion registro)
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
                datos.setearConsulta("UPDATE `TipoInstitucion` SET `tipoInstitucion` = @tipoInstitucion WHERE `id` = @clave0");
                datos.setearParametro("@tipoInstitucion", registro.TipoDeInstitucion);
                datos.setearParametro("@clave0", id);
                if (datos.ejecutarAccion() == 0)
                {
                    throw new KeyNotFoundException("No se encontró el registro de TipoInstitucion que se desea actualizar.");
                }
            }
            catch (MySqlException ex)
            {
                if (ex.Number == 1062)
                {
                    throw new ConstraintException("Ya existe un registro de TipoInstitucion con esos datos únicos.", ex);
                }
                if (ex.Number == 1452)
                {
                    throw new ConstraintException("No se puede guardar el registro de TipoInstitucion porque una de las entidades relacionadas no existe.", ex);
                }
                if (ex.Number == 1451)
                {
                    throw new ConstraintException("No se puede modificar o eliminar el registro de TipoInstitucion porque aparece en otras entidades.", ex);
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
                datos.setearConsulta("DELETE FROM `TipoInstitucion` WHERE `id` = @clave0");
                datos.setearParametro("@clave0", id);

                if (datos.ejecutarAccion() == 0)
                {
                    throw new KeyNotFoundException("No se encontró el registro de TipoInstitucion solicitado.");
                }
            }
            catch (MySqlException ex)
            {
                if (ex.Number == 1062)
                {
                    throw new ConstraintException("Ya existe un registro de TipoInstitucion con esos datos únicos.", ex);
                }
                if (ex.Number == 1452)
                {
                    throw new ConstraintException("No se puede guardar el registro de TipoInstitucion porque una de las entidades relacionadas no existe.", ex);
                }
                if (ex.Number == 1451)
                {
                    throw new ConstraintException("No se puede modificar o eliminar el registro de TipoInstitucion porque aparece en otras entidades.", ex);
                }
                throw;
            }
            finally
            {
                datos.cerrarConexion();
            }
        }

        private TipoInstitucion leerRegistro(MySqlDataReader lector)
        {
            TipoInstitucion registro = new TipoInstitucion();
            registro.Id = Convert.ToInt32(lector["id"]);
            registro.TipoDeInstitucion = (string)lector["tipoInstitucion"];
            return registro;
        }

        private void validar(TipoInstitucion registro)
        {
            if (registro == null)
            {
                throw new ArgumentException("Debe indicar los datos de TipoInstitucion.");
            }
            if (string.IsNullOrWhiteSpace(registro.TipoDeInstitucion))
            {
                throw new ArgumentException("El campo TipoDeInstitucion es obligatorio.");
            }
            if (registro.TipoDeInstitucion.Length > 20)
            {
                throw new ArgumentException("El campo TipoDeInstitucion no puede superar los 20 caracteres.");
            }
        }
    }
}
