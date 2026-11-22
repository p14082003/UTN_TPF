using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dominio.DTOs
{
    public class VW_Cursos_out
    {
        public int IdCurso { get; set; }
        public int IdInstitucion { get; set; }
        public int CicloLectivo { get; set; }
        public int? IdNivel { get; set; }
        public string? Nivel { get; set; }
        public int? Anio { get; set; }
        public string? NivelPersonalizado { get; set; }
        public string? Division { get; set; }
        public int IdTurno { get; set; }
        public string? Turno { get; set; }
        public int Capacidad { get; set; }
        public int Inscriptos { get; set; }
        public int VacantesDisponibles { get; set; }
    }
}
