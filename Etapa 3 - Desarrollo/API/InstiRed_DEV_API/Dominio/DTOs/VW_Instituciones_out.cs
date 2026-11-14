namespace Dominio.DTOs
{
    public class VW_Instituciones_out
    {
        public int IdInstitucion { get; set; }
        public string Nombre { get; set; } = "";
        public string Direccion { get; set; } = "";
        public string Email { get; set; } = "";
        public double Latitud { get; set; }
        public double Longitud { get; set; }
        public string Descripcion { get; set; } = "";
        public bool Estado { get; set; }
        public int IdTipoInstitucion { get; set; }
        public string TipoInstitucion { get; set; } = "";
        public bool? Movilidad { get; set; }
        public bool? Visual { get; set; }
        public bool? Auditiva { get; set; }
        public bool? Verbal { get; set; }
        public bool? Intelectual { get; set; }
        public bool? SindromeDown { get; set; }
        public bool? Autismo { get; set; }
        public string? Idiomas { get; set; }
        public string? Telefonos { get; set; }
        public decimal? PromedioCalificacion { get; set; }
        public long CantidadResenias { get; set; }
    }
}
