using System.Net.Sockets;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using backend.DTOs;
using Microsoft.Extensions.Logging;

namespace backend.Services
{
    public class TuyaLocalService
    {
        private readonly ILogger<TuyaLocalService> _logger;

        public TuyaLocalService(ILogger<TuyaLocalService> logger)
        {
            _logger = logger;
        }

        public async Task<bool> SendCommandAsync(TuyaCommandRequest request, CancellationToken cancellationToken = default)
        {
            try
            {
                _logger.LogInformation("Enviando comando Tuya a {IpAddress} para {DeviceId}", request.IpAddress, request.DeviceId);
                
                using var tcpClient = new TcpClient();
                // Tuya local port is usually 6668
                var connectTask = tcpClient.ConnectAsync(request.IpAddress, 6668, cancellationToken);
                
                if (await Task.WhenAny(connectTask.AsTask(), Task.Delay(3000, cancellationToken)) != connectTask.AsTask())
                {
                    _logger.LogWarning("Timeout al conectar a {IpAddress}:6668", request.IpAddress);
                    return false;
                }

                if (!tcpClient.Connected) return false;

                await using var stream = tcpClient.GetStream();

                // 1. Construir el payload JSON (DPS)
                // Tuya utiliza un diccionario DPS. 20 suele ser encendido/apagado en dispositivos modernos, o 1 en antiguos.
                // Como es un foco (SHOME-120), el ID del switch suele ser 20.
                var dps = new Dictionary<string, object>
                {
                    { "20", request.Encendido }
                };

                var payloadObj = new
                {
                    devId = request.DeviceId,
                    uid = request.DeviceId,
                    t = DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString(),
                    dps = dps
                };

                string jsonPayload = JsonSerializer.Serialize(payloadObj);
                _logger.LogInformation("Payload generado: {Payload}", jsonPayload);

                // 2. Aquí iría el cifrado AES-128-ECB usando request.LocalKey y el empaquetado
                // del protocolo Tuya (encabezado 0x000055aa, comando 0x07, crc32, etc.)
                // TODO: Implementar el cifrado binario exacto.
                
                // Por ahora, simulamos el envío para probar la conexión
                _logger.LogInformation("TODO: Cifrar con LocalKey '{Key}' y enviar por TCP", request.LocalKey);

                // Simulamos respuesta exitosa
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al comunicarse con el dispositivo Tuya en {IpAddress}", request.IpAddress);
                return false;
            }
        }
    }
}
