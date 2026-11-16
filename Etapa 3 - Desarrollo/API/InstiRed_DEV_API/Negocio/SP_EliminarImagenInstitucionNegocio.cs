using Dominio;
using Dominio.DTOs;
using Negocio.Helpers;

namespace Negocio
{
    public class SP_EliminarImagenInstitucionNegocio
    {
        public string? eliminar(SP_EliminarImagenInstitucion_in registro)
        {
            validar(registro);
            Ent_Imagen? imagen = new Ent_ImagenNegocio().obtenerPorId(registro.IdImagen);
            ProcedimientoHelper.ejecutar("sp_Imagen_Institucion_Eliminar", datos =>
            {
                datos.setearParametro("@p_idAdministrador", registro.IdAdministrador);
                datos.setearParametro("@p_idInstitucion", registro.IdInstitucion);
                datos.setearParametro("@p_idImagen", registro.IdImagen);
                datos.ejecutarAccion();
            });
            return imagen?.IdInstitucion == registro.IdInstitucion ? imagen.UrlImagen : null;
        }

        private void validar(SP_EliminarImagenInstitucion_in registro)
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
            if (registro.IdImagen <= 0)
            {
                throw new ArgumentException("El campo IdImagen debe ser mayor que cero.");
            }
        }
    }
}
