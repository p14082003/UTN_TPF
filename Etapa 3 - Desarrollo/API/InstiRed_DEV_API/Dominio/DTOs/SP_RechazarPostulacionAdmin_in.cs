namespace Dominio.DTOs
{
    public class SP_RechazarPostulacionAdmin_in
    {
        public int IdPostulacion { get; set; }
        public int IdUsuarioDevCo { get; set; }
        public string MotivoRechazo { get; set; } = "";
    }
}
