using System.Text.Json.Serialization;

namespace backend.DTOs
{
    public class AparatoControlDto
    {
        [JsonPropertyName("sk_aparato_control_id")]
        public int SkAparatoControlId { get; set; }

        [JsonPropertyName("sk_aparato_controlador_id")]
        public int SkAparatoControladorId { get; set; }

        [JsonPropertyName("sk_aparato_controlado_id")]
        public int SkAparatoControladoId { get; set; }

        [JsonPropertyName("comando_socket")]
        public string? ComandoSocket { get; set; }

        [JsonPropertyName("activo")]
        public bool Activo { get; set; } = true;

        [JsonPropertyName("fecha_creacion")]
        public DateTime FechaCreacion { get; set; }
    }
}
