using Dominio.DTOs;
using Negocio.Helpers;

namespace Negocio
{
    public class SP_AgregarTelefonoNegocio
    {
        public void agregar(SP_AgregarTelefono_in registro)
        {
            if (registro == null)
            {
                throw new ArgumentException("Debe indicar los datos de la operación.");
            }
            if (registro.IdUsuarioActor <= 0 || registro.IdDueno <= 0)
            {
                throw new ArgumentException("El ID del usuario actor y el ID del dueño deben ser mayores que cero.");
            }

            string tipoDueno = registro.TipoDueno?.Trim() ?? "";
            if (tipoDueno != "Institucion" && tipoDueno != "Administrador" && tipoDueno != "Responsable")
            {
                throw new ArgumentException("El tipo de dueño debe ser Institucion, Administrador o Responsable.");
            }

            if (string.IsNullOrWhiteSpace(registro.Telefono) || registro.Telefono.Trim().Length > 30)
            {
                throw new ArgumentException("El teléfono es obligatorio y puede tener hasta 30 caracteres.");
            }

            ProcedimientoHelper.ejecutar("sp_Telefono_Agregar", datos =>
            {
                datos.setearParametro("@p_idUsuarioActor", registro.IdUsuarioActor);
                datos.setearParametro("@p_tipoDueno", tipoDueno);
                datos.setearParametro("@p_idDueno", registro.IdDueno);
                datos.setearParametro("@p_telefono", registro.Telefono.Trim());
                datos.ejecutarAccion();
            });
        }
    }
}
