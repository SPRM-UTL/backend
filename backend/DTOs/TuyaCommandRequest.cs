using System.ComponentModel.DataAnnotations;

namespace backend.DTOs
{
    public class TuyaCommandRequest
    {
        [Required]
        public string IpAddress { get; set; } = string.Empty;

        [Required]
        public string DeviceId { get; set; } = string.Empty;

        [Required]
        public string LocalKey { get; set; } = string.Empty;

        [Required]
        public bool Encendido { get; set; }
        
        // Propiedades opcionales para comandos extra
        public int? Brightness { get; set; }
    }
}
