using backend.Models;
using backend.Services;
using Microsoft.AspNetCore.Mvc;
using System.Net.WebSockets;
using System.Text;
using Microsoft.EntityFrameworkCore;
using System.Net.Sockets;

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
        public async Task<IActionResult> GetAllStatus([FromServices] PruebaaspContext context)
        {
            var connectedDevices = _connections.GetAllConnectedDeviceKeys().ToList();

            // Buscar dispositivos WIFI/LAN locales en la BD
            var localDevices = await context.Aparatos
                .Include(a => a.ConfiguracionRed)
                .Where(a => (a.metodo_vinculacion == "WIFI" || a.metodo_vinculacion == "LAN") 
                            && a.ConfiguracionRed != null 
                            && !string.IsNullOrEmpty(a.ConfiguracionRed.ip_address))
                .ToListAsync();

            if (localDevices.Any())
            {
                var tasks = localDevices.Select(async device =>
                {
                    try
                    {
                        var ip = device.ConfiguracionRed!.ip_address;
                        using var client = new TcpClient();
                        // Ping rápido con timeout de 1 segundo (1000ms)
                        var connectTask = client.ConnectAsync(ip!, 5577);
                        if (await Task.WhenAny(connectTask, Task.Delay(1000)) == connectTask)
                        {
                            // Conexion exitosa, reportar como en línea usando su IP o device_key según convenga
                            // El frontend podría estar usando deviceKey o IP, añadimos el deviceKey si existe, o la IP.
                            var identifier = !string.IsNullOrEmpty(device.ConfiguracionRed.device_key) 
                                                ? device.ConfiguracionRed.device_key 
                                                : ip;
                            return identifier;
                        }
                    }
                    catch { }
                    return null;
                });

                var onlineLocalDevices = (await Task.WhenAll(tasks)).Where(id => id != null);
                connectedDevices.AddRange(onlineLocalDevices!);
            }

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
            [FromQuery] int? sk_gesto_id,
            [FromServices] PruebaaspContext context,
            CancellationToken cancellationToken)
        {
            var config = context.AparatoConfiguracionesRed
                .Include(c => c.Aparato)
                .ThenInclude(a => a.Tipo)
                .FirstOrDefault(c => c.sk_aparato_id == sk_aparato_id);
                
            if (config == null || (string.IsNullOrWhiteSpace(config.device_key) && string.IsNullOrWhiteSpace(config.ip_address)))
            {
                return NotFound("El aparato no tiene configuración de red válida.");
            }

            bool esCamara = config.Aparato?.Tipo?.nombre_tipo == "Cámara" || config.Aparato?.Tipo?.nombre_tipo == "Camara";
            string comando = esCamara 
                ? (estado ? "LED_ON" : "LED_OFF")
                : (estado ? "ON" : "OFF");

            bool esWifiLocal = config.Aparato?.metodo_vinculacion == "WIFI" || config.Aparato?.metodo_vinculacion == "LAN";

            if (esWifiLocal)
            {
                if (string.IsNullOrWhiteSpace(config.ip_address))
                {
                    return BadRequest("El dispositivo WIFI/LAN no tiene IP configurada.");
                }
                
                try
                {
                    using var client = new TcpClient();
                    await client.ConnectAsync(config.ip_address, 5577);
                    byte[] commandBytes = estado 
                        ? new byte[] { 0x71, 0x23, 0x0F, 0xA3 } 
                        : new byte[] { 0x71, 0x24, 0x0F, 0xA4 };
                    await client.GetStream().WriteAsync(commandBytes, 0, commandBytes.Length, cancellationToken);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error conectando al socket TCP WIFI/LAN en IP: {ip}", config.ip_address);
                    return BadRequest("Fallo al conectar con el dispositivo WIFI/LAN en red local.");
                }
            }
            else
            {
                if (!_connections.TryGetOpenSocket(config.device_key!, out var socket))
                {
                    return BadRequest("El dispositivo no está conectado actualmente.");
                }

                await socket!.SendAsync(
                    Encoding.UTF8.GetBytes(comando),
                    WebSocketMessageType.Text,
                    true,
                    cancellationToken);
            }

            await _stateService.ProcessOutboundCommandAsync(
                config.sk_aparato_configuracion_red_id,
                comando,
                estado,
                "toggle",
                cancellationToken);

            // === GUARDAR HISTORIAL DE ACTIVIDAD ===
            try
            {
                var now = DateTime.UtcNow;
                var nowOnly = DateOnly.FromDateTime(now);
                var tiempo = context.Tiempos.FirstOrDefault(t => t.fecha_completa == nowOnly && t.hora_periodo == now.Hour);
                if (tiempo == null)
                {
                    tiempo = new backend.Models.Tiempo {
                        fecha_completa = nowOnly,
                        anio = now.Year,
                        mes_numero = now.Month,
                        mes_nombre = now.ToString("MMMM"),
                        dia_semana_nombre = now.ToString("dddd"),
                        hora_periodo = now.Hour
                    };
                    context.Tiempos.Add(tiempo);
                    context.SaveChanges();
                }

                // 1 es un ID de gesto por defecto si no se manda uno (ej. Toggle Manual)
                int gestoIdToSave = sk_gesto_id ?? 1; 

                var historial = new backend.Models.HistorialActividad {
                    sk_usuario_id = config.Aparato?.sk_usuario_id ?? 1,
                    sk_gesto_id = gestoIdToSave,
                    sk_aparato_id = sk_aparato_id,
                    sk_tiempo_id = tiempo.sk_tiempo_id,
                    confianza_ia = 1.00m,
                    tiempo_respuesta = 50,
                    ejecucion_exitosa = true
                };
                context.HistorialActividades.Add(historial);
                await context.SaveChangesAsync(cancellationToken);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error al guardar el historial de actividad: {ex.Message}");
            }

            var mensaje_confirmacion = estado 
                ? $"Encendiste {config.Aparato?.nombre_aparato}" 
                : $"Apagaste {config.Aparato?.nombre_aparato}";

            return Ok(new
            {
                success = true,
                comando,
                estado_encendido = estado,
                estado_encendido_2 = config.estado_encendido_2,
                estado_encendido_3 = config.estado_encendido_3,
                estado_encendido_4 = config.estado_encendido_4,
                fecha_estado_actualizado = DateTime.UtcNow,
                mensaje_confirmacion
            });
        }

        [HttpPost("toggle/{sk_aparato_id}/contacto/{contacto:int}")]
        public async Task<IActionResult> ToggleAparatoContacto(
            int sk_aparato_id,
            int contacto,
            [FromQuery] bool estado,
            [FromQuery] int? sk_gesto_id,
            [FromServices] PruebaaspContext context,
            CancellationToken cancellationToken)
        {
            if (contacto is < 1 or > 4)
            {
                return BadRequest("El contacto debe estar entre 1 y 4.");
            }

            var config = context.AparatoConfiguracionesRed
                .Include(c => c.Aparato)
                .FirstOrDefault(c => c.sk_aparato_id == sk_aparato_id);
            if (config == null || string.IsNullOrWhiteSpace(config.device_key))
            {
                return NotFound("El aparato no tiene configuración de red o deviceKey.");
            }

            if (!_connections.TryGetOpenSocket(config.device_key, out var socket))
            {
                return BadRequest("El dispositivo no está conectado actualmente.");
            }

            string estadoStr = estado ? "ON" : "OFF";
            // Formato esperado por el firmware Arduino del MultiSocket: ON1, OFF1, ON2, OFF2....
            string comando = $"{estadoStr}{contacto}";

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
                
            // === GUARDAR HISTORIAL DE ACTIVIDAD ===
            try
            {
                var now = DateTime.UtcNow;
                var nowOnly = DateOnly.FromDateTime(now);
                var tiempo = context.Tiempos.FirstOrDefault(t => t.fecha_completa == nowOnly && t.hora_periodo == now.Hour);
                if (tiempo == null)
                {
                    tiempo = new backend.Models.Tiempo {
                        fecha_completa = nowOnly,
                        anio = now.Year,
                        mes_numero = now.Month,
                        mes_nombre = now.ToString("MMMM"),
                        dia_semana_nombre = now.ToString("dddd"),
                        hora_periodo = now.Hour
                    };
                    context.Tiempos.Add(tiempo);
                    context.SaveChanges();
                }

                // 1 es un ID de gesto por defecto si no se manda uno (ej. Toggle Manual)
                int gestoIdToSave = sk_gesto_id ?? 1; 

                var historial = new backend.Models.HistorialActividad {
                    sk_usuario_id = config.Aparato?.sk_usuario_id ?? 1,
                    sk_gesto_id = gestoIdToSave,
                    sk_aparato_id = sk_aparato_id,
                    sk_tiempo_id = tiempo.sk_tiempo_id,
                    confianza_ia = 1.00m,
                    tiempo_respuesta = 50,
                    ejecucion_exitosa = true
                };
                context.HistorialActividades.Add(historial);
                await context.SaveChangesAsync(cancellationToken);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error al guardar el historial de actividad: {ex.Message}");
            }

            var mensaje_confirmacion = estado 
                ? $"Encendiste {config.Aparato?.nombre_aparato}" 
                : $"Apagaste {config.Aparato?.nombre_aparato}";

            return Ok(new
            {
                success = true,
                comando,
                contacto,
                estado_encendido = estado,
                fecha_estado_actualizado = DateTime.UtcNow,
                mensaje_confirmacion
            });
        }
    }
}
