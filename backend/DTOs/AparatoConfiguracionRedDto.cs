using System.Text.Json.Serialization;

namespace backend.DTOs
{
    public class AparatoConfiguracionRedDto
    {
        [JsonPropertyName("sk_aparato_configuracion_red_id")]
        public int SkAparatoConfiguracionRedId { get; set; }

        [JsonPropertyName("sk_aparato_id")]
        public int SkAparatoId { get; set; }

        [JsonPropertyName("device_key")]
        public string? DeviceKey { get; set; }

        [JsonPropertyName("ip_address")]
        public string? IpAddress { get; set; }

        [JsonPropertyName("mac_address")]
        public string? MacAddress { get; set; }

        [JsonPropertyName("host_name")]
        public string? HostName { get; set; }

        [JsonPropertyName("puerto_socket")]
        public int? PuertoSocket { get; set; }

        [JsonPropertyName("protocolo_socket")]
        public string? ProtocoloSocket { get; set; }

        [JsonPropertyName("ruta_socket")]
        public string? RutaSocket { get; set; }

        [JsonPropertyName("activo")]
        public bool Activo { get; set; } = true;

        [JsonPropertyName("fecha_creacion")]
        public DateTime FechaCreacion { get; set; }

        [JsonPropertyName("fecha_ultima_conexion")]
        public DateTime? FechaUltimaConexion { get; set; }
    }
}
