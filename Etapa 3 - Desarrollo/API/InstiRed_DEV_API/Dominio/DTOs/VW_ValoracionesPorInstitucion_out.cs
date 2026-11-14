namespace Dominio.DTOs
{
    public class VW_ValoracionesPorInstitucion_out
    {
        public int IdResenia { get; set; }
        public int IdInstitucion { get; set; }
        public string Institucion { get; set; } = "";
        public int IdResponsable { get; set; }
        public string NombreResponsable { get; set; } = "";
        public string ApellidoResponsable { get; set; } = "";
        public int Calificacion { get; set; }
        public string Comentario { get; set; } = "";
        public string? RespuestaInstitucion { get; set; }
        public DateTime Fecha { get; set; }
    }
}
