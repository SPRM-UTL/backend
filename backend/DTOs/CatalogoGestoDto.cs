namespace backend.DTOs
{
    public class CatalogoGestoDto
    {
        public int SkCatalogoGestoId { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public string Icono { get; set; } = string.Empty;
        public bool IsBodyGesture { get; set; }
        public bool IsActive { get; set; }
    }

    public class GuardarConfiguracionGestosDto
    {
        public int SkCatalogoGestoId { get; set; }
        public bool IsActive { get; set; }
    }
}
