using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace backend.Models
{
    public class UsuarioGestoConfig
    {
        public int sk_usuario_id { get; set; }
        public int sk_catalogo_gesto_id { get; set; }

        public bool is_active { get; set; }

        [ForeignKey("sk_usuario_id")]
        public virtual Usuario Usuario { get; set; }

        [ForeignKey("sk_catalogo_gesto_id")]
        public virtual CatalogoGesto CatalogoGesto { get; set; }
    }
}
