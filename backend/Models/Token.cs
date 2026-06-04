using System.ComponentModel.DataAnnotations.Schema;

namespace backend.Models
{
    public class Token
    {
        public int Id { get; set; }

        public string? Cadena { get; set; }

        public DateTime FechaExpiracion { get; set; }

        public bool Activo { get; set; } = true;

        public DateTime? FechaBaja { get; set; }

        public int sk_usuario_id { get; set; }

        [ForeignKey(nameof(sk_usuario_id))]
        public Usuario? Usuario { get; set; }
    }
}
