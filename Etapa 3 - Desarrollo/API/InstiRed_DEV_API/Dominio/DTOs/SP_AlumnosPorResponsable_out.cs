namespace Dominio.DTOs
{
    public class SP_AlumnosPorResponsable_out
    {
        public int IdAlumno { get; set; }
        public string NombreCompleto { get; set; } = "";
        public string Documento { get; set; } = "";
        public DateTime FechaNacimiento { get; set; }
        public string Domicilio { get; set; } = "";
        public string Rol { get; set; } = "";
    }
}
