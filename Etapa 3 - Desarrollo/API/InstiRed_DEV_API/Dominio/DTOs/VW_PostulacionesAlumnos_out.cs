namespace Dominio.DTOs
{
    public class VW_PostulacionesAlumnos_out
    {
        public int IdPostulacion { get; set; }
        public DateTime Fecha { get; set; }
        public string CodigoSeguimiento { get; set; } = "";
        public int IdEstado { get; set; }
        public string Estado { get; set; } = "";
        public string? MotivoRechazo { get; set; }
        public int IdAlumno { get; set; }
        public string NombreAlumno { get; set; } = "";
        public string ApellidoAlumno { get; set; } = "";
        public string DocAlumno { get; set; } = "";
        public DateTime FechaNacimientoAlumno { get; set; }
        public int? IdResponsable { get; set; }
        public string? NombreResponsable { get; set; }
        public string? ApellidoResponsable { get; set; }
        public string? EmailResponsable { get; set; }
        public string? TelefonosResponsable { get; set; }
        public string RolResponsable { get; set; } = "";
        public int IdCurso { get; set; }
        public int CicloLectivo { get; set; }
        public string? Nivel { get; set; }
        public int? Anio { get; set; }
        public string Division { get; set; } = "";
        public string Turno { get; set; } = "";
        public int IdInstitucion { get; set; }
        public string Institucion { get; set; } = "";
    }
}
