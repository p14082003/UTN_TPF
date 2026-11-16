namespace Dominio.DTOs
{
    public class SP_RegistrarPostulacionAlumno_in
    {
        public int IdResponsable { get; set; }
        public int IdAlumno { get; set; }
        public int IdCurso { get; set; }
        public List<SP_DocumentoPostulacionAlumno_in> ListaDocumentos { get; set; } = new List<SP_DocumentoPostulacionAlumno_in>();
    }
}
