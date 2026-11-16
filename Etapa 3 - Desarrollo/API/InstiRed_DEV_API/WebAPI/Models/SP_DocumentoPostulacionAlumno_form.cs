namespace WebAPI.Models
{
    public class SP_DocumentoPostulacionAlumno_form
    {
        public string Descripcion { get; set; } = "";
        public IFormFile? Archivo { get; set; }
    }
}
