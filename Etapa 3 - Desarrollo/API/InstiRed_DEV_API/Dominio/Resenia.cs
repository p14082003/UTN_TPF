namespace Dominio
{
    public class Resenia
    {
        public int Id { get; set; }

        public int Calificacion { get; set; }

        public string Comentario { get; set; } = "";

        public string? RespuestaInstitucion { get; set; }

        public DateTime Fecha { get; set; }

        public int IdInstitucion { get; set; }

        public int IdResponsable { get; set; }
    }
}
