using System;
using System.Text.Json.Serialization;

namespace backend.DTOs
{
    public class RegistrarConsumoDto
    {
        [JsonPropertyName("sk_aparato_id")]
        public int? SkAparatoId { get; set; }

        [JsonPropertyName("sk_aparato_configuracion_red_id")]
        public int? SkAparatoConfiguracionRedId { get; set; }

        [JsonPropertyName("corriente_a")]
        public decimal CorrienteA { get; set; }

        [JsonPropertyName("potencia_w")]
        public decimal PotenciaW { get; set; }

        [JsonPropertyName("energia_wh")]
        public decimal? EnergiaWh { get; set; }

        [JsonPropertyName("fecha_medicion")]
        public DateTime? FechaMedicion { get; set; }
    }
}
