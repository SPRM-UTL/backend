using System.Text.Json.Serialization;

namespace backend.DTOs
{
    public class GestoPasoDto
    {
        [JsonPropertyName("sk_gesto_paso_id")]
        public int SkGestoPasoId { get; set; }

        [JsonPropertyName("orden")]
        public int Orden { get; set; }

        [JsonPropertyName("es_activador")]
        public bool EsActivador { get; set; }

        [JsonPropertyName("nombre_gesto")]
        public string NombreGesto { get; set; } = string.Empty;

        [JsonPropertyName("mano_objetivo")]
        public string ManoObjetivo { get; set; } = "ANY";

        [JsonPropertyName("cuadros_requeridos")]
        public int CuadrosRequeridos { get; set; }
    }
}
