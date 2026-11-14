using Dominio.DTOs;
using MySqlConnector;
using Negocio.Helpers;

namespace Negocio
{
    public class VW_PostulacionesPorResponsableNegocio
    {
        public List<VW_PostulacionesPorResponsable_out> listar(int? idResponsable = null, int? idAlumno = null, int? idInstitucion = null, int? idCurso = null, int? idEstado = null, int? cicloLectivo = null)
        {
            if (idResponsable != null && (idResponsable <= 0))
            {
                throw new ArgumentException("El filtro idResponsable debe ser mayor que cero.");
            }
            if (idAlumno != null && (idAlumno <= 0))
            {
                throw new ArgumentException("El filtro idAlumno debe ser mayor que cero.");
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
            List<VW_PostulacionesPorResponsable_out> lista = new List<VW_PostulacionesPorResponsable_out>();
            AccesoDatos datos = new AccesoDatos();
            try
            {
                datos.setearConsulta("SELECT idResponsable, idPostulacion, fecha, codigoSeguimiento, idEstado, estado, motivoRechazo, idAlumno, nombreAlumno, apellidoAlumno, idCurso, cicloLectivo, nivel, anio, division, turno, idInstitucion, institucion, direccionInstitucion FROM vw_Postulaciones_Por_Responsable WHERE (@idResponsable IS NULL OR idResponsable = @idResponsable) AND (@idAlumno IS NULL OR idAlumno = @idAlumno) AND (@idInstitucion IS NULL OR idInstitucion = @idInstitucion) AND (@idCurso IS NULL OR idCurso = @idCurso) AND (@idEstado IS NULL OR idEstado = @idEstado) AND (@cicloLectivo IS NULL OR cicloLectivo = @cicloLectivo) ORDER BY fecha DESC, idPostulacion DESC");
                datos.setearParametro("@idResponsable", idResponsable);
                datos.setearParametro("@idAlumno", idAlumno);
                datos.setearParametro("@idInstitucion", idInstitucion);
                datos.setearParametro("@idCurso", idCurso);
                datos.setearParametro("@idEstado", idEstado);
                datos.setearParametro("@cicloLectivo", cicloLectivo);
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

        private VW_PostulacionesPorResponsable_out leerRegistro(MySqlDataReader lector)
        {
            VW_PostulacionesPorResponsable_out registro = new VW_PostulacionesPorResponsable_out();
            registro.IdResponsable = Convert.ToInt32(lector["idResponsable"]);
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
            registro.DireccionInstitucion = (string)lector["direccionInstitucion"];
            return registro;
        }
    }
}
