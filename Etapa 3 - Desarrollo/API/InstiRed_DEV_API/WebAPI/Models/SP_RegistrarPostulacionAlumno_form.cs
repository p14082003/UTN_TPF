namespace WebAPI.Models
{
    public class SP_RegistrarPostulacionAlumno_form
    {
        public int IdResponsable { get; set; }
        public int IdAlumno { get; set; }
        public int IdCurso { get; set; }
        public List<SP_DocumentoPostulacionAlumno_form> Documentos { get; set; } = new List<SP_DocumentoPostulacionAlumno_form>();
    }
}
