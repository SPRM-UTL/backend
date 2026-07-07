using System.Text.Json.Serialization;

namespace backend.DTOs
{
    public class GestoDto
    {
        [JsonPropertyName("sk_gesto_id")]
        public int SkGestoId { get; set; }

        [JsonPropertyName("bk_gesto_id")]
        public int BkGestoId { get; set; }

        [JsonPropertyName("nombre_gesto")]
        public string? NombreGesto { get; set; }

        [JsonPropertyName("icono")]
        public string? Icono { get; set; }

        [JsonPropertyName("identificador_ia")]
        public int IdentificadorIa { get; set; }

        [JsonPropertyName("nivel_confianza_minimo")]
        public decimal NivelConfianzaMinimo { get; set; }

        [JsonPropertyName("tipo_disparador_nombre")]
        public string? TipoDisparadorNombre { get; set; }

        [JsonPropertyName("sk_aparato_id")]
        public int? SkAparatoId { get; set; }

        [JsonPropertyName("pasos")]
        public List<GestoPasoDto>? Pasos { get; set; }
    }
}
