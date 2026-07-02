using System.Text.Json.Serialization;

namespace backend.DTOs
{
    public class AparatoMensajeDto
    {
        [JsonPropertyName("sk_mensaje_id")]
        public long SkMensajeId { get; set; }

        [JsonPropertyName("sk_aparato_id")]
        public int SkAparatoId { get; set; }

        [JsonPropertyName("direccion")]
        public string Direccion { get; set; } = string.Empty;

        [JsonPropertyName("payload_json")]
        public string PayloadJson { get; set; } = "{}";

        [JsonPropertyName("comando")]
        public string? Comando { get; set; }

        [JsonPropertyName("procesado")]
        public bool Procesado { get; set; }

        [JsonPropertyName("fecha_creacion")]
        public DateTime FechaCreacion { get; set; }
    }

    public class AparatoEstadoRedDto
    {
        [JsonPropertyName("sk_aparato_id")]
        public int SkAparatoId { get; set; }

        [JsonPropertyName("device_key")]
        public string? DeviceKey { get; set; }

        [JsonPropertyName("estado_encendido")]
        public bool? EstadoEncendido { get; set; }

        [JsonPropertyName("conectado")]
        public bool Conectado { get; set; }

        [JsonPropertyName("fecha_estado_actualizado")]
        public DateTime? FechaEstadoActualizado { get; set; }

        [JsonPropertyName("origen_estado")]
        public string? OrigenEstado { get; set; }
    }
}
