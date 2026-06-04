using System.Text.Json.Serialization;

namespace backend.DTOs
{
    public class LoginResponseDto
    {
        [JsonPropertyName("id")]
        public int Id { get; set; }

        [JsonPropertyName("nombre")]
        public string? Nombre { get; set; }

        [JsonPropertyName("token")]
        public string Token { get; set; } = string.Empty;
    }

    public class RegisterResponseDto
    {
        [JsonPropertyName("mensaje")]
        public string Mensaje { get; set; } = string.Empty;
    }

    public class UsuarioProfileDto
    {
        [JsonPropertyName("id")]
        public int Id { get; set; }

        [JsonPropertyName("nombre")]
        public string? Nombre { get; set; }

        [JsonPropertyName("correo")]
        public string? Correo { get; set; }
    }

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

    public class GestoDto
    {
        [JsonPropertyName("sk_gesto_id")]
        public int SkGestoId { get; set; }

        [JsonPropertyName("bk_gesto_id")]
        public int BkGestoId { get; set; }

        [JsonPropertyName("nombre_gesto")]
        public string? NombreGesto { get; set; }

        [JsonPropertyName("identificador_ia")]
        public int IdentificadorIa { get; set; }

        [JsonPropertyName("nivel_confianza_minimo")]
        public decimal NivelConfianzaMinimo { get; set; }

        [JsonPropertyName("tipo_disparador_nombre")]
        public string? TipoDisparadorNombre { get; set; }

        [JsonPropertyName("sk_aparato_id")]
        public int? SkAparatoId { get; set; }
    }

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
