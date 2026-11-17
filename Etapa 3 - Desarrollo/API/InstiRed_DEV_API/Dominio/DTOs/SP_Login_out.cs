namespace Dominio.DTOs
{
    public class SP_Login_out
    {
        public bool Ok { get; set; }
        public int IdUsuario { get; set; }
        public int IdTipoUsuario { get; set; }
        public int? IdResponsable { get; set; }
        public int? IdInstitucion { get; set; }
        public string? Nombre { get; set; }
    }
}
