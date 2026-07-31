using System.Text.Json.Serialization;

namespace backend.DTOs
{
    public class ConsumoDonaDto
    {
        [JsonPropertyName("aparato")]
        public string Aparato { get; set; } = string.Empty;

        [JsonPropertyName("total_energia_wh")]
        public decimal TotalEnergiaWh { get; set; }
    }
}
