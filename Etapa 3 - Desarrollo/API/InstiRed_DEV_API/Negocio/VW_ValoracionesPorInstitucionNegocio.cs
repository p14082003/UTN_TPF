using Dominio.DTOs;
using MySqlConnector;
using Negocio.Helpers;

namespace Negocio
{
    public class VW_ValoracionesPorInstitucionNegocio
    {
        public List<VW_ValoracionesPorInstitucion_out> listar(int? idInstitucion = null, int? idResponsable = null)
        {
            if (idInstitucion != null && (idInstitucion <= 0))
            {
                throw new ArgumentException("El filtro idInstitucion debe ser mayor que cero.");
            }
            if (idResponsable != null && (idResponsable <= 0))
            {
                throw new ArgumentException("El filtro idResponsable debe ser mayor que cero.");
            }
            List<VW_ValoracionesPorInstitucion_out> lista = new List<VW_ValoracionesPorInstitucion_out>();
            AccesoDatos datos = new AccesoDatos();
            try
            {
                datos.setearConsulta("SELECT idResenia, idInstitucion, institucion, idResponsable, nombreResponsable, apellidoResponsable, calificacion, comentario, respuestaInstitucion, fecha FROM vw_Valoraciones_Por_Institucion WHERE (@idInstitucion IS NULL OR idInstitucion = @idInstitucion) AND (@idResponsable IS NULL OR idResponsable = @idResponsable) ORDER BY fecha DESC, idResenia DESC");
                datos.setearParametro("@idInstitucion", idInstitucion);
                datos.setearParametro("@idResponsable", idResponsable);
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

        public VW_ValoracionesPorInstitucion_out? obtenerPorId(int id)
        {
            if (id <= 0)
            {
                throw new ArgumentException("El ID debe ser mayor que cero.");
            }
            AccesoDatos datos = new AccesoDatos();
            try
            {
                datos.setearConsulta("SELECT idResenia, idInstitucion, institucion, idResponsable, nombreResponsable, apellidoResponsable, calificacion, comentario, respuestaInstitucion, fecha FROM vw_Valoraciones_Por_Institucion WHERE idResenia = @id");
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

        private VW_ValoracionesPorInstitucion_out leerRegistro(MySqlDataReader lector)
        {
            VW_ValoracionesPorInstitucion_out registro = new VW_ValoracionesPorInstitucion_out();
            registro.IdResenia = Convert.ToInt32(lector["idResenia"]);
            registro.IdInstitucion = Convert.ToInt32(lector["idInstitucion"]);
            registro.Institucion = (string)lector["institucion"];
            registro.IdResponsable = Convert.ToInt32(lector["idResponsable"]);
            registro.NombreResponsable = (string)lector["nombreResponsable"];
            registro.ApellidoResponsable = (string)lector["apellidoResponsable"];
            registro.Calificacion = Convert.ToInt32(lector["calificacion"]);
            registro.Comentario = (string)lector["comentario"];
            if (lector["respuestaInstitucion"] != DBNull.Value)
            {
                registro.RespuestaInstitucion = (string)lector["respuestaInstitucion"];
            }
            registro.Fecha = Convert.ToDateTime(lector["fecha"]);
            return registro;
        }
    }
}
