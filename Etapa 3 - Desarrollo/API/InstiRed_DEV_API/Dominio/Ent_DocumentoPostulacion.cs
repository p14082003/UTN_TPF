namespace Dominio
{
    public class Ent_DocumentoPostulacion
    {
        public int Id { get; set; }

        public int IdPostulacionAlumnos { get; set; }

        public string Descripcion { get; set; } = "";

        public string Ruta { get; set; } = "";
    }
}
