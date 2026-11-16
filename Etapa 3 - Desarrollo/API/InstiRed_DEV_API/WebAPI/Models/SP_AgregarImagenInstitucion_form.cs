namespace WebAPI.Models
{
    public class SP_AgregarImagenInstitucion_form
    {
        public int IdAdministrador { get; set; }
        public int IdInstitucion { get; set; }
        public string Descripcion { get; set; } = "";
        public IFormFile? Imagen { get; set; }
    }
}
