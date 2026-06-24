using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace backend.Models
{
    public class Habitacion
    {
        [Key]
        public int sk_habitacion_id { get; set; }

        [MaxLength(100)]
        public string? nombre_habitacion { get; set; }

        public int? sk_casa_id { get; set; }

        [ForeignKey(nameof(sk_casa_id))]
        [JsonIgnore]
        public Casa? Casa { get; set; }

        [JsonIgnore]
        public List<Aparato>? Aparatos { get; set; }
    }
}
