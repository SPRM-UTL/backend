using System.Text.Json.Serialization;

namespace backend.DTOs
{
    public class UsuarioVozConfigDto
    {
        [JsonPropertyName("control_voz_activado")]
        public bool ControlVozActivado { get; set; }

        [JsonPropertyName("confirmacion_hablada_activada")]
        public bool ConfirmacionHabladaActivada { get; set; }

        [JsonPropertyName("voz_tipo_seleccionado")]
        public string? VozTipoSeleccionado { get; set; }

        [JsonPropertyName("voz_velocidad")]
        public decimal VozVelocidad { get; set; }

        [JsonPropertyName("voz_idioma")]
        public string? VozIdioma { get; set; }
    }
}
