using Dominio.DTOs;
using Negocio.Helpers;

namespace Negocio
{
    public class SP_PostulacionesPorResponsableNegocio
    {
        public List<SP_PostulacionesPorResponsable_out> listar(int idResponsable)
        {
            if (idResponsable <= 0)
            {
                throw new ArgumentException("El idResponsable debe ser mayor que cero.");
            }
            return ProcedimientoHelper.ejecutar("sp_postulaciones_Por_Responsable", datos =>
            {
                datos.setearParametro("@p_idResponsable", idResponsable);
                datos.ejecutarLectura();
                var lector = datos.Lector;
                if (lector == null)
                {
                    throw new InvalidOperationException("No se inicializó el lector de datos.");
                }
                var lista = new List<SP_PostulacionesPorResponsable_out>();
                while (lector.Read())
                {
                    lista.Add(new SP_PostulacionesPorResponsable_out
                    {
                        IdPostulacion = Convert.ToInt32(lector["IdPostulacion"]),
                        Alumno = (string)lector["Alumno"],
                        Documento = (string)lector["Documento"],
                        Institucion = (string)lector["Institucion"],
                        CicloLectivo = Convert.ToInt32(lector["CicloLectivo"]),
                        Estado = (string)lector["Estado"],
                        IdEstado = Convert.ToInt32(lector["IdEstado"]),
                        IdAlumno = Convert.ToInt32(lector["IdAlumno"])
                    });
                }
                return lista;
            });
        }
    }
}
