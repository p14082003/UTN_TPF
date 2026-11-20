namespace Dominio.DTOs
{
    public class SP_Login_out
    {
        public bool Ok { get; set; }
        public int IdUsuario { get; set; }
        public string Email { get; set; } = "";
        public int IdTipoUsuario { get; set; }
        public int? IdPersona { get; set; }
        public int? IdResponsable { get; set; }
        public int? IdAlumno { get; set; }
        public int? IdInstitucion { get; set; }
        public int? IdAdministradorInstitucion { get; set; }
        public string? Nombre { get; set; }
        public string? Apellido { get; set; }
    }
}
