using backend.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Net.WebSockets;

namespace backend.Controllers
{
    [Route("ws/camera")]
    public class CameraProxyController : ControllerBase
    {
        private readonly PruebaaspContext _context;
        private readonly ILogger<CameraProxyController> _logger;

        public CameraProxyController(PruebaaspContext context, ILogger<CameraProxyController> logger)
        {
            _context = context;
            _logger = logger;
        }

        [HttpGet("view/{deviceKey}")]
        public async Task ConnectCamera(
            string deviceKey,
            CancellationToken cancellationToken)
        {
            if (!HttpContext.WebSockets.IsWebSocketRequest)
            {
                HttpContext.Response.StatusCode = 400;
                return;
            }

            using var clientSocket = await HttpContext.WebSockets.AcceptWebSocketAsync();

            var config = await _context.AparatoConfiguracionesRed
                .FirstOrDefaultAsync(c => c.device_key == deviceKey, cancellationToken);

            if (config == null || string.IsNullOrWhiteSpace(config.ip_address))
            {
                _logger.LogWarning("Cámara {DeviceKey} no tiene IP configurada", deviceKey);
                await clientSocket.CloseAsync(
                    WebSocketCloseStatus.EndpointUnavailable,
                    "Cámara sin IP configurada",
                    cancellationToken);
                return;
            }

            var esp32Ip = config.ip_address;
            var streamUrl = $"http://{esp32Ip}:81/stream";
            _logger.LogInformation("Proxy cámara {DeviceKey} → {Url}", deviceKey, streamUrl);

            using var httpClient = new HttpClient { Timeout = Timeout.InfiniteTimeSpan };
            using var response = await httpClient.GetAsync(
                streamUrl,
                HttpCompletionOption.ResponseHeadersRead,
                cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError("No se pudo conectar al stream de {DeviceKey} en {Url}", deviceKey, streamUrl);
                await clientSocket.CloseAsync(
                    WebSocketCloseStatus.EndpointUnavailable,
                    "No se pudo conectar al stream de la cámara",
                    cancellationToken);
                return;
            }

            var stream = await response.Content.ReadAsStreamAsync(cancellationToken);
            var buffer = new byte[4096];
            var jpegBuffer = new List<byte>();
            bool inJpeg = false;

            try
            {
                while (!cancellationToken.IsCancellationRequested &&
                       clientSocket.State == WebSocketState.Open)
                {
                    var bytesRead = await stream.ReadAsync(buffer, 0, buffer.Length, cancellationToken);
                    if (bytesRead == 0) break;

                    for (int i = 0; i < bytesRead; i++)
                    {
                        byte b = buffer[i];

                        // Detect JPEG SOI marker (FF D8)
                        if (!inJpeg && i + 1 < bytesRead && b == 0xFF && buffer[i + 1] == 0xD8)
                        {
                            inJpeg = true;
                            jpegBuffer.Clear();
                            jpegBuffer.Add(b);
                            continue;
                        }

                        if (inJpeg)
                        {
                            jpegBuffer.Add(b);

                            // Detect JPEG EOI marker (FF D9)
                            if (b == 0xD9 && jpegBuffer.Count >= 2 &&
                                jpegBuffer[jpegBuffer.Count - 2] == 0xFF)
                            {
                                inJpeg = false;
                                var jpegData = jpegBuffer.ToArray();
                                jpegBuffer.Clear();

                                // Send frame to frontend client
                                if (clientSocket.State == WebSocketState.Open)
                                {
                                    await clientSocket.SendAsync(
                                        jpegData,
                                        WebSocketMessageType.Binary,
                                        true,
                                        cancellationToken);
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                _logger.LogError(ex, "Error en proxy de cámara {DeviceKey}", deviceKey);
            }
            finally
            {
                try
                {
                    await clientSocket.CloseAsync(
                        WebSocketCloseStatus.NormalClosure,
                        "Stream finalizado",
                        CancellationToken.None);
                }
                catch { }
            }
        }
    }
}
