using System.Text.Json.Serialization;

namespace backend.DTOs
{
    public class AparatoTipoDto
    {
        [JsonPropertyName("sk_aparato_tipo_id")]
        public int SkAparatoTipoId { get; set; }

        [JsonPropertyName("nombre_tipo")]
        public string NombreTipo { get; set; } = string.Empty;

        [JsonPropertyName("icono")]
        public string? Icono { get; set; }

        [JsonPropertyName("es_asistente")]
        public bool EsAsistente { get; set; }

        [JsonPropertyName("soporta_bluetooth")]
        public bool SoportaBluetooth { get; set; }

        [JsonPropertyName("soporta_wifi")]
        public bool SoportaWifi { get; set; }

        [JsonPropertyName("requiere_vinculacion_bluetooth")]
        public bool RequiereVinculacionBluetooth { get; set; }

        [JsonPropertyName("orden")]
        public int Orden { get; set; }

        [JsonPropertyName("palabras_clave_busqueda")]
        public string? PalabrasClaveBusqueda { get; set; }
    }
}
