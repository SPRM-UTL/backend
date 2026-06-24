using System.Text.Json.Serialization;

namespace backend.DTOs
{
    public class CasaDto
    {
        [JsonPropertyName("sk_casa_id")]
        public int SkCasaId { get; set; }

        [JsonPropertyName("nombre_casa")]
        public string? NombreCasa { get; set; }

        [JsonPropertyName("sk_usuario_id")]
        public int? SkUsuarioId { get; set; }

        [JsonPropertyName("habitaciones")]
        public List<HabitacionDto>? Habitaciones { get; set; }
    }

    public class CreateCasaDto
    {
        [JsonPropertyName("nombre_casa")]
        public string? NombreCasa { get; set; }
    }
}
