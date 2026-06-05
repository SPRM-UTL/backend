using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace backend.Models
{
    public class AparatoTipo
    {
        [Key]
        public int sk_aparato_tipo_id { get; set; }

        [MaxLength(50)]
        public string nombre_tipo { get; set; } = string.Empty;

        [JsonIgnore]
        public List<Aparato>? Aparatos { get; set; }
    }
}
