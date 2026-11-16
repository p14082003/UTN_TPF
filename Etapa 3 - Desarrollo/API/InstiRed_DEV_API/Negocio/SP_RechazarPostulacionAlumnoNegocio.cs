using Dominio.DTOs;
using Negocio.Helpers;

namespace Negocio
{
    public class SP_RechazarPostulacionAlumnoNegocio
    {
        public void rechazar(SP_RechazarPostulacionAlumno_in registro)
        {
            validar(registro);
            ProcedimientoHelper.ejecutar("sp_Rechazar_Postulacion_Alumno", datos =>
            {
                datos.setearParametro("@p_idPostulacion", registro.IdPostulacion);
                datos.setearParametro("@p_idAdministradorInstitucion", registro.IdAdministradorInstitucion);
                datos.setearParametro("@p_motivoRechazo", registro.MotivoRechazo.Trim());
                datos.ejecutarAccion();
            });
        }

        private void validar(SP_RechazarPostulacionAlumno_in registro)
        {
            if (registro == null)
            {
                throw new ArgumentException("Debe indicar los datos de la operación.");
            }
            if (registro.IdPostulacion <= 0)
            {
                throw new ArgumentException("El campo IdPostulacion debe ser mayor que cero.");
            }
            if (registro.IdAdministradorInstitucion <= 0)
            {
                throw new ArgumentException("El campo IdAdministradorInstitucion debe ser mayor que cero.");
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
