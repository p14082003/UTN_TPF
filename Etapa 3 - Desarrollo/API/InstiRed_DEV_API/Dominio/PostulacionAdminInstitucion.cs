namespace Dominio
{
    public class PostulacionAdminInstitucion
    {
        public int Id { get; set; }

        public DateTime Fecha { get; set; }

        public string CodigoSeguimiento { get; set; } = "";

        public int IdAdministradorInstitucion { get; set; }

        public int IdEstado { get; set; }

        public string NombreInstitucion { get; set; } = "";

        public string CuitInstitucion { get; set; } = "";

        public string RutaConstanciaFiscal { get; set; } = "";

        public string RutaHabilitacion { get; set; } = "";

        public string RutaConstanciaLaboral { get; set; } = "";

        public string? MotivoRechazo { get; set; }
    }
}
