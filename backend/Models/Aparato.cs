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
        public string? icono { get; set; }

        public DateTime? fecha_sincronizacion { get; set; }

        public int? sk_aparato_tipo_id { get; set; }

        public int? sk_aparato_accion_id { get; set; }

        [JsonIgnore]
        public List<HistorialActividad>? Historico_Actividad { get; set; }

        [JsonIgnore]
        public List<Gesto>? Gestos { get; set; }

        [JsonIgnore]
        public AparatoConfiguracionRed? ConfiguracionRed { get; set; }

        [JsonIgnore]
        public AparatoBluetooth? Bluetooth { get; set; }

        [ForeignKey(nameof(sk_aparato_tipo_id))]
        [JsonIgnore]
        public AparatoTipo? Tipo { get; set; }

        [ForeignKey(nameof(sk_aparato_accion_id))]
        [JsonIgnore]
        public AparatoAccion? Accion { get; set; }

        [JsonIgnore]
        public List<AparatoControl>? AparatosControlados { get; set; }

        [JsonIgnore]
        public List<AparatoControl>? Controladores { get; set; }

        public int? sk_usuario_id { get; set; }

        [ForeignKey("sk_usuario_id")]
        [JsonIgnore]
        public Usuario? Usuario { get; set; }

        public int? sk_habitacion_id { get; set; }

        [ForeignKey(nameof(sk_habitacion_id))]
        [JsonIgnore]
        public Habitacion? Habitacion { get; set; }
    }
}
