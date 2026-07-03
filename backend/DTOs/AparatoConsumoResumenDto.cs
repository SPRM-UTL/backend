using System.Text.Json.Serialization;

namespace backend.DTOs
{
    public class AparatoConsumoPuntoDto
    {
        [JsonPropertyName("periodo")]
        public DateTime Periodo { get; set; }

        [JsonPropertyName("potencia_promedio_w")]
        public float PotenciaPromedioW { get; set; }

        [JsonPropertyName("corriente_promedio_a")]
        public float CorrientePromedioA { get; set; }

        [JsonPropertyName("energia_consumida_wh")]
        public float EnergiaConsumidaWh { get; set; }
    }

    public class AparatoConsumoResumenDto
    {
        [JsonPropertyName("granularidad")]
        public string Granularidad { get; set; } = string.Empty;

        [JsonPropertyName("desde")]
        public DateTime Desde { get; set; }

        [JsonPropertyName("hasta")]
        public DateTime Hasta { get; set; }

        [JsonPropertyName("puntos")]
        public List<AparatoConsumoPuntoDto> Puntos { get; set; } = new();
    }
}
