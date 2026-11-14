namespace Dominio.DTOs
{
    public class SP_RegistrarResenia_in
    {
        public string Comentario { get; set; } = "";
        public int Calificacion { get; set; }
        public int IdPostulacion { get; set; }
        public int IdResponsable { get; set; }
    }
}
