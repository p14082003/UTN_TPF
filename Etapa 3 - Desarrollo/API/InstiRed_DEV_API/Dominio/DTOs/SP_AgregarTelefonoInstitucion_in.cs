namespace Dominio.DTOs
{
    public class SP_AgregarTelefonoInstitucion_in
    {
        public int IdAdministrador { get; set; }
        public int IdInstitucion { get; set; }
        public string Telefono { get; set; } = "";
    }
}
