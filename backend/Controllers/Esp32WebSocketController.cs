using backend.Services;
using backend.Models;
using Microsoft.AspNetCore.Mvc;
using System.Net.WebSockets;
using System.Text;

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
            Console.WriteLine("DeviceKey: " + normalizedDeviceKey);
            using var socket = await HttpContext.WebSockets.AcceptWebSocketAsync();

            var sourceDevice = await _router.RegisterOrUpdateDeviceAsync(normalizedDeviceKey, cancellationToken);

            if (sourceDevice == null)
            {
                await socket.CloseAsync(
                    System.Net.WebSockets.WebSocketCloseStatus.PolicyViolation,
                    "deviceKey no registrado en configuracion de red",
                    cancellationToken);

                Console.WriteLine("No se encontro el device");

                return BadRequest("El deviceKey no esta registrado en la configuracion de red de un aparato activo.");
            }

            await _connections.AddOrReplaceAsync(normalizedDeviceKey, socket, cancellationToken);

            _logger.LogInformation("ESP32 conectado: {DeviceKey}", normalizedDeviceKey);

            try
            {
                await _router.ReceiveMessagesAsync(
                    socket,
                    sourceDevice.sk_aparato_configuracion_red_id,
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

        [HttpGet("accion")]
        public async Task<IActionResult> EnviarComando(
        [FromQuery] string comando,
        [FromQuery] string deviceKey,
        CancellationToken cancellationToken)
        {
            if (!_connections.TryGetOpenSocket(deviceKey, out var socket))
            {
                return NotFound($"No existe una conexión activa para '{deviceKey}'.");
            }

            await socket!.SendAsync(
                Encoding.UTF8.GetBytes(comando),
                WebSocketMessageType.Text,
                true,
                cancellationToken);

            return Ok(new
            {
                deviceKey,
                comando
            });
        }

        [HttpGet("status/{deviceKey}")]
        public IActionResult GetStatus(string deviceKey)
        {
            if (string.IsNullOrWhiteSpace(deviceKey))
            {
                return BadRequest(new { error = "Falta el parámetro deviceKey." });
            }

            bool isConnected = _connections.TryGetOpenSocket(deviceKey, out _);

            return Ok(new { connected = isConnected });
        }

        [HttpPost("toggle/{sk_aparato_id}")]
        public async Task<IActionResult> ToggleAparato(
            int sk_aparato_id, 
            [FromQuery] bool estado,
            [FromServices] PruebaaspContext context,
            CancellationToken cancellationToken)
        {
            var config = context.AparatoConfiguracionesRed.FirstOrDefault(c => c.sk_aparato_id == sk_aparato_id);
            if (config == null || string.IsNullOrWhiteSpace(config.device_key))
            {
                return NotFound("El aparato no tiene configuración de red o deviceKey.");
            }

            if (!_connections.TryGetOpenSocket(config.device_key, out var socket))
            {
                return BadRequest("El dispositivo no está conectado actualmente.");
            }

            string comando = estado ? "ON" : "OFF";
            await socket!.SendAsync(
                Encoding.UTF8.GetBytes(comando),
                WebSocketMessageType.Text,
                true,
                cancellationToken);

            return Ok(new { success = true, comando });
        }
    }
}
