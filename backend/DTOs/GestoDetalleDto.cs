using System.Text.Json.Serialization;

namespace backend.DTOs
{
    public class GestoDetalleDto
    {
        [JsonPropertyName("sk_gesto_detalle_id")]
        public int SkGestoDetalleId { get; set; }

        [JsonPropertyName("sk_gesto_id")]
        public int SkGestoId { get; set; }

        [JsonPropertyName("nombre_gesto")]
        public string NombreGesto { get; set; } = string.Empty;

        [JsonPropertyName("duracion_segundos")]
        public decimal DuracionSegundos { get; set; }

        [JsonPropertyName("iluminacion_recomendada")]
        public string IluminacionRecomendada { get; set; } = string.Empty;

        [JsonPropertyName("distancia_recomendada")]
        public string DistanciaRecomendada { get; set; } = string.Empty;

        [JsonPropertyName("medios_referencia")]
        public List<GestoMediaDto> MediosReferencia { get; set; } = new();
    }

    public class GestoMediaDto
    {
        [JsonPropertyName("sk_media_id")]
        public int SkMediaId { get; set; }

        [JsonPropertyName("url_archivo")]
        public string UrlArchivo { get; set; } = string.Empty;

        [JsonPropertyName("tipo_media")]
        public int TipoMedia { get; set; } // 1 = Imagen, 2 = Video

        [JsonPropertyName("extension")]
        public string? Extension { get; set; }
    }
}