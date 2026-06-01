using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace backend.Models
{
    public class Dim_Usuarios
    {
        [Key]
        public int sk_usuario_id {  get; set; }

        [MaxLength(100)]
        public string? nombre_usuario { get; set; }

        [MaxLength(150)]
        public string? email_usuario { get; set; }
        
        [MaxLength(500)]
        public string? contrasenia { get; set; }

        [MaxLength(100)]
        public string? nombre_arduino{ get; set; }

        [MaxLength(17)]
        public string? mac_address_usuario { get; set; }

        [JsonIgnore]
        public List<Fact_Historico_Actividad>? Historico_Actividad { get; set; }
        
        [JsonIgnore]
        public List<Token>? Tokens { get; set; }

    }
}
