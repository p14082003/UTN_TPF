using Dominio.DTOs;
using Negocio.Helpers;

namespace Negocio
{
    public class VW_TelefonosPorUsuarioNegocio
    {
        public List<VW_TelefonosPorUsuario_out> listar(int idUsuario)
        {
            if (idUsuario <= 0)
            {
                throw new ArgumentException("El ID del usuario debe ser mayor que cero.");
            }

            AccesoDatos datos = new AccesoDatos();
            try
            {
                datos.setearConsulta("SELECT idUsuario, idTelefono, telefono, tipoDueno, idDueno FROM vw_Telefonos_Por_Usuario WHERE idUsuario = @idUsuario ORDER BY idTelefono");
                datos.setearParametro("@idUsuario", idUsuario);
                datos.ejecutarLectura();
                var lector = datos.Lector;
                if (lector == null)
                {
                    throw new InvalidOperationException("No se inicializó el lector de datos.");
                }

                var telefonos = new List<VW_TelefonosPorUsuario_out>();
                while (lector.Read())
                {
                    telefonos.Add(new VW_TelefonosPorUsuario_out
                    {
                        IdUsuario = Convert.ToInt32(lector["idUsuario"]),
                        IdTelefono = Convert.ToInt32(lector["idTelefono"]),
                        Telefono = (string)lector["telefono"],
                        TipoDueno = (string)lector["tipoDueno"],
                        IdDueno = Convert.ToInt32(lector["idDueno"])
                    });
                }
                return telefonos;
            }
            finally
            {
                datos.cerrarConexion();
            }
        }
    }
}
