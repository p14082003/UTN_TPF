using Dominio.DTOs;
using Negocio.Helpers;

namespace Negocio
{
    public class SP_CancelarPostulacionAlumnoNegocio
    {
        public void cancelar(SP_CancelarPostulacionAlumno_in registro)
        {
            if (registro == null || registro.IdPostulacion <= 0 || registro.IdResponsable <= 0)
            {
                throw new ArgumentException("Debe indicar una postulación y un responsable válidos.");
            }
            ProcedimientoHelper.ejecutar("sp_Cancelar_Postulacion_Alumno", datos =>
            {
                datos.setearParametro("@p_idPostulacion", registro.IdPostulacion);
                datos.setearParametro("@p_idResponsable", registro.IdResponsable);
                datos.ejecutarAccion();
            });
        }
    }
}
