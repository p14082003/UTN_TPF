using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dominio
{
    public class Alumno : Persona
    {
        public int IdAlumno {  get; set; }
        public List<Curso> Cursos { get; set; }
        public List <PostulacionAlumno> Postulaciones { get; set; }
        
    }
}
