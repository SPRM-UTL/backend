using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace backend.Models
{
    public class Usuario
    {
        [Key]
        public int sk_usuario_id {  get; set; }

        [MaxLength(100)]
        public string? nombre_usuario { get; set; }

        [MaxLength(150)]
        public string? email_usuario { get; set; }
        
        [MaxLength(500)]
        public string? contrasenia { get; set; }

        [MaxLength(500)]
        public string? ruta_imagen { get; set; }

        public bool control_voz_activado { get; set; } = true;

        [MaxLength(50)]
        public string? voz_tipo_seleccionado { get; set; }

        [Column(TypeName = "decimal(3,2)")]
        public decimal voz_velocidad { get; set; } = 1.0m;

        [MaxLength(10)]
        public string? voz_idioma { get; set; } = "es-MX";

        public bool confirmacion_hablada_activada { get; set; } = true;

        [MaxLength(100)]
        public string? nombre_arduino{ get; set; }

        [MaxLength(17)]
        public string? mac_address_usuario { get; set; }

        [JsonIgnore]
        public List<HistorialActividad>? Historico_Actividad { get; set; }
        
        [JsonIgnore]
        public List<Token>? Tokens { get; set; }

        [JsonIgnore]
        public List<Aparato>? Aparatos { get; set; }

        [JsonIgnore]
        public List<Gesto>? Gestos { get; set; }

        [JsonIgnore]
        public List<Casa>? Casas { get; set; }

        [JsonIgnore]
        public virtual ICollection<UsuarioGestoConfig> UsuarioGestosConfig { get; set; } = new List<UsuarioGestoConfig>();
    }
}
