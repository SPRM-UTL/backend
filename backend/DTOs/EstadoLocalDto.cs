using System.Text.Json.Serialization;

namespace backend.DTOs
{
    public class EstadoLocalDto
    {
        [JsonPropertyName("estado_encendido")]
        public bool EstadoEncendido { get; set; }
    }
}
