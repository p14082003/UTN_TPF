using Dominio.DTOs;
using MySqlConnector;
using Negocio.Helpers;

namespace Negocio
{
    public class VW_PostulacionesAlumnosNegocio
    {
        public List<VW_PostulacionesAlumnos_out> listar(int? idAlumno = null, int? idResponsable = null, int? idInstitucion = null, int? idCurso = null, int? idEstado = null, int? cicloLectivo = null, string? codigoSeguimiento = null)
        {
            if (idAlumno != null && (idAlumno <= 0))
            {
                throw new ArgumentException("El filtro idAlumno debe ser mayor que cero.");
            }
            if (idResponsable != null && (idResponsable <= 0))
            {
                throw new ArgumentException("El filtro idResponsable debe ser mayor que cero.");
            }
            if (idInstitucion != null && (idInstitucion <= 0))
            {
                throw new ArgumentException("El filtro idInstitucion debe ser mayor que cero.");
            }
            if (idCurso != null && (idCurso <= 0))
            {
                throw new ArgumentException("El filtro idCurso debe ser mayor que cero.");
            }
            if (idEstado != null && (idEstado <= 0))
            {
                throw new ArgumentException("El filtro idEstado debe ser mayor que cero.");
            }
            if (cicloLectivo != null && (cicloLectivo <= 0))
            {
                throw new ArgumentException("El filtro cicloLectivo debe ser mayor que cero.");
            }
            if (codigoSeguimiento != null && (string.IsNullOrWhiteSpace(codigoSeguimiento) || codigoSeguimiento.Trim().Length > 30))
            {
                throw new ArgumentException("El filtro codigoSeguimiento no puede estar vacío ni superar los 30 caracteres.");
            }
            List<VW_PostulacionesAlumnos_out> lista = new List<VW_PostulacionesAlumnos_out>();
            AccesoDatos datos = new AccesoDatos();
            try
            {
                datos.setearConsulta("SELECT idPostulacion, fecha, codigoSeguimiento, idEstado, estado, motivoRechazo, idAlumno, nombreAlumno, apellidoAlumno, docAlumno, fechaNacimientoAlumno, idResponsable, nombreResponsable, apellidoResponsable, emailResponsable, telefonosResponsable, rolResponsable, idCurso, cicloLectivo, nivel, anio, division, turno, idInstitucion, institucion FROM vw_Postulaciones_Alumnos WHERE (@idAlumno IS NULL OR idAlumno = @idAlumno) AND (@idResponsable IS NULL OR idResponsable = @idResponsable) AND (@idInstitucion IS NULL OR idInstitucion = @idInstitucion) AND (@idCurso IS NULL OR idCurso = @idCurso) AND (@idEstado IS NULL OR idEstado = @idEstado) AND (@cicloLectivo IS NULL OR cicloLectivo = @cicloLectivo) AND (@codigoSeguimiento IS NULL OR codigoSeguimiento = @codigoSeguimiento) ORDER BY fecha DESC, idPostulacion DESC");
                datos.setearParametro("@idAlumno", idAlumno);
                datos.setearParametro("@idResponsable", idResponsable);
                datos.setearParametro("@idInstitucion", idInstitucion);
                datos.setearParametro("@idCurso", idCurso);
                datos.setearParametro("@idEstado", idEstado);
                datos.setearParametro("@cicloLectivo", cicloLectivo);
                datos.setearParametro("@codigoSeguimiento", codigoSeguimiento?.Trim());
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

        public VW_PostulacionesAlumnos_out? obtenerPorId(int id)
        {
            if (id <= 0)
            {
                throw new ArgumentException("El ID debe ser mayor que cero.");
            }
            AccesoDatos datos = new AccesoDatos();
            try
            {
                datos.setearConsulta("SELECT idPostulacion, fecha, codigoSeguimiento, idEstado, estado, motivoRechazo, idAlumno, nombreAlumno, apellidoAlumno, docAlumno, fechaNacimientoAlumno, idResponsable, nombreResponsable, apellidoResponsable, emailResponsable, telefonosResponsable, rolResponsable, idCurso, cicloLectivo, nivel, anio, division, turno, idInstitucion, institucion FROM vw_Postulaciones_Alumnos WHERE idPostulacion = @id");
                datos.setearParametro("@id", id);
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

        private VW_PostulacionesAlumnos_out leerRegistro(MySqlDataReader lector)
        {
            VW_PostulacionesAlumnos_out registro = new VW_PostulacionesAlumnos_out();
            registro.IdPostulacion = Convert.ToInt32(lector["idPostulacion"]);
            registro.Fecha = Convert.ToDateTime(lector["fecha"]);
            registro.CodigoSeguimiento = (string)lector["codigoSeguimiento"];
            registro.IdEstado = Convert.ToInt32(lector["idEstado"]);
            registro.Estado = (string)lector["estado"];
            if (lector["motivoRechazo"] != DBNull.Value)
            {
                registro.MotivoRechazo = (string)lector["motivoRechazo"];
            }
            registro.IdAlumno = Convert.ToInt32(lector["idAlumno"]);
            registro.NombreAlumno = (string)lector["nombreAlumno"];
            registro.ApellidoAlumno = (string)lector["apellidoAlumno"];
            registro.DocAlumno = (string)lector["docAlumno"];
            registro.FechaNacimientoAlumno = Convert.ToDateTime(lector["fechaNacimientoAlumno"]);
            if (lector["idResponsable"] != DBNull.Value)
            {
                registro.IdResponsable = Convert.ToInt32(lector["idResponsable"]);
            }
            if (lector["nombreResponsable"] != DBNull.Value)
            {
                registro.NombreResponsable = (string)lector["nombreResponsable"];
            }
            if (lector["apellidoResponsable"] != DBNull.Value)
            {
                registro.ApellidoResponsable = (string)lector["apellidoResponsable"];
            }
            if (lector["emailResponsable"] != DBNull.Value)
            {
                registro.EmailResponsable = (string)lector["emailResponsable"];
            }
            if (lector["telefonosResponsable"] != DBNull.Value)
            {
                registro.TelefonosResponsable = (string)lector["telefonosResponsable"];
            }
            registro.RolResponsable = (string)lector["rolResponsable"];
            registro.IdCurso = Convert.ToInt32(lector["idCurso"]);
            registro.CicloLectivo = Convert.ToInt32(lector["cicloLectivo"]);
            if (lector["nivel"] != DBNull.Value)
            {
                registro.Nivel = (string)lector["nivel"];
            }
            if (lector["anio"] != DBNull.Value)
            {
                registro.Anio = Convert.ToInt32(lector["anio"]);
            }
            registro.Division = (string)lector["division"];
            registro.Turno = (string)lector["turno"];
            registro.IdInstitucion = Convert.ToInt32(lector["idInstitucion"]);
            registro.Institucion = (string)lector["institucion"];
            return registro;
        }
    }
}
