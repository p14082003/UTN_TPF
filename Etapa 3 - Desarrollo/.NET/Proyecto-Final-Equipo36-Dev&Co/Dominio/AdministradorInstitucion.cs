using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dominio
{
    public class AdministradorInstitucion
    {
        public int ID { get; set; }
        public Usuario Usuario { get; set; }
        public Telefono Telefono { get; set; }

    }
}
