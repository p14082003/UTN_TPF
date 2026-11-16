namespace Dominio.DTOs
{
    public class SP_AgregarImagenInstitucion_in
    {
        public int IdAdministrador { get; set; }
        public int IdInstitucion { get; set; }
        public string UrlImagen { get; set; } = "";
        public string Descripcion { get; set; } = "";
    }
}
