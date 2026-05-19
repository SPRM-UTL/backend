namespace backend.Models
{
    public class Token
    {
        public int Id { get; set; }
        public string Cadena { get; set; }
        public DateTime FechaExpiracion { get; set; }
        public int UsuarioId { get; set; }
        public Usuario? Usuario { get; set; }
    }
}
