using Dominio.DTOs;
using Negocio.Helpers;

namespace Negocio
{
    public class SP_AgregarTelefonoInstitucionNegocio
    {
        public void agregar(SP_AgregarTelefonoInstitucion_in registro)
        {
            validar(registro);
            ProcedimientoHelper.ejecutar("sp_Telefono_Institucion_Agregar", datos =>
            {
                datos.setearParametro("@p_idAdministrador", registro.IdAdministrador);
                datos.setearParametro("@p_idInstitucion", registro.IdInstitucion);
                datos.setearParametro("@p_telefono", registro.Telefono.Trim());
                datos.ejecutarAccion();
            });
        }

        private void validar(SP_AgregarTelefonoInstitucion_in registro)
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
            if (string.IsNullOrWhiteSpace(registro.Telefono))
            {
                throw new ArgumentException("El campo Telefono es obligatorio.");
            }
            if (registro.Telefono.Trim().Length > 30)
            {
                throw new ArgumentException("El campo Telefono no puede superar los 30 caracteres.");
            }
        }
    }
}
