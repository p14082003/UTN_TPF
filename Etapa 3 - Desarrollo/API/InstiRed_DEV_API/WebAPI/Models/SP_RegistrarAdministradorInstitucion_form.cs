namespace WebAPI.Models
{
    public class SP_RegistrarAdministradorInstitucion_form
    {
        public string Nombre { get; set; } = "";
        public string Apellido { get; set; } = "";
        public string Documento { get; set; } = "";
        public string Domicilio { get; set; } = "";
        public DateTime FechaNacimiento { get; set; }
        public string Clave { get; set; } = "";
        public string Email { get; set; } = "";
        public string Telefono { get; set; } = "";
        public string NombreInstitucion { get; set; } = "";
        public string CuitInstitucion { get; set; } = "";
        public IFormFile? ConstanciaFiscal { get; set; }
        public IFormFile? Habilitacion { get; set; }
        public IFormFile? ConstanciaLaboral { get; set; }
    }
}
