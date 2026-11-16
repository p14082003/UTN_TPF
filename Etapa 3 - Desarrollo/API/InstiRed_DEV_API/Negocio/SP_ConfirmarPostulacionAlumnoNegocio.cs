using Dominio.DTOs;
using Negocio.Helpers;

namespace Negocio
{
    public class SP_ConfirmarPostulacionAlumnoNegocio
    {
        public void confirmar(SP_ConfirmarPostulacionAlumno_in registro)
        {
            validar(registro);
            ProcedimientoHelper.ejecutar("sp_Confirmar_Postulacion_Alumno", datos =>
            {
                datos.setearParametro("@p_idPostulacion", registro.IdPostulacion);
                datos.setearParametro("@p_idResponsable", registro.IdResponsable);
                datos.ejecutarAccion();
            });
        }

        private void validar(SP_ConfirmarPostulacionAlumno_in registro)
        {
            if (registro == null)
            {
                throw new ArgumentException("Debe indicar los datos de la operación.");
            }
            if (registro.IdPostulacion <= 0)
            {
                throw new ArgumentException("El campo IdPostulacion debe ser mayor que cero.");
            }
            if (registro.IdResponsable <= 0)
            {
                throw new ArgumentException("El campo IdResponsable debe ser mayor que cero.");
            }
        }
    }
}
