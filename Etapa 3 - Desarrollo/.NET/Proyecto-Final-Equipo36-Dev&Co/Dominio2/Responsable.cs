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
        public int Id { get; set; }
        public Usuario Usuario { get; set; }
        public List <Alumno> Alumnos { get; set; }
        public Rol Rol { get; set; }
        public Telefono Telefono    { get; set; }

    }
}
