using Negocio.Helpers;
using System.Data;
using Dominio;
using MySqlConnector;

namespace Negocio
{
    public class Ent_PostulacionAlumnoNegocio
    {
        public List<Ent_PostulacionAlumno> listar()
        {
            List<Ent_PostulacionAlumno> lista = new List<Ent_PostulacionAlumno>();
            AccesoDatos datos = new AccesoDatos();
            try
            {
                datos.setearConsulta("SELECT `id`, `fecha`, `codigoSeguimiento`, `idCurso`, `idAlumno`, `idEstado`, `motivoRechazo` FROM `PostulacionesAlumnos` ORDER BY `id`");
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

        public Ent_PostulacionAlumno? obtenerPorId(int id)
        {
            if (id <= 0)
            {
                throw new ArgumentException("El campo Id debe ser mayor que cero.");
            }
            AccesoDatos datos = new AccesoDatos();
            try
            {
                datos.setearConsulta("SELECT `id`, `fecha`, `codigoSeguimiento`, `idCurso`, `idAlumno`, `idEstado`, `motivoRechazo` FROM `PostulacionesAlumnos` WHERE `id` = @clave0");
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

        public int agregar(Ent_PostulacionAlumno nuevo)
        {
            validar(nuevo);
            AccesoDatos datos = new AccesoDatos();
            try
            {
                datos.setearConsulta("INSERT INTO `PostulacionesAlumnos` (`fecha`, `codigoSeguimiento`, `idCurso`, `idAlumno`, `idEstado`, `motivoRechazo`) VALUES (@fecha, @codigoSeguimiento, @idCurso, @idAlumno, @idEstado, @motivoRechazo)");
                datos.setearParametro("@fecha", nuevo.Fecha.Date);
                datos.setearParametro("@codigoSeguimiento", nuevo.CodigoSeguimiento);
                datos.setearParametro("@idCurso", nuevo.IdCurso);
                datos.setearParametro("@idAlumno", nuevo.IdAlumno);
                datos.setearParametro("@idEstado", nuevo.IdEstado);
                datos.setearParametro("@motivoRechazo", nuevo.MotivoRechazo);
                datos.ejecutarAccion();
                return Convert.ToInt32(datos.UltimoIdInsertado);
            }
            catch (MySqlException ex)
            {
                if (ex.Number == 1062)
                {
                    throw new ConstraintException("Ya existe un registro de PostulacionesAlumnos con esos datos únicos.", ex);
                }
                if (ex.Number == 1452)
                {
                    throw new ConstraintException("No se puede guardar el registro de PostulacionesAlumnos porque una de las entidades relacionadas no existe.", ex);
                }
                if (ex.Number == 1451)
                {
                    throw new ConstraintException("No se puede modificar o eliminar el registro de PostulacionesAlumnos porque aparece en otras entidades.", ex);
                }
                throw;
            }
            finally
            {
                datos.cerrarConexion();
            }
        }

        public void actualizar(Ent_PostulacionAlumno registro)
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
                datos.setearConsulta("UPDATE `PostulacionesAlumnos` SET `fecha` = @fecha, `codigoSeguimiento` = @codigoSeguimiento, `idCurso` = @idCurso, `idAlumno` = @idAlumno, `idEstado` = @idEstado, `motivoRechazo` = @motivoRechazo WHERE `id` = @clave0");
                datos.setearParametro("@fecha", registro.Fecha.Date);
                datos.setearParametro("@codigoSeguimiento", registro.CodigoSeguimiento);
                datos.setearParametro("@idCurso", registro.IdCurso);
                datos.setearParametro("@idAlumno", registro.IdAlumno);
                datos.setearParametro("@idEstado", registro.IdEstado);
                datos.setearParametro("@motivoRechazo", registro.MotivoRechazo);
                datos.setearParametro("@clave0", id);
                if (datos.ejecutarAccion() == 0)
                {
                    throw new KeyNotFoundException("No se encontró el registro de PostulacionesAlumnos que se desea actualizar.");
                }
            }
            catch (MySqlException ex)
            {
                if (ex.Number == 1062)
                {
                    throw new ConstraintException("Ya existe un registro de PostulacionesAlumnos con esos datos únicos.", ex);
                }
                if (ex.Number == 1452)
                {
                    throw new ConstraintException("No se puede guardar el registro de PostulacionesAlumnos porque una de las entidades relacionadas no existe.", ex);
                }
                if (ex.Number == 1451)
                {
                    throw new ConstraintException("No se puede modificar o eliminar el registro de PostulacionesAlumnos porque aparece en otras entidades.", ex);
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
                datos.setearConsulta("DELETE FROM `PostulacionesAlumnos` WHERE `id` = @clave0");
                datos.setearParametro("@clave0", id);

                if (datos.ejecutarAccion() == 0)
                {
                    throw new KeyNotFoundException("No se encontró el registro de PostulacionesAlumnos solicitado.");
                }
            }
            catch (MySqlException ex)
            {
                if (ex.Number == 1062)
                {
                    throw new ConstraintException("Ya existe un registro de PostulacionesAlumnos con esos datos únicos.", ex);
                }
                if (ex.Number == 1452)
                {
                    throw new ConstraintException("No se puede guardar el registro de PostulacionesAlumnos porque una de las entidades relacionadas no existe.", ex);
                }
                if (ex.Number == 1451)
                {
                    throw new ConstraintException("No se puede modificar o eliminar el registro de PostulacionesAlumnos porque aparece en otras entidades.", ex);
                }
                throw;
            }
            finally
            {
                datos.cerrarConexion();
            }
        }

        private Ent_PostulacionAlumno leerRegistro(MySqlDataReader lector)
        {
            Ent_PostulacionAlumno registro = new Ent_PostulacionAlumno();
            registro.Id = Convert.ToInt32(lector["id"]);
            registro.Fecha = Convert.ToDateTime(lector["fecha"]);
            registro.CodigoSeguimiento = (string)lector["codigoSeguimiento"];
            registro.IdCurso = Convert.ToInt32(lector["idCurso"]);
            registro.IdAlumno = Convert.ToInt32(lector["idAlumno"]);
            registro.IdEstado = Convert.ToInt32(lector["idEstado"]);
            if (lector["motivoRechazo"] != DBNull.Value)
            {
                registro.MotivoRechazo = (string)lector["motivoRechazo"];
            }
            return registro;
        }

        private void validar(Ent_PostulacionAlumno registro)
        {
            if (registro == null)
            {
                throw new ArgumentException("Debe indicar los datos de PostulacionAlumno.");
            }
            if (registro.Fecha.Year < 1000)
            {
                throw new ArgumentException("El campo Fecha debe contener una fecha válida desde el año 1000.");
            }
            if (string.IsNullOrWhiteSpace(registro.CodigoSeguimiento))
            {
                throw new ArgumentException("El campo CodigoSeguimiento es obligatorio.");
            }
            if (registro.CodigoSeguimiento.Length > 30)
            {
                throw new ArgumentException("El campo CodigoSeguimiento no puede superar los 30 caracteres.");
            }
            if (registro.IdCurso <= 0)
            {
                throw new ArgumentException("El campo IdCurso debe ser mayor que cero.");
            }
            if (registro.IdAlumno <= 0)
            {
                throw new ArgumentException("El campo IdAlumno debe ser mayor que cero.");
            }
            if (registro.IdEstado <= 0)
            {
                throw new ArgumentException("El campo IdEstado debe ser mayor que cero.");
            }
            if (registro.MotivoRechazo != null && registro.MotivoRechazo.Length > 255)
            {
                throw new ArgumentException("El campo MotivoRechazo no puede superar los 255 caracteres.");
            }
        }
    }
}
