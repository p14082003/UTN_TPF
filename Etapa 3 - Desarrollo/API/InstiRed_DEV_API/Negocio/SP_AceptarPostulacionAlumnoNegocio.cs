using Dominio.DTOs;
using MySqlConnector;
using Negocio.Helpers;

namespace Negocio
{
    public class SP_AceptarPostulacionAlumnoNegocio
    {
        public SP_AceptarPostulacionAlumno_out aceptar(SP_AceptarPostulacionAlumno_in registro)
        {
            validar(registro);
            return ProcedimientoHelper.ejecutar("sp_Aceptar_Postulacion_Alumno", datos =>
            {
                datos.setearParametro("@p_idAdministradorInstitucion", registro.IdAdministradorInstitucion);
                datos.setearParametro("@p_idPostulacion", registro.IdPostulacion);
                MySqlDataReader lector = ProcedimientoHelper.leerFila(datos);
                return new SP_AceptarPostulacionAlumno_out
                {
                    IdPostulacion = Convert.ToInt32(lector["IdPostulacion"]),
                    FechaPostulacion = Convert.ToDateTime(lector["FechaPostulacion"]),
                    CodigoSeguimiento = (string)lector["CodigoSeguimiento"],
                    IdCurso = Convert.ToInt32(lector["IdCurso"]),
                    IdAlumno = Convert.ToInt32(lector["IdAlumno"]),
                    IdEstado = Convert.ToInt32(lector["IdEstado"]),
                    IdInstitucion = Convert.ToInt32(lector["IdInstitucion"]),
                    Nombre = (string)lector["Nombre"],
                    Apellido = (string)lector["Apellido"],
                    Documento = (string)lector["Documento"],
                    Descripcion = (string)lector["Descripcion"]
                };
            });
        }

        private void validar(SP_AceptarPostulacionAlumno_in registro)
        {
            if (registro == null)
            {
                throw new ArgumentException("Debe indicar los datos de la operación.");
            }
            if (registro.IdAdministradorInstitucion <= 0)
            {
                throw new ArgumentException("El campo IdAdministradorInstitucion debe ser mayor que cero.");
            }
            if (registro.IdPostulacion <= 0)
            {
                throw new ArgumentException("El campo IdPostulacion debe ser mayor que cero.");
            }
        }
    }
}
