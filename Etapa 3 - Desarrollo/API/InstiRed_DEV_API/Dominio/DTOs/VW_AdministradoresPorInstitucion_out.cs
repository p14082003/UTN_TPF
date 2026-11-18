namespace Dominio.DTOs
{
    public class VW_AdministradoresPorInstitucion_out
    {
        public int IdAdministrador { get; set; }
        public int IdInstitucion { get; set; }
        public string NombreAdministrador { get; set; } = "";
        public string ApellidoAdministrador { get; set; } = "";
        public string DocAdministrador { get; set; } = "";
        public string EmailAdministrador { get; set; } = "";
        public bool UsuarioActivo { get; set; }
    }
}
