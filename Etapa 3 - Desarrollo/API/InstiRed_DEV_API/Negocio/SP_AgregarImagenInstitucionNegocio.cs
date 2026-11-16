using Dominio.DTOs;
using Negocio.Helpers;

namespace Negocio
{
    public class SP_AgregarImagenInstitucionNegocio
    {
        public void agregar(SP_AgregarImagenInstitucion_in registro)
        {
            validar(registro);
            ProcedimientoHelper.ejecutar("sp_Imagen_Institucion_Agregar", datos =>
            {
                datos.setearParametro("@p_idAdministrador", registro.IdAdministrador);
                datos.setearParametro("@p_idInstitucion", registro.IdInstitucion);
                datos.setearParametro("@p_urlImagen", registro.UrlImagen.Trim());
                datos.setearParametro("@p_descripcion", registro.Descripcion.Trim());
                datos.ejecutarAccion();
            });
        }

        private void validar(SP_AgregarImagenInstitucion_in registro)
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
            if (string.IsNullOrWhiteSpace(registro.UrlImagen))
            {
                throw new ArgumentException("El campo UrlImagen es obligatorio.");
            }
            if (registro.UrlImagen.Trim().Length > 255)
            {
                throw new ArgumentException("El campo UrlImagen no puede superar los 255 caracteres.");
            }
            if (string.IsNullOrWhiteSpace(registro.Descripcion))
            {
                throw new ArgumentException("El campo Descripcion es obligatorio.");
            }
            if (registro.Descripcion.Trim().Length > 255)
            {
                throw new ArgumentException("El campo Descripcion no puede superar los 255 caracteres.");
            }
        }
    }
}
