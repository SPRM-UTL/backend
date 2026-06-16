namespace backend.DTOs
{
    public class ControlResponseDto
    {
        public List<AparatoDto> Luces { get; set; } = [];

        public List<AparatoDto> Bocinas { get; set; } = [];

        public List<AparatoDto> Ventiladores { get; set; } = [];
    }
}
