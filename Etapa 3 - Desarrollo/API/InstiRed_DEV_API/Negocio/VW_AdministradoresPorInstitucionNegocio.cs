using Dominio.DTOs;
using MySqlConnector;
using Negocio.Helpers;

namespace Negocio
{
    public class VW_AdministradoresPorInstitucionNegocio
    {
        public List<VW_AdministradoresPorInstitucion_out> listar(int? idInstitucion = null)
        {
            return consultar(idInstitucion, null);
        }

        public VW_AdministradoresPorInstitucion_out? obtenerPorId(int id)
        {
            return consultar(null, id).FirstOrDefault();
        }

        private static List<VW_AdministradoresPorInstitucion_out> consultar(int? idInstitucion, int? idAdministrador)
        {
            if (idInstitucion <= 0 || idAdministrador <= 0)
            {
                throw new ArgumentException("El ID debe ser mayor que cero.");
            }

            List<VW_AdministradoresPorInstitucion_out> administradores = new List<VW_AdministradoresPorInstitucion_out>();
            AccesoDatos datos = new AccesoDatos();
            try
            {
                datos.setearConsulta(
                    "SELECT idAdministrador, idInstitucion, nombreAdministrador, apellidoAdministrador, docAdministrador, emailAdministrador, usuarioActivo " +
                    "FROM vw_Administradores_Por_Institucion " +
                    "WHERE (@idInstitucion IS NULL OR idInstitucion = @idInstitucion) " +
                    "AND (@idAdministrador IS NULL OR idAdministrador = @idAdministrador) " +
                    "ORDER BY idInstitucion, idAdministrador");
                datos.setearParametro("@idInstitucion", idInstitucion);
                datos.setearParametro("@idAdministrador", idAdministrador);
                datos.ejecutarLectura();

                MySqlDataReader? lector = datos.Lector;
                if (lector == null)
                {
                    throw new InvalidOperationException("No se inicializó el lector de datos.");
                }

                while (lector.Read())
                {
                    administradores.Add(new VW_AdministradoresPorInstitucion_out
                    {
                        IdAdministrador = Convert.ToInt32(lector["idAdministrador"]),
                        IdInstitucion = Convert.ToInt32(lector["idInstitucion"]),
                        NombreAdministrador = (string)lector["nombreAdministrador"],
                        ApellidoAdministrador = (string)lector["apellidoAdministrador"],
                        DocAdministrador = (string)lector["docAdministrador"],
                        EmailAdministrador = (string)lector["emailAdministrador"],
                        UsuarioActivo = Convert.ToBoolean(lector["usuarioActivo"])
                    });
                }

                return administradores;
            }
            finally
            {
                datos.cerrarConexion();
            }
        }
    }
}
