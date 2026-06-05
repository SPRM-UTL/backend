using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace backend.Models
{
    public class AparatoAccion
    {
        [Key]
        public int sk_aparato_accion_id { get; set; }

        [MaxLength(100)]
        public string accion_nombre { get; set; } = string.Empty;

        [MaxLength(50)]
        public string? comando_bluetooth { get; set; }

        [JsonIgnore]
        public List<Aparato>? Aparatos { get; set; }
    }
}
