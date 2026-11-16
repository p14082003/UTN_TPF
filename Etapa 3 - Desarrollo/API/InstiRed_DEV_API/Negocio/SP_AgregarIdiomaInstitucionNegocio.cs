using Dominio.DTOs;
using Negocio.Helpers;

namespace Negocio
{
    public class SP_AgregarIdiomaInstitucionNegocio
    {
        public void agregar(SP_AgregarIdiomaInstitucion_in registro)
        {
            validar(registro);
            ProcedimientoHelper.ejecutar("sp_Idioma_Institucion_Agregar", datos =>
            {
                datos.setearParametro("@p_idAdministrador", registro.IdAdministrador);
                datos.setearParametro("@p_idInstitucion", registro.IdInstitucion);
                datos.setearParametro("@p_idIdioma", registro.IdIdioma);
                datos.ejecutarAccion();
            });
        }

        private void validar(SP_AgregarIdiomaInstitucion_in registro)
        {
            if (registro == null)
            {
                throw new ArgumentException("Debe indicar los datos de la operación.");
            }
            if (registro.IdAdministrador <= 0)
            {
                throw new ArgumentException("El campo IdAdministrador debe ser mayor que cero.");
            }
            if (registro.IdInstitucion <= 0)
            {
                throw new ArgumentException("El campo IdInstitucion debe ser mayor que cero.");
            }
            if (registro.IdIdioma <= 0 || registro.IdIdioma > 127)
            {
                throw new ArgumentException("El campo IdIdioma debe estar entre 1 y 127.");
            }
        }
    }
}
