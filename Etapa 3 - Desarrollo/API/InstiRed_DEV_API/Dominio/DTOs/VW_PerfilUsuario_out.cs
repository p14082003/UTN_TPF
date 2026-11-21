namespace Dominio.DTOs
{
    public class VW_PerfilUsuario_out
    {
        public int IdUsuario { get; set; }
        public string Email { get; set; } = "";
        public int IdTipoUsuario { get; set; }
        public string TipoUsuario { get; set; } = "";
        public string? Nombre { get; set; }
        public string? Apellido { get; set; }
        public string? Documento { get; set; }
        public DateTime? FechaNacimiento { get; set; }
        public string? Domicilio { get; set; }
        public int? IdResponsable { get; set; }
        public int? IdAdministrador { get; set; }
        public int? IdInstitucion { get; set; }
        public string? NombreInstitucion { get; set; }
    }
}
