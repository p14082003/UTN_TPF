using Negocio.Helpers;
using System.Data;
using Dominio;
using MySqlConnector;

namespace Negocio
{
    public class Ent_ReseniaNegocio
    {
        public List<Ent_Resenia> listar()
        {
            List<Ent_Resenia> lista = new List<Ent_Resenia>();
            AccesoDatos datos = new AccesoDatos();
            try
            {
                datos.setearConsulta("SELECT `id`, `calificacion`, `comentario`, `respuestaInstitucion`, `fecha`, `idInstitucion`, `idResponsable` FROM `Resenias` ORDER BY `id`");
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

        public Ent_Resenia? obtenerPorId(int id)
        {
            if (id <= 0)
            {
                throw new ArgumentException("El campo Id debe ser mayor que cero.");
            }
            AccesoDatos datos = new AccesoDatos();
            try
            {
                datos.setearConsulta("SELECT `id`, `calificacion`, `comentario`, `respuestaInstitucion`, `fecha`, `idInstitucion`, `idResponsable` FROM `Resenias` WHERE `id` = @clave0");
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

        public int agregar(Ent_Resenia nuevo)
        {
            validar(nuevo);
            AccesoDatos datos = new AccesoDatos();
            try
            {
                datos.setearConsulta("INSERT INTO `Resenias` (`calificacion`, `comentario`, `respuestaInstitucion`, `fecha`, `idInstitucion`, `idResponsable`) VALUES (@calificacion, @comentario, @respuestaInstitucion, @fecha, @idInstitucion, @idResponsable)");
                datos.setearParametro("@calificacion", nuevo.Calificacion);
                datos.setearParametro("@comentario", nuevo.Comentario);
                datos.setearParametro("@respuestaInstitucion", nuevo.RespuestaInstitucion);
                datos.setearParametro("@fecha", nuevo.Fecha.Date);
                datos.setearParametro("@idInstitucion", nuevo.IdInstitucion);
                datos.setearParametro("@idResponsable", nuevo.IdResponsable);
                datos.ejecutarAccion();
                return Convert.ToInt32(datos.UltimoIdInsertado);
            }
            catch (MySqlException ex)
            {
                if (ex.Number == 1062)
                {
                    throw new ConstraintException("Ya existe un registro de Resenias con esos datos únicos.", ex);
                }
                if (ex.Number == 1452)
                {
                    throw new ConstraintException("No se puede guardar el registro de Resenias porque una de las entidades relacionadas no existe.", ex);
                }
                if (ex.Number == 1451)
                {
                    throw new ConstraintException("No se puede modificar o eliminar el registro de Resenias porque aparece en otras entidades.", ex);
                }
                throw;
            }
            finally
            {
                datos.cerrarConexion();
            }
        }

        public void actualizar(Ent_Resenia registro)
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
                datos.setearConsulta("UPDATE `Resenias` SET `calificacion` = @calificacion, `comentario` = @comentario, `respuestaInstitucion` = @respuestaInstitucion, `fecha` = @fecha, `idInstitucion` = @idInstitucion, `idResponsable` = @idResponsable WHERE `id` = @clave0");
                datos.setearParametro("@calificacion", registro.Calificacion);
                datos.setearParametro("@comentario", registro.Comentario);
                datos.setearParametro("@respuestaInstitucion", registro.RespuestaInstitucion);
                datos.setearParametro("@fecha", registro.Fecha.Date);
                datos.setearParametro("@idInstitucion", registro.IdInstitucion);
                datos.setearParametro("@idResponsable", registro.IdResponsable);
                datos.setearParametro("@clave0", id);
                if (datos.ejecutarAccion() == 0)
                {
                    throw new KeyNotFoundException("No se encontró el registro de Resenias que se desea actualizar.");
                }
            }
            catch (MySqlException ex)
            {
                if (ex.Number == 1062)
                {
                    throw new ConstraintException("Ya existe un registro de Resenias con esos datos únicos.", ex);
                }
                if (ex.Number == 1452)
                {
                    throw new ConstraintException("No se puede guardar el registro de Resenias porque una de las entidades relacionadas no existe.", ex);
                }
                if (ex.Number == 1451)
                {
                    throw new ConstraintException("No se puede modificar o eliminar el registro de Resenias porque aparece en otras entidades.", ex);
                }
                throw;
            }
            finally
            {
                datos.cerrarConexion();
            }
        }

        public void eliminar(int id, int? idResponsable = null)
        {
            if (id <= 0)
            {
                throw new ArgumentException("El campo Id debe ser mayor que cero.");
            }
            if (idResponsable != null && idResponsable <= 0)
            {
                throw new ArgumentException("El campo IdResponsable debe ser mayor que cero.");
            }
            AccesoDatos datos = new AccesoDatos();
            try
            {
                datos.setearConsulta("DELETE FROM `Resenias` WHERE `id` = @clave0 AND (@idResponsable IS NULL OR `idResponsable` = @idResponsable)");
                datos.setearParametro("@clave0", id);
                datos.setearParametro("@idResponsable", idResponsable);

                if (datos.ejecutarAccion() == 0)
                {
                    throw new KeyNotFoundException("No se encontró el registro de Resenias solicitado.");
                }
            }
            catch (MySqlException ex)
            {
                if (ex.Number == 1062)
                {
                    throw new ConstraintException("Ya existe un registro de Resenias con esos datos únicos.", ex);
                }
                if (ex.Number == 1452)
                {
                    throw new ConstraintException("No se puede guardar el registro de Resenias porque una de las entidades relacionadas no existe.", ex);
                }
                if (ex.Number == 1451)
                {
                    throw new ConstraintException("No se puede modificar o eliminar el registro de Resenias porque aparece en otras entidades.", ex);
                }
                throw;
            }
            finally
            {
                datos.cerrarConexion();
            }
        }

        public void eliminarRespuesta(int id, int idAdministradorInstitucion)
        {
            if (id <= 0 || idAdministradorInstitucion <= 0)
            {
                throw new ArgumentException("Debe indicar una reseña y un administrador válidos.");
            }
            AccesoDatos datos = new AccesoDatos();
            try
            {
                datos.setearConsulta("UPDATE Resenias r " +
                    "INNER JOIN AdministradoresInstitucion a ON a.idInstitucion = r.idInstitucion " +
                    "INNER JOIN Usuarios u ON u.id = a.idUsuario " +
                    "INNER JOIN Instituciones i ON i.id = r.idInstitucion " +
                    "SET r.respuestaInstitucion = NULL " +
                    "WHERE r.id = @id AND a.id = @idAdministradorInstitucion AND u.activo = TRUE " +
                    "AND i.estado = TRUE AND r.respuestaInstitucion IS NOT NULL");
                datos.setearParametro("@id", id);
                datos.setearParametro("@idAdministradorInstitucion", idAdministradorInstitucion);
                if (datos.ejecutarAccion() == 0)
                {
                    throw new KeyNotFoundException("No se encontró una respuesta para eliminar o el administrador no está habilitado para esa institución.");
                }
            }
            finally
            {
                datos.cerrarConexion();
            }
        }

        private Ent_Resenia leerRegistro(MySqlDataReader lector)
        {
            Ent_Resenia registro = new Ent_Resenia();
            registro.Id = Convert.ToInt32(lector["id"]);
            registro.Calificacion = Convert.ToInt32(lector["calificacion"]);
            registro.Comentario = (string)lector["comentario"];
            if (lector["respuestaInstitucion"] != DBNull.Value)
            {
                registro.RespuestaInstitucion = (string)lector["respuestaInstitucion"];
            }
            registro.Fecha = Convert.ToDateTime(lector["fecha"]);
            registro.IdInstitucion = Convert.ToInt32(lector["idInstitucion"]);
            registro.IdResponsable = Convert.ToInt32(lector["idResponsable"]);
            return registro;
        }

        private void validar(Ent_Resenia registro)
        {
            if (registro == null)
            {
                throw new ArgumentException("Debe indicar los datos de Resenia.");
            }
            if (registro.Calificacion < 1 || registro.Calificacion > 5)
            {
                throw new ArgumentException("El campo Calificacion debe estar entre 1 y 5.");
            }
            if (string.IsNullOrWhiteSpace(registro.Comentario))
            {
                throw new ArgumentException("El campo Comentario es obligatorio.");
            }
            if (registro.Comentario.Length > 255)
            {
                throw new ArgumentException("El campo Comentario no puede superar los 255 caracteres.");
            }
            if (registro.RespuestaInstitucion != null && registro.RespuestaInstitucion.Length > 255)
            {
                throw new ArgumentException("El campo RespuestaInstitucion no puede superar los 255 caracteres.");
            }
            if (registro.Fecha.Year < 1000)
            {
                throw new ArgumentException("El campo Fecha debe contener una fecha válida desde el año 1000.");
            }
            if (registro.IdInstitucion <= 0)
            {
                throw new ArgumentException("El campo IdInstitucion debe ser mayor que cero.");
            }
            if (registro.IdResponsable <= 0)
            {
                throw new ArgumentException("El campo IdResponsable debe ser mayor que cero.");
            }
        }
    }
}
