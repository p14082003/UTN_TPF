using Dominio.DTOs;
using Negocio.Helpers;

namespace Negocio
{
    public class SP_ReactivarInstitucionNegocio
    {
        public void reactivar(SP_ReactivarInstitucion_in registro)
        {
            validar(registro);
            ProcedimientoHelper.ejecutar("sp_Institucion_Reactivar", datos =>
            {
                datos.setearParametro("@p_idInstitucion", registro.IdInstitucion);
                datos.setearParametro("@p_idUsuarioDevCo", registro.IdUsuarioDevCo);
                datos.ejecutarAccion();
            });
        }

        private void validar(SP_ReactivarInstitucion_in registro)
        {
            if (registro == null)
            {
                throw new ArgumentException("Debe indicar los datos de la operación.");
            }
            if (registro.IdInstitucion <= 0)
            {
                throw new ArgumentException("El campo IdInstitucion debe ser mayor que cero.");
            }
            if (registro.IdUsuarioDevCo <= 0)
            {
                throw new ArgumentException("El campo IdUsuarioDevCo debe ser mayor que cero.");
            }
        }
    }
}
