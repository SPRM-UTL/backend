using System.ComponentModel.DataAnnotations.Schema;

namespace backend.Models
{
    public class Token
    {
        public int Id { get; set; }

        public string? Cadena { get; set; }

        public DateTime FechaExpiracion { get; set; }

        public int sk_usuario_id { get; set; }

        [ForeignKey(nameof(sk_usuario_id))]
        public Dim_Usuarios? Usuario { get; set; }
    }
}