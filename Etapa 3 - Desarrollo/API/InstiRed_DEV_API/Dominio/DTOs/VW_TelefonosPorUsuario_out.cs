namespace Dominio.DTOs
{
    public class VW_TelefonosPorUsuario_out
    {
        public int IdUsuario { get; set; }
        public int IdTelefono { get; set; }
        public string Telefono { get; set; } = "";
        public string TipoDueno { get; set; } = "";
        public int IdDueno { get; set; }
    }
}
