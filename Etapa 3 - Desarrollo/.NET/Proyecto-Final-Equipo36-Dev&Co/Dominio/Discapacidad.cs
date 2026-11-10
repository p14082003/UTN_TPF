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

        public List<string> ObtenerAccesibilidades()
        {
            List<string> lista = new List<string>();

            if (Visual) lista.Add("Visual");
            if (Verbal) lista.Add("Verbal");
            if (Auditiva) lista.Add("Auditiva");
            if (Intelectual) lista.Add("Intelectual");
            if (Movilidad) lista.Add("Movilidad");
            if (SindromeDown) lista.Add("Síndrome de Down");
            if (Autismo) lista.Add("Autismo");

            return lista;
        }

    }
}
