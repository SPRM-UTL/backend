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

        [JsonPropertyName("sk_habitacion_id")]
        public int? SkHabitacionId { get; set; }

        [JsonPropertyName("nombre_habitacion")]
        public string? NombreHabitacion { get; set; }

        [JsonPropertyName("estado_encendido")]
        public bool? EstadoEncendido { get; set; }

        [JsonPropertyName("estado_encendido_2")]
        public bool? EstadoEncendido2 { get; set; }

        [JsonPropertyName("estado_encendido_3")]
        public bool? EstadoEncendido3 { get; set; }

        [JsonPropertyName("estado_encendido_4")]
        public bool? EstadoEncendido4 { get; set; }

        [JsonPropertyName("conectado_red")]
        public bool? ConectadoRed { get; set; }

        [JsonPropertyName("ip_address")]
        public string? IpAddress { get; set; }

        [JsonPropertyName("fecha_estado_actualizado")]
        public DateTime? FechaEstadoActualizado { get; set; }

        [JsonPropertyName("corriente_a")]
        public decimal? CorrienteA { get; set; }

        [JsonPropertyName("potencia_w")]
        public decimal? PotenciaW { get; set; }

        [JsonPropertyName("energia_acumulada_wh")]
        public decimal? EnergiaAcumuladaWh { get; set; }

        [JsonPropertyName("fecha_medicion_consumo")]
        public DateTime? FechaMedicionConsumo { get; set; }
    }
}
