using System.Text.Json.Serialization;

namespace backend.DTOs
{
    public class RegisterResponseDto
    {
        [JsonPropertyName("mensaje")]
        public string Mensaje { get; set; } = string.Empty;
    }
}
