using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dominio
{
    public class Curso
    {
        public int Id { get; set;}
        public Institucion Institucion { get; set;}
        public Turno Turno { get; set;}
        public int anio { get; set;}
        public NivelEscolar NivelEscolar {  get; set;}
        public int CicloLectivo {get; set;}
        public string NivelPersonalizado { get; set;}
        public string Division {  get; set;}
        public int Capacidad { get; set;}
        public int Inscriptos { get; set;}
    }
}
