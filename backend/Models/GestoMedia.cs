using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace backend.Models
{
    [Table("gesto_media")]
    public class GestoMedia
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }

        [Required]
        [Column("gesto_detalle_id")]
        public int GestoDetalleId { get; set; }

        [JsonIgnore]
        [ForeignKey("GestoDetalleId")]
        public virtual GestoDetalle? GestoDetalle { get; set; }

        [Required]
        [MaxLength(500)]
        [Column("url_archivo")]
        public string UrlArchivo { get; set; } = string.Empty;

        [Required]
        [Column("tipo_media")]
        public int TipoMedia { get; set; } // 1 = Imagen, 2 = Video

        [MaxLength(10)]
        [Column("extension")]
        public string? Extension { get; set; }
    }
}