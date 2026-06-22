using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace backend.Models
{
    [Table("gesto_detalle")]
    public class GestoDetalle
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }

        [Required]
        [Column("sk_gesto_id")]
        public int GestoId { get; set; }

        // Aquí dejas tu propiedad de navegación hacia la tabla Gesto si ya existe

        [JsonIgnore]
        [ForeignKey("GestoId")]
        public virtual Gesto? Gesto { get; set; }

        [Required]
        [Column("duracion_segundos", TypeName = "decimal(5,2)")]
        public decimal DuracionSegundos { get; set; }

        [Required]
        [MaxLength(100)]
        [Column("iluminacion_recomendada")]
        public string IluminacionRecomendada { get; set; } = string.Empty;

        [Required]
        [MaxLength(100)]
        [Column("distancia_recomendada")]
        public string DistanciaRecomendada { get; set; } = string.Empty;

        public virtual ICollection<GestoMedia> MediosReferencia { get; set; } = new List<GestoMedia>();
    }
}