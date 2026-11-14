using Dominio.DTOs;
using MySqlConnector;
using Negocio.Helpers;

namespace Negocio
{
    public class VW_InstitucionesNegocio
    {
        public List<VW_Instituciones_out> listar(int? idTipoInstitucion = null, bool? estado = null, string? nombre = null)
        {
            if (idTipoInstitucion != null && (idTipoInstitucion <= 0 || idTipoInstitucion > 127))
            {
                throw new ArgumentException("El filtro idTipoInstitucion debe ser mayor que cero y no superar 127.");
            }
            if (nombre != null && (string.IsNullOrWhiteSpace(nombre) || nombre.Trim().Length > 100))
            {
                throw new ArgumentException("El filtro nombre no puede estar vacío ni superar los 100 caracteres.");
            }
            List<VW_Instituciones_out> lista = new List<VW_Instituciones_out>();
            AccesoDatos datos = new AccesoDatos();
            try
            {
                datos.setearConsulta("SELECT idInstitucion, nombre, direccion, email, latitud, longitud, descripcion, estado, idTipoInstitucion, tipoInstitucion, movilidad, visual, auditiva, verbal, intelectual, sindromeDown, autismo, idiomas, telefonos, promedioCalificacion, cantidadResenias FROM vw_Instituciones WHERE (@idTipoInstitucion IS NULL OR idTipoInstitucion = @idTipoInstitucion) AND (@estado IS NULL OR estado = @estado) AND (@nombre IS NULL OR nombre LIKE CONCAT('%', @nombre, '%')) ORDER BY nombre, idInstitucion");
                datos.setearParametro("@idTipoInstitucion", idTipoInstitucion);
                datos.setearParametro("@estado", estado);
                datos.setearParametro("@nombre", nombre?.Trim());
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

        public VW_Instituciones_out? obtenerPorId(int id)
        {
            if (id <= 0)
            {
                throw new ArgumentException("El ID debe ser mayor que cero.");
            }
            AccesoDatos datos = new AccesoDatos();
            try
            {
                datos.setearConsulta("SELECT idInstitucion, nombre, direccion, email, latitud, longitud, descripcion, estado, idTipoInstitucion, tipoInstitucion, movilidad, visual, auditiva, verbal, intelectual, sindromeDown, autismo, idiomas, telefonos, promedioCalificacion, cantidadResenias FROM vw_Instituciones WHERE idInstitucion = @id");
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

        private VW_Instituciones_out leerRegistro(MySqlDataReader lector)
        {
            VW_Instituciones_out registro = new VW_Instituciones_out();
            registro.IdInstitucion = Convert.ToInt32(lector["idInstitucion"]);
            registro.Nombre = (string)lector["nombre"];
            registro.Direccion = (string)lector["direccion"];
            registro.Email = (string)lector["email"];
            registro.Latitud = Convert.ToDouble(lector["latitud"]);
            registro.Longitud = Convert.ToDouble(lector["longitud"]);
            registro.Descripcion = (string)lector["descripcion"];
            registro.Estado = Convert.ToBoolean(lector["estado"]);
            registro.IdTipoInstitucion = Convert.ToInt32(lector["idTipoInstitucion"]);
            registro.TipoInstitucion = (string)lector["tipoInstitucion"];
            if (lector["movilidad"] != DBNull.Value)
            {
                registro.Movilidad = Convert.ToBoolean(lector["movilidad"]);
            }
            if (lector["visual"] != DBNull.Value)
            {
                registro.Visual = Convert.ToBoolean(lector["visual"]);
            }
            if (lector["auditiva"] != DBNull.Value)
            {
                registro.Auditiva = Convert.ToBoolean(lector["auditiva"]);
            }
            if (lector["verbal"] != DBNull.Value)
            {
                registro.Verbal = Convert.ToBoolean(lector["verbal"]);
            }
            if (lector["intelectual"] != DBNull.Value)
            {
                registro.Intelectual = Convert.ToBoolean(lector["intelectual"]);
            }
            if (lector["sindromeDown"] != DBNull.Value)
            {
                registro.SindromeDown = Convert.ToBoolean(lector["sindromeDown"]);
            }
            if (lector["autismo"] != DBNull.Value)
            {
                registro.Autismo = Convert.ToBoolean(lector["autismo"]);
            }
            if (lector["idiomas"] != DBNull.Value)
            {
                registro.Idiomas = (string)lector["idiomas"];
            }
            if (lector["telefonos"] != DBNull.Value)
            {
                registro.Telefonos = (string)lector["telefonos"];
            }
            if (lector["promedioCalificacion"] != DBNull.Value)
            {
                registro.PromedioCalificacion = Convert.ToDecimal(lector["promedioCalificacion"]);
            }
            registro.CantidadResenias = Convert.ToInt64(lector["cantidadResenias"]);
            return registro;
        }
    }
}
