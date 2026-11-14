using Dominio.DTOs;
using MySqlConnector;
using Negocio.Helpers;

namespace Negocio
{
    public class VW_PostulacionesAdminInstitucionNegocio
    {
        public List<VW_PostulacionesAdminInstitucion_out> listar(int? idAdministradorInstitucion = null, int? idInstitucion = null, int? idEstado = null, string? codigoSeguimiento = null)
        {
            if (idAdministradorInstitucion != null && (idAdministradorInstitucion <= 0))
            {
                throw new ArgumentException("El filtro idAdministradorInstitucion debe ser mayor que cero.");
            }
            if (idInstitucion != null && (idInstitucion <= 0))
            {
                throw new ArgumentException("El filtro idInstitucion debe ser mayor que cero.");
            }
            if (idEstado != null && (idEstado <= 0))
            {
                throw new ArgumentException("El filtro idEstado debe ser mayor que cero.");
            }
            if (codigoSeguimiento != null && (string.IsNullOrWhiteSpace(codigoSeguimiento) || codigoSeguimiento.Trim().Length > 30))
            {
                throw new ArgumentException("El filtro codigoSeguimiento no puede estar vacío ni superar los 30 caracteres.");
            }
            List<VW_PostulacionesAdminInstitucion_out> lista = new List<VW_PostulacionesAdminInstitucion_out>();
            AccesoDatos datos = new AccesoDatos();
            try
            {
                datos.setearConsulta("SELECT idPostulacion, fecha, codigoSeguimiento, idEstado, estado, idAdministradorInstitucion, nombreAdministrador, apellidoAdministrador, docAdministrador, emailAdministrador, usuarioActivo, nombreInstitucion, cuitInstitucion, rutaConstanciaFiscal, rutaHabilitacion, rutaConstanciaLaboral, motivoRechazo, idInstitucion FROM vw_Postulaciones_Admin_Institucion WHERE (@idAdministradorInstitucion IS NULL OR idAdministradorInstitucion = @idAdministradorInstitucion) AND (@idInstitucion IS NULL OR idInstitucion = @idInstitucion) AND (@idEstado IS NULL OR idEstado = @idEstado) AND (@codigoSeguimiento IS NULL OR codigoSeguimiento = @codigoSeguimiento) ORDER BY fecha DESC, idPostulacion DESC");
                datos.setearParametro("@idAdministradorInstitucion", idAdministradorInstitucion);
                datos.setearParametro("@idInstitucion", idInstitucion);
                datos.setearParametro("@idEstado", idEstado);
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

        public VW_PostulacionesAdminInstitucion_out? obtenerPorId(int id)
        {
            if (id <= 0)
            {
                throw new ArgumentException("El ID debe ser mayor que cero.");
            }
            AccesoDatos datos = new AccesoDatos();
            try
            {
                datos.setearConsulta("SELECT idPostulacion, fecha, codigoSeguimiento, idEstado, estado, idAdministradorInstitucion, nombreAdministrador, apellidoAdministrador, docAdministrador, emailAdministrador, usuarioActivo, nombreInstitucion, cuitInstitucion, rutaConstanciaFiscal, rutaHabilitacion, rutaConstanciaLaboral, motivoRechazo, idInstitucion FROM vw_Postulaciones_Admin_Institucion WHERE idPostulacion = @id");
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

        private VW_PostulacionesAdminInstitucion_out leerRegistro(MySqlDataReader lector)
        {
            VW_PostulacionesAdminInstitucion_out registro = new VW_PostulacionesAdminInstitucion_out();
            registro.IdPostulacion = Convert.ToInt32(lector["idPostulacion"]);
            registro.Fecha = Convert.ToDateTime(lector["fecha"]);
            registro.CodigoSeguimiento = (string)lector["codigoSeguimiento"];
            registro.IdEstado = Convert.ToInt32(lector["idEstado"]);
            registro.Estado = (string)lector["estado"];
            registro.IdAdministradorInstitucion = Convert.ToInt32(lector["idAdministradorInstitucion"]);
            registro.NombreAdministrador = (string)lector["nombreAdministrador"];
            registro.ApellidoAdministrador = (string)lector["apellidoAdministrador"];
            registro.DocAdministrador = (string)lector["docAdministrador"];
            registro.EmailAdministrador = (string)lector["emailAdministrador"];
            registro.UsuarioActivo = Convert.ToBoolean(lector["usuarioActivo"]);
            registro.NombreInstitucion = (string)lector["nombreInstitucion"];
            registro.CuitInstitucion = (string)lector["cuitInstitucion"];
            registro.RutaConstanciaFiscal = (string)lector["rutaConstanciaFiscal"];
            registro.RutaHabilitacion = (string)lector["rutaHabilitacion"];
            registro.RutaConstanciaLaboral = (string)lector["rutaConstanciaLaboral"];
            if (lector["motivoRechazo"] != DBNull.Value)
            {
                registro.MotivoRechazo = (string)lector["motivoRechazo"];
            }
            if (lector["idInstitucion"] != DBNull.Value)
            {
                registro.IdInstitucion = Convert.ToInt32(lector["idInstitucion"]);
            }
            return registro;
        }
    }
}
