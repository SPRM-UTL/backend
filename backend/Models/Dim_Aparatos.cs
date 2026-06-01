using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace backend.Models
{
    public class Dim_Aparatos
    {
        [Key]
        public int sk_aparato_id { get; set; }

        [MaxLength(100)]
        public string? nombre_aparato { get; set; }

        [MaxLength(50)]
        public string? tipo_aparato { get; set; }

        [MaxLength(100)]
        public string? accion_nombre { get; set; }

        [MaxLength(50)]
        public string? comando_bluetooth { get; set; }

        [MaxLength(50)]
        public string? icono { get; set; }

        [JsonIgnore]
        public List<Fact_Historico_Actividad>? Historico_Actividad { get; set; }
    }
}