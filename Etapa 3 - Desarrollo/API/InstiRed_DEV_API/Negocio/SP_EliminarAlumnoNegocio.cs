using Dominio.DTOs;
using Negocio.Helpers;

namespace Negocio
{
    public class SP_EliminarAlumnoNegocio
    {
        public void eliminar(SP_EliminarAlumno_in registro)
        {
            if (registro == null || registro.IdResponsable <= 0 || registro.IdAlumno <= 0)
            {
                throw new ArgumentException("Debe indicar un responsable y un alumno válidos.");
            }
            ProcedimientoHelper.ejecutar("sp_Alumno_Eliminar", datos =>
            {
                datos.setearParametro("@p_idResponsable", registro.IdResponsable);
                datos.setearParametro("@p_idAlumno", registro.IdAlumno);
                datos.ejecutarAccion();
            });
        }
    }
}
