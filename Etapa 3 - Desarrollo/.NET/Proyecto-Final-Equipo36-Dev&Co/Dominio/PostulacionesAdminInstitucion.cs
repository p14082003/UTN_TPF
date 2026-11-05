using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dominio
{
    public class PostulacionesAdminInstitucion
    {
        public int IdPostulacionAdminInstitucion { get; set; }
        public string CodigoSeguimiento { get; set; }
        public DateTime Fecha { get; set; }
        public AdministradorInstitucion Administrador { get; set; }
        public EstadoPostulacion EstadoPostulacion { get; set; }
        public string NombreInstitucion { get; set; }
        public string CuitInstitucion { get; set; }
        public string RutaConstanciaFiscal { get; set; }
        public string RutaHabilitacion { get; set; }
        public string MotivoRechazo { get; set; }

    }
}
