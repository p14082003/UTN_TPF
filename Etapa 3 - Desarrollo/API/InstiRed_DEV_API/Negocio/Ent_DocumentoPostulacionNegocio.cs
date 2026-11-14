using Negocio.Helpers;
using System.Data;
using Dominio;
using MySqlConnector;

namespace Negocio
{
    public class Ent_DocumentoPostulacionNegocio
    {
        public List<Ent_DocumentoPostulacion> listar()
        {
            List<Ent_DocumentoPostulacion> lista = new List<Ent_DocumentoPostulacion>();
            AccesoDatos datos = new AccesoDatos();
            try
            {
                datos.setearConsulta("SELECT `id`, `idPostulacionAlumnos`, `descripcion`, `ruta` FROM `DocumentosPostulacion` ORDER BY `id`");
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

        public Ent_DocumentoPostulacion? obtenerPorId(int id)
        {
            if (id <= 0)
            {
                throw new ArgumentException("El campo Id debe ser mayor que cero.");
            }
            AccesoDatos datos = new AccesoDatos();
            try
            {
                datos.setearConsulta("SELECT `id`, `idPostulacionAlumnos`, `descripcion`, `ruta` FROM `DocumentosPostulacion` WHERE `id` = @clave0");
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

        public int agregar(Ent_DocumentoPostulacion nuevo)
        {
            validar(nuevo);
            AccesoDatos datos = new AccesoDatos();
            try
            {
                datos.setearConsulta("INSERT INTO `DocumentosPostulacion` (`idPostulacionAlumnos`, `descripcion`, `ruta`) VALUES (@idPostulacionAlumnos, @descripcion, @ruta)");
                datos.setearParametro("@idPostulacionAlumnos", nuevo.IdPostulacionAlumnos);
                datos.setearParametro("@descripcion", nuevo.Descripcion);
                datos.setearParametro("@ruta", nuevo.Ruta);
                datos.ejecutarAccion();
                return Convert.ToInt32(datos.UltimoIdInsertado);
            }
            catch (MySqlException ex)
            {
                if (ex.Number == 1062)
                {
                    throw new ConstraintException("Ya existe un registro de DocumentosPostulacion con esos datos únicos.", ex);
                }
                if (ex.Number == 1452)
                {
                    throw new ConstraintException("No se puede guardar el registro de DocumentosPostulacion porque una de las entidades relacionadas no existe.", ex);
                }
                if (ex.Number == 1451)
                {
                    throw new ConstraintException("No se puede modificar o eliminar el registro de DocumentosPostulacion porque aparece en otras entidades.", ex);
                }
                throw;
            }
            finally
            {
                datos.cerrarConexion();
            }
        }

        public void actualizar(Ent_DocumentoPostulacion registro)
        {
            validar(registro);
            int id = registro.Id;
            if (id <= 0)
            {
                throw new ArgumentException("El campo Id debe ser mayor que cero.");
            }
            AccesoDatos datos = new AccesoDatos();
            try
            {
                datos.setearConsulta("UPDATE `DocumentosPostulacion` SET `idPostulacionAlumnos` = @idPostulacionAlumnos, `descripcion` = @descripcion, `ruta` = @ruta WHERE `id` = @clave0");
                datos.setearParametro("@idPostulacionAlumnos", registro.IdPostulacionAlumnos);
                datos.setearParametro("@descripcion", registro.Descripcion);
                datos.setearParametro("@ruta", registro.Ruta);
                datos.setearParametro("@clave0", id);
                if (datos.ejecutarAccion() == 0)
                {
                    throw new KeyNotFoundException("No se encontró el registro de DocumentosPostulacion que se desea actualizar.");
                }
            }
            catch (MySqlException ex)
            {
                if (ex.Number == 1062)
                {
                    throw new ConstraintException("Ya existe un registro de DocumentosPostulacion con esos datos únicos.", ex);
                }
                if (ex.Number == 1452)
                {
                    throw new ConstraintException("No se puede guardar el registro de DocumentosPostulacion porque una de las entidades relacionadas no existe.", ex);
                }
                if (ex.Number == 1451)
                {
                    throw new ConstraintException("No se puede modificar o eliminar el registro de DocumentosPostulacion porque aparece en otras entidades.", ex);
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
            if (id <= 0)
            {
                throw new ArgumentException("El campo Id debe ser mayor que cero.");
            }
            AccesoDatos datos = new AccesoDatos();
            try
            {
                datos.setearConsulta("DELETE FROM `DocumentosPostulacion` WHERE `id` = @clave0");
                datos.setearParametro("@clave0", id);

                if (datos.ejecutarAccion() == 0)
                {
                    throw new KeyNotFoundException("No se encontró el registro de DocumentosPostulacion solicitado.");
                }
            }
            catch (MySqlException ex)
            {
                if (ex.Number == 1062)
                {
                    throw new ConstraintException("Ya existe un registro de DocumentosPostulacion con esos datos únicos.", ex);
                }
                if (ex.Number == 1452)
                {
                    throw new ConstraintException("No se puede guardar el registro de DocumentosPostulacion porque una de las entidades relacionadas no existe.", ex);
                }
                if (ex.Number == 1451)
                {
                    throw new ConstraintException("No se puede modificar o eliminar el registro de DocumentosPostulacion porque aparece en otras entidades.", ex);
                }
                throw;
            }
            finally
            {
                datos.cerrarConexion();
            }
        }

        private Ent_DocumentoPostulacion leerRegistro(MySqlDataReader lector)
        {
            Ent_DocumentoPostulacion registro = new Ent_DocumentoPostulacion();
            registro.Id = Convert.ToInt32(lector["id"]);
            registro.IdPostulacionAlumnos = Convert.ToInt32(lector["idPostulacionAlumnos"]);
            registro.Descripcion = (string)lector["descripcion"];
            registro.Ruta = (string)lector["ruta"];
            return registro;
        }

        private void validar(Ent_DocumentoPostulacion registro)
        {
            if (registro == null)
            {
                throw new ArgumentException("Debe indicar los datos de DocumentoPostulacion.");
            }
            if (registro.IdPostulacionAlumnos <= 0)
            {
                throw new ArgumentException("El campo IdPostulacionAlumnos debe ser mayor que cero.");
            }
            if (string.IsNullOrWhiteSpace(registro.Descripcion))
            {
                throw new ArgumentException("El campo Descripcion es obligatorio.");
            }
            if (registro.Descripcion.Length > 100)
            {
                throw new ArgumentException("El campo Descripcion no puede superar los 100 caracteres.");
            }
            if (string.IsNullOrWhiteSpace(registro.Ruta))
            {
                throw new ArgumentException("El campo Ruta es obligatorio.");
            }
            if (registro.Ruta.Length > 255)
            {
                throw new ArgumentException("El campo Ruta no puede superar los 255 caracteres.");
            }
        }
    }
}
