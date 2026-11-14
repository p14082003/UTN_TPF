namespace Dominio.DTOs
{
    public class VW_PostulacionesAdminInstitucion_out
    {
        public int IdPostulacion { get; set; }
        public DateTime Fecha { get; set; }
        public string CodigoSeguimiento { get; set; } = "";
        public int IdEstado { get; set; }
        public string Estado { get; set; } = "";
        public int IdAdministradorInstitucion { get; set; }
        public string NombreAdministrador { get; set; } = "";
        public string ApellidoAdministrador { get; set; } = "";
        public string DocAdministrador { get; set; } = "";
        public string EmailAdministrador { get; set; } = "";
        public bool UsuarioActivo { get; set; }
        public string NombreInstitucion { get; set; } = "";
        public string CuitInstitucion { get; set; } = "";
        public string RutaConstanciaFiscal { get; set; } = "";
        public string RutaHabilitacion { get; set; } = "";
        public string RutaConstanciaLaboral { get; set; } = "";
        public string? MotivoRechazo { get; set; }
        public int? IdInstitucion { get; set; }
    }
}
