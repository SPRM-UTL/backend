using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace backend.Models
{
    public class CatalogoGesto
    {
        [Key]
        public int sk_catalogo_gesto_id { get; set; }

        [Required]
        [MaxLength(100)]
        public string nombre { get; set; }

        [Required]
        [MaxLength(50)]
        public string icono { get; set; }

        public bool is_body_gesture { get; set; }

        public virtual ICollection<UsuarioGestoConfig> UsuarioConfiguraciones { get; set; } = new List<UsuarioGestoConfig>();
    }
}
