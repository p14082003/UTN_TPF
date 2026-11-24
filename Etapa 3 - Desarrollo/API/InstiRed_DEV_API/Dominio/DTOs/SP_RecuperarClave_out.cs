namespace Dominio.DTOs
{
    public class SP_RecuperarClave_out
    {
        public int IdUsuario { get; set; }
        public string Email { get; set; } = "";
        public string? Nombre { get; set; }
        public string? Apellido { get; set; }
        // La WebApp usa esta clave únicamente en el servidor para preparar el correo.
        public string NuevaClaveGenerada { get; set; } = "";
    }
}
