using System.Text.Json.Serialization;

namespace backend.DTOs
{
    public class UsuarioProfileDto
    {
        [JsonPropertyName("id")]
        public int Id { get; set; }

        [JsonPropertyName("nombre")]
        public string? Nombre { get; set; }

        [JsonPropertyName("correo")]
        public string? Correo { get; set; }
    }
}
