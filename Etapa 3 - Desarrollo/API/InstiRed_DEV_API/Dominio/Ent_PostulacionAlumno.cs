namespace Dominio
{
    public class Ent_PostulacionAlumno
    {
        public int Id { get; set; }

        public DateTime Fecha { get; set; }

        public string CodigoSeguimiento { get; set; } = "";

        public int IdCurso { get; set; }

        public int IdAlumno { get; set; }

        public int IdEstado { get; set; }

        public string? MotivoRechazo { get; set; }
    }
}
