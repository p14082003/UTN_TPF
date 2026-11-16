namespace Dominio.DTOs
{
    public class SP_RegistrarPostulacionAlumno_out
    {
        public int IdPostulacion { get; set; }
        public DateTime FechaPostulacion { get; set; }
        public string CodigoSeguimiento { get; set; } = "";
        public int IdCurso { get; set; }
        public int IdAlumno { get; set; }
        public int IdEstado { get; set; }
        public int IdInstitucion { get; set; }
        public string Nombre { get; set; } = "";
        public string Apellido { get; set; } = "";
        public string Documento { get; set; } = "";
        public string Descripcion { get; set; } = "";
    }
}
