namespace Dominio.DTOs
{
    public class SP_CrearInstitucion_in
    {
        public int IdAdministrador { get; set; }
        public string Direccion { get; set; } = "";
        public string Email { get; set; } = "";
        public double? Latitud { get; set; }
        public double? Longitud { get; set; }
        public string Descripcion { get; set; } = "";
        public int IdTipoInstitucion { get; set; }
        public bool? Movilidad { get; set; }
        public bool? Visual { get; set; }
        public bool? Auditiva { get; set; }
        public bool? Verbal { get; set; }
        public bool? Intelectual { get; set; }
        public bool? SindromeDown { get; set; }
        public bool? Autismo { get; set; }
    }
}
