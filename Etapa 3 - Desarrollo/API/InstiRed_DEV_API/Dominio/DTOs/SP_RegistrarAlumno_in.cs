namespace Dominio.DTOs
{
    public class SP_RegistrarAlumno_in
    {
        public int IdResponsable { get; set; }
        public int? IdRol { get; set; }
        public string Documento { get; set; } = "";
        public string? Nombre { get; set; }
        public string? Apellido { get; set; }
        public string? Domicilio { get; set; }
        public DateTime? FechaNacimiento { get; set; }
    }
}
