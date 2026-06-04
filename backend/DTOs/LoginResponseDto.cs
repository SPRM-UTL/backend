using System.Text.Json.Serialization;

namespace backend.DTOs
{
    public class LoginResponseDto
    {
        [JsonPropertyName("id")]
        public int Id { get; set; }

        [JsonPropertyName("nombre")]
        public string? Nombre { get; set; }

        [JsonPropertyName("token")]
        public string Token { get; set; } = string.Empty;
    }
}
