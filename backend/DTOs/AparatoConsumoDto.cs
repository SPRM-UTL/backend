using System.Text.Json.Serialization;

namespace backend.DTOs
{
    public class AparatoConsumoDto
    {
        [JsonPropertyName("sk_consumo_id")]
        public long SkConsumoId { get; set; }

        [JsonPropertyName("sk_aparato_id")]
        public int SkAparatoId { get; set; }

        [JsonPropertyName("corriente_a")]
        public decimal CorrienteA { get; set; }

        [JsonPropertyName("potencia_w")]
        public decimal PotenciaW { get; set; }

        [JsonPropertyName("energia_wh")]
        public decimal EnergiaWh { get; set; }

        [JsonPropertyName("fecha_medicion")]
        public DateTime FechaMedicion { get; set; }
    }

    public class AparatoConsumoActualDto
    {
        [JsonPropertyName("corriente_a")]
        public decimal? CorrienteA { get; set; }

        [JsonPropertyName("potencia_w")]
        public decimal? PotenciaW { get; set; }

        [JsonPropertyName("energia_acumulada_wh")]
        public decimal? EnergiaAcumuladaWh { get; set; }

        [JsonPropertyName("fecha_medicion_consumo")]
        public DateTime? FechaMedicionConsumo { get; set; }
    }
}
