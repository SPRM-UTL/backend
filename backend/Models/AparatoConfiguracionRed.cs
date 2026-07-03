using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace backend.Models
{
    public class AparatoConfiguracionRed
    {
        [Key]
        public int sk_aparato_configuracion_red_id { get; set; }

        public int sk_aparato_id { get; set; }

        [MaxLength(100)]
        public string? device_key { get; set; }

        [MaxLength(45)]
        public string? ip_address { get; set; }

        [MaxLength(17)]
        public string? mac_address { get; set; }

        [MaxLength(100)]
        public string? host_name { get; set; }

        public int? puerto_socket { get; set; }

        [MaxLength(20)]
        public string? protocolo_socket { get; set; }

        [MaxLength(200)]
        public string? ruta_socket { get; set; }

        public bool activo { get; set; } = true;

        public DateTime fecha_creacion { get; set; } = DateTime.UtcNow;

        public DateTime? fecha_ultima_conexion { get; set; }

        public bool? estado_encendido { get; set; }

        public DateTime? fecha_estado_actualizado { get; set; }

        [MaxLength(20)]
        public string? origen_estado { get; set; }

        [Column(TypeName = "decimal(8,3)")]
        public decimal? corriente_actual { get; set; }

        [Column(TypeName = "decimal(10,2)")]
        public decimal? potencia_actual { get; set; }

        [Column(TypeName = "decimal(12,3)")]
        public decimal? energia_acumulada_wh { get; set; }

        public DateTime? fecha_medicion_consumo { get; set; }

        [JsonIgnore]
        public List<AparatoMensaje>? Mensajes { get; set; }

        [JsonIgnore]
        public List<AparatoConsumoHistorico>? ConsumoHistorico { get; set; }

        [ForeignKey(nameof(sk_aparato_id))]
        [JsonIgnore]
        public Aparato? Aparato { get; set; }
    }
}
