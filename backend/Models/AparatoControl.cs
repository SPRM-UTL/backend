using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace backend.Models
{
    public class AparatoControl
    {
        [Key]
        public int sk_aparato_control_id { get; set; }

        public int sk_aparato_controlador_id { get; set; }

        public int sk_aparato_controlado_id { get; set; }

        [MaxLength(100)]
        public string? comando_socket { get; set; }

        public bool activo { get; set; } = true;

        public DateTime fecha_creacion { get; set; } = DateTime.UtcNow;

        [ForeignKey(nameof(sk_aparato_controlador_id))]
        [JsonIgnore]
        public Aparato? Controlador { get; set; }

        [ForeignKey(nameof(sk_aparato_controlado_id))]
        [JsonIgnore]
        public Aparato? Controlado { get; set; }
    }
}
