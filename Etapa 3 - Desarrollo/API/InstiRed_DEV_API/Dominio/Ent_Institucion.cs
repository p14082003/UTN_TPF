namespace Dominio
{
    public class Ent_Institucion
    {
        public int Id { get; set; }

        public string Nombre { get; set; } = "";

        public string Cuit { get; set; } = "";

        public string Direccion { get; set; } = "";

        public string Email { get; set; } = "";

        public double Latitud { get; set; }

        public double Longitud { get; set; }

        public string Descripcion { get; set; } = "";

        public int IdTipoInstitucion { get; set; }

        public bool Estado { get; set; }
    }
}
