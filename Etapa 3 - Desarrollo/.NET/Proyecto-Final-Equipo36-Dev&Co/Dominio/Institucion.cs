using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dominio
{
    public class Institucion
    {
        public int Id { get; set; }
        public string Nombre {  get; set; }
        public string Cuit { get; set; }
        public string Direccion { get; set; }
        public string Email { get; set;}
        public double Latitud { get; set; }
        public double Longitud { get; set; }
        public Discapacidad Discapacidad { get; set; }
        public List <Idioma> Idiomas { get; set; }
        public List <Curso> Cursos { get; set; }
        public TipoInstitucion TipoInstitucion { get; set; }
        public List <AdministradorInstitucion> AdministradoresInstitucion { get; set; }
        public List <Telefono> Telefonos { get; set; }
        public List <Resenia>  Resenias { get; set; }
        public List <Imagen> Imagenes { get; set; } 


        
    }
}
