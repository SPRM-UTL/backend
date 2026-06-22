using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace backend.Models
{
    public class AparatoTipo
    {
        [Key]
        public int sk_aparato_tipo_id { get; set; }

        [MaxLength(50)]
        public string nombre_tipo { get; set; } = string.Empty;

        [MaxLength(50)]
        public string? icono { get; set; }

        public bool es_asistente { get; set; } = false;

        public bool soporta_bluetooth { get; set; } = false;

        public bool soporta_wifi { get; set; } = true;

        public bool requiere_vinculacion_bluetooth { get; set; } = true;

        public int orden { get; set; } = 99;

        [MaxLength(255)]
        public string? palabras_clave_busqueda { get; set; }

        [JsonIgnore]
        public List<Aparato>? Aparatos { get; set; }
    }
}
