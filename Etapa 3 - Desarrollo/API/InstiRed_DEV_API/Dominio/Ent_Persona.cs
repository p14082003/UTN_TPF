namespace Dominio
{
    public class Ent_Persona
    {
        public int Id { get; set; }

        public string Nombre { get; set; } = "";

        public string Apellido { get; set; } = "";

        public string Documento { get; set; } = "";

        public string Domicilio { get; set; } = "";

        public DateTime FechaNacimiento { get; set; }
    }
}
