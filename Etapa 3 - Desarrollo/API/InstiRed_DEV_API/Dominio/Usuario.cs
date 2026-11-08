namespace Dominio
{
    public class Usuario
    {
        public int Id { get; set; }

        public string Clave { get; set; } = "";

        public string Email { get; set; } = "";

        public bool Activo { get; set; }

        public int IdTipoUsuario { get; set; }
    }
}
