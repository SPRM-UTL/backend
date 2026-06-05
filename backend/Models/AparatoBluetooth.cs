using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace backend.Models
{
    public class AparatoBluetooth
    {
        [Key]
        public int sk_aparato_bluetooth_id { get; set; }

        public int sk_aparato_id { get; set; }

        [MaxLength(17)]
        public string? mac_bluetooth { get; set; }

        [MaxLength(100)]
        public string? nombre_bluetooth { get; set; }

        [ForeignKey(nameof(sk_aparato_id))]
        [JsonIgnore]
        public Aparato? Aparato { get; set; }
    }
}
