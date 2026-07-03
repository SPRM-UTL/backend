using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace backend.Models
{
    public class AparatoConsumoHistorico
    {
        [Key]
        public long sk_consumo_id { get; set; }

        public int sk_aparato_configuracion_red_id { get; set; }

        [Column(TypeName = "decimal(8,3)")]
        public decimal corriente_a { get; set; }

        [Column(TypeName = "decimal(10,2)")]
        public decimal potencia_w { get; set; }

        [Column(TypeName = "decimal(12,3)")]
        public decimal energia_wh { get; set; }

        public DateTime fecha_medicion { get; set; } = DateTime.UtcNow;

        [ForeignKey(nameof(sk_aparato_configuracion_red_id))]
        [JsonIgnore]
        public AparatoConfiguracionRed? ConfiguracionRed { get; set; }
    }
}
