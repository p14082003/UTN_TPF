namespace Dominio.DTOs
{
    public class SP_RechazarPostulacionAlumno_in
    {
        public int IdPostulacion { get; set; }
        public int IdAdministradorInstitucion { get; set; }
        public string MotivoRechazo { get; set; } = "";
    }
}
