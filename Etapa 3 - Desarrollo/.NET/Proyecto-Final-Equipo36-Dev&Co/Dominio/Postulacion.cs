using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;

namespace Dominio
{
    public class Postulacion
    {
        public int Id { get; set; }
        public string CodigoSeguimiento { get; set; }
        public DateTime Fecha { get; set; }
        public Alumno Alumno { get; set; }
        public Institucion Institucion { get; set; }
        public EstadoPostulacion EstadoPostulacion { get; set; }

    }
}
