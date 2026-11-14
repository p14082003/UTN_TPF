namespace Dominio.DTOs
{
    public class SP_RegistrarAdministradorInstitucion_in
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
        public string RutaConstanciaFiscal { get; set; } = "";
        public string RutaHabilitacion { get; set; } = "";
        public string RutaConstanciaLaboral { get; set; } = "";
    }
}
