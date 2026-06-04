using backend.Services;
using Microsoft.AspNetCore.Mvc;

namespace backend.Controllers
{
    [Route("ws")]
    public class Esp32WebSocketController : ControllerBase
    {
        private readonly Esp32ConnectionManager _connections;
        private readonly Esp32MessageRouter _router;
        private readonly ILogger<Esp32WebSocketController> _logger;

        public Esp32WebSocketController(
            Esp32ConnectionManager connections,
            Esp32MessageRouter router,
            ILogger<Esp32WebSocketController> logger)
        {
            _connections = connections;
            _router = router;
            _logger = logger;
        }

        [HttpGet]
        public async Task<IActionResult> Connect(
            [FromQuery] string deviceKey,
            [FromQuery] string? targetDeviceKey,
            CancellationToken cancellationToken)
        {
            if (!HttpContext.WebSockets.IsWebSocketRequest)
            {
                return BadRequest("La ruta /ws solo acepta peticiones WebSocket.");
            }

            if (string.IsNullOrWhiteSpace(deviceKey))
            {
                return BadRequest("Falta el parametro deviceKey.");
            }

            var normalizedDeviceKey = deviceKey.Trim();
            using var socket = await HttpContext.WebSockets.AcceptWebSocketAsync();

            await _connections.AddOrReplaceAsync(normalizedDeviceKey, socket, cancellationToken);
            var sourceDevice = await _router.RegisterOrUpdateDeviceAsync(normalizedDeviceKey, cancellationToken);

            _logger.LogInformation("ESP32 conectado: {DeviceKey}", normalizedDeviceKey);

            try
            {
                await _router.ReceiveMessagesAsync(
                    socket,
                    sourceDevice.Id,
                    targetDeviceKey,
                    cancellationToken);

                return new EmptyResult();
            }
            finally
            {
                _connections.Remove(normalizedDeviceKey, socket);
                _logger.LogInformation("ESP32 desconectado: {DeviceKey}", normalizedDeviceKey);
            }
        }
    }
}
