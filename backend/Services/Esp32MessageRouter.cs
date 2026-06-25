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
        private readonly ILogger<Esp32MessageRouter> _logger;

        public Esp32MessageRouter(
            IServiceScopeFactory scopeFactory,
            Esp32ConnectionManager connections,
            ILogger<Esp32MessageRouter> logger)
        {
            _scopeFactory = scopeFactory;
            _connections = connections;
            _logger = logger;
        }

        public async Task<AparatoConfiguracionRed?> RegisterOrUpdateDeviceAsync(
            string deviceKey,
            string? tokenString,
            CancellationToken cancellationToken)
        {
            await using var scope = _scopeFactory.CreateAsyncScope();
            var db = scope.ServiceProvider.GetRequiredService<PruebaaspContext>();
            var normalizedDeviceKey = deviceKey.Trim();

            var configuracion = await db.AparatoConfiguracionesRed
                .FirstOrDefaultAsync(item => item.device_key == normalizedDeviceKey && item.activo, cancellationToken);

            if (configuracion is null)
            {
                var aparatoBluetooth = await db.AparatoBluetooth
                    .FirstOrDefaultAsync(b => b.mac_bluetooth == normalizedDeviceKey, cancellationToken);

                if (aparatoBluetooth == null && !string.IsNullOrWhiteSpace(tokenString))
                {
                    var tokenObj = await db.Token.FirstOrDefaultAsync(t => t.Cadena == tokenString && t.Activo, cancellationToken);
                    if (tokenObj != null)
                    {
                        var nuevoAparato = new Aparato {
                            sk_usuario_id = tokenObj.sk_usuario_id,
                            nombre_aparato = "Nuevo Dispositivo",
                            sk_aparato_tipo_id = 4,
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
                    configuracion = new AparatoConfiguracionRed
                    {
                        sk_aparato_id = aparatoBluetooth.sk_aparato_id,
                        device_key = normalizedDeviceKey,
                        activo = true,
                        fecha_creacion = DateTime.UtcNow,
                        fecha_ultima_conexion = DateTime.UtcNow
                    };
                    db.AparatoConfiguracionesRed.Add(configuracion);
                    await db.SaveChangesAsync(cancellationToken);
                    
                    return configuracion;
                }
                
                return null;
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
                    var reason = string.IsNullOrWhiteSpace(targetDeviceKey)
                        ? "sin aparato destino configurado"
                        : $"{targetDeviceKey.Trim()} no esta conectado";

                    _logger.LogInformation(
                        "Comando de aparato {SourceConfiguracionRedId} recibido, {Reason}.",
                        sourceConfiguracionRedId,
                        reason);
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
