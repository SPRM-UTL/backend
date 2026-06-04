using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace backend.Models
{
    public class Aparato
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

        [MaxLength(17)]
        public string? mac_bluetooth { get; set; }

        [MaxLength(100)]
        public string? nombre_bluetooth { get; set; }

        public DateTime? fecha_sincronizacion { get; set; }

        [JsonIgnore]
        public List<HistorialActividad>? Historico_Actividad { get; set; }

        [JsonIgnore]
        public List<Gesto>? Gestos { get; set; }

        public int? sk_usuario_id { get; set; }

        [ForeignKey("sk_usuario_id")]
        [JsonIgnore]
        public Usuario? Usuario { get; set; }
    }
}
