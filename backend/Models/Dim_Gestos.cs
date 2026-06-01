using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace backend.Models
{
    public class Dim_Gestos
    {
        [Key]
        public int sk_gesto_id { get; set; }

        public int bk_gesto_id { get; set; }

        [MaxLength(100)]
        public string? nombre_gesto { get; set; }

        public int identificador_ia { get; set; }

        [Column(TypeName = "decimal(5,2)")]
        public decimal nivel_confianza_minimo { get; set; }

        [MaxLength(100)]
        public string? tipo_disparador_nombre { get; set; }

        [JsonIgnore]
        public List<Fact_Historico_Actividad>? Historico_Actividad { get; set; }
    }
}