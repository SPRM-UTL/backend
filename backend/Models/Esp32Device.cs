using System.ComponentModel.DataAnnotations.Schema;

namespace backend.Models
{
    public class Esp32Device
    {
        public int Id { get; set; }

        public string DeviceKey { get; set; } = string.Empty;

        public string Name { get; set; } = string.Empty;

        public string? Description { get; set; }

        public bool IsActive { get; set; } = true;

        public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;

        public DateTime? LastSeenAtUtc { get; set; }

        [InverseProperty(nameof(Esp32Message.SourceDevice))]
        public ICollection<Esp32Message> SentMessages { get; set; } = new List<Esp32Message>();

        [InverseProperty(nameof(Esp32Message.TargetDevice))]
        public ICollection<Esp32Message> ReceivedMessages { get; set; } = new List<Esp32Message>();
    }
}
