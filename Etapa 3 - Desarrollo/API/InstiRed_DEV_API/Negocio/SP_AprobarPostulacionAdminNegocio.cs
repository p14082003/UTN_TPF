using Dominio.DTOs;
using Negocio.Helpers;

namespace Negocio
{
    public class SP_AprobarPostulacionAdminNegocio
    {
        public void aprobar(SP_AprobarPostulacionAdmin_in registro)
        {
            validar(registro);
            ProcedimientoHelper.ejecutar("sp_PostulacionAdmin_Aprobar", datos =>
            {
                datos.setearParametro("@p_idPostulacion", registro.IdPostulacion);
                datos.setearParametro("@p_idUsuarioDevCo", registro.IdUsuarioDevCo);
                datos.ejecutarAccion();
            });
        }

        private void validar(SP_AprobarPostulacionAdmin_in registro)
        {
            if (registro == null)
            {
                throw new ArgumentException("Debe indicar los datos de la operación.");
            }
            if (registro.IdPostulacion <= 0)
            {
                throw new ArgumentException("El campo IdPostulacion debe ser mayor que cero.");
            }
            if (registro.IdUsuarioDevCo <= 0)
            {
                throw new ArgumentException("El campo IdUsuarioDevCo debe ser mayor que cero.");
            }
        }
    }
}
