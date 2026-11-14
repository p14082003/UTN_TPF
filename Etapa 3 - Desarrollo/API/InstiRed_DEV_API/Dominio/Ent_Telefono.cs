namespace Dominio
{
    public class Ent_Telefono
    {
        public int Id { get; set; }

        public int? IdInstitucion { get; set; }

        public int? IdAdministrador { get; set; }

        public int? IdResponsable { get; set; }

        public string NumeroTelefono { get; set; } = "";

    }
}
