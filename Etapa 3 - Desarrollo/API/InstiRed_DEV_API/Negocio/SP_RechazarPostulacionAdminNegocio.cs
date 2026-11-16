using Dominio.DTOs;
using Negocio.Helpers;

namespace Negocio
{
    public class SP_RechazarPostulacionAdminNegocio
    {
        public void rechazar(SP_RechazarPostulacionAdmin_in registro)
        {
            validar(registro);
            ProcedimientoHelper.ejecutar("sp_PostulacionAdmin_Rechazar", datos =>
            {
                datos.setearParametro("@p_idPostulacion", registro.IdPostulacion);
                datos.setearParametro("@p_idUsuarioDevCo", registro.IdUsuarioDevCo);
                datos.setearParametro("@p_motivoRechazo", registro.MotivoRechazo.Trim());
                datos.ejecutarAccion();
            });
        }

        private void validar(SP_RechazarPostulacionAdmin_in registro)
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
            if (string.IsNullOrWhiteSpace(registro.MotivoRechazo))
            {
                throw new ArgumentException("El campo MotivoRechazo es obligatorio.");
            }
            if (registro.MotivoRechazo.Trim().Length > 255)
            {
                throw new ArgumentException("El campo MotivoRechazo no puede superar los 255 caracteres.");
            }
        }
    }
}
