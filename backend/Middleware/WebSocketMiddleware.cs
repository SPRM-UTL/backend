using backend.Models;
using Microsoft.EntityFrameworkCore;
using System.Collections.Concurrent;
using System.Net.WebSockets;
using System.Text;
using System.Threading.Channels;

namespace backend.Middleware
{
    public class WebSocketMiddleware
    {
        private readonly RequestDelegate _next;

        private static readonly ConcurrentDictionary<string, WebSocket> _sockets
        = new(StringComparer.OrdinalIgnoreCase);

        private static readonly ConcurrentDictionary<Guid, Channel<MessageEvent>> _messageStreams
            = new();
        public WebSocketMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context, PruebaaspContext db)
        {

            if (context.Request.Path.StartsWithSegments("/ws"))
            {
                if (!context.WebSockets.IsWebSocketRequest)
                {
                    context.Response.StatusCode = StatusCodes.Status400BadRequest;
                    return;
                }

                var deviceKey = context.Request.Query["deviceKey"].ToString();
                var targetDeviceKey = context.Request.Query["targetDeviceKey"].ToString();

                if (string.IsNullOrWhiteSpace(deviceKey))
                {
                    context.Response.StatusCode = StatusCodes.Status400BadRequest;
                    await context.Response.WriteAsync("Falta el parametro deviceKey.");
                    return;
                }

                var socket = await context.WebSockets.AcceptWebSocketAsync();
                if (_sockets.TryGetValue(deviceKey, out var previousSocket) && previousSocket.State == WebSocketState.Open)
                {
                    await previousSocket.CloseAsync(WebSocketCloseStatus.PolicyViolation, "DeviceKey duplicado", CancellationToken.None);
                }

                _sockets[deviceKey] = socket;

                var sourceDevice = await RegisterOrUpdateDeviceAsync(db, deviceKey, context.RequestAborted);

                Console.WriteLine($"ESP32 conectado: {deviceKey}");

                try
                {
                    await ReceiveMessagesAsync(socket, sourceDevice.Id, targetDeviceKey, _sockets, _messageStreams, context.RequestServices, context.RequestAborted);
                    return;
                }
                finally
                {
                    if (_sockets.TryGetValue(deviceKey, out var currentSocket) && ReferenceEquals(currentSocket, socket))
                    {
                        _sockets.TryRemove(deviceKey, out _);
                    }

                    Console.WriteLine($"ESP32 desconectado: {deviceKey}");
                }
            }
            else
            {
                await _next(context);
                return;
            }
        }
        static async Task ReceiveMessagesAsync(
            WebSocket socket,
            int sourceDeviceId,
            string targetDeviceKey,
            ConcurrentDictionary<string, WebSocket> sockets,
            ConcurrentDictionary<Guid, Channel<MessageEvent>> messageStreams,
            IServiceProvider services,
            CancellationToken cancellationToken)
        {
            var buffer = new byte[4096];

            while (socket.State == WebSocketState.Open && !cancellationToken.IsCancellationRequested)
            {
                var result = await socket.ReceiveAsync(buffer, cancellationToken);

                if (result.MessageType == WebSocketMessageType.Close)
                {
                    await socket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Conexion cerrada", cancellationToken);
                    break;
                }

                var message = Encoding.UTF8.GetString(buffer, 0, result.Count);
                await ProcessEsp32MessageAsync(sourceDeviceId, targetDeviceKey, message, sockets, messageStreams, services, cancellationToken);
            }
        }

        static async Task ProcessEsp32MessageAsync(
            int sourceDeviceId,
            string targetDeviceKey,
            string message,
            ConcurrentDictionary<string, WebSocket> sockets,
            ConcurrentDictionary<Guid, Channel<MessageEvent>> messageStreams,
            IServiceProvider services,
            CancellationToken cancellationToken)
        {
            await using var scope = services.CreateAsyncScope();
            var db = scope.ServiceProvider.GetRequiredService<PruebaaspContext>();

            Esp32Device? targetDevice = null;
            WebSocket? targetSocket = null;

            if (!string.IsNullOrWhiteSpace(targetDeviceKey))
            {
                targetDevice = await db.Esp32Device.FirstOrDefaultAsync(device => device.DeviceKey == targetDeviceKey, cancellationToken);
                sockets.TryGetValue(targetDeviceKey, out targetSocket);
            }

            var log = new Esp32Message
            {
                SourceDeviceId = sourceDeviceId,
                TargetDeviceId = targetDevice?.Id,
                Message = message
            };

            try
            {
                if (targetSocket is not { State: WebSocketState.Open })
                {
                    log.Response = string.IsNullOrWhiteSpace(targetDeviceKey)
                        ? "Mensaje recibido, sin ESP32 destino configurado."
                        : $"Mensaje recibido, pero {targetDeviceKey} no esta conectado.";
                    log.WasProcessed = false;
                }
                else
                {
                    var payload = Encoding.UTF8.GetBytes(message);
                    await targetSocket.SendAsync(payload, WebSocketMessageType.Text, true, cancellationToken);
                    log.Response = $"Mensaje reenviado a {targetDeviceKey}.";
                    log.WasProcessed = true;
                }
            }
            catch (Exception ex)
            {
                log.Response = "No se pudo procesar el mensaje.";
                log.ProcessingError = ex.Message;
                log.WasProcessed = false;
            }

            db.Esp32Message.Add(log);
            await db.SaveChangesAsync(cancellationToken);

            var sourceDeviceKey = await db.Esp32Device
                .Where(device => device.Id == sourceDeviceId)
                .Select(device => device.DeviceKey)
                .FirstAsync(cancellationToken);

            PublishMessage(messageStreams, new MessageEvent(
                log.Id,
                sourceDeviceKey,
                targetDevice?.DeviceKey,
                log.Message,
                log.Response,
                log.WasProcessed,
                log.ProcessingError,
                log.CreatedAtUtc));
        }

        static void PublishMessage(
            ConcurrentDictionary<Guid, Channel<MessageEvent>> messageStreams,
            MessageEvent message)
        {
            foreach (var stream in messageStreams.Values)
            {
                stream.Writer.TryWrite(message);
            }
        }

        static async Task<Esp32Device> RegisterOrUpdateDeviceAsync(
            PruebaaspContext db,
            string deviceKey,
            CancellationToken cancellationToken)
        {
            var device = await db.Esp32Device.FirstOrDefaultAsync(item => item.DeviceKey == deviceKey, cancellationToken);

            if (device is null)
            {
                device = new Esp32Device
                {
                    DeviceKey = deviceKey.Trim(),
                    Name = deviceKey.Trim()
                };
                db.Esp32Device.Add(device);
            }

            device.LastSeenAtUtc = DateTime.UtcNow;
            await db.SaveChangesAsync(cancellationToken);
            return device;
        }

    }
}