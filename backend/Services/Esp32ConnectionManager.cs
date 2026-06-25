using System.Collections.Concurrent;
using System.Net.WebSockets;

namespace backend.Services
{
    public class Esp32ConnectionManager
    {
        private readonly ConcurrentDictionary<string, WebSocket> _sockets =
            new(StringComparer.OrdinalIgnoreCase);

        public async Task AddOrReplaceAsync(string deviceKey, WebSocket socket, CancellationToken cancellationToken)
        {
            var normalizedDeviceKey = NormalizeDeviceKey(deviceKey);

            if (_sockets.TryGetValue(normalizedDeviceKey, out var previousSocket) &&
                previousSocket.State == WebSocketState.Open)
            {
                await previousSocket.CloseAsync(
                    WebSocketCloseStatus.PolicyViolation,
                    "DeviceKey duplicado",
                    cancellationToken);
            }

            _sockets[normalizedDeviceKey] = socket;
        }

        public bool TryGetOpenSocket(string deviceKey, out WebSocket? socket)
        {
            socket = null;
            var normalizedDeviceKey = NormalizeDeviceKey(deviceKey);

            if (!_sockets.TryGetValue(normalizedDeviceKey, out var currentSocket))
            {
                return false;
            }

            if (currentSocket.State != WebSocketState.Open)
            {
                return false;
            }

            socket = currentSocket;
            return true;
        }

        public void Remove(string deviceKey, WebSocket socket)
        {
            var normalizedDeviceKey = NormalizeDeviceKey(deviceKey);

            if (_sockets.TryGetValue(normalizedDeviceKey, out var currentSocket) &&
                ReferenceEquals(currentSocket, socket))
            {
                _sockets.TryRemove(normalizedDeviceKey, out _);
            }
        }

        private static string NormalizeDeviceKey(string deviceKey)
        {
            return deviceKey.Trim();
        }

        public IEnumerable<string> GetAllConnectedDeviceKeys()
        {
            return _sockets
                .Where(kvp => kvp.Value.State == WebSocketState.Open)
                .Select(kvp => kvp.Key);
        }
    }
}
