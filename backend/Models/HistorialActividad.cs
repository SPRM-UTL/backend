using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace backend.Models
{
    public class HistorialActividad
    {
        [Key]
        public int sk_actividad_id { get; set; }

        public int sk_usuario_id { get; set; }

        public int sk_gesto_id { get; set; }

        public int sk_aparato_id { get; set; }

        public int sk_tiempo_id { get; set; }

        [Column(TypeName = "decimal(5,2)")]
        public decimal confianza_ia { get; set; }

        public int tiempo_respuesta { get; set; }

        public bool ejecucion_exitosa { get; set; }

        [ForeignKey(nameof(sk_usuario_id))]
        public Usuario? Usuario { get; set; }

        [ForeignKey(nameof(sk_gesto_id))]
        public Gesto? Gesto { get; set; }

        [ForeignKey(nameof(sk_aparato_id))]
        public Aparato? Aparato { get; set; }

        [ForeignKey(nameof(sk_tiempo_id))]
        public Tiempo? Tiempo { get; set; }
    }
}
