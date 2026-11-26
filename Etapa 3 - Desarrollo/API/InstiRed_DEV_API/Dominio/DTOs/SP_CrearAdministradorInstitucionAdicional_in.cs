namespace Dominio.DTOs
{
    public class SP_CrearAdministradorInstitucionAdicional_in
    {
        public int IdUsuarioActor { get; set; }
        public string Nombre { get; set; } = "";
        public string Apellido { get; set; } = "";
        public string Documento { get; set; } = "";
        public string Domicilio { get; set; } = "";
        public DateTime FechaNacimiento { get; set; }
        public string Email { get; set; } = "";
        public string Telefono { get; set; } = "";
    }
}
