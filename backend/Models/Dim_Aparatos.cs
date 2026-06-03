using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
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

        [MaxLength(17)]
        public string? mac_bluetooth { get; set; }

        [MaxLength(100)]
        public string? nombre_bluetooth { get; set; }

        public DateTime? fecha_sincronizacion { get; set; }

        [JsonIgnore]
        public List<Fact_Historico_Actividad>? Historico_Actividad { get; set; }

        [JsonIgnore]
        public List<Dim_Gestos>? Gestos { get; set; }

        public int? sk_usuario_id { get; set; }

        [ForeignKey("sk_usuario_id")]
        [JsonIgnore]
        public Dim_Usuarios? Usuario { get; set; }
    }
}