using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace backend.Models
{
    public class Casa
    {
        [Key]
        public int sk_casa_id { get; set; }

        [MaxLength(100)]
        public string? nombre_casa { get; set; }

        public int? sk_usuario_id { get; set; }

        [ForeignKey(nameof(sk_usuario_id))]
        [JsonIgnore]
        public Usuario? Usuario { get; set; }

        [JsonIgnore]
        public List<Habitacion>? Habitaciones { get; set; }
    }
}
