namespace Dominio.DTOs
{
    public class SP_CrearAdministradorInstitucionAdicional_out
    {
        public int IdAdministrador { get; set; }
        public string Nombre { get; set; } = "";
        public string Apellido { get; set; } = "";
        public string Email { get; set; } = "";
        public string Clave { get; set; } = "";
    }
}
