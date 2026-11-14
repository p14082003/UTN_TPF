using Negocio.Helpers;
using System.Data;
using Dominio;
using MySqlConnector;

namespace Negocio
{
    public class Ent_ResponsablePorAlumnoNegocio
    {
        public List<Ent_ResponsablePorAlumno> listar()
        {
            List<Ent_ResponsablePorAlumno> lista = new List<Ent_ResponsablePorAlumno>();
            AccesoDatos datos = new AccesoDatos();
            try
            {
                datos.setearConsulta("SELECT `id`, `idResponsable`, `idAlumno`, `idRol` FROM `ResponsablesPorAlumno` ORDER BY `id`");
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

        public Ent_ResponsablePorAlumno? obtenerPorId(int id)
        {
            if (id <= 0)
            {
                throw new ArgumentException("El campo Id debe ser mayor que cero.");
            }
            AccesoDatos datos = new AccesoDatos();
            try
            {
                datos.setearConsulta("SELECT `id`, `idResponsable`, `idAlumno`, `idRol` FROM `ResponsablesPorAlumno` WHERE `id` = @clave0");
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

        public int agregar(Ent_ResponsablePorAlumno nuevo)
        {
            validar(nuevo);
            AccesoDatos datos = new AccesoDatos();
            try
            {
                datos.setearConsulta("INSERT INTO `ResponsablesPorAlumno` (`idResponsable`, `idAlumno`, `idRol`) VALUES (@idResponsable, @idAlumno, @idRol)");
                datos.setearParametro("@idResponsable", nuevo.IdResponsable);
                datos.setearParametro("@idAlumno", nuevo.IdAlumno);
                datos.setearParametro("@idRol", nuevo.IdRol);
                datos.ejecutarAccion();
                return Convert.ToInt32(datos.UltimoIdInsertado);
            }
            catch (MySqlException ex)
            {
                if (ex.Number == 1062)
                {
                    throw new ConstraintException("Ya existe un registro de ResponsablesPorAlumno con esos datos únicos.", ex);
                }
                if (ex.Number == 1452)
                {
                    throw new ConstraintException("No se puede guardar el registro de ResponsablesPorAlumno porque una de las entidades relacionadas no existe.", ex);
                }
                if (ex.Number == 1451)
                {
                    throw new ConstraintException("No se puede modificar o eliminar el registro de ResponsablesPorAlumno porque aparece en otras entidades.", ex);
                }
                throw;
            }
            finally
            {
                datos.cerrarConexion();
            }
        }

        public void actualizar(Ent_ResponsablePorAlumno registro)
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
                datos.setearConsulta("UPDATE `ResponsablesPorAlumno` SET `idResponsable` = @idResponsable, `idAlumno` = @idAlumno, `idRol` = @idRol WHERE `id` = @clave0");
                datos.setearParametro("@idResponsable", registro.IdResponsable);
                datos.setearParametro("@idAlumno", registro.IdAlumno);
                datos.setearParametro("@idRol", registro.IdRol);
                datos.setearParametro("@clave0", id);
                if (datos.ejecutarAccion() == 0)
                {
                    throw new KeyNotFoundException("No se encontró el registro de ResponsablesPorAlumno que se desea actualizar.");
                }
            }
            catch (MySqlException ex)
            {
                if (ex.Number == 1062)
                {
                    throw new ConstraintException("Ya existe un registro de ResponsablesPorAlumno con esos datos únicos.", ex);
                }
                if (ex.Number == 1452)
                {
                    throw new ConstraintException("No se puede guardar el registro de ResponsablesPorAlumno porque una de las entidades relacionadas no existe.", ex);
                }
                if (ex.Number == 1451)
                {
                    throw new ConstraintException("No se puede modificar o eliminar el registro de ResponsablesPorAlumno porque aparece en otras entidades.", ex);
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
                datos.setearConsulta("DELETE FROM `ResponsablesPorAlumno` WHERE `id` = @clave0");
                datos.setearParametro("@clave0", id);

                if (datos.ejecutarAccion() == 0)
                {
                    throw new KeyNotFoundException("No se encontró el registro de ResponsablesPorAlumno solicitado.");
                }
            }
            catch (MySqlException ex)
            {
                if (ex.Number == 1062)
                {
                    throw new ConstraintException("Ya existe un registro de ResponsablesPorAlumno con esos datos únicos.", ex);
                }
                if (ex.Number == 1452)
                {
                    throw new ConstraintException("No se puede guardar el registro de ResponsablesPorAlumno porque una de las entidades relacionadas no existe.", ex);
                }
                if (ex.Number == 1451)
                {
                    throw new ConstraintException("No se puede modificar o eliminar el registro de ResponsablesPorAlumno porque aparece en otras entidades.", ex);
                }
                throw;
            }
            finally
            {
                datos.cerrarConexion();
            }
        }

        private Ent_ResponsablePorAlumno leerRegistro(MySqlDataReader lector)
        {
            Ent_ResponsablePorAlumno registro = new Ent_ResponsablePorAlumno();
            registro.Id = Convert.ToInt32(lector["id"]);
            registro.IdResponsable = Convert.ToInt32(lector["idResponsable"]);
            registro.IdAlumno = Convert.ToInt32(lector["idAlumno"]);
            registro.IdRol = Convert.ToInt32(lector["idRol"]);
            return registro;
        }

        private void validar(Ent_ResponsablePorAlumno registro)
        {
            if (registro == null)
            {
                throw new ArgumentException("Debe indicar los datos de ResponsablePorAlumno.");
            }
            if (registro.IdResponsable <= 0)
            {
                throw new ArgumentException("El campo IdResponsable debe ser mayor que cero.");
            }
            if (registro.IdAlumno <= 0)
            {
                throw new ArgumentException("El campo IdAlumno debe ser mayor que cero.");
            }
            if (registro.IdRol <= 0 || registro.IdRol > 127)
            {
                throw new ArgumentException("El campo IdRol debe ser mayor que cero y no superar 127.");
            }
        }
    }
}
