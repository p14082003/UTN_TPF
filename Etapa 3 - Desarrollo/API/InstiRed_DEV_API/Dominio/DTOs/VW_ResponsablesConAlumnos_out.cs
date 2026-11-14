namespace Dominio.DTOs
{
    public class VW_ResponsablesConAlumnos_out
    {
        public int IdResponsable { get; set; }
        public string NombreResponsable { get; set; } = "";
        public string ApellidoResponsable { get; set; } = "";
        public string DocResponsable { get; set; } = "";
        public string EmailResponsable { get; set; } = "";
        public bool UsuarioActivo { get; set; }
        public int? IdAlumno { get; set; }
        public string? NombreAlumno { get; set; }
        public string? ApellidoAlumno { get; set; }
        public string? DocAlumno { get; set; }
        public string? Rol { get; set; }
        public int? IdCurso { get; set; }
        public string? Institucion { get; set; }
    }
}
