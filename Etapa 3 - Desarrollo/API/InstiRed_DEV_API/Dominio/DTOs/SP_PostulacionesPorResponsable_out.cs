namespace Dominio.DTOs
{
    public class SP_PostulacionesPorResponsable_out
    {
        public int IdPostulacion { get; set; }
        public string Alumno { get; set; } = "";
        public string Documento { get; set; } = "";
        public string Institucion { get; set; } = "";
        public int CicloLectivo { get; set; }
        public string Estado { get; set; } = "";
        public int IdEstado { get; set; }
        public int IdAlumno { get; set; }
    }
}
