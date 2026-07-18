using backend.DTOs;
using backend.Services;
using Microsoft.AspNetCore.Mvc;

namespace backend.Controllers
{
    [Route("api/externos")]
    [ApiController]
    public class ExternosController : ControllerBase
    {
        private readonly TuyaLocalService _tuyaService;

        public ExternosController(TuyaLocalService tuyaService)
        {
            _tuyaService = tuyaService;
        }

        // POST api/externos/tuya
        [HttpPost("tuya")]
        public async Task<IActionResult> EnviarComandoTuya([FromBody] TuyaCommandRequest request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var result = await _tuyaService.SendCommandAsync(request);

            if (result)
            {
                return Ok(new { success = true, message = "Comando enviado al dispositivo Tuya exitosamente." });
            }
            else
            {
                return StatusCode(500, new { success = false, message = "No se pudo enviar el comando al dispositivo Tuya. Revisa los logs para más detalles." });
            }
        }
    }
}
