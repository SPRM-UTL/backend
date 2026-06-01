using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace backend.Models
{
    public class Fact_Historico_Actividad
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
        public Dim_Usuarios? Dim_Usuario { get; set; }

        [ForeignKey(nameof(sk_gesto_id))]
        public Dim_Gestos? Dim_Gesto { get; set; }

        [ForeignKey(nameof(sk_aparato_id))]
        public Dim_Aparatos? Dim_Aparato { get; set; }

        [ForeignKey(nameof(sk_tiempo_id))]
        public Dim_Tiempo? Dim_Tiempo { get; set; }
    }
}