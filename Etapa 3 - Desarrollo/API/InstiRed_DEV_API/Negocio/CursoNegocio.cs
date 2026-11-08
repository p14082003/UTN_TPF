using System.Data;
using Dominio;
using MySqlConnector;

namespace Negocio
{
    public class CursoNegocio
    {
        public List<Curso> listar()
        {
            List<Curso> lista = new List<Curso>();
            AccesoDatos datos = new AccesoDatos();
            try
            {
                datos.setearConsulta("SELECT id, idInstitucion, idTurno, cicloLectivo, idNivel, anio, nivelPersonalizado, division, capacidad, inscriptos FROM Cursos ORDER BY id");
                datos.ejecutarLectura();
                MySqlDataReader? lector = datos.Lector;
                if (lector == null)
                {
                    throw new InvalidOperationException("No se inicializó el lector de datos.");
                }

                while (lector.Read())
                {
                    lista.Add(leerCurso(lector));
                }
                return lista;
            }
            finally
            {
                datos.cerrarConexion();
            }
        }

        public Curso? obtenerPorId(int id)
        {
            if (id <= 0)
            {
                throw new ArgumentException("El ID del curso debe ser mayor que cero.");
            }

            AccesoDatos datos = new AccesoDatos();
            try
            {
                datos.setearConsulta("SELECT id, idInstitucion, idTurno, cicloLectivo, idNivel, anio, nivelPersonalizado, division, capacidad, inscriptos FROM Cursos WHERE id = @id");
                datos.setearParametro("@id", id);
                datos.ejecutarLectura();
                MySqlDataReader? lector = datos.Lector;
                if (lector == null)
                {
                    throw new InvalidOperationException("No se inicializó el lector de datos.");
                }

                if (lector.Read())
                {
                    return leerCurso(lector);
                }
                return null;
            }
            finally
            {
                datos.cerrarConexion();
            }
        }

        public int agregar(Curso nuevo)
        {
            validar(nuevo);
            AccesoDatos datos = new AccesoDatos();
            try
            {
                datos.setearConsulta("INSERT INTO Cursos (idInstitucion, idTurno, cicloLectivo, idNivel, anio, nivelPersonalizado, division, capacidad, inscriptos) VALUES (@idInstitucion, @idTurno, @cicloLectivo, @idNivel, @anio, @nivelPersonalizado, @division, @capacidad, @inscriptos)");
                setearParametros(datos, nuevo);
                datos.ejecutarAccion();
                return Convert.ToInt32(datos.UltimoIdInsertado);
            }
            catch (MySqlException ex)
            {
                if (ex.Number == 1452)
                {
                    throw new ConstraintException("La institución, el turno o el nivel escolar indicado no existe.", ex);
                }
                throw;
            }
            finally
            {
                datos.cerrarConexion();
            }
        }

        public void actualizar(Curso curso)
        {
            validar(curso);
            if (curso.Id <= 0)
            {
                throw new ArgumentException("El ID del curso debe ser mayor que cero.");
            }

            AccesoDatos datos = new AccesoDatos();
            try
            {
                datos.setearConsulta("UPDATE Cursos SET idInstitucion = @idInstitucion, idTurno = @idTurno, cicloLectivo = @cicloLectivo, idNivel = @idNivel, anio = @anio, nivelPersonalizado = @nivelPersonalizado, division = @division, capacidad = @capacidad, inscriptos = @inscriptos WHERE id = @id");
                setearParametros(datos, curso);
                datos.setearParametro("@id", curso.Id);
                int filas = datos.ejecutarAccion();
                if (filas == 0)
                {
                    throw new KeyNotFoundException("No se encontró el curso que se desea actualizar.");
                }
            }
            catch (MySqlException ex)
            {
                if (ex.Number == 1452)
                {
                    throw new ConstraintException("La institución, el turno o el nivel escolar indicado no existe.", ex);
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
                throw new ArgumentException("El ID del curso debe ser mayor que cero.");
            }

            AccesoDatos datos = new AccesoDatos();
            try
            {
                datos.setearConsulta("DELETE FROM Cursos WHERE id = @id");
                datos.setearParametro("@id", id);
                int filas = datos.ejecutarAccion();
                if (filas == 0)
                {
                    throw new KeyNotFoundException("No se encontró el curso que se desea eliminar.");
                }
            }
            catch (MySqlException ex)
            {
                if (ex.Number == 1451)
                {
                    throw new ConstraintException("No se puede eliminar el curso porque está relacionado con otros registros.", ex);
                }
                throw;
            }
            finally
            {
                datos.cerrarConexion();
            }
        }

        private Curso leerCurso(MySqlDataReader lector)
        {
            Curso curso = new Curso();
            curso.Id = Convert.ToInt32(lector["id"]);
            curso.IdInstitucion = Convert.ToInt32(lector["idInstitucion"]);
            curso.IdTurno = Convert.ToInt32(lector["idTurno"]);
            curso.CicloLectivo = Convert.ToInt32(lector["cicloLectivo"]);
            curso.Division = (string)lector["division"];
            curso.Capacidad = Convert.ToInt32(lector["capacidad"]);
            curso.Inscriptos = Convert.ToInt32(lector["inscriptos"]);

            if (lector["idNivel"] != DBNull.Value)
            {
                curso.IdNivel = Convert.ToInt32(lector["idNivel"]);
            }
            if (lector["anio"] != DBNull.Value)
            {
                curso.Anio = Convert.ToInt32(lector["anio"]);
            }
            if (lector["nivelPersonalizado"] != DBNull.Value)
            {
                curso.NivelPersonalizado = (string)lector["nivelPersonalizado"];
            }
            return curso;
        }

        private void setearParametros(AccesoDatos datos, Curso curso)
        {
            datos.setearParametro("@idInstitucion", curso.IdInstitucion);
            datos.setearParametro("@idTurno", curso.IdTurno);
            datos.setearParametro("@cicloLectivo", curso.CicloLectivo);
            datos.setearParametro("@idNivel", curso.IdNivel);
            datos.setearParametro("@anio", curso.Anio);
            datos.setearParametro("@nivelPersonalizado", curso.NivelPersonalizado);
            datos.setearParametro("@division", curso.Division);
            datos.setearParametro("@capacidad", curso.Capacidad);
            datos.setearParametro("@inscriptos", curso.Inscriptos);
        }

        private void validar(Curso curso)
        {
            if (curso == null)
            {
                throw new ArgumentException("Debe indicar los datos del curso.");
            }
            if (curso.IdInstitucion <= 0)
            {
                throw new ArgumentException("Debe indicar una institución válida.");
            }
            if (curso.IdTurno <= 0)
            {
                throw new ArgumentException("Debe indicar un turno válido.");
            }
            if (curso.IdNivel != null && curso.IdNivel <= 0)
            {
                throw new ArgumentException("El nivel escolar debe ser mayor que cero o quedar sin especificar.");
            }
            if (curso.Anio != null && (curso.Anio < -128 || curso.Anio > 127))
            {
                throw new ArgumentException("El año escolar debe estar entre -128 y 127 o quedar sin especificar.");
            }
            if (curso.NivelPersonalizado != null && curso.NivelPersonalizado.Length > 30)
            {
                throw new ArgumentException("El nivel personalizado no puede superar los 30 caracteres.");
            }
            if (string.IsNullOrWhiteSpace(curso.Division))
            {
                throw new ArgumentException("La división es obligatoria.");
            }
            if (curso.Division.Length > 50)
            {
                throw new ArgumentException("La división no puede superar los 50 caracteres.");
            }
            if (curso.Capacidad < -32768 || curso.Capacidad > 32767)
            {
                throw new ArgumentException("La capacidad debe estar entre -32768 y 32767.");
            }
            if (curso.Inscriptos < -32768 || curso.Inscriptos > 32767)
            {
                throw new ArgumentException("La cantidad de inscriptos debe estar entre -32768 y 32767.");
            }
            if (curso.Inscriptos > curso.Capacidad)
            {
                throw new ArgumentException("La cantidad de inscriptos no puede superar la capacidad.");
            }
        }
    }
}
