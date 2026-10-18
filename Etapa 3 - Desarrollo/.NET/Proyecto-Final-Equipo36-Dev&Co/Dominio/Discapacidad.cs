using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dominio
{
    public class Discapacidad
    {
        public int Id { get; set; }
        public bool Visual { get; set;}
        public bool Verbal { get; set;}
        public bool Auditiva { get; set;}
        public bool Intelectual { get; set;}
        public bool Movilidad { get; set; }
        public bool SindromeDown { get; set; }
        public bool Autismo { get; set; }

    }
}
