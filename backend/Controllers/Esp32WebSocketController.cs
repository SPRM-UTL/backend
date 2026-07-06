using backend.Models;
using backend.Services;
using Microsoft.AspNetCore.Mvc;
using System.Net.WebSockets;
using System.Text;
using Microsoft.EntityFrameworkCore;

namespace backend.Controllers
{
    [Route("ws")] 
    public class Esp32WebSocketController : ControllerBase
    {
        private readonly Esp32ConnectionManager _connections;
        private readonly Esp32MessageRouter _router;
        private readonly Esp32DeviceStateService _stateService;
        private readonly ILogger<Esp32WebSocketController> _logger;

        public Esp32WebSocketController(
            Esp32ConnectionManager connections,
            Esp32MessageRouter router,
            Esp32DeviceStateService stateService,
            ILogger<Esp32WebSocketController> logger)
        {
            _connections = connections;
            _router = router;
            _stateService = stateService;
            _logger = logger;
        }

        [HttpGet]
        public async Task<IActionResult> Connect(
            [FromQuery] string deviceKey,
            [FromQuery] string? token,
            [FromQuery] string? targetDeviceKey,
            [FromQuery] string? tipoAparato,
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
            
            var remoteIpAddress = HttpContext.Connection.RemoteIpAddress;
            if (remoteIpAddress != null && remoteIpAddress.IsIPv4MappedToIPv6)
            {
                remoteIpAddress = remoteIpAddress.MapToIPv4();
            }
            var remoteIp = remoteIpAddress?.ToString();
            
            using var socket = await HttpContext.WebSockets.AcceptWebSocketAsync();

            var sourceDevice = await _router.RegisterOrUpdateDeviceAsync(normalizedDeviceKey, token, tipoAparato, remoteIp, cancellationToken);

            if (sourceDevice == null)
            {
                await socket.CloseAsync(
                    WebSocketCloseStatus.PolicyViolation,
                    "deviceKey no registrado en configuracion de red",
                    cancellationToken);

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
            [FromServices] PruebaaspContext context,
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

            var config = await context.AparatoConfiguracionesRed
                .FirstOrDefaultAsync(c => c.device_key == deviceKey, cancellationToken);

            if (config is not null)
            {
                var estado = _stateService.TryParsePowerState(comando);
                await _stateService.ProcessOutboundCommandAsync(
                    config.sk_aparato_configuracion_red_id,
                    comando,
                    estado,
                    "accion",
                    cancellationToken);
            }

            var contactoComando = _stateService.TryParseOutletCommand(comando, out var contacto, out _)
                ? contacto
                : (int?)null;

            return Ok(new
            {
                deviceKey,
                comando,
                estado_encendido = _stateService.TryParsePowerState(comando),
                contacto = contactoComando
            });
        }

        [HttpGet("status/{deviceKey}")]
        public async Task<IActionResult> GetStatus(
            string deviceKey,
            [FromServices] PruebaaspContext context)
        {
            if (string.IsNullOrWhiteSpace(deviceKey))
            {
                return BadRequest(new { error = "Falta el parámetro deviceKey." });
            }

            bool isConnected = _connections.TryGetOpenSocket(deviceKey, out _);
            var config = await context.AparatoConfiguracionesRed
                .FirstOrDefaultAsync(c => c.device_key == deviceKey);

            return Ok(new
            {
                connected = isConnected,
                estado_encendido = config?.estado_encendido,
                estado_encendido_2 = config?.estado_encendido_2,
                estado_encendido_3 = config?.estado_encendido_3,
                estado_encendido_4 = config?.estado_encendido_4,
                fecha_estado_actualizado = config?.fecha_estado_actualizado,
                origen_estado = config?.origen_estado,
                corriente_a = config?.corriente_actual,
                potencia_w = config?.potencia_actual,
                energia_acumulada_wh = config?.energia_acumulada_wh,
                fecha_medicion_consumo = config?.fecha_medicion_consumo
            });
        }

        [HttpGet("status/all")]
        public IActionResult GetAllStatus()
        {
            var connectedDevices = _connections.GetAllConnectedDeviceKeys();
            return Ok(new { connectedDevices });
        }

        [HttpGet("state/{sk_aparato_id}")]
        public async Task<IActionResult> GetAparatoState(
            int sk_aparato_id,
            [FromServices] PruebaaspContext context)
        {
            var config = await context.AparatoConfiguracionesRed
                .FirstOrDefaultAsync(c => c.sk_aparato_id == sk_aparato_id);

            if (config == null || string.IsNullOrWhiteSpace(config.device_key))
            {
                return NotFound("El aparato no tiene configuración de red.");
            }

            var conectado = _connections.TryGetOpenSocket(config.device_key, out _);

            return Ok(new
            {
                sk_aparato_id,
                device_key = config.device_key,
                estado_encendido = config.estado_encendido,
                estado_encendido_2 = config.estado_encendido_2,
                estado_encendido_3 = config.estado_encendido_3,
                estado_encendido_4 = config.estado_encendido_4,
                conectado,
                fecha_estado_actualizado = config.fecha_estado_actualizado,
                origen_estado = config.origen_estado,
                corriente_a = config.corriente_actual,
                potencia_w = config.potencia_actual,
                energia_acumulada_wh = config.energia_acumulada_wh,
                fecha_medicion_consumo = config.fecha_medicion_consumo
            });
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

            await _stateService.ProcessOutboundCommandAsync(
                config.sk_aparato_configuracion_red_id,
                comando,
                estado,
                "toggle",
                cancellationToken);

            return Ok(new
            {
                success = true,
                comando,
                estado_encendido = estado,
                estado_encendido_2 = config.estado_encendido_2,
                estado_encendido_3 = config.estado_encendido_3,
                estado_encendido_4 = config.estado_encendido_4,
                fecha_estado_actualizado = DateTime.UtcNow
            });
        }

        [HttpPost("toggle/{sk_aparato_id}/contacto/{contacto:int}")]
        public async Task<IActionResult> ToggleAparatoContacto(
            int sk_aparato_id,
            int contacto,
            [FromQuery] bool estado,
            [FromServices] PruebaaspContext context,
            CancellationToken cancellationToken)
        {
            if (contacto is < 1 or > 4)
            {
                return BadRequest("El contacto debe estar entre 1 y 4.");
            }

            var config = await context.AparatoConfiguracionesRed
                .FirstOrDefaultAsync(c => c.sk_aparato_id == sk_aparato_id, cancellationToken);
            if (config == null || string.IsNullOrWhiteSpace(config.device_key))
            {
                return NotFound("El aparato no tiene configuración de red o deviceKey.");
            }

            if (!_connections.TryGetOpenSocket(config.device_key, out var socket))
            {
                return BadRequest("El dispositivo no está conectado actualmente.");
            }

            var comando = $"{(estado ? "ON" : "OFF")}{contacto}";
            await socket!.SendAsync(
                Encoding.UTF8.GetBytes(comando),
                WebSocketMessageType.Text,
                true,
                cancellationToken);

            await _stateService.ProcessOutboundCommandAsync(
                config.sk_aparato_configuracion_red_id,
                comando,
                estado,
                $"toggle_contacto_{contacto}",
                cancellationToken);

            return Ok(new
            {
                success = true,
                comando,
                contacto,
                estado,
                estado_encendido = contacto == 1 ? estado : config.estado_encendido,
                estado_encendido_2 = contacto == 2 ? estado : config.estado_encendido_2,
                estado_encendido_3 = contacto == 3 ? estado : config.estado_encendido_3,
                estado_encendido_4 = contacto == 4 ? estado : config.estado_encendido_4,
                fecha_estado_actualizado = DateTime.UtcNow
            });
        }
    }
}
