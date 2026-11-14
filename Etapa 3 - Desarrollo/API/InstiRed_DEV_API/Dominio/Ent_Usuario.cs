namespace Dominio
{
    public class Ent_Usuario
    {
        public int Id { get; set; }

        public string Clave { get; set; } = "";

        public string Email { get; set; } = "";

        public bool Activo { get; set; }

        public int IdTipoUsuario { get; set; }
    }
}
