using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace backend.Models
{
    public class AparatoMensaje
    {
        [Key]
        public long sk_mensaje_id { get; set; }

        public int sk_aparato_configuracion_red_id { get; set; }

        [MaxLength(10)]
        public string direccion { get; set; } = "inbound";

        public string payload_json { get; set; } = "{}";

        [MaxLength(100)]
        public string? comando { get; set; }

        public bool procesado { get; set; } = true;

        [MaxLength(500)]
        public string? error_procesamiento { get; set; }

        public DateTime fecha_creacion { get; set; } = DateTime.UtcNow;

        [ForeignKey(nameof(sk_aparato_configuracion_red_id))]
        [JsonIgnore]
        public AparatoConfiguracionRed? ConfiguracionRed { get; set; }
    }
}
