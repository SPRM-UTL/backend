using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace backend.Models
{
    public class Tiempo
    {
        [Key]
        public int sk_tiempo_id { get; set; }

        public DateOnly fecha_completa { get; set; }

        public int anio { get; set; }

        public int mes_numero { get; set; }

        public string? mes_nombre { get; set; }

        public string? dia_semana_nombre { get; set; }

        public int hora_periodo { get; set; }

        [JsonIgnore]
        public List<HistorialActividad>? Historico_Actividad { get; set; }
    }
}
