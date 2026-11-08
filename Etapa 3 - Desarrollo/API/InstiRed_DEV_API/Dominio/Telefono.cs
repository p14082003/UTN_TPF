namespace Dominio
{
    public class Telefono
    {
        public int Id { get; set; }

        public int? IdInstitucion { get; set; }

        public int? IdAdministrador { get; set; }

        public int? IdResponsable { get; set; }

        public string NumeroTelefono { get; set; } = "";

    }
}
