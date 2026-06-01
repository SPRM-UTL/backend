namespace backend.DTOs
{
    public class ActividadHistorialDto
    {
        public int Id { get; set; }
        public string Hora { get; set; } = string.Empty;
        public string Accion { get; set; } = string.Empty;
        public string Dispositivo { get; set; } = string.Empty;
        public string Icono { get; set; } = string.Empty;
        public string Color { get; set; } = string.Empty;
        public string Estado { get; set; } = string.Empty;
        public string Metodo { get; set; } = "Gesto";
    }
}
