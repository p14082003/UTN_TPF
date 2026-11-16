using Dominio.DTOs;
using MySqlConnector;
using Negocio.Helpers;

namespace Negocio
{
    public class SP_ConsultarPostulacionPorCodigoNegocio
    {
        public SP_ConsultarPostulacionPorCodigo_out consultar(SP_ConsultarPostulacionPorCodigo_in registro)
        {
            validar(registro);
            return ProcedimientoHelper.ejecutar("sp_Postulacion_ConsultarPorCodigo", datos =>
            {
                datos.setearParametro("@p_codigo", registro.Codigo.Trim());
                datos.setearParametro("@p_email", registro.Email.Trim().ToLowerInvariant());
                MySqlDataReader lector = ProcedimientoHelper.leerFila(datos);
                return new SP_ConsultarPostulacionPorCodigo_out
                {
                    Tipo = (string)lector["Tipo"],
                    Fecha = Convert.ToDateTime(lector["Fecha"]),
                    Estado = (string)lector["Estado"],
                    NombreInstitucion = (string)lector["NombreInstitucion"],
                    Cuit = (string)lector["Cuit"],
                    MotivoRechazo = lector["MotivoRechazo"] as string
                };
            });
        }

        private void validar(SP_ConsultarPostulacionPorCodigo_in registro)
        {
            if (registro == null)
            {
                throw new ArgumentException("Debe indicar los datos de la operación.");
            }
            if (string.IsNullOrWhiteSpace(registro.Codigo))
            {
                throw new ArgumentException("El campo Codigo es obligatorio.");
            }
            if (registro.Codigo.Trim().Length > 30)
            {
                throw new ArgumentException("El campo Codigo no puede superar los 30 caracteres.");
            }
            if (string.IsNullOrWhiteSpace(registro.Email))
            {
                throw new ArgumentException("El campo Email es obligatorio.");
            }
            if (registro.Email.Trim().Length > 100)
            {
                throw new ArgumentException("El campo Email no puede superar los 100 caracteres.");
            }
        }
    }
}
