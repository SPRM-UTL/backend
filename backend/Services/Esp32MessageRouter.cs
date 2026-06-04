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
        private readonly Esp32MessageEventHub _events;
        private readonly ILogger<Esp32MessageRouter> _logger;

        public Esp32MessageRouter(
            IServiceScopeFactory scopeFactory,
            Esp32ConnectionManager connections,
            Esp32MessageEventHub events,
            ILogger<Esp32MessageRouter> logger)
        {
            _scopeFactory = scopeFactory;
            _connections = connections;
            _events = events;
            _logger = logger;
        }

        public async Task<Esp32Device> RegisterOrUpdateDeviceAsync(
            string deviceKey,
            CancellationToken cancellationToken)
        {
            await using var scope = _scopeFactory.CreateAsyncScope();
            var db = scope.ServiceProvider.GetRequiredService<PruebaaspContext>();
            var normalizedDeviceKey = deviceKey.Trim();

            var device = await db.Esp32Device
                .FirstOrDefaultAsync(item => item.DeviceKey == normalizedDeviceKey, cancellationToken);

            if (device is null)
            {
                device = new Esp32Device
                {
                    DeviceKey = normalizedDeviceKey,
                    Name = normalizedDeviceKey
                };
                db.Esp32Device.Add(device);
            }

            device.LastSeenAtUtc = DateTime.UtcNow;
            await db.SaveChangesAsync(cancellationToken);

            return device;
        }

        public async Task ReceiveMessagesAsync(
            WebSocket socket,
            int sourceDeviceId,
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
                    sourceDeviceId,
                    targetDeviceKey,
                    readResult.Message,
                    cancellationToken);
            }
        }

        private async Task ProcessMessageAsync(
            int sourceDeviceId,
            string? targetDeviceKey,
            string message,
            CancellationToken cancellationToken)
        {
            await using var scope = _scopeFactory.CreateAsyncScope();
            var db = scope.ServiceProvider.GetRequiredService<PruebaaspContext>();

            Esp32Device? targetDevice = null;
            WebSocket? targetSocket = null;

            if (!string.IsNullOrWhiteSpace(targetDeviceKey))
            {
                var normalizedTargetDeviceKey = targetDeviceKey.Trim();

                targetDevice = await db.Esp32Device
                    .FirstOrDefaultAsync(device => device.DeviceKey == normalizedTargetDeviceKey, cancellationToken);

                _connections.TryGetOpenSocket(normalizedTargetDeviceKey, out targetSocket);
            }

            var log = new Esp32Message
            {
                SourceDeviceId = sourceDeviceId,
                TargetDeviceId = targetDevice?.Id,
                Message = message
            };

            try
            {
                if (targetSocket is null)
                {
                    log.Response = string.IsNullOrWhiteSpace(targetDeviceKey)
                        ? "Mensaje recibido, sin ESP32 destino configurado."
                        : $"Mensaje recibido, pero {targetDeviceKey.Trim()} no esta conectado.";
                    log.WasProcessed = false;
                }
                else
                {
                    var payload = Encoding.UTF8.GetBytes(message);
                    await targetSocket.SendAsync(payload, WebSocketMessageType.Text, true, cancellationToken);
                    log.Response = $"Mensaje reenviado a {targetDeviceKey!.Trim()}.";
                    log.WasProcessed = true;
                }
            }
            catch (Exception ex)
            {
                log.Response = "No se pudo procesar el mensaje.";
                log.ProcessingError = ex.Message;
                log.WasProcessed = false;
                _logger.LogError(ex, "Error procesando mensaje ESP32.");
            }

            db.Esp32Message.Add(log);
            await db.SaveChangesAsync(cancellationToken);

            var sourceDeviceKey = await db.Esp32Device
                .Where(device => device.Id == sourceDeviceId)
                .Select(device => device.DeviceKey)
                .FirstAsync(cancellationToken);

            _events.Publish(new MessageEvent(
                log.Id,
                sourceDeviceKey,
                targetDevice?.DeviceKey,
                log.Message,
                log.Response,
                log.WasProcessed,
                log.ProcessingError,
                log.CreatedAtUtc));
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
