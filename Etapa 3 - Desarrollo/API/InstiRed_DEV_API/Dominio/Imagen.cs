namespace Dominio
{
    public class Imagen
    {
        public int Id { get; set; }

        public int IdInstitucion { get; set; }

        public string UrlImagen { get; set; } = "";

        public string Descripcion { get; set; } = "";
    }
}
