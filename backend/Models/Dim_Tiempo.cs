using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace backend.Models
{
    public class Dim_Tiempo
    {
        [Key]
        public int sk_tiempo_id { get; set; }
        public DateOnly fecha_completa { get; set; }
        public int anio { get; set; }
        public int mes_numero { get; set; }

        [MaxLength(15)]
        public string mes_nombre { get; set; }

        [MaxLength(15)]
        public string dia_semana_nombre { get; set; }
        public int hora_periodo { get; set; }

        [JsonIgnore]
        public List<Fact_Historico_Actividad>? Historico_Actividad { get; set; }
    }
}
