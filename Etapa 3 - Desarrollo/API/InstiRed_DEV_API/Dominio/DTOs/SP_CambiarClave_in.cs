namespace Dominio.DTOs
{
    public class SP_CambiarClave_in
    {
        public int IdUsuarioActor { get; set; }
        public string ClaveActual { get; set; } = "";
        public string ClaveNueva { get; set; } = "";
    }
}
