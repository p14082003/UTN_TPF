using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dominio
{
    public class Usuario
    {
        public int Id { get; set; }
        public string NombreUsuario {  get; set; }
        public string Clave { get; set; }
        public TipoUsuario TipoUsuario { get; set; }
        public bool Activo { get; set; }
        public string Email { get; set; }


    }
}
