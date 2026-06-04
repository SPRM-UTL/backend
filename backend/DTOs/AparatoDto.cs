using System.Text.Json.Serialization;

namespace backend.DTOs
{
    public class AparatoDto
    {
        [JsonPropertyName("sk_aparato_id")]
        public int SkAparatoId { get; set; }

        [JsonPropertyName("nombre_aparato")]
        public string? NombreAparato { get; set; }

        [JsonPropertyName("tipo_aparato")]
        public string? TipoAparato { get; set; }

        [JsonPropertyName("accion_nombre")]
        public string? AccionNombre { get; set; }

        [JsonPropertyName("comando_bluetooth")]
        public string? ComandoBluetooth { get; set; }

        [JsonPropertyName("icono")]
        public string? Icono { get; set; }

        [JsonPropertyName("mac_bluetooth")]
        public string? MacBluetooth { get; set; }

        [JsonPropertyName("nombre_bluetooth")]
        public string? NombreBluetooth { get; set; }

        [JsonPropertyName("fecha_sincronizacion")]
        public DateTime? FechaSincronizacion { get; set; }
    }
}
