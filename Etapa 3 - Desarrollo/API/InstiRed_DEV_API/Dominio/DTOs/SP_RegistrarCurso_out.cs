namespace Dominio.DTOs
{
    public class SP_RegistrarCurso_out
    {
        public int Id { get; set; }
        public int IdInstitucion { get; set; }
        public int IdTurno { get; set; }
        public int CicloLectivo { get; set; }
        public int? IdNivel { get; set; }
        public int? Anio { get; set; }
        public string? NivelPersonalizado { get; set; }
        public string Division { get; set; } = "";
        public int Capacidad { get; set; }
        public int Inscriptos { get; set; }
    }
}
