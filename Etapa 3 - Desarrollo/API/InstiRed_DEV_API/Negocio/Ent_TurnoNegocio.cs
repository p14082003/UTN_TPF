using Negocio.Helpers;
using System.Data;
using Dominio;
using MySqlConnector;

namespace Negocio
{
    public class Ent_TurnoNegocio
    {
        public List<Ent_Turno> listar()
        {
            List<Ent_Turno> lista = new List<Ent_Turno>();
            AccesoDatos datos = new AccesoDatos();
            try
            {
                datos.setearConsulta("SELECT `id`, `descripcion` FROM `Turnos` ORDER BY `id`");
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

        public Ent_Turno? obtenerPorId(int id)
        {
            if (id <= 0)
            {
                throw new ArgumentException("El campo Id debe ser mayor que cero.");
            }
            AccesoDatos datos = new AccesoDatos();
            try
            {
                datos.setearConsulta("SELECT `id`, `descripcion` FROM `Turnos` WHERE `id` = @clave0");
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

        public int agregar(Ent_Turno nuevo)
        {
            validar(nuevo);
            AccesoDatos datos = new AccesoDatos();
            try
            {
                datos.setearConsulta("INSERT INTO `Turnos` (`descripcion`) VALUES (@descripcion)");
                datos.setearParametro("@descripcion", nuevo.Descripcion);
                datos.ejecutarAccion();
                return Convert.ToInt32(datos.UltimoIdInsertado);
            }
            catch (MySqlException ex)
            {
                if (ex.Number == 1062)
                {
                    throw new ConstraintException("Ya existe un registro de Turnos con esos datos únicos.", ex);
                }
                if (ex.Number == 1452)
                {
                    throw new ConstraintException("No se puede guardar el registro de Turnos porque una de las entidades relacionadas no existe.", ex);
                }
                if (ex.Number == 1451)
                {
                    throw new ConstraintException("No se puede modificar o eliminar el registro de Turnos porque aparece en otras entidades.", ex);
                }
                throw;
            }
            finally
            {
                datos.cerrarConexion();
            }
        }

        public void actualizar(Ent_Turno registro)
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
                datos.setearConsulta("UPDATE `Turnos` SET `descripcion` = @descripcion WHERE `id` = @clave0");
                datos.setearParametro("@descripcion", registro.Descripcion);
                datos.setearParametro("@clave0", id);
                if (datos.ejecutarAccion() == 0)
                {
                    throw new KeyNotFoundException("No se encontró el registro de Turnos que se desea actualizar.");
                }
            }
            catch (MySqlException ex)
            {
                if (ex.Number == 1062)
                {
                    throw new ConstraintException("Ya existe un registro de Turnos con esos datos únicos.", ex);
                }
                if (ex.Number == 1452)
                {
                    throw new ConstraintException("No se puede guardar el registro de Turnos porque una de las entidades relacionadas no existe.", ex);
                }
                if (ex.Number == 1451)
                {
                    throw new ConstraintException("No se puede modificar o eliminar el registro de Turnos porque aparece en otras entidades.", ex);
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
                datos.setearConsulta("DELETE FROM `Turnos` WHERE `id` = @clave0");
                datos.setearParametro("@clave0", id);

                if (datos.ejecutarAccion() == 0)
                {
                    throw new KeyNotFoundException("No se encontró el registro de Turnos solicitado.");
                }
            }
            catch (MySqlException ex)
            {
                if (ex.Number == 1062)
                {
                    throw new ConstraintException("Ya existe un registro de Turnos con esos datos únicos.", ex);
                }
                if (ex.Number == 1452)
                {
                    throw new ConstraintException("No se puede guardar el registro de Turnos porque una de las entidades relacionadas no existe.", ex);
                }
                if (ex.Number == 1451)
                {
                    throw new ConstraintException("No se puede modificar o eliminar el registro de Turnos porque aparece en otras entidades.", ex);
                }
                throw;
            }
            finally
            {
                datos.cerrarConexion();
            }
        }

        private Ent_Turno leerRegistro(MySqlDataReader lector)
        {
            Ent_Turno registro = new Ent_Turno();
            registro.Id = Convert.ToInt32(lector["id"]);
            registro.Descripcion = (string)lector["descripcion"];
            return registro;
        }

        private void validar(Ent_Turno registro)
        {
            if (registro == null)
            {
                throw new ArgumentException("Debe indicar los datos de Turno.");
            }
            if (string.IsNullOrWhiteSpace(registro.Descripcion))
            {
                throw new ArgumentException("El campo Descripcion es obligatorio.");
            }
            if (registro.Descripcion.Length > 20)
            {
                throw new ArgumentException("El campo Descripcion no puede superar los 20 caracteres.");
            }
        }
    }
}
