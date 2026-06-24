using System.Text.Json.Serialization;

namespace backend.DTOs
{
    public class HabitacionDto
    {
        [JsonPropertyName("sk_habitacion_id")]
        public int SkHabitacionId { get; set; }

        [JsonPropertyName("nombre_habitacion")]
        public string? NombreHabitacion { get; set; }

        [JsonPropertyName("sk_casa_id")]
        public int? SkCasaId { get; set; }

        [JsonPropertyName("aparatos")]
        public List<AparatoDto>? Aparatos { get; set; }
    }

    public class CreateHabitacionDto
    {
        [JsonPropertyName("nombre_habitacion")]
        public string? NombreHabitacion { get; set; }

        [JsonPropertyName("sk_casa_id")]
        public int SkCasaId { get; set; }
    }
}
