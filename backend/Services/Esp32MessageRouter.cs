using System.Net.WebSockets;
using System.Text;
using backend.Models;
using Microsoft.EntityFrameworkCore;

namespace backend.Services
{
    public class Esp32MessageRouter
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly Esp32ConnectionManager _connections;
        private readonly Esp32DeviceStateService _stateService;
        private readonly ILogger<Esp32MessageRouter> _logger;

        public Esp32MessageRouter(
            IServiceScopeFactory scopeFactory,
            Esp32ConnectionManager connections,
            Esp32DeviceStateService stateService,
            ILogger<Esp32MessageRouter> logger)
        {
            _scopeFactory = scopeFactory;
            _connections = connections;
            _stateService = stateService;
            _logger = logger;
        }

        public async Task<AparatoConfiguracionRed?> RegisterOrUpdateDeviceAsync(
            string deviceKey,
            string? tokenString,
            string? tipoAparato,
            string? ipAddress,
            CancellationToken cancellationToken)
        {
            await using var scope = _scopeFactory.CreateAsyncScope();
            var db = scope.ServiceProvider.GetRequiredService<PruebaaspContext>();
            var normalizedDeviceKey = deviceKey.Trim();

            var configuracion = await db.AparatoConfiguracionesRed
                .FirstOrDefaultAsync(item => item.device_key == normalizedDeviceKey, cancellationToken);

            if (configuracion is null)
            {
                var aparatoBluetooth = await db.AparatoBluetooth
                    .FirstOrDefaultAsync(b => b.mac_bluetooth == normalizedDeviceKey, cancellationToken);

                if (aparatoBluetooth == null && !string.IsNullOrWhiteSpace(tokenString))
                {
                    // Los '+' en el token se decodifican como espacios en query params de URL.
                    // Se normaliza antes de comparar con la BD.
                    var normalizedToken = tokenString.Replace(" ", "+");
                    var tokenObj = await db.Token.FirstOrDefaultAsync(t => t.Cadena == normalizedToken && t.Activo, cancellationToken);

                    if (tokenObj != null)
                    {
                        int idTipo = 4; // Por defecto: Sockets Inteligentes
                        string nombreAparato = "Nuevo Dispositivo";

                        if (!string.IsNullOrWhiteSpace(tipoAparato))
                        {
                            var tipoDb = await db.AparatoTipos.FirstOrDefaultAsync(t => t.nombre_tipo == tipoAparato, cancellationToken);
                            if (tipoDb != null)
                            {
                                idTipo = tipoDb.sk_aparato_tipo_id;
                                if (tipoAparato == "Cámara") {
                                    nombreAparato = "Cámara ESP32";
                                }
                            }
                        }

                        var nuevoAparato = new Aparato {
                            sk_usuario_id = tokenObj.sk_usuario_id,
                            nombre_aparato = nombreAparato,
                            sk_aparato_tipo_id = idTipo,
                            fecha_sincronizacion = DateTime.UtcNow
                        };
                        db.Aparatos.Add(nuevoAparato);
                        await db.SaveChangesAsync(cancellationToken);

                        aparatoBluetooth = new AparatoBluetooth {
                            sk_aparato_id = nuevoAparato.sk_aparato_id,
                            mac_bluetooth = normalizedDeviceKey,
                            nombre_bluetooth = "ESP32"
                        };
                        db.AparatoBluetooth.Add(aparatoBluetooth);
                        await db.SaveChangesAsync(cancellationToken);
                    }
                }

                if (aparatoBluetooth != null)
                {
                    // Verify if a configuration already exists for this aparato_id to avoid duplicate key
                    var existingConfig = await db.AparatoConfiguracionesRed
                        .FirstOrDefaultAsync(c => c.sk_aparato_id == aparatoBluetooth.sk_aparato_id, cancellationToken);

                    if (existingConfig != null)
                    {
                        existingConfig.device_key = normalizedDeviceKey;
                        existingConfig.activo = true;
                        existingConfig.fecha_ultima_conexion = DateTime.UtcNow;
                        if (!string.IsNullOrEmpty(ipAddress))
                        {
                            existingConfig.ip_address = ipAddress;
                        }
                        configuracion = existingConfig;
                    }
                    else
                    {
                        configuracion = new AparatoConfiguracionRed
                        {
                            sk_aparato_id = aparatoBluetooth.sk_aparato_id,
                            device_key = normalizedDeviceKey,
                            activo = true,
                            fecha_creacion = DateTime.UtcNow,
                            fecha_ultima_conexion = DateTime.UtcNow,
                            ip_address = ipAddress
                        };
                        db.AparatoConfiguracionesRed.Add(configuracion);
                    }
                    
                    await db.SaveChangesAsync(cancellationToken);
                    return configuracion;
                }
                
                return null;
            }

            configuracion.activo = true;
            if (!string.IsNullOrEmpty(ipAddress))
            {
                configuracion.ip_address = ipAddress;
            }

            configuracion.fecha_ultima_conexion = DateTime.UtcNow;
            await db.SaveChangesAsync(cancellationToken);

            return configuracion;
        }

        public async Task ReceiveMessagesAsync(
            WebSocket socket,
            int sourceConfiguracionRedId,
            string? targetDeviceKey,
            CancellationToken cancellationToken)
        {
            while (socket.State == WebSocketState.Open && !cancellationToken.IsCancellationRequested)
            {
                var readResult = await ReadTextMessageAsync(socket, cancellationToken);

                if (readResult.CloseRequested)
                {
                    await CloseNormalAsync(socket, cancellationToken);
                    break;
                }

                if (string.IsNullOrWhiteSpace(readResult.Message))
                {
                    continue;
                }

                await ProcessMessageAsync(
                    sourceConfiguracionRedId,
                    targetDeviceKey,
                    readResult.Message,
                    cancellationToken);
            }
        }

        private async Task ProcessMessageAsync(
            int sourceConfiguracionRedId,
            string? targetDeviceKey,
            string message,
            CancellationToken cancellationToken)
        {
            await using var scope = _scopeFactory.CreateAsyncScope();
            var db = scope.ServiceProvider.GetRequiredService<PruebaaspContext>();

            await _stateService.ProcessInboundMessageAsync(
                sourceConfiguracionRedId,
                message,
                cancellationToken);

            WebSocket? targetSocket = null;
            AparatoConfiguracionRed? targetConfiguracion = null;

            if (!string.IsNullOrWhiteSpace(targetDeviceKey))
            {
                var normalizedTargetDeviceKey = targetDeviceKey.Trim();

                targetConfiguracion = await db.AparatoConfiguracionesRed
                    .FirstOrDefaultAsync(configuracion => configuracion.device_key == normalizedTargetDeviceKey &&
                        configuracion.activo, cancellationToken);

                _connections.TryGetOpenSocket(normalizedTargetDeviceKey, out targetSocket);
            }

            try
            {
                if (targetSocket is null)
                {
                    if (!string.IsNullOrWhiteSpace(targetDeviceKey))
                    {
                        var reason = $"{targetDeviceKey.Trim()} no esta conectado";
                        _logger.LogInformation(
                            "Mensaje de aparato {SourceConfiguracionRedId} recibido, {Reason}.",
                            sourceConfiguracionRedId,
                            reason);
                    }
                }
                else
                {
                    var payload = Encoding.UTF8.GetBytes(message);
                    await targetSocket.SendAsync(payload, WebSocketMessageType.Text, true, cancellationToken);

                    if (targetConfiguracion is not null)
                    {
                        targetConfiguracion.fecha_ultima_conexion = DateTime.UtcNow;
                    }

                    await db.SaveChangesAsync(cancellationToken);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error procesando mensaje ESP32.");
            }
        }

        private static async Task<WebSocketReadResult> ReadTextMessageAsync(
            WebSocket socket,
            CancellationToken cancellationToken)
        {
            var buffer = new byte[4096];
            using var messageBuffer = new MemoryStream();

            while (true)
            {
                var result = await socket.ReceiveAsync(buffer, cancellationToken);

                if (result.MessageType == WebSocketMessageType.Close)
                {
                    return new WebSocketReadResult(null, true);
                }

                if (result.MessageType != WebSocketMessageType.Text)
                {
                    continue;
                }

                messageBuffer.Write(buffer, 0, result.Count);

                if (result.EndOfMessage)
                {
                    var message = Encoding.UTF8.GetString(messageBuffer.ToArray());
                    return new WebSocketReadResult(message, false);
                }
            }
        }

        private static async Task CloseNormalAsync(WebSocket socket, CancellationToken cancellationToken)
        {
            if (socket.State is WebSocketState.Open or WebSocketState.CloseReceived)
            {
                await socket.CloseAsync(
                    WebSocketCloseStatus.NormalClosure,
                    "Conexion cerrada",
                    cancellationToken);
            }
        }

        private sealed record WebSocketReadResult(string? Message, bool CloseRequested);
    }
}
