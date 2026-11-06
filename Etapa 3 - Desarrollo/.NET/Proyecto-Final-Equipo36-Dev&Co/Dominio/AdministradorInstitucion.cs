using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dominio
{
    public class AdministradorInstitucion : Persona
    {
        public int IdAdministradorInstitucion { get; set; }
        public Usuario Usuario { get; set; }
        public Telefono Telefono { get; set; }
        public Institucion Institucion { get; set; }

    }
}
