using Dominio.DTOs;
using Negocio.Helpers;

namespace Negocio
{
    public class SP_AlumnosPorResponsableNegocio
    {
        public List<SP_AlumnosPorResponsable_out> listar(int idResponsable)
        {
            if (idResponsable <= 0)
            {
                throw new ArgumentException("El idResponsable debe ser mayor que cero.");
            }
            return ProcedimientoHelper.ejecutar("sp_alumnos_Por_Responsable", datos =>
            {
                datos.setearParametro("@p_idResponsable", idResponsable);
                datos.ejecutarLectura();
                var lector = datos.Lector;
                if (lector == null)
                {
                    throw new InvalidOperationException("No se inicializó el lector de datos.");
                }
                var lista = new List<SP_AlumnosPorResponsable_out>();
                while (lector.Read())
                {
                    lista.Add(new SP_AlumnosPorResponsable_out
                    {
                        IdAlumno = Convert.ToInt32(lector["IdAlumno"]),
                        NombreCompleto = (string)lector["NombreCompleto"]
                    });
                }
                return lista;
            });
        }
    }
}
