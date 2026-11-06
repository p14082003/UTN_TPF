using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Principal;
using System.Text;
using System.Threading.Tasks;

namespace Dominio
{
    public class Responsable : Persona
    {
        public int IdResponsable { get; set; }
        public Usuario Usuario { get; set; }
        public List <AlumnoACargo> Alumnos { get; set; }
        public Telefono Telefono { get; set; }

    }
}
