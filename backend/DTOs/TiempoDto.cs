using System.Text.Json.Serialization;

namespace backend.DTOs
{
    public class TiempoDto
    {
        [JsonPropertyName("sk_tiempo_id")]
        public int SkTiempoId { get; set; }

        [JsonPropertyName("fecha_completa")]
        public DateOnly FechaCompleta { get; set; }

        [JsonPropertyName("anio")]
        public int Anio { get; set; }

        [JsonPropertyName("mes_numero")]
        public int MesNumero { get; set; }

        [JsonPropertyName("mes_nombre")]
        public string? MesNombre { get; set; }

        [JsonPropertyName("dia_semana_nombre")]
        public string? DiaSemanaNombre { get; set; }

        [JsonPropertyName("hora_periodo")]
        public int HoraPeriodo { get; set; }
    }
}
