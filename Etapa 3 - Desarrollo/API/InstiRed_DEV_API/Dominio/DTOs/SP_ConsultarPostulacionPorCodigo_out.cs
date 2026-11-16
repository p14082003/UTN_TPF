namespace Dominio.DTOs
{
    public class SP_ConsultarPostulacionPorCodigo_out
    {
        public string Tipo { get; set; } = "";
        public DateTime Fecha { get; set; }
        public string Estado { get; set; } = "";
        public string NombreInstitucion { get; set; } = "";
        public string Cuit { get; set; } = "";
        public string? MotivoRechazo { get; set; }
    }
}
