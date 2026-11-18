namespace Dominio.DTOs
{
    public class SP_EliminarTelefono_in
    {
        public int IdUsuarioActor { get; set; }
        public string TipoDueno { get; set; } = "";
        public int IdDueno { get; set; }
        public int IdTelefono { get; set; }
    }
}
