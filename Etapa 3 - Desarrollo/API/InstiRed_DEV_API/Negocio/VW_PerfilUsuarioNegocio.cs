using Dominio.DTOs;
using Negocio.Helpers;

namespace Negocio
{
    public class VW_PerfilUsuarioNegocio
    {
        public VW_PerfilUsuario_out? obtenerPorId(int idUsuario)
        {
            if (idUsuario <= 0)
            {
                throw new ArgumentException("El ID del usuario debe ser mayor que cero.");
            }

            AccesoDatos datos = new AccesoDatos();
            try
            {
                datos.setearConsulta("SELECT idUsuario, email, idTipoUsuario, tipoUsuario, nombre, apellido, documento, fechaNacimiento, domicilio, idResponsable, idAdministrador, idInstitucion, nombreInstitucion FROM vw_Perfil_Usuario WHERE idUsuario = @idUsuario");
                datos.setearParametro("@idUsuario", idUsuario);
                datos.ejecutarLectura();
                var lector = datos.Lector;
                if (lector == null)
                {
                    throw new InvalidOperationException("No se inicializó el lector de datos.");
                }
                if (!lector.Read())
                {
                    return null;
                }

                var perfil = new VW_PerfilUsuario_out
                {
                    IdUsuario = Convert.ToInt32(lector["idUsuario"]),
                    Email = (string)lector["email"],
                    IdTipoUsuario = Convert.ToInt32(lector["idTipoUsuario"]),
                    TipoUsuario = (string)lector["tipoUsuario"],
                    Nombre = lector["nombre"] as string,
                    Apellido = lector["apellido"] as string,
                    Documento = lector["documento"] as string,
                    Domicilio = lector["domicilio"] as string,
                    NombreInstitucion = lector["nombreInstitucion"] as string
                };
                if (lector["fechaNacimiento"] != DBNull.Value)
                {
                    perfil.FechaNacimiento = Convert.ToDateTime(lector["fechaNacimiento"]);
                }
                if (lector["idResponsable"] != DBNull.Value)
                {
                    perfil.IdResponsable = Convert.ToInt32(lector["idResponsable"]);
                }
                if (lector["idAdministrador"] != DBNull.Value)
                {
                    perfil.IdAdministrador = Convert.ToInt32(lector["idAdministrador"]);
                }
                if (lector["idInstitucion"] != DBNull.Value)
                {
                    perfil.IdInstitucion = Convert.ToInt32(lector["idInstitucion"]);
                }
                return perfil;
            }
            finally
            {
                datos.cerrarConexion();
            }
        }
    }
}
