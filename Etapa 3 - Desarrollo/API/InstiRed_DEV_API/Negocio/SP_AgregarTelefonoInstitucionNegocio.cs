using Dominio.DTOs;
using Dominio;

namespace Negocio
{
    public class SP_AgregarTelefonoInstitucionNegocio
    {
        public void agregar(SP_AgregarTelefonoInstitucion_in registro)
        {
            validar(registro);
            Ent_AdministradorInstitucion? administrador = new Ent_AdministradorInstitucionNegocio().obtenerPorId(registro.IdAdministrador);
            if (administrador == null || administrador.IdInstitucion != registro.IdInstitucion)
            {
                throw new ArgumentException("No cuenta con permiso para modificar esta institución.");
            }

            new SP_AgregarTelefonoNegocio().agregar(new SP_AgregarTelefono_in
            {
                IdUsuarioActor = administrador.IdUsuario,
                TipoDueno = "Institucion",
                IdDueno = registro.IdInstitucion,
                Telefono = registro.Telefono
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
