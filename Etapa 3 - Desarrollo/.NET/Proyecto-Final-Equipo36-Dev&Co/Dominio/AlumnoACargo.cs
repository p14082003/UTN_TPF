using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dominio
{
    public class AlumnoACargo // Para vincular con responsable
    {
        public Alumno Alumno { get; set; }
        public Rol Rol { get; set; } //El rol que ocupa el responsable por cada alumno que tiene a cargo (padre, tutor, etc)

    }
}
