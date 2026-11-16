using System.Text.Json.Serialization;

namespace Dominio.DTOs
{
    public class SP_DocumentoPostulacionAlumno_in
    {
        [JsonPropertyName("descripcion")]
        public string Descripcion { get; set; } = "";

        [JsonPropertyName("ruta")]
        public string Ruta { get; set; } = "";
    }
}
