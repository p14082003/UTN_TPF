namespace Dominio.DTOs
{
    public class SP_AgregarTelefono_in
    {
        public int IdUsuarioActor { get; set; }
        public string TipoDueno { get; set; } = "";
        public int IdDueno { get; set; }
        public string Telefono { get; set; } = "";
    }
}
