using Dominio.DTOs;
using Negocio.Helpers;

namespace Negocio
{
    public class SP_ReactivarAdministradorInstitucionNegocio
    {
        public void reactivar(SP_ReactivarAdministradorInstitucion_in registro)
        {
            if (registro == null)
            {
                throw new ArgumentException("Debe indicar los datos de la operación.");
            }
            if (registro.IdUsuarioActor <= 0 || registro.IdAdministrador <= 0)
            {
                throw new ArgumentException("El ID del usuario actor y el ID del administrador deben ser mayores que cero.");
            }

            ProcedimientoHelper.ejecutar("sp_AdministradorInstitucion_Reactivar", datos =>
            {
                datos.setearParametro("@p_idUsuarioActor", registro.IdUsuarioActor);
                datos.setearParametro("@p_idAdministrador", registro.IdAdministrador);
                datos.ejecutarAccion();
            });
        }
    }
}
