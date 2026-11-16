using Dominio.DTOs;
using Negocio.Helpers;

namespace Negocio
{
    public class SP_DarDeBajaInstitucionNegocio
    {
        public void darDeBaja(SP_DarDeBajaInstitucion_in registro)
        {
            validar(registro);
            ProcedimientoHelper.ejecutar("sp_Institucion_DarDeBaja", datos =>
            {
                datos.setearParametro("@p_idInstitucion", registro.IdInstitucion);
                datos.setearParametro("@p_idUsuarioDevCo", registro.IdUsuarioDevCo);
                datos.ejecutarAccion();
            });
        }

        private void validar(SP_DarDeBajaInstitucion_in registro)
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
