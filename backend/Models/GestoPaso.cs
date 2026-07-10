using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace backend.Models
{
    [Table("gesto_paso")]
    public class GestoPaso
    {
        [Key]
        [Column("sk_gesto_paso_id")]
        public int sk_gesto_paso_id { get; set; }

        [Required]
        [Column("sk_gesto_id")]
        public int sk_gesto_id { get; set; }

        [JsonIgnore]
        [ForeignKey("sk_gesto_id")]
        public virtual Gesto? Gesto { get; set; }

        [Required]
        [Column("orden")]
        public int orden { get; set; }

        [Required]
        [Column("es_activador")]
        public bool es_activador { get; set; }

        [Required]
        [MaxLength(100)]
        [Column("nombre_gesto")]
        public string nombre_gesto { get; set; } = string.Empty;

        [Required]
        [MaxLength(20)]
        [Column("mano_objetivo")]
        public string mano_objetivo { get; set; } = "ANY";

        [Required]
        [Column("cuadros_requeridos")]
        public int cuadros_requeridos { get; set; }
    }
}
