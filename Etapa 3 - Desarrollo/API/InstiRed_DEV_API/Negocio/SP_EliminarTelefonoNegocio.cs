using Dominio.DTOs;
using Negocio.Helpers;

namespace Negocio
{
    public class SP_EliminarTelefonoNegocio
    {
        public void eliminar(SP_EliminarTelefono_in registro)
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

            if (registro.IdTelefono <= 0)
            {
                throw new ArgumentException("El campo IdTelefono debe ser mayor que cero.");
            }

            ProcedimientoHelper.ejecutar("sp_Telefono_Eliminar", datos =>
            {
                datos.setearParametro("@p_idUsuarioActor", registro.IdUsuarioActor);
                datos.setearParametro("@p_tipoDueno", tipoDueno);
                datos.setearParametro("@p_idDueno", registro.IdDueno);
                datos.setearParametro("@p_idTelefono", registro.IdTelefono);
                datos.ejecutarAccion();
            });
        }
    }
}
